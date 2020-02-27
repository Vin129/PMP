﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class BasicScript : MonoBehaviour {
	private  int[] mSortArray = {6,2,1,3,8,7,5,4,9,10,5};
	
	void Start () {
		mSortArray.SortByMerge().Log();
	}



	
}

public static class BaseUtil
{
#region  Util
	public static void Log(this int[] t){
		StringBuilder sb = new StringBuilder();
		sb.Append("{");
		for(int i = 0;i <t.Length;i++)
		{
			if(i == 0)
				sb.AppendFormat("{0}",t[i]);
			else
				sb.AppendFormat(",{0}",t[i]);
		}
		sb.Append("}");
		Debug.Log(sb.ToString());
	}
	public static int[] SplitAry(this int[] array,int start,int end)
	{
		if(start >= array.Length || end >= array.Length || start < 0 || end < 0 || end < start)
			return array;
		int[] newArray = new int[end - start + 1];
		for(int i = start;i<=end;i++)
		{
			newArray[i - start] = array[i];
		}
		return newArray;
	}
#endregion


#region InsertionSort
	public static int[] InsertionSort(this int[] A)
	{
		for(int i = 0;i<A.Length - 1;i++)
		{
			int c = i;
			int key = A[i+1];
			while(c>=0 &&(key < A[c]))
			{
				A[c+1] = A[c];
				c--;
			}	
			A[c+1] = key;
		}
		return A;
	}

#endregion


#region MergeSort
	public static int[] SortByMerge(this int[] A)
	{
		return MergeSort(A);
	}

	private static int[] MergeSort(int[] A)
	{
		if(A.Length <= 1)
			return A;
		int c = Mathf.CeilToInt(A.Length/2) - 1;
		return Merge(MergeSort(A.SplitAry(0,c)),MergeSort(A.SplitAry(c+1,A.Length - 1)));
	}

	private static int[] Merge(int[] A,int[] B)
	{
		int[] S = new int[A.Length + B.Length];
		for(int index = 0,i = 0,j = 0;index < S.Length;index ++)
		{
			if(i>= A.Length)
				S[index] = B[j++];
			else if(j >= B.Length)
				S[index] = A[i++];
			else if (A[i] < B[j])
				S[index] = A[i++];
			else
				S[index] = B[j++];
		}
		return S;
	}
#endregion
}
