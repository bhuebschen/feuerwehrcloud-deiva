using System;

namespace de.SYStemiya.Helper
{

	public static class Helper {
		public static string[] RemoveAt(this string[] source, int index)
		{
			if (source == null)
				throw new ArgumentNullException("source");

			if (0 > index || index >= source.Length)
				throw new ArgumentOutOfRangeException("index", index, "index is outside the bounds of source array");

			Array dest = Array.CreateInstance(source.GetType().GetElementType(), source.Length - 1);
			Array.Copy(source, 0, dest, 0, index);
			Array.Copy(source, index + 1, dest, index, source.Length - index - 1);

			return (string[])dest;
		}

	}

	public static class String {
		public static string Left(this string str, int length)
		{
			str = (str ?? string.Empty);
			return str.Substring(0, Math.Min(length, str.Length));
		}

		public static string Right(this string str, int length)
		{
			str = (str ?? string.Empty);
			return (str.Length >= length)
				? str.Substring(str.Length - length, length)
					: str;
		}

		public static int Len(this string str) {
			return str.Length;
		}

		public static string Mid(this string str, int start, int length) {
			return str.Substring (start - 1, length);
		}
	}

}


