﻿// #define NET_40
#define NET_35
using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using System.Reflection;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.ComponentModel;
using System.Threading;
using PMP.Extension;
#if NET_40
using System.Threading.Tasks;
#endif

[assembly:AssemblyVersion("1.0.0")]
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


#region SKILL 11
	public void Skill11(){
		List<int> a = new List<int>();
		a.Add(3);
		var q = a.Where(x=>(x%2) == 0);
		q = q.Select(x=>x*10);
		
		a.Add(2);
		a.Add(4);
		UnityEngine.Debug.Log(a[1]);
		UnityEngine.Debug.Log("---foreach a----");
		foreach(int v in a)
		{
			UnityEngine.Debug.Log(v);
		}
		UnityEngine.Debug.Log("---foreach q----");
		foreach(int v in q)
		{
			UnityEngine.Debug.Log(v);
		}
		UnityEngine.Debug.Log("---foreach q again----");
		foreach(int v in q)
		{
			UnityEngine.Debug.Log(v);
		}

	}
#endregion
}

#region SKILL 9
	public class SimpleEventClass
	{
		public int A;
		public class TempEventArgs:System.EventArgs{}

		public event Action<int> TempAction = delegate{};
		public event EventHandler<TempEventArgs> TempEvent = delegate{};


		public void Add(Action<int> action,EventHandler<TempEventArgs> e)
		{
			// TempAction += (int a)=>{};
			TempAction += action;
			// TempEvent += (object sender,TempEventArgs a) => {};
			TempEvent += e;
		}
		public void Invoke(){
			TempAction(1);
			TempEvent(this,new TempEventArgs());
		}
	}

	public class TestClass{
		public TestClass(){
			SimpleEventClass eClass = new SimpleEventClass();
			eClass.TempAction += (int a)=>{};
			eClass.TempAction -= (int a)=>{};
			// eClass.TempAction(1);
			// eClass.TempAction = (int a) =>{};
		}
	}
#endregion

#region  SKILL 10
[QuickExecute(true)]
public class YieldTestClass
{
	public class LikeIEnumerable
	{
		public IEnumerator GetEnumerator()
		{
			yield return 1;
			yield return 2;
			yield return 3;
		}
	}

    public class YieldClass : IEnumerator<object>, IEnumerator,IEnumerable<object>,IEnumerable,IDisposable
    {
        public object Current
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public IEnumerator GetEnumerator()
        {
            throw new NotImplementedException();
        }

        public bool MoveNext()
        {
            throw new NotImplementedException();
        }

        public void Reset()
        {
            throw new NotImplementedException();
        }

        IEnumerator<object> IEnumerable<object>.GetEnumerator()
        {
            throw new NotImplementedException();
        }
    }
    public class LikeYield
	{
		int state = 0;
		private object mCurrent;
		public object Current
		{
			get
			{
				return mCurrent;
			}
		}
		public bool MoveNext()
		{
			switch (state)
			{
				case 0:
				state++;
				mCurrent = 1;
				return true;
				case 1:
				state++;
				mCurrent = 2;
				return true;
				case 2:
				state++;
				mCurrent = 3;
				return true;
				default:
				return false;
			}
		}
	}
	[ExecuteMethod]
	public void Test()
	{
		// LikeIEnumerable likeEnumerable = new LikeIEnumerable();
		// foreach (var item in likeEnumerable)
		// {
		// 	Log.I(item);
		// }
		LikeYield likeYield = new LikeYield();
		while(likeYield.MoveNext())
		{
			var item = likeYield.Current;
			Log.I(item);
		}
	}
}

#endregion

#region  SKILL 13
[SimpleCalledClass]
public class Skill13Class{
	[return:Description("Call Hellow")]
	public void Hellow(){
		UnityEngine.Debug.Log("Hello world!");
		// Serialize();
		// Deserialize();
	}

	public void Serialize(){
		using(Stream stream = File.Open("Test.bin",FileMode.Create)){
			BinaryFormatter formatter = new BinaryFormatter();
			formatter.Serialize(stream,new Skill13Data("Hello world"));
		}
	}

	public void Deserialize(){
		using(Stream stream = File.Open("Test.bin",FileMode.Open)){
			BinaryFormatter formatter = new BinaryFormatter();
			var data = (Skill13Data)formatter.Deserialize(stream);
			UnityEngine.Debug.Log(data.content);
			UnityEngine.Debug.Log(data.name);
		}
	}
}

[Serializable]
public class Skill13Data
{
	public string content;
	[NonSerialized]
	public string name;
	public Skill13Data(string content){
		name = "Skill13Dara";
		this.content = content;
	}
}

#endregion

#region SKILL 14
	[QuickExecute(true)]
	public class SKill14Class
	{
		[ExecuteMethod]
		public void DoThread(){
#if NET_35
			ThreadPool.QueueUserWorkItem(ThreadPoolMethod);
			// ThreadStart ts =  SubThread;
			// Thread subT = new Thread(ts);
			// subT.Start();
			// while(subT.IsAlive)
			// {
			// 	UnityEngine.Debug.Log("A");
			// }
			for(int i = 0;i<10000;i++)
			{
				UnityEngine.Debug.Log("-");
			}
			
			// subT.Join();
#elif NET_40
			// Task t = new Task(()=>{
			// 	for(int i = 0;i<10000;i++)
			// 	{
			// 		UnityEngine.Debug.Log("-");
			// 	}
			// });
			// Task<string> task = Task.Run(()=>{
			// 	for(int i = 0;i<10000;i++)
			// 	{
			// 		UnityEngine.Debug.Log("+");
			// 	}
			// 	return "over";
			// });
			// t.Start();
			// task.Wait();
			// Task task = Task
			// .Run(()=>{Console.WriteLine("Hellow");})
			// .ContinueWith((t)=>{Console.WriteLine("Wrold");});
			CancellationTokenSource tokenSource = new CancellationTokenSource();
			Task task = Task.Run(()=>{},tokenSource.Token);
			tokenSource.Cancel();
#endif
		}
		public void ThreadPoolMethod(object state)
		{
			for(int i = 0;i<10000;i++)
			{
				UnityEngine.Debug.Log("+");
			}
		}
		public void SubThread(){
			for(int i = 0;i<10000;i++)
			{
				UnityEngine.Debug.Log("+");
			}
		}
	}
#endregion