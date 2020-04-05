using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using PMP.Extension;

[QuickExecute(true)]
public class LeetCodeSolution
{
	[ExecuteMethod]
	public void Execute()
	{
		CountBinarySubstrings("01");
	}


	#region  1380
	///<summary>
	///1380
	///幸运数是指矩阵中满足同时下列两个条件的元素：在同一行的所有元素中最小,在同一列的所有元素中最大
	///</summary>
	 public IList<int> LuckyNumbers1380 (int[][] matrix) {
		 //[[1,10,4,2],[9,3,8,7],[15,16,17,12]]
        IList<int> ln = new List<int>();
		for(int m = 0;m<matrix.Length;m++)
		{
			int index;
			var A = SolutionExtension.GetBestArrayValue(matrix[m],false,out index);
			int[] col = new int[matrix.Length];
			for(int n = 0;n<matrix.Length;n++)
			{
				col[n] = matrix[n][index];
			}
			var B = SolutionExtension.GetBestArrayValue(col,true,out index);
			if(A == B && A > 0)
				ln.Add(A);
		}
        return ln;
    }
	#endregion

	#region 696. 计数二进制子串

	///<summary>
	///696
	///给定一个字符串 s，计算具有相同数量0和1的非空(连续)子字符串的数量，并且这些子字符串中的所有0和所有1都是组合在一起的。 重复出现的子串要计算它们出现的次数。
	///</summary>
	public int CountBinarySubstrings(string s) 
	{
		var num = 0;
		int lastNum = 0;
		int curNum = 1;
		if(s.Length < 2)
			return 0;
		for(int i = 1;i<s.Length;i++)
		{
			if(s[i] == s[i-1]) 
				curNum ++;
			else
			{
				lastNum = curNum;
				curNum = 1;
			}
			if(lastNum >= curNum)
				num++;
		}
        return num;
    }
	#endregion
}

public static class SolutionExtension
{
    public static int GetBestArrayValue(int[] target, bool isMax,out int index)
    {
        index = 0;
        if(isMax)
        {
            for(int i = 0;i<target.Length;i++)
            {
                if(target[i] > target[index])
                    index = i;
            }
        }else
        {
            for(int i = 0;i<target.Length;i++)
            {
                if(target[i] < target[index])
                    index = i;
            }
        }
        return target[index];
    }

	public static List<int> IndexOfAll(this string s,string value)
	{
		List<int> all = new List<int>();
		int length = 0;
		while(s.IndexOf(value) >= 0)
		{
			var index = s.IndexOf(value);
			if(index >= 0)
			{
				all.Add(index + length);
				s = s.Substring(index + 1,s.Length - (index + 1));
				length += index + 1;
			}
		}
		return all;
	}
}
