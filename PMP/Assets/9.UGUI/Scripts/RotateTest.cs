using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotateTest : MonoBehaviour {
	public RectTransform t;

	private void Start() {
		var scale = 1 - ( (1 - 822/1295) * t.anchoredPosition.y/1295);
		var v = t.anchoredPosition;
		v.x*=scale;
		t.anchoredPosition = v;
	}
}
