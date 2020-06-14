using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class SkillStart : MonoBehaviour {
	SimpleSkill mSimpleSkill;
	private void Awake() {
		Debug.Log("Awake");
	}

	private void OnEnable() {
		Debug.Log("OnEnable");
	}

	private void Reset() {
		Debug.Log("Rest");
	}

	void Start () {
		Debug.Log("Start");
		// mSimpleSkill = new SimpleSkill();
		// mSimpleSkill.Skill11();

		// var s = new Skill13Class();
		// s.Hellow();

		// var s = new SKill14Class();
		// s.DoThread();
	}
	void Update () {
		
	}
}
