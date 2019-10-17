using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// ボリュームを管理するクラス
/// </summary>
[DisallowMultipleComponent]
public class VolumeController : MonoBehaviour
{
    private const int VOLUMETYPE_MASTER = 0;
    private const int VOLUMETYPE_BGM = 1;
    private const int VOLUMETYPE_SE = 2;

    [SerializeField, Header("サウンドマネージャー")]
    private GameObject soundMgr;
    [SerializeField, Header("スライダー")]
    private GameObject[] sliderObjects;

    private List<Slider> sliders = new List<Slider>();
    private SoundMgr soundMgrController;

    private void Start()
    {
        for (int i = 0; i < sliderObjects.Length; i++)
        {
            this.sliders.Add(sliderObjects[i].GetComponent<Slider>());
        }

        soundMgrController = soundMgr.GetComponent<SoundMgr>();

        sliders[VOLUMETYPE_MASTER].value = soundMgrController.Volume;
        sliders[VOLUMETYPE_BGM].value = soundMgrController.BgmVolume;
        sliders[VOLUMETYPE_SE].value = soundMgrController.SeVolume;
    }

    // マスクのvalueの値が変更されたら処理が呼ばれる
    public void OnValueChanged_MASTER()
    {
        soundMgrController.Volume = sliders[VOLUMETYPE_MASTER].value;  
    }

    // BGMのvalueの値が変更されたら処理が呼ばれる
    public void OnValueChanged_BGM()
    {
        soundMgrController.BgmVolume = sliders[VOLUMETYPE_BGM].value;
    }

    // SEのvalueの値が変更されたら処理が呼ばれる
    public void OnValueChanged_SE()
    {
        soundMgrController.SeVolume = sliders[VOLUMETYPE_SE].value;
    }
}
