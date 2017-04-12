using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UISize : MonoBehaviour {

	private float width_default = 640;
	private float height_default = 960;

	void Awake () {
		RectTransform rt = GetComponent<RectTransform> ();
		UnityEngine.UI.Text text = GetComponent<UnityEngine.UI.Text> ();
		if (rt.sizeDelta.x != rt.sizeDelta.y) {
			rt.localPosition = new Vector3 (rt.localPosition.x * Screen.width / width_default, rt.localPosition.y * Screen.height / height_default, 0);
			rt.sizeDelta = new Vector2 (rt.sizeDelta.x * Screen.width / width_default, rt.sizeDelta.y * Screen.height / height_default);
		} else {
			rt.localPosition = new Vector3 (rt.localPosition.x * Screen.width / width_default, rt.localPosition.y * Screen.height / height_default, 0);
			rt.sizeDelta = new Vector2 (rt.sizeDelta.x * Screen.width / width_default, rt.sizeDelta.y * Screen.height / height_default);
			rt.sizeDelta = new Vector2 (Mathf.Max (rt.sizeDelta.x, rt.sizeDelta.y), Mathf.Max (rt.sizeDelta.x, rt.sizeDelta.y));
		}
		if (text != null) {
			text.fontSize = (int)(text.fontSize * Mathf.Min (Screen.width / width_default, Screen.height / height_default));
		}
	}
}
