using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UpWall : MonoBehaviour {

    public bool up;
    public GameObject wall;

	// Use this for initialization
	void Start () {
        up = true;
	}
	
	// Update is called once per frame
	void Update () {
        if (up == true && wall.transform.position.y <= 4){
            wall.transform.position += new Vector3(0, 0.1f, 0);
        }
        else if(up == false && wall.transform.position.y >= 0){
            wall.transform.position -= new Vector3(0, 0.1f, 0);
        }
	}
}
