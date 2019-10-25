using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/******************************************************************
 * * タイトルを管理するクラス
 * ****************************************************************/
public class TitleMgr : MonoBehaviour
{
	void Start ()
    {
        		
	}

    void Update()
    {
        if(Input.GetButtonDown("PAD_B_BUTTON") && !GameMgr.IsLock)
        {
            SceneMgr.NextScene("Select");
        }
    }
}
