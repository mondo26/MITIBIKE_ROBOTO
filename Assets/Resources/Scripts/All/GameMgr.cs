using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

/******************************************************************
 * * ステージに関する列挙型
 * ****************************************************************/
public enum STAGE
{
    _01,
    _02,
    _03,

    _MAX,
};

/******************************************************************
 * * データ
 * ****************************************************************/
[System.Serializable]
public class DATA
{
    public bool[] isClear = new bool[(int)STAGE._MAX];
    public bool[] isPreClear = new bool[(int)STAGE._MAX];
    public bool[] isStart = new bool[(int)STAGE._MAX];
}


/******************************************************************
 * * ゲームを管理するクラス　(SingletonMonoBehaviourから継承)
 * ****************************************************************/
[DisallowMultipleComponent]
public class GameMgr : SingletonMonoBehaviour<GameMgr>
{
    #region 定数
    private const string KEY = "KEY";
    #endregion

    #region フレームレート計測に使う変数
    private float m_updateInterval = 0.5f;
    private float m_accum;
    private int m_frames;
    private float m_timeleft;
    private float m_fps;
    #endregion

    [SerializeField, Header("ステージ")]
    private List<GameObject> stages;
    [SerializeField, Header("データを破棄するか？")]
    private bool isDestoryData;

    public DATA Data { get; set; }
    // 静的変数
    public static bool IsLock { get; set; }             // 処理を止める変数
    public static STAGE Stage { get; set; }

    /// <summary>
    /// Start 関数の前およびプレハブのインスタンス化直後に呼び出される
    /// </summary>
    public　void Awake()
    {
        // インスタンスが既にあったら削除
        if (this != Instance)
        {
            Destroy(gameObject);
            return;
        }
        CreateData();                                   // データを生成
        DontDestroyOnLoad(gameObject);                  // シーンをまたいでも削除されないようにする
    }

    /// <summary>
    /// 毎フレーム更新
    /// </summary>
	void Update ()
    {
        CheckFPS();                                                         // FPSを調べる                                                                    
    }

    /// FPSを調べる
    void CheckFPS()
    {
        m_timeleft -= Time.deltaTime;
        m_accum += Time.timeScale / Time.deltaTime;
        m_frames++;

        if (0 < m_timeleft) return;

        m_fps = m_accum / m_frames;
        m_timeleft = m_updateInterval;
        m_accum = 0;
        m_frames = 0;
    }

    /// FPS描画
    void OnGUI()
    {
        GUI.color = Color.red;
        GUILayout.Label("FPS: " + m_fps.ToString("f2"));
    }

    // データを生成
    void CreateData()
    {
        // データを削除
        if(isDestoryData)
        {
            PlayerPrefsUtils.Reload<DATA>(KEY);
        }

        // 前回のセーブデータを格納
        this.Data = PlayerPrefsUtils.GetObject<DATA>(KEY);

        // 前回のセーブデータがなければ新しくデータを生成する
        if (Data == null)
        {
            Debug.LogError("データがありませんでした。データを生成します。");
            Data = new DATA();
            PlayerPrefsUtils.SetObject<DATA>(KEY, Data);
        }
    }

    /// <summary>
    /// ステージを生成する処理
    /// </summary>
    /// <param name="_stageSelect"></param>
    public void CreateStage(STAGE _stageSelect)
    {
        //switch(_stageSelect)
        //{
        //    case STAGE._01:
        //        Instantiate(stages[0], Vector3.zero, Quaternion.identity);              // Stageを生成
        //        SoundMgr.Instance.PlayBgm("MainBGM");                                   // MainBGM再生
        //        break;
        //    case STAGE._02:
        //        break;
        //    case STAGE._03:
        //        break;
        //}
        Instantiate(stages[0], Vector3.zero, Quaternion.identity);              // Stageを生成
        SoundMgr.Instance.PlayBgm("MainBGM");                                   // MainBGM再生
    }

    // アプリケーション終了時に呼ばれる
    void　OnApplicationQuit()
    {
        // データを保存する
        PlayerPrefsUtils.SetObject<DATA>(KEY, Data);
    }
}
