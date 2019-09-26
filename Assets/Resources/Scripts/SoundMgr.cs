using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// シングルトンを継承したサウンドマネージャー
/// </summary>
[DisallowMultipleComponent]
public class SoundMgr : SingletonMonoBehaviour<SoundMgr>
{
    [SerializeField, Range(0, 1), Tooltip("マスタ音量")]
    private float volume = 1.0f;
    [SerializeField, Range(0, 1), Tooltip("BGMの音量")]
    private float bgmVolume = 1.0f;
    [SerializeField, Range(0, 1), Tooltip("SEの音量")]
    private float seVolume = 1.0f;

    private AudioClip[] bgm, se;
    private Dictionary<string, int> seIndex = new Dictionary<string, int>();
    private Dictionary<string, int> bgmIndex = new Dictionary<string, int>();
    private AudioSource bgmAudioSource, seAudioSource;

    public void Awake()
    {
        // インスタンスが既にあったら削除
        if (this != Instance)
        {
            Destroy(gameObject);
            return;
        }
        DontDestroyOnLoad(gameObject);                                      // シーンをまたいでも削除されないようにする

        bgmAudioSource = gameObject.AddComponent<AudioSource>();
        seAudioSource = gameObject.AddComponent<AudioSource>();
        bgm = Resources.LoadAll<AudioClip>("Sounds/BGM");                   // Sounds/BGM内のsound全て取得
        se = Resources.LoadAll<AudioClip>("Sounds/SE");                     // Sounds/SE内のsound全て取得

        for (int i = 0; i < bgm.Length; i++)
        {
            bgmIndex.Add(bgm[i].name, i);
        }
        for (int i = 0; i < se.Length; i++)
        {
            seIndex.Add(se[i].name, i);
        }
    }

    // Volume設定
    public float Volume
    {
        set
        {
            volume = Mathf.Clamp01(value);
            bgmAudioSource.volume = bgmVolume * volume;
            seAudioSource.volume = seVolume * volume;
        }
        get
        {
            return volume;
        }
    }

    //=================================================================================
    // BGM
    //=================================================================================

    // BgmVolume設定
    public float BgmVolume
    {
        set
        {
            bgmVolume = Mathf.Clamp01(value);
            bgmAudioSource.volume = bgmVolume * volume;
        }
        get
        {
            return bgmVolume;
        }
    }

    // 指定された名前のBGMが存在するか調べる
    public int GetBgmIndex(string name)
    {
        // 指定したキーが存在するか？
        if (bgmIndex.ContainsKey(name))
        {
            return bgmIndex[name];
        }
        else
        {
            Debug.LogError("指定された名前のBGMファイルが存在しません。");
            return 0;
        }
    }

    // BGM再生
    public void PlayBgm(string name)
    {
        int index = GetBgmIndex(name);
        index = Mathf.Clamp(index, 0, bgm.Length);
        bgmAudioSource.clip = bgm[index];
        bgmAudioSource.loop = true;
        bgmAudioSource.volume = BgmVolume * Volume;
        bgmAudioSource.Play();
    }

    // BGM停止
    public void StopBgm()
    {
        bgmAudioSource.Stop();
        bgmAudioSource.clip = null;
    }

    //=================================================================================
    // SE
    //=================================================================================

    // SeVolume設定
    public float SeVolume
    {
        set
        {
            seVolume = Mathf.Clamp01(value);
            seAudioSource.volume = seVolume * volume;
        }
        get
        {
            return seVolume;
        }
    }

    // 指定された名前のSEが存在するか調べる
    public int GetSeIndex(string name)
    {
        // 指定したキーが存在するか？
        if (seIndex.ContainsKey(name))
        {
            return seIndex[name];
        }
        else
        {
            Debug.LogError("指定された名前のSEファイルが存在しません。");
            return 0;
        }
    }

    // SE再生
    public void PlaySe(string name)
    {
        int index = GetSeIndex(name);
        index = Mathf.Clamp(index, 0, se.Length);
        seAudioSource.PlayOneShot(se[index], SeVolume * Volume);
    }

    // SE停止
    public void StopSe()
    {
        seAudioSource.Stop();
        seAudioSource.clip = null;
    }

}
