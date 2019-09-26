using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StageMgr : MonoBehaviour
{
    private const int SIXTY = 60;

    [SerializeField, Header("プレイヤー")]
    private GameObject player;
    [SerializeField, Header("制限時間のテキスト")]
    private GameObject limitTimeText;
    [SerializeField, Header("制限時間のテキスト背景")]
    private GameObject limitTimeBackGround;
    [SerializeField, Header("制限時間")]
    private int timeLimit;

    private PlayerController playerController;
    private Text limitText;
    private Image limitBackGround;
    private int fps;
    private bool stageClearFlg;

    void Start ()
    {
        this.limitText = limitTimeText.GetComponent<Text>();
        this.limitBackGround = limitTimeBackGround.GetComponent<Image>();
        UIRender();                                         
        GenerateRobot();
	}

    void Update()
    {
    }

    void FixedUpdate()
    {
        CheckTimeMeasurement();
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
        GameObject prefab = Instantiate(player, new Vector3(10, 5, 0), Quaternion.identity);
        this.playerController = prefab.GetComponent<PlayerController>();
        this.playerController._StageMgr = this.gameObject.GetComponent<StageMgr>();
    }

    /*******************************************************************
     * *　ステージクリア処理
    * *****************************************************************/
    public void StageClear()
    {
        Debug.Log("Clear");
    }

}
