using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/******************************************************************
 * * 見下ろし視点カメラ　(CameraBaseから継承)
 * ****************************************************************/
public class LookingDownCameraController : CameraBase
{
    #region 定数(const)
    private const int MAX_ZOOM_IN = 100;                    // フェードインできる最大値
    private const int MAX_ZOOM_OUT = 10;                    // フェードアウトできる最大値
    #endregion
    [SerializeField, Header("ズームスピード")]
    private float zoomSpeed;
    [SerializeField, Header("カメラ"), Tooltip("場所 Stage01/LookingDownCamera/MCamera")]
    private GameObject mCamera;

    private Camera myCamera;
    private float LRTrigger;

    /// <summary>
    /// コンストラクタ (cameraBase Override)
    /// </summary>
    public override void Awake()
    {
        this.MIN_ANGLE = -45;
        this.MAX_ANGLE = 30;
        this.myCamera = mCamera.GetComponent<Camera>();
    }

    /// <summary>
    /// 更新処理 (cameraBase Override)
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
        // BaseCameraのFixedUpdate実行
        base.FixedUpdate();
        // Zoom処理
        Zoom();
    }

    /// <summary>
    /// ズーム処理
    /// </summary>
    private void Zoom()
    {
        this.LRTrigger = Input.GetAxis("PAD_LR_TRIGGER");

        // ズームイン処理
        if(myCamera.fieldOfView <= MAX_ZOOM_IN && 0 < LRTrigger)
        {
            this.myCamera.fieldOfView += LRTrigger * Time.deltaTime * zoomSpeed;
        }
        // ズームアウト処理
        else if(myCamera.fieldOfView >= MAX_ZOOM_OUT && 0 > LRTrigger)
        {
            this.myCamera.fieldOfView += LRTrigger * Time.deltaTime * zoomSpeed;
        }
    }
}
