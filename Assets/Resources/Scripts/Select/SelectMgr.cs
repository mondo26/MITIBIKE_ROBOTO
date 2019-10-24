using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/******************************************************************
 * * セレクト画面を管理するクラス
 * ****************************************************************/
public class SelectMgr : MonoBehaviour
{

    void Start ()
    {
		
	}
	
	void Update ()
    {
        if (Input.GetButtonDown("PAD_B_BUTTON") && !GameMgr.IsLock)
        {
            SceneMgr.NextScene("Main", STAGE._01);
        }
    }

    void FixedUpdate()
    {
    }
}
