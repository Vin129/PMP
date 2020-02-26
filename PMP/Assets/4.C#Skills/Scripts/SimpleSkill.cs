using System.Collections;
using System.Collections.Generic;

public class SimpleSkill{
	
	public SimpleSkill(){}
		
#region SKILL 1
	public void Skill1()
	{
		// int i = null; 
		int? i = null;
	}
#endregion


#region SKILL 2
	interface SKILL_2_Interface
	{
		void Change(int a,int b);
		void Print(); 
	}
	struct SKILL_2_Struct:SKILL_2_Interface
	{
		public int a;
		public int b;
		public SKILL_2_Struct(int a,int b)
		{
			this.a = a;
            this.b = b;
		}
        public void Change(int a, int b)
        {
            this.a = a;
            this.b = b;
        }
        public void Print(){
			UnityEngine.Debug.Log(a);
		}
	}
	public void Skill2()
	{
		SKILL_2_Struct s1 = new SKILL_2_Struct(1,1);
		object s2 = s1; //boxing

		((SKILL_2_Struct)s2).Change(2,2);
		((SKILL_2_Struct)s2).Print(); // 1

		((SKILL_2_Interface)s1).Change(2,2);
		s1.Print(); // 1
		((SKILL_2_Interface)s1).Print(); // 1

		((SKILL_2_Interface)s2).Change(2,2);
		((SKILL_2_Struct)s2).Print(); // 2
		((SKILL_2_Interface)s2).Print();//2
	}
	#endregion
}
