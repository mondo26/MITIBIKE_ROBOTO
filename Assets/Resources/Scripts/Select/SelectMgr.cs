using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/******************************************************************
 * * セレクト画面を管理するクラス
 * ****************************************************************/
public class SelectMgr : MonoBehaviour
{

    // アニメーションの名前
    private List<string> m_animationName = new List<string>
    {
        "Base Layer.STAGE_01",
        "Base Layer.STAGE_02",
        "Base Layer.STAGE_03",
    };


    [SerializeField, Header("NewImage")]
    private GameObject m_newImage;
    [SerializeField, Header("B_Button_UI")]
    private GameObject m_B_Button_UI;
    [SerializeField, Header("Clear_UI")]
    private GameObject m_Clear_UI;
    [SerializeField, Header("Stages")]
    private GameObject m_stages;
    [SerializeField, Header("Arrow")]
    private GameObject[] m_arrow;
    [SerializeField, Header("CountText")]
    private Text m_countText;
    
    private float m_horizontal, m_preHorizontal;
    private int maxRight;
    private bool isClearProduction;
    private Animator m_animator;
    private AnimatorStateInfo m_aniStateInfo;

    void Start ()
    {
        m_animator = m_stages.GetComponent<Animator>();
        AnimationState();
        // 初めてステージクリアしていたら
        if(GameMgr.Instance.Data.isClear[(int)GameMgr.Stage] && !GameMgr.Instance.Data.isPreClear[(int)GameMgr.Stage])
        {
            // クリア演出表示
            StartCoroutine("ClearProduction");
        }
        ChangeText();
    }
	
	void Update ()
    {
        if (GameMgr.IsLock || isClearProduction) { return; }

        if (CheckAnimationEnd(ref m_animator, m_animationName[(int)GameMgr.Stage]))
        {
            // Bボタンで選択　→　メインシーンへ
            if (Input.GetButtonDown("PAD_B_BUTTON"))
            {
                // 初めてプレイするステージなら
                if (!GameMgr.Instance.Data.isStart[(int)GameMgr.Stage])
                {
                    GameMgr.Instance.Data.isStart[(int)GameMgr.Stage] = true;
                }

                SceneMgr.NextScene("Main", GameMgr.Stage);
                return;                
            }

            // Aボタン選択　→　タイトルシーンへ
            if (Input.GetButtonDown("PAD_A_BUTTON"))
            {
                SceneMgr.NextScene("Title");
            }

            CheckSelectStageUI();
            MoveUI();
        }
    } 

    // UIの動き処理
    private void MoveUI()
    {
        m_horizontal = Input.GetAxis("Horizontal");

        if (m_preHorizontal == 0)
        {
            // 右移動
            if (m_horizontal > 0 && GameMgr.Stage < STAGE._MAX - 1 && GameMgr.Instance.Data.isClear[(int)GameMgr.Stage])
            {
                InitializeUI();
                GameMgr.Stage++;
            }
            // 左移動
            else if (m_horizontal < 0 && GameMgr.Stage > 0)
            {
                InitializeUI();
                GameMgr.Stage--;
            }
        }

        AnimationState();
        m_preHorizontal = m_horizontal;
    }

    // UI初期化
    private void InitializeUI()
    {
        m_newImage.SetActive(false);
        m_B_Button_UI.SetActive(false);
        m_Clear_UI.SetActive(false);
        //foreach(var arrow in m_arrow)
        //{
        //    arrow.SetActive(false);
        //}
    }

    /// <summary>
    /// どのステージが選択されているか調べる
    /// </summary>
    private void CheckSelectStageUI()
    {
        // すでにクリア済みか？
        if (GameMgr.Instance.Data.isClear[(int)GameMgr.Stage])
        {
            m_Clear_UI.SetActive(true);
        }

        // 初めてプレイするステージか？
        if (!GameMgr.Instance.Data.isStart[(int)GameMgr.Stage])
        {
            m_newImage.SetActive(true);
        }

        // 矢印アイコンの表示
        switch (GameMgr.Stage)
        {
            case STAGE._01:
                m_arrow[0].SetActive(true);
                m_arrow[1].SetActive(false);
                break;
            case STAGE._MAX - 1:
                m_arrow[0].SetActive(false);
                m_arrow[1].SetActive(true);
                break;
            default:
                m_arrow[0].SetActive(true);
                m_arrow[1].SetActive(true);
                break;
                
        }

        if(!GameMgr.Instance.Data.isClear[(int)GameMgr.Stage])
        {
            m_arrow[0].SetActive(false);
        }

        m_B_Button_UI.SetActive(true);
    }

    /// <summary>
    /// クリア演出
    /// </summary>
    IEnumerator ClearProduction()
    {
        isClearProduction = true;
        m_Clear_UI.SetActive(true);
        yield return new WaitForSeconds(1.0f);
        Animator c_animator = m_Clear_UI.GetComponent<Animator>();
        c_animator.SetBool("MOVE", true);

        while(!CheckAnimationEnd(ref c_animator, "Base Layer.MOVE"))
        {
            yield return null;
        }

        GameMgr.Instance.Data.isPreClear[(int)GameMgr.Stage] = true;
        isClearProduction = false;
    }

    /// <summary>
    /// アニメーションの処理
    /// </summary>
    void AnimationState()
    {
        // アニメーション初期化
        InitializeAnimation();

        switch (GameMgr.Stage)
        {
            case STAGE._01:
                m_animator.SetBool("STAGE_01", true);
                break;
            case STAGE._02:
                m_animator.SetBool("STAGE_02", true);
                break;
            case STAGE._03:
                m_animator.SetBool("STAGE_03", true);
                break;
        }

    }

    // テキストの値を変える処理
    void ChangeText()
    {
        int clearCount = 0;
        foreach (var isClear in GameMgr.Instance.Data.isClear)
        {
            if (isClear) { clearCount++; }
        }
        m_countText.text = clearCount.ToString();
    }

    // アニメーションの初期化
    void InitializeAnimation()
    {
        m_animator.SetBool("STAGE_01", false);
        m_animator.SetBool("STAGE_02", false);
        m_animator.SetBool("STAGE_03", false);
    }

    // アニメーションの終了判定
    bool CheckAnimationEnd(ref Animator animator, string name)
    {
        m_aniStateInfo = animator.GetCurrentAnimatorStateInfo(0);

        if (m_aniStateInfo.fullPathHash == Animator.StringToHash(name))
        {
            if (m_aniStateInfo.normalizedTime >= 1.0f)
            {
                return true;
            }
        }
        return false;
    }
}
