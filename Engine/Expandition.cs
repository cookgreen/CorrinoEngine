using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CorrinoEngine
{
	public static class Expandition
	{
		public static int StartsWithCharCount(this string str, char ch)
		{
			int count = 0;
			foreach (char c in str)
			{
				if (c == ch)
				{
					count++;
				}
				else
				{
					break;
				}
			}
			return count;
		}

		/// <summary>
		/// Extract the string from str which surrounded by the startStr and endStr
		/// if endStr is null, which means endStr equals to the startStr
		/// </summary>
		/// <param name="str"></param>
		/// <param name="startStr"></param>
		/// <param name="endStr"></param>
		/// <returns></returns>
		public static string Extract(this string str, string startStr, string endStr = null)
		{
            int len;
            if (string.IsNullOrEmpty(endStr))
			{
				len = startStr.Length * 2;
			}
            else
			{
				len = startStr.Length + endStr.Length;
			}
			return str.Substring(startStr.Length, str.Length - len);
		}
	}
}
