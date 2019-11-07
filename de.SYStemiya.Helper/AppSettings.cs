using System;
using System.Collections.Generic;
using System.IO;
using System.Web.Script.Serialization;

namespace FeuerwehrCloud.Helper
{

	/// <summary>
	/// Diese Klasse stellt Funktionen zum Lesen und Schreiben von Konfigurationen zur Verfügung.
	/// </summary>
	public static class AppSettings
	{

		private static string StriPX(string X) {
			return X.Replace ("\\", "\\\\").Replace ("\n", "\\n").Replace ("\r", "\\r").Replace ("\t", "\\t");
		}

		/// <summary>
		/// Speichert Konfigurationsdaten in einer Datei.
		/// </summary>
		/// <param name="dictionary">Enthält ein Dictionary<string, string> mit den zu sichernden Werten.</param>
		/// <param name="fileName">Gibt den Dateiname der Datei an, in der die Konfiguration geschrieben werden soll.</param>
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
						writer.WriteLine(StriPX(pair.Value));
					}
				}

			}
			catch (Exception) { 
				return;
			}   // If fails - just don't use preferences
		}

		/// <summary>
		/// Liest Konfigurationsdaten aus einer Datei und gibt diese zurück.
		/// </summary>
		/// <returns>Die Konfigurationsdaten in einem Dictionary<string, string></returns>
		/// <param name="fileName">Gibt den Dateiname der Datei an, aus der die Konfigurationsdaten gelesen werden sollen.</param>
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
						string value = StriPX(reader.ReadLine());
						result[key] = value;
					}
				}
				return result;
			}
			catch (Exception) { return null; }   // If fails - just don't use preferences.

		}
	}

}
