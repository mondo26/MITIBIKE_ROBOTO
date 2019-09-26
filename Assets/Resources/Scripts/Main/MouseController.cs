﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MouseController : MonoBehaviour
{
    private const int MAX_OPERATING_TIME = 10;                  // ロボットの稼働時間の最大値
    private const float RAY_LENGTH = 1.5f;                      // レイを放つ距離

    enum ANIMATION_STATE    // アニメーションの状態
    {
        _IDLE_ANIMATION,
        _WALK_ANIMATION,
        _RUN_ANIMATION,
    };

    enum XBOX_BUTTON        // X_BOXのボタン入力
    {
        _A_BUTTON,
        _B_BUTTON,
        _X_BUTTON,
        _Y_BUTTON,
        _RB_BUTTON,
        _LB_BUTTON,

        _MAX_BUTTON,
    };

    enum PLAYER_STATE       // プレイヤーの状態
    {
        _MOVE,
        _JUMP,
    };

    [SerializeField, Header("歩く速度")]
    private float walkSpeed;
    [SerializeField, Header("走る速度")]
    private float runSpeed;
    [SerializeField, Header("ロボットを追従するカメラ(三人称カメラの時に使用)")]
    private GameObject rayHitCamera;
    [SerializeField, Header("一人称カメラ")]
    private GameObject firstPersonCamera;
    [SerializeField, Header("三人称カメラ")]
    private GameObject thirdPersonCamera;
    [SerializeField, Header("ジャンプにかかる時間")]
    private float jumpSpeed;
    [SerializeField, Header("ジャンプ量")]
    private float jumpForce;

    private StageMgr stageMgr;
    private Rigidbody rigidBody;
    private Animator animator;
    private Vector3 moveDirection, cameraDirection;
    private Vector3 startPoint, wayPoint, endPoint;
    private Vector3 screenToWorldPointPosition;
    private Vector3 velocity;
    private Ray ray, upRay;
    private RaycastHit rayHit;
    private Dictionary<string, bool> xboxButton = new Dictionary<string, bool>();
    private ANIMATION_STATE AniState, preAniState;
    private PLAYER_STATE playerState;
    private float horizontal, vertical;
    private float jumpTimer, moveTimer;
    private int seconds;
    private bool isGround;

    public StageMgr _StageMgr { set { stageMgr = value; } }
    public int _Seconds { get { return seconds; } set { seconds = value; } }

    void Start()
    {
        this.screenToWorldPointPosition = transform.position;
        this.seconds = MAX_OPERATING_TIME;
        rigidBody = GetComponent<Rigidbody>();
        animator = GetComponent<Animator>();
        playerState = PLAYER_STATE._MOVE;

        xboxButton.Add("A", false);
        xboxButton.Add("B", false);
        xboxButton.Add("X", false);
        xboxButton.Add("Y", false);
        xboxButton.Add("RB", false);
        xboxButton.Add("LB", false);
    }

    void Update()
    {
        // 左ボタンクリック
        if (Input.GetMouseButtonUp(0))
        {
            Vector3 clickedGameObject = Vector3.zero;

            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit = new RaycastHit();

            if (Physics.Raycast(ray, out hit))
            {
                this.screenToWorldPointPosition = hit.collider.gameObject.transform.position;
                //this.screenToWorldPointPosition.y = 0.0f;
            }
            moveTimer = 0;
        }


        this.horizontal = Input.GetAxis("Horizontal");
        this.vertical = Input.GetAxis("Vertical");

        Check_XBOX_Button();                                // XBOXのボタン入力チェック

        //// 現在ロボットがジャンプしていなかったら
        //if(playerState != PLAYER_STATE._JUMP)
        //{
        //    CheckJumping();
        //}
        //PlayerMove();
        //AnimationState();
    }

    /*******************************************************************
     * * ボタンの入力チェック (FixedUpdateの際に使用)
     * *****************************************************************/
    void Check_XBOX_Button()
    {
        if (Input.GetButtonDown("PAD_A_BUTTON"))
        {
            xboxButton["A"] = true;
        }

        if (Input.GetButtonDown("PAD_B_BUTTON"))
        {
            xboxButton["B"] = true;
        }

        if (Input.GetButtonDown("PAD_X_BUTTON"))
        {
            xboxButton["X"] = true;
        }

        if (Input.GetButtonDown("PAD_Y_BUTTON"))
        {
            xboxButton["Y"] = true;
        }

        if (Input.GetButtonDown("PAD_RB_BUTTON"))
        {
            xboxButton["RB"] = true;
        }

        if (Input.GetButtonDown("PAD_LB_BUTTON"))
        {
            xboxButton["LB"] = true;
        }
    }

    /*******************************************************************
     * * ロボットがジャンプできるか調べる処理 
     * *****************************************************************/
    void CheckJumping()
    {
        // Rayを生成
        this.ray = new Ray(transform.position + Vector3.up, transform.up * -1);
        this.upRay = new Ray(transform.position + Vector3.up * 3, transform.up * -1);
        // Rayを視覚的に描画
        Debug.DrawRay(ray.origin, ray.direction * RAY_LENGTH, Color.blue);
        Debug.DrawRay(upRay.origin, upRay.direction * RAY_LENGTH, Color.red);
        // ロボットの前方にあるRayがHitし、ロボットの上方にあるRayがHitしていなければ
        if (Physics.Raycast(ray, out rayHit, RAY_LENGTH) && !Physics.Raycast(upRay, RAY_LENGTH))
        {
            // スタート地点、中間地点、最終地点の座標格納
            this.startPoint = transform.position;
            this.wayPoint = (transform.position + rayHit.transform.position) * 0.5f + Vector3.up * jumpForce;
            this.endPoint = rayHit.transform.position + Vector3.up * 2;
            this.playerState = PLAYER_STATE._JUMP;
        }
    }

    void FixedUpdate()
    {
        // ロボットが稼働可能か調べる
        if (CheckOperatingTimer())
        {
            // 状態に合わせて処理を実行
            switch (playerState)
            {
                case PLAYER_STATE._MOVE:
                    PlayerMove();
                    break;
                case PLAYER_STATE._JUMP:
                    PlayerJumping();
                    break;
            }
        }

        if (!isGround) { return; }
        this.velocity = Vector3.zero;

        if (Vector3.Distance(transform.position, screenToWorldPointPosition) > 0.1f)
        {
            this.moveDirection = (screenToWorldPointPosition - transform.position).normalized;
            this.velocity = new Vector3(moveDirection.x * 5, velocity.y, moveDirection.z * 5);
            if (playerState != PLAYER_STATE._JUMP && transform.position.y < screenToWorldPointPosition.y)
            {
                Debug.Log("ロボットのy軸座標" + transform.position.y);
                Debug.Log("障害物のy軸座標" + transform.position.y);
                CheckJumping();
            }
        }
        //transform.LookAt(transform.position + new Vector3(moveDirection.y, 0, moveDirection.y));
        velocity.y += rigidBody.velocity.y;
        rigidBody.velocity = velocity * Time.deltaTime;
    }

    /*******************************************************************
     * * 動きの処理  
     * *****************************************************************/
    void PlayerMove()
    {
        // ロボットが空中にいたらこれ以降処理を読まない
        if (!isGround) { return; }

        this.moveDirection = Vector3.zero;

        this.moveDirection = (transform.right * horizontal + transform.up * vertical * -1) * 100;
        moveDirection.y += Physics.gravity.y * Time.deltaTime;
        rigidBody.velocity = moveDirection * Time.deltaTime;

        // Yボタンが押されたら
        if (xboxButton["Y"])
        {
            StopMove();                                         // ロボットを稼働停止させる
            xboxButton["Y"] = false;
        }
        //// カメラ切り替え
        //if (Input.GetButtonDown("PAD_LB_BUTTON"))
        //{
        //    Debug.Log("OK");
        //    firstPersonCamera.SetActive(!firstPersonCamera.activeSelf);
        //    thirdPersonCamera.SetActive(!thirdPersonCamera.activeSelf);
        //}

        //this.horizontal = Input.GetAxis("Horizontal");
        //this.vertical = Input.GetAxis("Vertical");

        //// 一人称視点時のロボットの移動
        //if(firstPersonCamera.activeSelf)
        //{
        //    this.cameraDirection = Vector3.Scale(firstPersonCamera.transform.forward, new Vector3(1, 0, 1)).normalized;              // カメラの方向から、X-Z平面の単位ベクトルを取得 
        //    transform.rotation = Quaternion.LookRotation(cameraDirection);                                                           // カメラの向きを進行方向に

        //    if (horizontal != 0 || vertical != 0)
        //    {
        //        moveDirection = transform.forward * vertical + transform.right * horizontal;

        //        if (Input.GetButton("PAD_RB_BUTTON"))
        //        {
        //            rigidBody.velocity = moveDirection * runSpeed;
        //            this.AniState = ANIMATION_STATE._RUN_ANIMATION;
        //        }
        //        else
        //        {
        //            rigidBody.velocity = moveDirection * walkSpeed;
        //            this.AniState = ANIMATION_STATE._WALK_ANIMATION;
        //        }
        //    }
        //    else
        //    {
        //        AniState = ANIMATION_STATE._IDLE_ANIMATION;
        //        this.moveDirection = Vector3.zero;                                      // 移動方向初期化
        //        rigidBody.velocity = Vector3.zero;                                      // 移動量初期化
        //    }
        //}
        //// 三人称視点時のロボットの移動
        //else
        //{
        //    this.cameraDirection = Vector3.Scale(rayHitCamera.transform.forward, new Vector3(1, 0, 1)).normalized;              // カメラの方向から、X-Z平面の単位ベクトルを取得 

        //    if (horizontal != 0 || vertical != 0)
        //    {
        //        this.moveDirection = cameraDirection * vertical + rayHitCamera.transform.right * horizontal;                    // 方向キーの入力値とカメラの向きから、移動方向を決定

        //        if (Input.GetButton("PAD_RB_BUTTON"))
        //        {
        //            rigidBody.velocity = moveDirection * runSpeed;
        //            this.AniState = ANIMATION_STATE._RUN_ANIMATION;
        //        }
        //        else
        //        {
        //            rigidBody.velocity = moveDirection * walkSpeed;
        //            this.AniState = ANIMATION_STATE._WALK_ANIMATION;
        //        }
        //        transform.rotation = Quaternion.LookRotation(moveDirection);            // キャラクターの向きを進行方向に
        //    }
        //    else
        //    {
        //        AniState = ANIMATION_STATE._IDLE_ANIMATION;
        //        this.moveDirection = Vector3.zero;                                      // 移動方向初期化
        //        rigidBody.velocity = Vector3.zero;                                      // 移動量初期化
        //    }
        //}

    }

    /*******************************************************************
     * * ジャンプの処理  
     * *****************************************************************/
    void PlayerJumping()
    {
        // ジャンプが終了したMOVE状態へ
        if (jumpTimer >= 1.0f)
        {
            playerState = PLAYER_STATE._MOVE;
            this.jumpTimer = 0;
            return;
        }

        this.jumpTimer += Time.deltaTime / jumpSpeed;
        var s = Vector3.Lerp(startPoint, wayPoint, jumpTimer);
        var e = Vector3.Lerp(wayPoint, endPoint, jumpTimer);
        transform.position = Vector3.Lerp(s, e, jumpTimer);
    }

    /*******************************************************************
     * * アニメーションの処理
     * *****************************************************************/
    void AnimationState()
    {
        // 現在流れているanimationが前のアニメーションと違うなら処理
        if (preAniState != AniState)
        {
            switch (AniState)
            {
                case ANIMATION_STATE._IDLE_ANIMATION:
                    animator.SetBool("WALK", false);
                    animator.SetBool("RUN", false);
                    break;
                case ANIMATION_STATE._WALK_ANIMATION:
                    animator.SetBool("WALK", true);
                    break;
                case ANIMATION_STATE._RUN_ANIMATION:
                    animator.SetBool("RUN", true);
                    break;
            }
            this.preAniState = this.AniState;
        }
    }

    /*******************************************************************
     * *　ロボットの稼働時間を調べる　TRUE(稼働中)　FALSE(稼働終了)
     * *****************************************************************/
    bool CheckOperatingTimer()
    {
        // 稼働時間が０になったら
        if (seconds <= 0)
        {
            StopMove();
            return false;
        }
        return true;
    }

    /*******************************************************************
     * *　ロボットの稼働を停止させる処理
     * *****************************************************************/
    void StopMove()
    {
        // ロボットが空中にいたらこれ以降処理を読まない
        if (!isGround) { return; }

        stageMgr.GenerateRobot();                               // 次のロボットを生成
        this.seconds = 0;                                       // 秒数初期化
        GetComponent<Rigidbody>().isKinematic = true;           // 物理演算の影響を受けないようにする
        GetComponent<Renderer>().material.color = Color.blue;
        Destroy(GetComponent<PlayerController>());              // このコンポーネント削除
    }

    /*******************************************************************
     * *　コリジョン判定
     * *****************************************************************/
    private void OnCollisionStay(Collision collision)
    {
        this.isGround = true;
    }

    private void OnCollisionExit(Collision collision)
    {
        this.isGround = false;
        rigidBody.velocity = Vector3.zero;
    }
}