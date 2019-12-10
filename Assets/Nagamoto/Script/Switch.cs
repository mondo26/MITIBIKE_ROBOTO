using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Switch : MonoBehaviour {

    private BoxCollider switchbottom;
    public Animator switchdown;
    public UpWall upwall;

	// Use this for initialization
	void Start () {
        switchbottom = GameObject.Find("BottomTrigger").GetComponent<BoxCollider>();
        switchdown.SetBool("down", false);
        upwall.up = false;
	}

    private void OnTriggerEnter(Collider order){
        if(order.tag == "Player"){
            switchdown.SetBool("down", true);
            upwall.up = true;
        }
    }
    private void OnTriggerExit(Collider other){
        if (other.tag == "Player"){
            switchdown.SetBool("down", false);
            upwall.up = false;
        }
    }

    // Update is called once per frame
    void Update () {
        if (Input.GetKeyDown(KeyCode.Space)){
            upwall.up = true;
        }
        if (Input.GetKeyDown(KeyCode.K)){
            upwall.up = false;
        }
	}
}
