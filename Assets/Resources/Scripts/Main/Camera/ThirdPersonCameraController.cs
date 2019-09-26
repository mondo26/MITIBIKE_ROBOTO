﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThirdPersonCameraController : MonoBehaviour
{

    [SerializeField, Header("プレイヤー")]
    private GameObject player;
    [SerializeField, Header("カメラの回転する速度")]                                      
    private float rotateSpeed;                                            
    private float yaw, pitch;                                                   // 横縦の回転量を格納
    private const float MIN_ANGLE = -30;                                        // Angleの最小値
    private const float MAX_ANGLE = 30;                                         // Angleの最大値

    public virtual void Start()
    {
    }

    public virtual void Update()
    {
        // プレイヤー位置を追従する
        transform.position = new Vector3(player.transform.position.x, transform.position.y, player.transform.position.z);

        yaw += Input.GetAxis("Horizontal_Camera") * rotateSpeed;                // 横回転入力
        pitch -= Input.GetAxis("Vertical_Camera") * rotateSpeed;                // 縦回転入力
        pitch = Mathf.Clamp(pitch, MIN_ANGLE, MAX_ANGLE);                       // 縦回転角度制限する

        transform.eulerAngles = new Vector3(pitch, yaw, 0.0f);                  // 回転の実行
    }
}