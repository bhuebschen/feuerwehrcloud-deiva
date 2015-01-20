using System;
using System.Collections.Generic;
using System.IO;
using System.Web.Script.Serialization;

namespace de.SYStemiya.Helper
{

	public static class AppSettings
	{
		public static void Save(Dictionary<string, string> dictionary, string fileName)
		{
			try
			{
				// GetUserStoreForApplication doesn't work - can't identify.
				// application unless published by ClickOnce or Silverlight
				using (System.IO.FileStream stream = new System.IO.FileStream(fileName,System.IO.FileMode.OpenOrCreate))
				using (StreamWriter writer = new StreamWriter(stream))
				{
					writer.WriteLine(dictionary.Count);
					// Write pairs.
					foreach (var pair in dictionary)
					{
						writer.WriteLine(pair.Key);
						writer.WriteLine(pair.Value.Replace("\\","\\\\").Replace("\n","\\n").Replace("\r","\\r").Replace("\t","\\t"));
					}
				}

			}
			catch (Exception) { }   // If fails - just don't use preferences
		}
		public static Dictionary<string, string> Load(string fileName)
		{
			try
			{
				var result = new Dictionary<string, string>();
				// GetUserStoreForApplication doesn't work - can't identify
				// application unless published by ClickOnce or Silverlight
				using (System.IO.FileStream stream = new System.IO.FileStream(fileName,System.IO.FileMode.OpenOrCreate))
				using (StreamReader reader = new StreamReader(stream))
				{
					int count = int.Parse(reader.ReadLine());
					// Read in all pairs.
					for (int i = 0; i < count; i++)
					{
						string key = reader.ReadLine();
						string value = reader.ReadLine().Replace("\\\\","\\").Replace("\\n","\n").Replace("\\r","\r").Replace("\\t","\t");
						result[key] = value;
					}
				}
				return result;
			}
			catch (Exception) { return null; }   // If fails - just don't use preferences.

		}
	}

}
