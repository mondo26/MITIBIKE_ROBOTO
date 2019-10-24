using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/******************************************************************
 * * 三人称カメラ視点 (CameraBaseから継承)
 * ****************************************************************/
public class ThirdPersonCameraController : CameraBase
{
    [SerializeField, Header("カメラが障害物に衝突した際に、移動する速度")]
    protected float hitCameraSpeed;
    [SerializeField, Header("プレイヤー")]
    protected GameObject player;
    [SerializeField, Header("カメラ"), Tooltip("場所 Player/ThirdPersonCamera/MCamera")]
    private GameObject mCamera;
    [SerializeField, Header("カメラが障害物の衝突から離れた際に、元に戻るカメラの位置")]
    private GameObject preCameraPos;

    private Camera myCamera;
    private RaycastHit hit;

    /// <summary>
    /// コンストラクタ (CameraBase Override)
    /// </summary>
    public override void Awake()
    {
        this.MIN_ANGLE = -35;
        this.MAX_ANGLE = 25;
        this.preCameraPos.transform.position = mCamera.transform.position;
        this.preCameraPos.transform.rotation = mCamera.transform.rotation;
    }

    /// <summary>
    /// 更新処理 (CameraBase Ovrride)
    /// </summary>
    public override void Update()
    {
        // BaseCameraのUpdate実行
        base.Update();
    }

    /// <summary>
    /// 指定フレーム更新処理(cameraBase Override)
    /// </summary>
    public override void FixedUpdate()
    {
        // プレイヤーを追従する
        transform.position = new Vector3(player.transform.position.x, transform.position.y, player.transform.position.z);
        // BaseCameraのFixedUpdate実行
        base.FixedUpdate();
        // 障害物に当たったらズーム
        //RayCheckCamera();
    }

    /// <summary>
    /// 障害物があったらズームする処理
    /// </summary>
    private void RayCheckCamera()
    {
        // カメラが障害物と接触してたら障害物の位置に移動
        if (Physics.Linecast(player.transform.position + Vector3.up, mCamera.transform.position, out hit, LayerMask.GetMask("Block")))
        {
            mCamera.transform.position = Vector3.Lerp(mCamera.transform.position, hit.point, hitCameraSpeed * Time.deltaTime);
        }
        // 障害物と接触してなければ元のカメラ位置に移動
        else
        {
            // 前のカメラ位置と同じじゃなければ、前の位置に移動
            if (mCamera.transform.position != preCameraPos.transform.position)
            {
                if (!Physics.Linecast(mCamera.transform.position, preCameraPos.transform.position))
                {
                    mCamera.transform.position = Vector3.Lerp(mCamera.transform.position, preCameraPos.transform.position, hitCameraSpeed * Time.deltaTime);
                }
                else
                {
                    mCamera.transform.position = Vector3.Lerp(mCamera.transform.position, preCameraPos.transform.position, Time.deltaTime * 0.25f);
                }
            }
        }
        //例を視覚的に確認
        //Debug.DrawLine(player.transform.position + Vector3.up, mCamera.transform.position, Color.red, 0f, false);
        //Debug.DrawLine(mCamera.transform.position, preCameraPos.transform.position, Color.blue, 0f, false);
    }
}
