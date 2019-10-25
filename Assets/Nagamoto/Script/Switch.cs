using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Switch : MonoBehaviour {

    private BoxCollider switchbottom;
    public Animator switchdown;

	// Use this for initialization
	void Start () {
        switchbottom = GameObject.Find("BottomTrigger").GetComponent<BoxCollider>();
        switchdown.SetBool("down", false);

	}

    private void OnTriggerEnter(Collider order){
        if(order.name == "Player"){
            switchdown.SetBool("down", true);
        }
    }

    // Update is called once per frame
    void Update () {
		
	}
}
