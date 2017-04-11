using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EatObject : MonoBehaviour {

	private Transform thistrans;
	private float timecnt;
	private int updatecnt;

	void Awake () {
		thistrans = transform;
	}

	// Use this for initialization
	void Start () {
		timecnt = 0;
		updatecnt = 0;
	}
	
	// Update is called once per frame
	void Update () {
		updatecnt++;
		timecnt += Time.deltaTime;
		if (updatecnt >= 3) {
			thistrans.RotateAround (thistrans.position, new Vector3 (0f, 1f, 0f), 220f * timecnt);
			timecnt = 0;
			updatecnt = 0;
		}
	}
}
