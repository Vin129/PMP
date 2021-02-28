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

	#region 137. 只出现一次的数字 II
	///<summary>
	///137
	///给定一个非空整数数组，除了某个元素只出现一次以外，其余每个元素均出现了三次。找出那个只出现了一次的元素。
	///你的算法应该具有线性时间复杂度。 你可以不使用额外空间来实现吗？
	///</summary>
	public int SingleNumber1(int[] nums) {
		//普通算法
		//时间复杂度：O(n^2) 空间复杂度：O(1)
		for(int i = 0;i<nums.Length;i++)
		{
			bool isGet = true;
			for(int n = 0;n<nums.Length;n++)
			{
				if(i != n && nums[i] == nums[n])
					isGet = false;
			}
			if(isGet)
				return nums[i];
		}
		return 0;
    }

	public int SingleNumber2(int[] nums) {
		//位运算算法
		//时间复杂度：O(n) 空间复杂度：O(1)
		int one = 0;
		int two = 0;
		int three = 0;
		foreach (var n in nums)
		{
			two |= one&n; //二进制某位出现1次时twos = 0，出现2, 3次时twos = 1
			one ^= n; //二进制某位出现2次时ones = 0，出现1, 3次时ones = 1
			three = two & one; //二进制某位出现3次时（即twos = ones = 1时）three = 1，其余即出现1, 2次时three = 0；
			one &= ~three; // 将二进制下出现3次的位置零，实现`三进制下不考虑进位的加法`；
			two &= ~three;
		}
		return one;
    }
	#endregion

	#region 1
    public ListNode AddTwoNumbers(ListNode l1, ListNode l2) {
		if(l1.val == 0 && l2.val == 0)
			return new ListNode(0);
		var ln = new ListNode(-1);
		ListNode last = null;
		var T = 0;
		do
		{
			int v;
			if(ln.val != -1 && ln.next == null)
				ln.next = new ListNode(0);
			if(last == null && ln.next != null)
				last = ln.next;
		
			v = l1.val + l2.val + T;
			if(v>=10)
			{
				T = 1;
				v -= 10;
			}else
			{
				T = 0;
			}
			if(ln.val == -1)
			{
				ln.val = v;
			}
			else
			{
				last.val = v;
			}
			
			var l1Next = l1.next;
			var l2Next = l2.next;
			if(l1Next != null)
			{
				l1.val = l1Next.val;
				l1.next = l1Next.next;
			}
			else
			{
				l1.val = 0;
				l1.next = null;
			}
			if(l2Next != null)
			{
				l2.val = l2Next.val;
				l2.next = l2Next.next;
			}
			else
			{
				l2.val = 0;
				l2.next = null;
			}
		}while(l1.next != null || l2.next != null);
		return ln;
    }

	#endregion

	#region 1122
// 给你两个数组，arr1 和 arr2，
// arr2 中的元素各不相同
// arr2 中的每个元素都出现在 arr1 中
// 对 arr1 中的元素进行排序，使 arr1 中项的相对顺序和 arr2 中的相对顺序相同。未在 arr2 中出现过的元素需要按照升序放在 arr1 的末尾。
//5 3 9 2 1
	public int[] RelativeSortArray(int[] arr1, int[] arr2) 
	{
        for(int i = 0;i<arr1.Length - 1;i++)
		{
			var value = arr1[i+1];
			var key = i;
            while(key >= 0 && ComparerBoth1122(value,arr1[key],arr2))
            {
                arr1[key + 1] = arr1[key];
                key--;
            }
			arr1[key + 1] = value;
		}
        return arr1;
    }

	public bool ComparerBoth1122(int a,int b ,int[] arr2)
	{
		var indexA = Array.IndexOf(arr2,a);
		var indexB = Array.IndexOf(arr2,b);
		if(indexA < 0 && indexB < 0)
			return a < b;
		if(indexA < 0)
			return false;
		if(indexB < 0)
			return true;
		return indexA < indexB;
	}

	#endregion

	#region 1528
// 给你一个字符串 s 和一个 长度相同 的整数数组 indices 。
// 请你重新排列字符串 s ，其中第 i 个字符需要移动到 indices[i] 指示的位置。
// 返回重新排列后的字符串。

	public string RestoreString(string s, int[] indices) {
		char [] temp = s.ToCharArray();
		for(int i = 0;i<indices.Length - 1;i++)
		{
			var value = indices[i+1];
			var valueStr = temp[i+1];
			var key = i;
			while(key >= 0 && value<indices[key])
			{
				indices[key + 1] = indices[key];
				temp[key + 1] = temp[key];
				key--;
			}
			indices[key+1] = value;
			temp[key+1] = valueStr;
		}
		return new string(temp);
    }
	#endregion



	#region 1502
// 给你一个数字数组 arr 。
// 如果一个数列中，任意相邻两项的差总等于同一个常数，那么这个数列就称为 等差数列 。
// 如果可以重新排列数组形成等差数列，请返回 true ；否则，返回 false 。
    public bool CanMakeArithmeticProgression(int[] arr) {
		if(arr.Length == 2)
			return true;
		for(int i = 0;i<arr.Length - 1;i++)
		{
			for(int j = 0;j<arr.Length - 1 - i;j++)
			{
				if(arr[j] > arr[j+1])
				{
					var temp = arr[j];
					arr[j] = arr[j+1];
					arr[j+1] = temp;
				}
			}
		}
		var value = arr[1] - arr[2];
		for(int i = 0;i<arr.Length - 1;i++)
		{
			if(arr[i] - arr[i+1] != value)
				return false;
		}
		return true;
    }
	#endregion



	
	#region 976
    public int LargestPerimeter(int[] A) 
	{
		QuickSort(ref A,0,A.Length-1);
		for(int i = 0;i<A.Length - 2;i++)
		{
			if(A[i] < A[i+1] + A[i+2])
				return A[i] + A[i+1] + A[i+2];
		}	
		return 0;
    }

	public void QuickSort(ref int[] A,int start,int end)
	{
		if(start >= end)
			return;
		var p = QuickSortDivide(ref A,start,end);
		QuickSort(ref A,start,p);
		QuickSort(ref A,p+1,end);
	}
	
	public int QuickSortDivide(ref int[]A,int start,int end)
	{
        var value = A[start];

		var j = start;
		for(int i = start + 1;i <= end;i++)
		{
			if(A[i] > value)
			{
				j++;
				var tempV = A[j];
				A[j] = A[i];
				A[i] = tempV;
			}
		}
		var temp = A[start];
		A[start] = A[j];
		A[j] = temp;
		return j;
	}
	#endregion

	#region 350
    public int[] Intersect(int[] nums1, int[] nums2) {
		QuickSort(ref nums1,0,nums1.Length - 1);
		QuickSort(ref nums2,0,nums2.Length - 1);
		int i = 0;
		int j = 0;
		var list = new List<int>();
		while(i < nums1.Length && j < nums2.Length)
		{
			if(nums1[i]==nums2[j])
			{
				list.Add(nums1[i]);
				i++;
				j++;
			}
			else if(nums1[i] > nums2[j])
			{
				i++;
			}
			else
			{
				j++;
			}
		}
		return list.ToArray();
    }
	#endregion


}
  public class ListNode {
      public int val;
      public ListNode next;
      public ListNode(int x) { val = x; }
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
