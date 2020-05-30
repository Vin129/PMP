using System.Collections.Generic;
namespace EasyFramework
{
	public interface IReusable
	{
		void Reuse();
		void Recycle();
	}

    public class EasyPool<T> where T : IReusable,new()
	{
		private Queue<T> mCache;
		public EasyPool()
		{
			mCache = new Queue<T>();
		}
		public T Get()
		{
			if(mCache.Count <= 0)
				return new T();
			var v = mCache.Dequeue();
			v.Reuse();
			return v;
		}

		public void Recycle(T t)
		{
			t.Recycle();
			mCache.Enqueue(t);
		}

		public void Release()
		{
			mCache.Clear();
		}
	}
}
