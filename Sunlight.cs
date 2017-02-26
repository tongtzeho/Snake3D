using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sunlight : MonoBehaviour {

	public static float light_strength;
	public Light sunlight;

	// Use this for initialization
	void Start () {
		light_strength = 0.5f;
		sunlight = GetComponent<Light> ();
	}
	
	// Update is called once per frame
	void Update () {
		sunlight.intensity = light_strength;
		light_strength = Mathf.Max (0.5f, light_strength - 1.4f * Time.deltaTime);
	}
}
