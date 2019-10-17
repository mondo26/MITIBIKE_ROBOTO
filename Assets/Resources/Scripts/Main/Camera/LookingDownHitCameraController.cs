using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LookingDownHitCameraController : MonoBehaviour
{
    [SerializeField, Header("カメラが障害物の衝突から離れた際に、元に戻るカメラの位置")]
    private GameObject preLookingDownCamera;

    private Camera myCamera;
    private float lrTrigger;

    void Awake()
    {
        preLookingDownCamera.transform.position = transform.position;
        preLookingDownCamera.transform.rotation = transform.rotation;
        myCamera = GetComponent<Camera>();
    }

    void Update()
    {
        this.lrTrigger = Input.GetAxis("PAD_LR_TRIGGER");

        // ズーム処理
        if( lrTrigger != 0)
        {
            this.myCamera.fieldOfView += lrTrigger;
        }
    }
}
