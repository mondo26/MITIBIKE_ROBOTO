using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

/******************************************************************
 * * シーンを管理するクラス　(SingletonMonoBehaviourから継承)
 * ****************************************************************/
[DisallowMultipleComponent]
public class SceneMgr : SingletonMonoBehaviour<SceneMgr>
{
    [SerializeField, Header("フェードに使用するマテリアル")]
    private Material fadeMaterial;
    [SerializeField, Header("GameMgr")]
    private GameMgr gameMgr;

    /// <summary>
    ///  Start 関数の前およびプレハブのインスタンス化直後に呼び出される
    /// </summary>
    public void Awake()
    {
        // インスタンスが既にあったら削除
        if (this != Instance)
        {
            Destroy(gameObject);
            return;
        }
        DontDestroyOnLoad(gameObject);                  // シーンをまたいでも削除されないようにする
        fadeMaterial.SetFloat("_Radius", 1.1f);
    }

    /// <summary>
    /// 次に行くシーンを設定
    /// </summary>
    /// <param name="_name">シーン名</param>
    public static void NextScene(string _name)
    {
        GameMgr.IsLock = true;
        Instance.StartCoroutine(Instance.SceneTransition(_name));
    }


    public static void NextScene(string _name, STAGE _stage)
    {
        GameMgr.IsLock = true;
        Instance.StartCoroutine(Instance.SceneTransition(_name, _stage));
    }

    /// <summary>
    /// シーン遷移
    /// </summary>
    /// <param name="_sceneName">シーン名</param>
    public IEnumerator SceneTransition(string _sceneName)
    {
        // フェードイン
        for (float i = 1.1f; i > 0.0f; i -= 0.02f)
        {
            fadeMaterial.SetFloat("_Radius", i);
            yield return null;
        }

        SceneManager.LoadScene(_sceneName);                     // シーン遷移
        yield return new WaitForSeconds(1.0f);                  // 1秒間待つ
        // フェードアウト
        for (float i = 0.2f; i < 1.1f; i += 0.06f)
        {
            fadeMaterial.SetFloat("_Radius", i);
            yield return null;
        }

        GameMgr.IsLock = false;
        //SoundMgr.Instance.PlayBgm(_sceneName + "BGM");          // BGMを再生
    }

    /// <summary>
    /// メインシーン遷移の際に使用する
    /// </summary>
    /// <param name="_sceneName">シーン名</param>
    /// <param name="_stage">ステージ名</param>
    /// <returns></returns>
    public IEnumerator SceneTransition(string _sceneName, STAGE _stage)
    {
        // フェードイン
        for (float i = 1.1f; i > 0.0f; i -= 0.02f)
        {
            fadeMaterial.SetFloat("_Radius", i);
            yield return null;
        }

        SceneManager.LoadScene(_sceneName);                     // シーン遷移
        yield return new WaitForSeconds(1.0f);                  // 1秒間待つ
        gameMgr.CreateStage(_stage);                            // ステージを生成

        // ワイプを表示する
        for (float i = 0.0f; i < 0.21f; i += 0.04f)
        {
            fadeMaterial.SetFloat("_Radius", i);
            yield return null;
        }

        yield return new WaitForSeconds(1.0f);                  // 1秒間待つ

        // フェードアウト
        for (float i = 0.2f; i < 1.1f; i += 0.06f)
        {
            fadeMaterial.SetFloat("_Radius", i);
            yield return null;
        }

        GameMgr.IsLock = false;
    }
}