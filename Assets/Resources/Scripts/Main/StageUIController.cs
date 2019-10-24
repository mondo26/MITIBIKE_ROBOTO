using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/******************************************************************
 * * ステージのUIコントローラー
 * ****************************************************************/
public class StageUIController : MonoBehaviour
{
    private const int ADJUST_VALUE = 3;
    private const float CHANGE_VALUE = 0.1f;
    private const int HORIZONTAL_INPUT_TIME = 8;

    enum STATE
    {
        FIRST,
        SECOND,
        TRANSITION,
    };

    enum SELECT
    {
        PLAY,
        STAGE,
        SETTING,
        MASTER,
        BGM,
        SE,
        CANCEL,
        CHANGE,

    };

    [SerializeField, Header("音を設定するUI")]
    private GameObject volumeUI = null;
    [SerializeField, Header("スライダー")]
    private GameObject[] sliderObjects = null;
    [SerializeField, Header("ボリュームを表示するテキスト")]
    private Text[] volumeText = null;

    private int value;
    private int horizontalInputTimer;
    private float horizontal, vertical, sliderValue;
    private float preHorizontal, preVertical;
    private STATE state;
    private SELECT modeSelect, preModeSelect;
    private Animator animator;
    private AnimatorStateInfo aniStateInfo;

    private List<Slider> sliders = new List<Slider>();
    private List<float> preSliderValue = new List<float>();
    private SoundMgr soundMgrController;

    private float testValue;

    private void Start()
    {
        this.animator = GetComponent<Animator>();
        this.animator.updateMode = AnimatorUpdateMode.UnscaledTime;                 // タイムスケールを無視する
        this.soundMgrController = GameObject.FindWithTag("SoundMgr").GetComponent<SoundMgr>();

        for (int i = 0; i < sliderObjects.Length; i++)
        {
            this.sliders.Add(sliderObjects[i].GetComponent<Slider>());
            this.preSliderValue.Add(0.0f);
        }

        this.sliders[(int)SELECT.MASTER - ADJUST_VALUE].value = soundMgrController.Volume;
        this.sliders[(int)SELECT.BGM - ADJUST_VALUE].value = soundMgrController.BgmVolume;
        this.sliders[(int)SELECT.SE - ADJUST_VALUE].value = soundMgrController.SeVolume;
        this.preSliderValue[(int)SELECT.MASTER - ADJUST_VALUE] = soundMgrController.Volume;
        this.preSliderValue[(int)SELECT.BGM - ADJUST_VALUE] = soundMgrController.BgmVolume;
        this.preSliderValue[(int)SELECT.SE - ADJUST_VALUE] = soundMgrController.SeVolume;
    }

    void Update ()
    {
        // ロック中ならこれ以降処理を読まない
        if (GameMgr.IsLock) { return; }

        this.horizontal = Input.GetAxisRaw("Horizontal");
        this.vertical = Input.GetAxisRaw("Vertical") * -1;

        switch (state)
        {
            case STATE.FIRST:
                FirstUI();
                break;
            case STATE.SECOND:
                SecondUI();
                break;
        }

        // 値をモード選択状態に代入
        this.modeSelect = (SELECT)value;
        // 1フレーム前の値を格納
        this.preHorizontal = horizontal;
        this.preVertical = vertical;
        // アニメーション状態処理
        AnimationState();

        // Bボタンでモード選択
        if (Input.GetButtonDown("PAD_B_BUTTON") && CheckStartAnimation())
        {
            ModeTransition();
        }
    }

    /// <summary>
    /// 最初に表示されるUIの入力処理
    /// </summary>
    void FirstUI()
    {
        // アニメーションが一定時間再生されていなかったらこれ以降処理を読まない
        if (!CheckStartAnimation()) { return; }

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
        if (preHorizontal == 0)
        {
            if (horizontal > 0)
            {
                this.value = (int)SELECT.SETTING;
            }
            else if (horizontal < 0 && modeSelect == SELECT.SETTING)
            {
                this.value = (int)SELECT.STAGE;
            }
        }

        // 最小値と最大値以内に値を設定する
        this.value = Mathf.Clamp(value, 0, (int)SELECT.SETTING);
    }

    /// <summary>
    /// サウンド設定画面UIの入力処理
    /// </summary>
    void SecondUI()
    {
        // アニメーションが一定時間再生されていなかったらこれ以降処理を読まない
        if (!CheckStartAnimation()) { return; }

        // 上下の移動量格納
        if (preVertical == 0)
        {
            switch (modeSelect)
            {
                case SELECT.CANCEL:
                    if (vertical < 0) { this.value = (int)SELECT.SE; }
                    break;
                case SELECT.CHANGE:
                    if (vertical < 0) { this.value = (int)SELECT.SE; }
                    break;
                default:
                    if (vertical > 0)
                    {
                        this.value++;
                    }
                    else if (vertical < 0)
                    {
                        this.value--;
                    }
                    break;
            }

        }

        // 左右の移動量格納
        if (modeSelect == SELECT.CANCEL || modeSelect == SELECT.CHANGE && preHorizontal == 0)
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
        this.value = Mathf.Clamp(value, (int)SELECT.MASTER, (int)SELECT.CHANGE);

        // ボリュームを変更する処理へ
        if ((SELECT)value != SELECT.CANCEL && (SELECT)value != SELECT.CHANGE)
        {
            ChangeVolume(value - ADJUST_VALUE);
        }
    }

    /// <summary>
    /// アニメーションの状態
    /// </summary>
    void AnimationState()
    {
        if(modeSelect != preModeSelect)
        {
            // アニメーション初期化
            InitializeAnimation();

            switch (modeSelect)
            {
                case SELECT.PLAY:
                    animator.SetBool("SELECT_PLAY", true);
                    break;
                case SELECT.STAGE:
                    animator.SetBool("SELECT_STAGE", true);
                    break;
                case SELECT.SETTING:
                    animator.SetBool("SELECT_SETTING", true);
                    break;
                case SELECT.MASTER:
                    animator.SetBool("SELECT_ALL", true);
                    break;
                case SELECT.BGM:
                    animator.SetBool("SELECT_BGM", true);
                    break;
                case SELECT.SE:
                    animator.SetBool("SELECT_SE", true);
                    break;
                case SELECT.CANCEL:
                    animator.SetBool("SELECT_CANCEL", true);
                    break;
                case SELECT.CHANGE:
                    animator.SetBool("SELECT_CHANGE", true);
                    break;

            }
            this.preModeSelect = modeSelect;
        }
    }

    // アニメーション初期化
    void InitializeAnimation()
    {
        animator.SetBool("SELECT_PLAY", false);
        animator.SetBool("SELECT_STAGE", false);
        animator.SetBool("SELECT_SETTING", false);
        animator.SetBool("SELECT_ALL", false);
        animator.SetBool("SELECT_BGM", false);
        animator.SetBool("SELECT_SE", false);
        animator.SetBool("SELECT_CANCEL", false);
        animator.SetBool("SELECT_CHANGE", false);
    }

    /// <summary>
    /// モードを選択する処理
    /// </summary>
    void ModeTransition()
    {
        switch (modeSelect)
        {
            case SELECT.PLAY:               // プレイ画面に戻る
                SelectPlay();
                break;
            case SELECT.STAGE:              // セレクトシーンに戻る
                SelectStage();
                break;
            case SELECT.SETTING:            // 設定画面標示
                SelectSetting();
                break;
            case SELECT.CANCEL:             // 前の画面に戻る
                SelectCancel();
                break;
            case SELECT.CHANGE:             // 設定を変更し前の画面に戻る
                SelectChange();
                break;
        }
    }

    // プレイ画面に戻る
    void SelectPlay()
    {
        this.state = STATE.FIRST;
        this.modeSelect = SELECT.PLAY;
        Time.timeScale = 1.0f;
        this.gameObject.SetActive(false);
    }

    // セレクトシーンに戻る
    void SelectStage()
    {
        if (!GameMgr.IsLock)
        {
            Time.timeScale = 1.0f;
            SceneMgr.NextScene("Select");
        }
    }

    // 設定画面表示
    void SelectSetting()
    {
        this.volumeUI.SetActive(true);
        this.state = STATE.SECOND;
        this.modeSelect = SELECT.MASTER;
    }

    // 前の画面に戻る処理
    void SelectCancel()
    {
        this.state = STATE.FIRST;
        this.modeSelect = SELECT.PLAY;
        this.volumeUI.SetActive(false);

        // ボリュームを変更しない処理
        for (int i = 0; i < sliderObjects.Length; i++)
        {
            sliders[i].value = preSliderValue[i];
        }
    }

    // 設定を変更し前の画面に戻る処理
    void SelectChange()
    {
        this.state = STATE.FIRST;
        this.modeSelect = SELECT.PLAY;
        this.volumeUI.SetActive(false);

        // ボリュームを変更する処理
        for(int i = 0; i < sliderObjects.Length; i++)
        {
            preSliderValue[i] = sliders[i].value;
        }
    }

    // アニメーションの終了判定
    bool CheckAnimationEnd(string _name)
    {
        aniStateInfo = animator.GetCurrentAnimatorStateInfo(0);

        if(aniStateInfo.fullPathHash == Animator.StringToHash(_name))
        {
            if(aniStateInfo.normalizedTime >= 1.0f)
            {
                return true;
            }
        }
        return false;
    }

    // 一定時間アニメーションが再生されているか？
    bool CheckStartAnimation()
    {
        aniStateInfo = animator.GetCurrentAnimatorStateInfo(0);

        if (aniStateInfo.fullPathHash == Animator.StringToHash(GetLayerName()))
        {
            if (aniStateInfo.normalizedTime >= 0.25f)
            {
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// レイヤーの名前を取得する
    /// </summary>
    string GetLayerName()
    {
        switch (modeSelect)
        {
            case SELECT.PLAY:
                return "Base Layer.SELECT_PLAY";
            case SELECT.STAGE:
                return "Base Layer.SELECT_STAGE";
            case SELECT.SETTING:
                return "Base Layer.SELECT_SETTING";
            case SELECT.MASTER:
                return "Base Layer.SELECT_ALL";
            case SELECT.BGM:
                return "Base Layer.SELECT_BGM";
            case SELECT.SE:
                return "Base Layer.SELECT_SE";
            case SELECT.CANCEL:
                return "Base Layer.SELECT_CANCEL";
            case SELECT.CHANGE:
                return "Base Layer.SELECT_CHANGE";
        }

        return "NONAME";
    }

    /// <summary>
    /// ボリュームを変更する処理
    /// </summary>
    void ChangeVolume(int _v)
    {
        if (horizontalInputTimer > 0) { horizontalInputTimer--; }

        this.sliderValue = sliders[_v].value;

        if (horizontal != 0 && horizontalInputTimer == 0)
        {
            if (horizontal > 0)
            {
                this.sliderValue += 0.1f;
            }
            else if (horizontal < 0)
            {
                this.sliderValue += -0.1f;
            }
            this.horizontalInputTimer = HORIZONTAL_INPUT_TIME;
        }

        this.sliderValue = Mathf.Clamp(sliderValue, 0, 1);
        // 小数点第二位で四捨五入した値を格納
        this.sliders[_v].value = Mathf.Round(sliderValue * 10) / 10;
    }

    // マスタのvalueの値が変更されたら処理が呼ばれる
    public void OnValueChanged_MASTER()
    {
        soundMgrController.Volume = sliders[0].value;
        volumeText[0].text = (sliders[0].value * 10).ToString();
    }

    // BGMのvalueの値が変更されたら処理が呼ばれる
    public void OnValueChanged_BGM()
    {
        soundMgrController.BgmVolume = sliders[1].value;
        volumeText[1].text = (sliders[1].value * 10).ToString();
    }

    // SEのvalueの値が変更されたら処理が呼ばれる
    public void OnValueChanged_SE()
    {
        soundMgrController.SeVolume = sliders[2].value;
        volumeText[2].text = (sliders[2].value * 10).ToString();
    }

}
