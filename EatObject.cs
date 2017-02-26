using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EatObject : MonoBehaviour {

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		transform.RotateAround (transform.position, new Vector3 (0f, 1f, 0f), 220f * Time.deltaTime);
	}
}
