using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/******************************************************************
 * * プレイヤーコントローラー
 * ****************************************************************/
public class PlayerController : MonoBehaviour
{
    #region 定数(const)
    private const float RAY_LENGTH = 1.5f;                                  // レイを放つ距離
    private const int MIN_STAGE_POS = -10;                                  // ステージの最小座標（画面外）
    #endregion

    #region 列挙型(enum)
    public enum ANIMATION_STATE    // アニメーションの状態
    {
        _IDLE_ANIMATION,
        _WALK_ANIMATION,
        _RUN_ANIMATION,
        _STOP_ANIMATION,
        _CLEAR_ANIMATION,
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
    #endregion

    [SerializeField, Header("ロボットを追従するカメラ(三人称カメラの時に使用)")]
    private GameObject mCamera;
    [SerializeField, Header("三人称カメラ")]
    private GameObject thirdPersonCamera;
    [SerializeField, Header("UI")]
    private GameObject UI;
    [SerializeField, Header("ロボットのゲージ画像")]
    private GameObject gauge;
    [SerializeField, Header("色を変えるオブジェクト"), Tooltip("Player/Cube.002")]
    private GameObject changeColorObj;
    [SerializeField, Header("ジャンプできるか調べるレイヤー")]
    private LayerMask jumpLayerMask;
    [SerializeField, Header("稼働停止時にサイズを変えるコライダー")]
    private BoxCollider changeCollider;
    [SerializeField, Header("稼働停止の際に変えるマテリアル")]
    private Material[] materials;
    [SerializeField, Header("歩く速度")]
    private float walkSpeed;
    [SerializeField, Header("ロボットの稼働時間の最大値")]
    private int MAX_OPERATING_TIME = 10;
    [SerializeField, Header("ジャンプにかかる時間")]
    private float jumpSpeed;
    [SerializeField, Header("ジャンプ量")]
    private float jumpForce;

    private StageMgr stageMgr;
    private XboxInput xboxInput;
    private Rigidbody rigidBody;
    private Animator animator;
    private Vector3 moveDirection, cameraDirection;
    private Vector3 startPoint, wayPoint, endPoint;
    private Ray ray, upRay;
    private RaycastHit rayHit;
    private Image image;
    private ANIMATION_STATE preAniState;
    private PLAYER_STATE playerState;
    private float horizontal, vertical;
    private float jumpTimer, lifeTime;
    private bool isGround;
    private int MAX_OPERATING_TIME_FPS;

    // StageMgr.csで使用 getter, setter
    public ANIMATION_STATE AniState { get; set; }
    public StageMgr _StageMgr { set { stageMgr = value; } }
    public GameObject _ThirdPersonCamera { get { return thirdPersonCamera; } set { thirdPersonCamera = value; } }
    public float _LifeTime { get { return lifeTime; } set { lifeTime = value; } }

    void Start ()
    {
        this.MAX_OPERATING_TIME_FPS =  MAX_OPERATING_TIME * 60;            // ロボットの稼働時間（フレーム単位変換）
        this.lifeTime = MAX_OPERATING_TIME_FPS;
        this.rigidBody = GetComponent<Rigidbody>();
        this.animator = GetComponent<Animator>();
        this.image = gauge.GetComponent<Image>();
        this.xboxInput = new XboxInput();
        this.playerState = PLAYER_STATE._MOVE;
    }

    /*******************************************************************
     * * 毎フレーム更新
     * *****************************************************************/
    void Update()
    {
        this.xboxInput.InputUpdate();                   // xBox入力更新
        this.horizontal = Input.GetAxis("Horizontal");
        this.vertical = Input.GetAxis("Vertical");
    }

    /*******************************************************************
     * * ロボットがジャンプできるか調べる処理 
     * *****************************************************************/
    void CheckJumping()
    {
        // Rayを生成
        this.ray = new Ray(transform.position + Vector3.up / 2, transform.forward);
        this.upRay = new Ray(transform.position + Vector3.up * 2, transform.forward);
        //// Rayを視覚的に描画
        //Debug.DrawRay(ray.origin, ray.direction * RAY_LENGTH, Color.blue);
        //Debug.DrawRay(upRay.origin, upRay.direction * RAY_LENGTH, Color.red);

        // ロボットの前方にあるRayがHitし、ロボットの上方にあるRayがHitしていなければ
        if (Physics.Raycast(ray, out rayHit, RAY_LENGTH, jumpLayerMask) && !Physics.Raycast(upRay, RAY_LENGTH))
        {
            // Bボタンでジャンプ処理
            if (xboxInput.Check(XboxInput.KEYMODE.DOWN, XboxInput.PAD.KEY_B))
            {
                // スタート地点、中間地点、最終地点の座標格納
                this.startPoint = transform.position;
                this.wayPoint = (transform.position + rayHit.transform.position) * 0.5f + Vector3.up * jumpForce;
                this.endPoint = rayHit.transform.position + Vector3.up * 2;
                // スピード初期化、目的地にプレイヤーを向ける、ジャンプ状態へ
                this.moveDirection = Vector3.zero;
                this.rigidBody.velocity = Vector3.zero;
                Vector3 dir = (rayHit.transform.position - transform.position).normalized;
                transform.rotation = Quaternion.LookRotation(new Vector3(dir.x, 0.0f, dir.z));
                this.playerState = PLAYER_STATE._JUMP;
            }
        }
    }

    /*******************************************************************
     * * 指定フレーム更新
     * *****************************************************************/
    void FixedUpdate()
    {
        // ロック中じゃなければ処理を読む
        if (!GameMgr.IsLock)
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

            // 現在ロボットがジャンプしていなかったらジャンプできるか調べる
            if (playerState != PLAYER_STATE._JUMP)
            {
                CheckJumping();
            }
        }

        AnimationState();
        this.xboxInput.Initialize();                // Xbox入力初期化
    }

    /*******************************************************************
     * * 動きの処理  
     * *****************************************************************/
    void PlayerMove()
    {
        // ロボットが空中にいたらこれ以降処理を読まない
        if (!isGround) { return; }

        this.cameraDirection = Vector3.Scale(mCamera.transform.forward, new Vector3(1, 0, 1)).normalized;                   // カメラの方向から、X-Z平面の単位ベクトルを取得 

        if (horizontal != 0 || vertical != 0)
        {
            this.moveDirection = (cameraDirection * vertical + mCamera.transform.right * horizontal).normalized;
            transform.rotation = Quaternion.LookRotation(moveDirection);                                                    // キャラクターの向きを進行方向に
            this.moveDirection *= walkSpeed;
            this.AniState = ANIMATION_STATE._WALK_ANIMATION;
        }
        else
        {
            this.AniState = ANIMATION_STATE._IDLE_ANIMATION;
            this.moveDirection = Vector3.zero;                                      // 移動方向初期化
            rigidBody.velocity = Vector3.zero;                                      // 移動量初期化
        }

        // Yボタンが押されたらロボットを稼働停止にさせる
        if(xboxInput.Check(XboxInput.KEYMODE.DOWN, XboxInput.PAD.KEY_Y))
        {
            StopMove();
            return;
        }

        this.rigidBody.velocity = moveDirection * Time.deltaTime;                   // 速度設定
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
                    InitializeAnimation();
                    break;
                case ANIMATION_STATE._WALK_ANIMATION:
                    InitializeAnimation();
                    animator.SetBool("WALK", true);
                    break;
                case ANIMATION_STATE._RUN_ANIMATION:
                    InitializeAnimation();
                    animator.SetBool("RUN", true);
                    break;
                case ANIMATION_STATE._STOP_ANIMATION:
                    InitializeAnimation();
                    animator.SetBool("STOP", true);
                    break;
                case ANIMATION_STATE._CLEAR_ANIMATION:
                    break;
            }
            this.preAniState = this.AniState;
        }
    }

    /// <summary>
    /// アニメーションを初期化
    /// </summary>
    void InitializeAnimation()
    {
        animator.SetBool("WALK", false);
        animator.SetBool("RUN", false);
        animator.SetBool("STOP", false);
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
        this.AniState = ANIMATION_STATE._STOP_ANIMATION;                    // ロボットを止める処理
        this.stageMgr._Prefab = null;                                       // ロボットの情報をnullにして次のロボットを生成するようにする
        this.lifeTime = 0;                                                  // 秒数初期化
        changeCollider.center = new Vector3(0, 0.8f, 0);
        GetComponent<Rigidbody>().isKinematic = true;                       // 物理演算の影響を受けないようにする
        var sMeshCon = changeColorObj.GetComponent<SkinnedMeshRenderer>();
        sMeshCon.materials = materials;                                     // メッシュの色を変える
        Destroy(GetComponent<PlayerController>());                          // このコンポーネント削除
        Destroy(UI);                                                        // UIを削除
        Destroy(thirdPersonCamera);                                         // カメラを削除
    }

    /*******************************************************************
     * *　衝突判定
     * *****************************************************************/

    private void OnTriggerEnter(Collider other)
    {
        // クリア処理
        if (other.name == "ClearFlag")
        {
            this.stageMgr.StageClear();
        }
    }

    private void OnTriggerStay(Collider other)
    {
        // 地上に着いたらisGroundをTRUE
        if (!isGround)
        {
            this.isGround = true;
        }

        // ジャンプ中にオブジェクトにぶつかったら、落下する
        if(playerState == PLAYER_STATE._JUMP && jumpTimer >= 0.5f)
        {
            this.playerState = PLAYER_STATE._MOVE;
            this.jumpTimer = 0;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        // 地上を離れたらisGroundをFALSE
        if (isGround)
        {
            this.isGround = false;
        }
    }
}
