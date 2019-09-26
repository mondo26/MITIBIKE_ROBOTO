using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RayHitCameraController : MonoBehaviour
{
    [SerializeField, Header("カメラが障害物の衝突から離れた際に、元に戻るカメラの位置")]
    private GameObject preCameraPos;
    [SerializeField, Header("プレイヤー")]
    private GameObject player;
    [SerializeField, Header("カメラが障害物に衝突した際に、移動する速度")]
    private float hitCameraSpeed;
    private RaycastHit hit;                                                     //レイに当たったオブジェクトの情報取得するため

    void Start()
    {
        preCameraPos.transform.position = transform.position;
        preCameraPos.transform.rotation = transform.rotation;
    }

    void Update()
    {
        //カメラが障害物と接触してたら障害物の位置に移動
        if (Physics.Linecast(player.transform.position + Vector3.up, transform.position, out hit, LayerMask.GetMask("Wall")))
        {
            transform.position = Vector3.Lerp(transform.position, hit.point, hitCameraSpeed * Time.deltaTime);
        }
        //障害物と接触してなければ元のカメラ位置に移動
        else
        {
            //元の位置ではないときだけ元の位置に移動
            if (transform.position != preCameraPos.transform.position)
            {
                transform.position = Vector3.Lerp(transform.position, preCameraPos.transform.position, hitCameraSpeed * Time.deltaTime);
            }
        }
        //例を視覚的に確認
        Debug.DrawLine(player.transform.position + Vector3.up, transform.position, Color.red, 0f, false);
    }
}
