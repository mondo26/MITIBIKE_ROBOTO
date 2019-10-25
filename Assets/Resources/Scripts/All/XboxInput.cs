using UnityEngine;
using System.Collections.Generic;

/**********************************************************************
 * * XBoxコントローラーの入力クラス 
 *   (FixedUpdate内で入力処理をする際に生成して使用する)
 * *******************************************************************/
public class XboxInput
{
    // 入力モード
    public enum KEYMODE
    {
        NOW,                // 押されている
        DOWN,               // 押された瞬間
        UP,                 // 離した瞬間
    };

    // パッドキーの名前を列挙型に変換
    public enum PAD
    {
        KEY_A,              // PAD_A_BUTTON
        KEY_B,              // PAD_B_BUTTON
        KEY_X,              // PAD_X_BUTTON
        KEY_Y,              // PAD_Y_BUTTON
        KEY_RB,             // PAD_RB_BUTTON
        KEY_LB,             // PAD_LB_BUTTON
        KEY_VIEW,           // PAD_VIEW_BUTTON
        KEY_MENU,           // PAD_MENU_BUTTON
        KEY_MAX,
    };

    // パッドキーの名前
    private List<string> keyName = new List<string>
    {
        "PAD_A_BUTTON",
        "PAD_B_BUTTON",
        "PAD_X_BUTTON",
        "PAD_Y_BUTTON",
        "PAD_RB_BUTTON",
        "PAD_LB_BUTTON",
        "PAD_VIEW_BUTTON",
        "PAD_MENU_BUTTON",
        "PAD_LR_TRIGGER",
    };

    // 押されている間のキーを格納
    private Dictionary<PAD, bool> nowKey = new Dictionary<PAD, bool>();
    // 押した瞬間のキーを格納
    private Dictionary<PAD, bool> downKey = new Dictionary<PAD, bool>();
    // 離した瞬間のキーを格納
    private Dictionary<PAD, bool> upKey = new Dictionary<PAD, bool>();

    /// <summary>
    /// コンストラクタ
    /// </summary>
    public XboxInput()
    {
        AddPadKey();
    }

    /// <summary>
    /// 入力キーを追加する
    /// </summary>
    private void AddPadKey()
    {
        for(int i = 0; i < (int)PAD.KEY_MAX; i++)
        {
            nowKey.Add((PAD)i, false);
            downKey.Add((PAD)i, false);
            upKey.Add((PAD)i, false);
        }
    }

    /// <summary>
    /// 入力を更新する
    /// </summary>
    public void InputUpdate()
    {
        foreach(var name in keyName)
        {
            // 押されている間のキーを格納
            if (Input.GetButton(name))
            {
                nowKey[GetPadKeyName(name)] = true;
            }
            // 押した瞬間のキーを格納
            if(Input.GetButtonDown(name))
            {
                downKey[GetPadKeyName(name)] = true;
            }
            // 離した瞬間のキーを格納
            if (Input.GetButtonUp(name))
            {
                upKey[GetPadKeyName(name)] = true;
            }
        }
    }

    /// <summary>
    /// パッドキーを初期化
    /// </summary>
    public void Initialize()
    {
        for (int i = 0; i < (int)PAD.KEY_MAX; i++)
        {
            nowKey[(PAD)i] = false;
            downKey[(PAD)i] = false;
            upKey[(PAD)i] = false;
        }
    }

    /// <summary>
    /// キー入力チェック
    /// </summary>
    /// <param name="Mode">　入力モード　</param>
    /// <param name="KEYMODE">　入力されたキー　</param>
    public bool Check(KEYMODE _mode, PAD _key)
    {
        switch(_mode)
        {
            case KEYMODE.NOW:               // 押している間
                if (nowKey[_key]) { return true; }
                break;
            case KEYMODE.DOWN:              // 押した瞬間
                if (downKey[_key])
                {
                    downKey[_key] = false;
                    return true;
                }
                break;
            case KEYMODE.UP:                // 離した瞬間
                if(upKey[_key])
                {
                    upKey[_key] = false;
                    return true;
                }
                break;
        }
        return false;
    }

    /// <summary>
    /// PADKeyの名前を列挙型にして返す処理
    /// </summary>
    /// <param name="_name"> ゲームパッドキーの名前 </param>
    /// <returns></returns>
    private PAD GetPadKeyName(string _name)
    {
        switch(_name)
        {
            case "PAD_A_BUTTON":
                return PAD.KEY_A;
            case "PAD_B_BUTTON":
                return PAD.KEY_B;
            case "PAD_X_BUTTON":
                return PAD.KEY_X;
            case "PAD_Y_BUTTON":
                return PAD.KEY_Y;
            case "PAD_RB_BUTTON":
                return PAD.KEY_RB;
            case "PAD_LB_BUTTON":
                return PAD.KEY_LB;
            case "PAD_VIEW_BUTTON":
                return PAD.KEY_VIEW;
            case "PAD_MENU_BUTTON":
                return PAD.KEY_MENU;
        }
        return PAD.KEY_MAX;
    }
}
