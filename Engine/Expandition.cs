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
	}
}
