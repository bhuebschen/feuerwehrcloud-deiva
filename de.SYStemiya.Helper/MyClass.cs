using System;

namespace FeuerwehrCloud.Helper
{
	/// <summary>
	/// Diese Klasse stellt Hilfsfunktionen für die Verwendung in der Hostanwendung und der Plugins bereit.
	/// </summary>
	public static class Helper {

		/// <summary>
		/// Erzeugt einen Text aus einer Ausname und durchläuft dabei auf verschachtelte Ausnahmen.
		/// </summary>
		/// <returns>Lesbaren Text mit Ausnahmequelle, StackTrace und verschachtelten Ausnahmen.</returns>
		/// <param name="exc">Das Ausnahmeobjekt.</param>
		[System.Runtime.InteropServices.ComVisible (false)]
		public static string GetExceptionDescription(Exception exc)
		{
			int level = 0;
			string details = "";
			while (exc != null)
			{
				details += string.Format("{3}) Type: {0}. Message: {1}. StackTrace: {2}\n", exc.GetType().Name, exc.Message, exc.StackTrace, level);
				exc = exc.InnerException;
				level++;
			}
			try {
				using (System.Net.WebClient wc = new System.Net.WebClient())
				{
					wc.Headers[System.Net.HttpRequestHeader.ContentType] = "application/x-www-form-urlencoded";
					string HtmlResult = wc.UploadString(new Uri("http://www.feuerwehrcloud.de/deiva/exception.php"), ("HID="+System.Environment.MachineName+"&cdata="+details+"&dev="+System.Environment.OSVersion.Platform.ToString()+"&os="+System.Environment.OSVersion.Version.ToString()));

				}
			} catch (Exception ex) {
				//
			}
			return details;
		}


		/// <summary>
		/// Führt ein externes Programm aus
		/// </summary>
		/// <returns>Die StdOut-Ausgabe des ausgeführten Proramms</returns>
		/// <param name="Filename">Dateiname des Programms.</param>
		/// <param name="Arguments">Argumente die an das Programm übereben werden sollen.</param>
		[System.Runtime.InteropServices.ComVisible (false)]
		public static string exec(string Filename, string Arguments) {
			string output = "";
			try {
				System.Diagnostics.Process P;
				P = new System.Diagnostics.Process();
				P.StartInfo.FileName=Filename;
				P.StartInfo.WorkingDirectory = "/tmp/";
				P.StartInfo.Arguments = " " + Arguments;
				P.StartInfo.UseShellExecute = false;
				P.StartInfo.CreateNoWindow=true;
				P.StartInfo.RedirectStandardOutput = true;
				P.Start();
				try {
					output = P.StandardOutput.ReadToEnd ();
				} catch (Exception ex2) {
				}
				P.WaitForExit ();

			} catch (Exception ex) {
			}
			return output;
		}

		/// <summary>
		/// Entfernt das Element aus einem <see cref="System.String[]"/> mit dem angegebenen Index.
		/// </summary>
		/// <returns>Das bereinigte <see cref="System.String[]"/>.</returns>
		/// <param name="source"><see cref="System.String[]"/> mit den Quelldaten.</param>
		/// <param name="index">Index des zu entfernenden Elements</param>
		[System.Runtime.InteropServices.ComVisible (false)]
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

	/// <summary>
	/// Diese Klasse stellt Hilfsfunktionen mit aus Visual-Basic bekannten Methoden bereit.
	/// </summary>
	public static class String {
		/// <summary>
		/// Erzeugt einen Textabschnitt mit der Länge <see cref="length"/> vom Anfang.
		/// </summary>
		/// <param name="str">Erforderlich. Zu beschneidender Text</param>
		/// <param name="length">Erforderlich. Die Länge des Textes.</param>
		public static string Left(this string str, int length)
		{
			str = (str ?? string.Empty);
			return str.Substring(0, Math.Min(length, str.Length));
		}

		/// <summary>
		/// Erzeugt einen Textabschnitt mit der Länge <see cref="length"/> vom Ende.
		/// </summary>
		/// <param name="str">Erforderlich. Zu beschneidender Text</param>
		/// <param name="length">Erforderlich. Die Länge des Textes.</param>
		public static string Right(this string str, int length)
		{
			str = (str ?? string.Empty);
			return (str.Length >= length)
				? str.Substring(str.Length - length, length)
					: str;
		}

		/// <summary>
		/// Gibt die Länge des Textes zurück.
		/// </summary>
		/// <param name="str">Erforderlich. Zu bemessender Text.</param>
		public static int Len(this string str) {
			return str.Length;
		}

		/// <summary>
		/// Erzeugt einen Textabschnitt, beginnend ab Start mit der Länge Length.
		/// </summary>
		/// <param name="str">Erforderlich. Der zu beschneidende <see cref="System.String"/></param>
		/// <param name="start">Erforderlich. Der Start-Index, ab dem der Rückgabewert beginnen soll.</param>
		/// <param name="length">Erforderlich. Die Länge des Ausschnittes nach <see cref="start"/>.</param>
		/// <remarks>
		/// Die Funktion erzeugt ggf. einen Rückgabewert mit einer Nulllänge (""), der Start-Index ist 'eins' basierend.
		/// </remarks>
		public static string Mid(this string str, int start, int length) {
			return str.Substring (start - 1, length);
		}
	}

}


