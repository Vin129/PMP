﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkillStart : MonoBehaviour {
	SimpleSkill mSimpleSkill;
	void Start () {
		mSimpleSkill = new SimpleSkill();
		mSimpleSkill.Skill11();
	}
	void Update () {
		
	}
}
