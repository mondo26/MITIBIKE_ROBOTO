using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/******************************************************************
 * * ボリュームをコントロールするクラス
 * ****************************************************************/
[DisallowMultipleComponent]
public class VolumeController : MonoBehaviour
{
    private const int MAX_VALUE = 4;
    private const int VOLUMETYPE_MASTER = 0;
    private const int VOLUMETYPE_BGM = 1;
    private const int VOLUMETYPE_SE = 2;
    private const float CHANGE_VALUE = 0.1f;

    enum SELECT
    {
        MASTER,
        BGM,
        SE,
        CANCEL,
        CHANGE,
    };

    private float horizontal, vertical, value, sliderValue;
    private float preHorizontal, preVertical;
    private SELECT modeSelect, preModeSelect;

    [SerializeField, Header("スライダー")]
    private GameObject[] sliderObjects;
    private List<Slider> sliders = new List<Slider>();
    private SoundMgr soundMgrController;

    private void Start()
    {
        this.soundMgrController = GameObject.FindWithTag("SoundMgr").GetComponent<SoundMgr>();

        for (int i = 0; i < sliderObjects.Length; i++)
        {
            this.sliders.Add(sliderObjects[i].GetComponent<Slider>());
        }

        sliders[VOLUMETYPE_MASTER].value = soundMgrController.Volume;
        sliders[VOLUMETYPE_BGM].value = soundMgrController.BgmVolume;
        sliders[VOLUMETYPE_SE].value = soundMgrController.SeVolume;
    }

    private void Update()
    {
        this.horizontal = Input.GetAxisRaw("Horizontal");
        this.vertical = Input.GetAxisRaw("Vertical") * -1;

        // 上下の移動量格納
        if (preVertical == 0)
        {
            if (vertical > 0)
            {
                this.value++;
            }
            else if (vertical < 0)
            {
                this.value--;
            }
        }

        // 左右の移動量格納
        if (preHorizontal == 0 && modeSelect == SELECT.CANCEL || modeSelect == SELECT.CHANGE)
        {
            if (horizontal > 0)
            {
                this.value = (int)SELECT.CHANGE;
            }
            else if (horizontal < 0)
            {
                this.value = (int)SELECT.CANCEL;
            }
        }

        // 最小値と最大値以内に値を設定する
        this.value = Mathf.Clamp(value, 0, (int)SELECT.CHANGE);
        // 値をモード選択状態に代入
        this.modeSelect = (SELECT)value;

        // ボリュームを変更する処理へ
        if(modeSelect != SELECT.CANCEL && modeSelect != SELECT.CHANGE)
        {
            ChangeVolume();
        }

        // 1フレーム前の値を格納
        this.preHorizontal = horizontal;
        this.preVertical = vertical;

    }

    /// <summary>
    /// ボリュームを変更する処理
    /// </summary>
    void ChangeVolume()
    {
        this.sliderValue = sliders[(int)modeSelect].value;

        if(horizontal != 0 && preHorizontal == 0)
        {
            if(horizontal > 0)
            {
                this.sliderValue += CHANGE_VALUE;
            }
            else if(horizontal < 0)
            {
                this.sliderValue += -CHANGE_VALUE;
            }
        }

        this.sliderValue = Mathf.Clamp(sliderValue, 0, 1.0f);
        this.sliders[(int)modeSelect].value = sliderValue;
        Debug.Log(sliders[(int)modeSelect].value);
    }

    // マスタのvalueの値が変更されたら処理が呼ばれる
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
