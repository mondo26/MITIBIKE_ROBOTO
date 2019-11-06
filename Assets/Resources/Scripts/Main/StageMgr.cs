using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/******************************************************************
 * * ステージを管理するクラス
 * ****************************************************************/
public class StageMgr : MonoBehaviour
{
    #region 定数
    private const int SIXTY = 60;
    #endregion

    [SerializeField, Header("プレイヤー")]
    private GameObject player;
    [SerializeField, Header("ステージUI")]
    private GameObject stageUI;

    private GameObject startCamera;
    private GameObject prefab;
    private PlayerController playerController;
    private XboxInput xboxInput;

    public GameObject _Prefab { set { prefab = value; } }

    void Start ()
    {
        this.xboxInput = new XboxInput();
        this.startCamera = GameObject.Find("StartCamera");
	}

    void Update()
    {
        xboxInput.InputUpdate();        // 入力更新
    }

    void FixedUpdate()
    {
        // ロック中ならこれ以降処理を読まない
        if (GameMgr.IsLock) { return; }

        // プレイヤーが生成されていたら
        if(prefab)
        {
            // 現在ゲーム上にいるロボットの稼働時間を引いていく
            --playerController._LifeTime;
        }
        // Playerが生成されてなく
        else
        {
            // Xボタンを押したらロボット生成
            if (xboxInput.Check(XboxInput.KEYMODE.DOWN, XboxInput.PAD.KEY_X))
            {
                GenerateRobot();
            }
        }

        // MENUボタンを押すとMENU画面へ
        if (xboxInput.Check(XboxInput.KEYMODE.DOWN, XboxInput.PAD.KEY_MENU))
        {
            ShowingMenu();
        }

        xboxInput.Initialize();        // 入力初期化
    }


    /// <summary>
    /// ロボットを生成する処理
    /// </summary>
    void GenerateRobot()
    {
        this.prefab = Instantiate(player, new Vector3(10, 5, 0), Quaternion.identity);
        this.playerController = prefab.GetComponent<PlayerController>();
        this.playerController._StageMgr = this.gameObject.GetComponent<StageMgr>();
        this.playerController._ThirdPersonCamera.SetActive(true);
    }

    /// <summary>
    /// メニューを表示する処理
    /// </summary>
    void ShowingMenu()
    {
        Time.timeScale = 0.0f;
        stageUI.SetActive(true);
    }

    /// <summary>
    /// ステージをクリアする処理
    /// </summary>
    public void StageClear()
    {
        //startCamera.transform.position = prefab.transform.position + (prefab.transform.forward * 10) + (prefab.transform.up * 10);
        //startCamera.transform.LookAt(prefab.transform.position);
        //var cameraController = startCamera.GetComponent<Camera>();
        //cameraController.depth = 10;
        //if (!GameMgr.IsLock)
        //{
        //    Debug.Log("Clear");
        //    SceneMgr.NextScene("Select");
        //}

        StartCoroutine("StageClearProduction");
    }

    // ステージクリアの演出
    IEnumerator StageClearProduction()
    {
        GameMgr.IsLock = true;              // 入力を受け付けないようにする
        yield return null;
        // カメラでキャラクターを捉える
        startCamera.transform.position = prefab.transform.position + (prefab.transform.forward * 6) + (prefab.transform.up);
        startCamera.transform.LookAt(prefab.transform.position + new Vector3(0, 1, 0));
        var cameraController = startCamera.GetComponent<Camera>();
        cameraController.depth = 10;
        // クリアアニメーション再生
        playerController.AniState = PlayerController.ANIMATION_STATE._CLEAR_ANIMATION;
        yield return new WaitForSeconds(2.0f);
        // セレクトシーンに移動
        SceneMgr.NextScene("Select");
    }
}
