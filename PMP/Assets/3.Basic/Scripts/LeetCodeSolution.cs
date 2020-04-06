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

	#region 1389
	///<summary>
	///1389
	///给你两个整数数组 nums 和 index。你需要按照以下规则创建目标数组：
	/// 目标数组 target 最初为空。
	/// 按从左到右的顺序依次读取 nums[i] 和 index[i]，在 target 数组中的下标 index[i] 处插入值 nums[i] 。
	/// 重复上一步，直到在 nums 和 index 中都没有要读取的元素。
	///</summary>
	public int[] CreateTargetArray(int[] nums, int[] index) {
        var list = new List<int>();
        for(int i = 0;i<nums.Length;i++)
        {
			list.Insert(index[i],nums[i]);
        }
		return list.ToArray();
    }

	#endregion

	#region 189. 旋转数组
	///<summary>
	///189
	///给定一个数组，将数组中的元素向右移动 k 个位置，其中 k 是非负数。
	///尽可能想出更多的解决方案，至少有三种不同的方法可以解决这个问题。
	///要求使用空间复杂度为 O(1) 的 原地 算法。
	///</summary>
    public void Rotate1(int[] nums, int k) 
	{
		//巧妙使用Array.Reverse 翻转数组的方法
		//时间复杂度：O(n) 空间复杂度：O(1)
		k %= nums.Length;
		if(nums.Length < 2 || k<= 0)
			return;
		Array.Reverse(nums,0,nums.Length);
		Array.Reverse(nums,0,k);
		Array.Reverse(nums,k,nums.Length - k);
    }

	public void Rotate2(int[] nums, int k) 
	{
		//旋转几次？那就旋转几次吧
		//时间复杂度：O(kn) 空间复杂度：O(1)
		k %= nums.Length;
		if(nums.Length < 2)
			return;
		var temp = nums[nums.Length - 1];
		for(int i = 0;i<k;i++)
		{
			temp = nums[nums.Length - 1];
			for(int n = nums.Length-1;n>0;n--)
			{
				nums[n] = nums[n-1];
			}
			nums[0] = temp;
		}
    }

	public void Rotate3(int[] nums, int k) 
	{
		//环状替换
		//时间复杂度：O(n) 空间复杂度：O(1)
		k %= nums.Length;
		if(nums.Length < 2 || k<= 0)
			return;
		var swopCount = 0; 
		for(int i = 0;swopCount<nums.Length;i++)
		{
			var cur = i;
			var temp = nums[i];
			do
			{
				var next = (cur + k) % nums.Length;
				var t = nums[next];
				nums[next] = temp;
				temp = t;
				cur = next;
				swopCount++;
			} 
			while (cur != i);
		}
    }

	public void DoRotate3()
	{

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
