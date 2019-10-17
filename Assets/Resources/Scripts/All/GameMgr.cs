using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// シングルトンを継承したゲームマネージャー
/// </summary>
[DisallowMultipleComponent]
public class GameMgr : SingletonMonoBehaviour<GameMgr>
{
    [SerializeField, Header("ステージ")]
    private List<GameObject> stages;

    private float m_updateInterval = 0.5f;
    private float m_accum;
    private int m_frames;
    private float m_timeleft;
    private float m_fps;

    public void Awake()
    {
        // インスタンスが既にあったら削除
        if (this != Instance)
        {
            Destroy(gameObject);
            return;
        }
        DontDestroyOnLoad(gameObject);                  // シーンをまたいでも削除されないようにする
    }

    void Start ()
    {
        SoundMgr.Instance.PlayBgm("MainBGM");                               // MainBGM再生
        Instantiate(stages[0], Vector3.zero, Quaternion.identity);          // Stageを生成
	}
	
	void Update ()
    {
        CheckFPS();
        //if(Input.GetMouseButtonDown(0))
        //{
        //    SceneManager.LoadScene("Title");
        //}
    }

    /// <summary>
    /// FPSを調べる
    /// </summary>
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

    /// <summary>
    /// FPS描画
    /// </summary>
    void OnGUI()
    {
        GUI.color = Color.red;
        GUILayout.Label("FPS: " + m_fps.ToString("f2"));
    }
}
