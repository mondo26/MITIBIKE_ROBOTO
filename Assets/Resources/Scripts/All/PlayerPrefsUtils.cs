using UnityEngine;

/******************************************************************
 * * どんな型でも簡単に保存と読み込みができるクラス
 * ****************************************************************/
public static class PlayerPrefsUtils
{
    // 指定されたオブジェクトの情報を保存します
    public static void SetObject<T>(string key, T obj)
    {
        // オブジェクトをJSON形式のデータに変換
        var json = JsonUtility.ToJson(obj);
        PlayerPrefs.SetString(key, json);
    }

    // 指定されたオブジェクトの情報を読み込みます
    public static T GetObject<T>(string key)
    {
        var json = PlayerPrefs.GetString(key);
        // JSON形式のデータをオブジェクトに変換
        var obj = JsonUtility.FromJson<T>(json);
        return obj;
    }

    // 指定されたオブジェクトの情報を削除
    public static void Reload<T>(string key)
    {
        // JSON形式のデータをオブジェクトに変換
        PlayerPrefs.DeleteKey(key);
    }
}