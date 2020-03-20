using System.Collections.Generic;
using UnityEngine;

public interface IRecycleableObj
{
	void OnReuse();
	void OnRecyle();
}


public class Pool<T> where T : IRecycleableObj,new()
{
	private List<T> mCaching = new List<T>();

	public Pool(int iAllocCnt)
	{
		for(int i = 0;i < iAllocCnt;i++)
		{
			mCaching.Add(new T());
		}
	}


	public T Get()
	{
		if(0 >= mCaching.Count)
		{
			T val = new T();
			return val;
		}
		else
		{
			T val = mCaching[0];
			mCaching.RemoveAt(0);
			val.OnReuse();
			return val;
		}
	}

	public void Recyle(T t)
	{
		if(mCaching.Contains(t))
			return;
		mCaching.Add(t);
		t.OnRecyle();
	}

	public void Release()
	{
		while(mCaching.Count > 0)
			mCaching.RemoveAt(0);
	}
}
