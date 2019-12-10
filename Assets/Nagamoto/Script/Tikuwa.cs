using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tikuwa : MonoBehaviour {
    private GameObject tikuwa;
    private float time = 0;
    public Animator anitikuwa;
	// Use this for initialization
	void Start () {
        tikuwa = transform.parent.gameObject;
	}

    private void OnTriggerStay(Collider other){
        if(other.tag == "Player"){
            time += Time.deltaTime;
            Debug.Log(time);
            if(time >= 2){
                Destroy(tikuwa);
            }
        }
    }

    // Update is called once per frame
    void Update () {
		
	}
}
