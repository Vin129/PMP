using System.Collections;
using System.Collections.Generic;

public class SimpleSkill{
	
	public SimpleSkill(){}
	
	public void Skill1()
	{
		// int i = null; 
		int? i = null;
	}

	struct Skill2Struct
	{
		public int a;
		public int b;

		public void Print(){

		}
	}

	public void Skill2(){
		var st = new Skill2Struct();
		st.a = 1;
		st.b = 2;
	}
}
