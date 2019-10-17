using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StageMgr : MonoBehaviour
{
    #region 定数
    private const int SIXTY = 60;
    #endregion

    [SerializeField, Header("プレイヤー")]
    private GameObject player;
    [SerializeField, Header("見下ろし視点カメラ")]
    private GameObject LookingDownCamera;
    [SerializeField, Header("制限時間のテキスト")]
    private GameObject limitTimeText;
    [SerializeField, Header("制限時間のテキスト背景")]
    private GameObject limitTimeBackGround;
    [SerializeField, Header("制限時間")]
    private int timeLimit;

    private GameObject prefab;
    private PlayerController playerController;
    private Text limitText;
    private Image limitBackGround;
    private int fps;

    void Start ()
    {
        this.limitText = limitTimeText.GetComponent<Text>();
        this.limitBackGround = limitTimeBackGround.GetComponent<Image>();
        //UIRender();                                         
	}

    void Update()
    {
        // Playerが生成されてなく、Xボタンを押したら
        if(!prefab && Input.GetButtonDown("PAD_X_BUTTON"))
        {
            GenerateRobot();                // ロボットを生成
        }
    }

    void FixedUpdate()
    {
        // プレイヤーが生成されていたら
        if(prefab)
        {
            // 現在ゲーム上にいるロボットの稼働時間を引いていく
            --playerController._LifeTime;
           // CheckTimeMeasurement();         // ロボットの稼働時間を引く
        }
    }

    /*******************************************************************
     * *　経過時間を計測する処理
    * *****************************************************************/
    void CheckTimeMeasurement()
    {
        // 現在ゲーム上にいるロボットの稼働時間を引いていく
        --playerController._LifeTime;
        // 秒数加算処理
        if (++fps >= SIXTY)
        {
            --timeLimit;                                    // タイムリミットの時間を引いていく
            UIRender();                                     // UIを描画
            fps = 0;
        }
    }

    /*******************************************************************
     * *　UI描画
    * *****************************************************************/
    void UIRender()
    {
        // 制限時間テキスト描画
        string a = timeLimit / SIXTY >= 10 ? (timeLimit / SIXTY).ToString() : "0" + (timeLimit / SIXTY).ToString();
        string b = timeLimit % SIXTY >= 10 ? (timeLimit % SIXTY).ToString() : "0" + (timeLimit % SIXTY).ToString();
        this.limitText.text = a.ToString() +":"+ b.ToString();

        if(timeLimit <= 0)
        {
            this.limitBackGround.color = Color.black;
        }
    }

    /*******************************************************************
     * *　ロボットを生成する処理
    * *****************************************************************/
    public void GenerateRobot()
    {
        this.prefab = Instantiate(player, new Vector3(10, 5, 0), Quaternion.identity);
        this.playerController = prefab.GetComponent<PlayerController>();
        this.playerController._StageMgr = this.gameObject.GetComponent<StageMgr>();
        this.playerController._ThirdPersonCamera.SetActive(true);
    }

    /*******************************************************************
     * *　ステージクリア処理
    * *****************************************************************/
    public void StageClear()
    {
        Debug.Log("Clear");
    }

    

}
