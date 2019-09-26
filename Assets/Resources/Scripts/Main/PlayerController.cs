using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour
{
    private const int MAX_OPERATING_TIME = 10;                              // ロボットの稼働時間の最大値
    private const int MAX_OPERATING_TIME_FPS = MAX_OPERATING_TIME * 60;     // ロボットの稼働時間（フレーム単位）
    private const float RAY_LENGTH = 1.5f;                                  // レイを放つ距離
    private const int MIN_STAGE_POS = -10;                                  // ステージの最小座標（画面外）

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
    [SerializeField, Header("ロボットを追従するカメラ(三人称カメラの時に使用)")]
    private GameObject rayHitCamera;
    [SerializeField, Header("一人称カメラ")]
    private GameObject firstPersonCamera;
    [SerializeField, Header("三人称カメラ")]
    private GameObject thirdPersonCamera;
    [SerializeField, Header("ロボットのゲージ画像")]
    private GameObject gauge;
    [SerializeField, Header("ジャンプにかかる時間")]
    private float jumpSpeed;
    [SerializeField, Header("ジャンプ量")]
    private float jumpForce;

    private StageMgr stageMgr;
    private Rigidbody rigidBody;
    private Animator animator;
    private Vector3 moveDirection, cameraDirection;
    private Vector3 startPoint, wayPoint, endPoint;
    private Ray ray, upRay;
    private RaycastHit rayHit;
    private Image image;
    private Dictionary<string, bool> xboxButton = new Dictionary<string, bool>();
    private ANIMATION_STATE AniState, preAniState;
    private PLAYER_STATE playerState;
    private float horizontal, vertical;
    private float jumpTimer, totalTimer;
    private float lifeTime;
    private bool isGround;
    private int iCount;

    public StageMgr _StageMgr { set { stageMgr = value; } }
    public float _LifeTime { get { return lifeTime; } set { lifeTime = value; } }

    void Start ()
    {
        this.lifeTime = MAX_OPERATING_TIME_FPS;
        this.rigidBody = GetComponent<Rigidbody>();
        this.animator = GetComponent<Animator>();
        this.image = gauge.GetComponent<Image>();
        this.playerState = PLAYER_STATE._MOVE;

        xboxButton.Add("A", false);
        xboxButton.Add("B", false);
        xboxButton.Add("X", false);
        xboxButton.Add("Y", false);
        xboxButton.Add("RB", false);
        xboxButton.Add("LB", false);
    }

    /*******************************************************************
     * * 毎フレーム更新
     * *****************************************************************/
    void Update()
    {

        this.horizontal = Input.GetAxis("Horizontal");
        this.vertical = Input.GetAxis("Vertical");

        Check_XBOX_Button();                                // XBOXのボタン入力チェック

        // 現在ロボットがジャンプしていなかったらジャンプできるか調べる
        if (playerState != PLAYER_STATE._JUMP)
        {
            this.moveDirection = Vector3.zero;
            CheckJumping();
        }
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
        this.ray = new Ray(transform.position + Vector3.up / 2, transform.forward);
        this.upRay = new Ray(transform.position + Vector3.up * 2, transform.forward);
        // Rayを視覚的に描画
        Debug.DrawRay(ray.origin, ray.direction * RAY_LENGTH, Color.blue);
        Debug.DrawRay(upRay.origin, upRay.direction * RAY_LENGTH, Color.red);
        // ロボットの前方にあるRayがHitし、ロボットの上方にあるRayがHitしていなければ
        if (Physics.Raycast(ray, out rayHit, RAY_LENGTH) && !Physics.Raycast(upRay, RAY_LENGTH))
        {
            if (Input.GetButtonDown("PAD_B_BUTTON"))
            {
                // スタート地点、中間地点、最終地点の座標格納
                this.startPoint = transform.position;
                this.wayPoint = (transform.position + rayHit.transform.position) * 0.5f + Vector3.up * jumpForce;
                this.endPoint = rayHit.transform.position + Vector3.up * 2;
                this.playerState = PLAYER_STATE._JUMP;
            }
        }
    }

    /*******************************************************************
     * * 指定フレーム更新
     * *****************************************************************/
    void FixedUpdate()
    {
        // ロボットが稼働可能か調べる
        if (CheckOperating())
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
    }

    /*******************************************************************
     * * 動きの処理  
     * *****************************************************************/
    void PlayerMove()
    {
        // ロボットが空中にいたらこれ以降処理を読まない
        if (!isGround) { return; }

        this.moveDirection = Vector3.zero;
        this.moveDirection = new Vector3(horizontal, 0.0f, vertical).normalized * walkSpeed;

        if(horizontal != 0 || vertical != 0)
        {
            transform.rotation = Quaternion.LookRotation(new Vector3(moveDirection.x, 0.0f, moveDirection.z).normalized);
        }

        // Yボタンが押されたらロボットを稼働停止させる
        if (xboxButton["Y"])
        {
            StopMove();                                         
            xboxButton["Y"] = false;
            return;
        }

        //this.totalTimer += Time.deltaTime;                                                               // total時間計測
        //this.moveDirection.y = -500f * totalTimer;                                   　                  // 重力設定
        this.rigidBody.velocity = moveDirection * Time.deltaTime;                                          // 速度設定
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
        this.jumpTimer += Time.deltaTime / jumpSpeed;                      　     // ロボットの進む割合をTime.deltaTimeで決める
        var start = Vector3.Lerp(startPoint, wayPoint, jumpTimer);                // スタート地点から中継地点までのベクトル上を通る点の現在の位置
        var end = Vector3.Lerp(wayPoint, endPoint, jumpTimer);                    // 中継地点からターゲットまでのベクトル上を通る点の現在の位置
        rigidBody.MovePosition(Vector3.Lerp(start, end, jumpTimer));              // 上の二つの点を結んだベクトル上を通る点の現在の位置（ロボットの位置）                  

        // ジャンプが終了したMOVE状態へ
        if (jumpTimer >= 1.0f)
        {
            playerState = PLAYER_STATE._MOVE;
            this.jumpTimer = 0;
            return;
        }

    }

    /*******************************************************************
     * * アニメーションの処理
     * *****************************************************************/
    void AnimationState()
    {
        // 現在流れているanimationが前のアニメーションと違うなら処理
        if(preAniState != AniState)
        {
            switch(AniState)
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
     * *　ロボットが稼働可能か調べる　TRUE(稼働中)　FALSE(稼働終了)
     * *****************************************************************/
    bool CheckOperating()
    {
        this.image.fillAmount = lifeTime / MAX_OPERATING_TIME_FPS;               // ロボットの稼働時間ゲージを描画

        // ロボットがステージ外に落ちたら活動を停止し、ロボットを消す
        if (transform.position.y <= MIN_STAGE_POS)
        {
            StopMove();
            Destroy(this.gameObject);
            return false;
        }

        // 稼働時間が０でロボットが地面についていれば、活動を停止させる
        if (lifeTime <= 0 && isGround)
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
        stageMgr.GenerateRobot();                               // 次のロボットを生成
        this.lifeTime = 0;                                      // 秒数初期化
        GetComponent<Rigidbody>().isKinematic = true;           // 物理演算の影響を受けないようにする
        Destroy(GetComponent<PlayerController>());              // このコンポーネント削除
    }

    /*******************************************************************
     * *　衝突判定
     * *****************************************************************/

    private void OnTriggerStay(Collider other)
    {
        // 地上に着いたらisGroundをTRUE
        if (!isGround)
        {
            Debug.LogError("地面に着きました");
            this.isGround = true;
        }

        if(playerState == PLAYER_STATE._JUMP && jumpTimer >= 0.5f)
        {
            playerState = PLAYER_STATE._MOVE;
            this.jumpTimer = 0;
            //Debug.LogError("空中でぶつかりました");
        }

    }

    private void OnTriggerExit(Collider other)
    {
        // 地上を離れたらisGroundをFALSE
        if (isGround)
        {
            List<Ray> groundRays = new List<Ray>();
            groundRays.Add(new Ray(transform.position + new Vector3(0, -0.1f, 0), transform.up * -1));
            groundRays.Add(new Ray(transform.position + new Vector3(0.4f, -0.1f, 0.4f), transform.up * -1));
            groundRays.Add(new Ray(transform.position + new Vector3(0.4f, -0.1f, -0.4f), transform.up * -1));
            groundRays.Add(new Ray(transform.position + new Vector3(-0.4f, -0.1f, 0.4f), transform.up * -1));
            groundRays.Add(new Ray(transform.position + new Vector3(-0.4f, -0.1f, -0.4f), transform.up * -1));

            for (int i = 0; i < groundRays.Count; i++)
            {
                if (Physics.Raycast(groundRays[i], 0.1f))
                {
                    break;   
                }
                else
                {
                    this.iCount++;
                }
                Debug.DrawRay(groundRays[i].origin, groundRays[i].direction * 0.1f, Color.red);
            }

            if(iCount == groundRays.Count)
            {
                Debug.Log("ジャンプ中");
                rigidBody.velocity = Vector3.zero;
                this.isGround = false;
            }

            this.iCount = 0;
        }
    }
}
