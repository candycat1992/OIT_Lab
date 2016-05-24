using UnityEngine;
using System.Collections;

public class Rotate : MonoBehaviour {
    public Vector3 lookAt = Vector3.zero;
    public float speed = 10.0f;
    
	// Use this for initialization
	void Start () {
	    
	}
	
	// Update is called once per frame
	void Update () {
        transform.RotateAround(lookAt, transform.up, speed * Time.deltaTime);
	    transform.LookAt(lookAt);
	}
}
