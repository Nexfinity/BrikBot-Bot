using System;
using System.Linq;
using JetBrains.Annotations;

namespace BrikBotCore.Extensions
{
	public static class StringExtensions
	{
		private static readonly Random Rnd = new Random();

		public static string TrimTo(this string str, int maxLength, bool hideDots = false)
		{
			if (maxLength < 0)
				throw new ArgumentOutOfRangeException(nameof(maxLength), $@"Argument {nameof(maxLength)} can't be negative.");
			if (maxLength == 0)
				return string.Empty;
			if (maxLength <= 3)
				return string.Concat(str.Select(c => '.'));
			if (str.Length < maxLength)
				return str;
			return string.Concat(str.Take(maxLength - 3)) + (hideDots ? "" : "...");
		}

		[ContractAnnotation("null => true")]
		public static bool IsNull(this string str)
		{
			return string.IsNullOrEmpty(str?.Trim());
		}

		public static string UnescapeOnly(this string str)
		{
			return str?.Replace("`", string.Empty);
		}
	}
}