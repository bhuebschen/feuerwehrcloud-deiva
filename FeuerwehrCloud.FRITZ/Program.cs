using System;

namespace FeuerwehrCloud.FRITZ
{
	class MainClass
	{
		public static void Main (string[] args)
		{
			Console.Write (System.Environment.GetEnvironmentVariable("SCRIPT_FILENAME"));

		}
	}
}
