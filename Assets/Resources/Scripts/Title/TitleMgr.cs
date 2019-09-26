using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// タイトルに関するスクリプト
/// </summary>
public class TitleMgr : MonoBehaviour
{

	void Start ()
    {
		
	}

    void Update()
    {
        // Bボタンか左クリックで次のシーンへ
        if (Input.GetButtonDown("PAD_B_BUTTON") || Input.GetMouseButtonDown(0))
        {
            SceneManager.LoadScene("Main");
        }
    }
}
