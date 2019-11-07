using System;

namespace FeuerwehrCloud.Helper
{
	/// <summary>
	/// Diese Klasse stellt Funktionen für die Ausgabe und Protokollierung bereit
	/// </summary>
	public class Logger {
	
		/// <summary>
		/// Console = Die Ausgabe findet ausschließlich über StdOut statt,
		/// LogFile = Die Ausgabe wird ausschließlich in die Logdatei geschrieben,
		/// ConsoleAndLog = Die Ausgabe wird sowohl an StdOut gesendet, als auch in die Logdatei geschrieben.
		/// </summary>
		public enum LogMode {
			Console = 0,
			LogFile = 1,
			ConsoleAndLog=2
		}
				
		System.Collections.Queue FileOutQueue = new System.Collections.Queue();
		static Object thisLock = new Object();

		/// <summary>
		/// Legt den Ausgabemodus fest.
		/// </summary>
		public static LogMode Mode = LogMode.ConsoleAndLog;

		/// <summary>
		/// Schreibt eine Textzeile an das in LogMode festgelegte Ziel.
		/// </summary>
		/// <param name="line">Legt den Text fest, der ausgegeben werden soll.</param>
		/// <param name="arg0">Ein Objekt das in der Ausgabe formatiert werden soll.</param>
		/// <param name="arg1">Das zweite Objekt das in der Ausgabe formatiert werden soll.</param>
		/// <param name="arg2">Das dritte Objekt das in der Ausgabe formatiert werden soll.</param>
		/// <param name="arg3">Das vierte Objekt das in der Ausgabe formatiert werden soll.</param>
		public static void WriteLine(string line, object arg0, object arg1, object arg2, object arg3) {
			WriteLine (string.Format (line, arg0, arg1,arg2,arg3));
		}

		/// <summary>
		/// Schreibt eine Textzeile an das in LogMode festgelegte Ziel.
		/// </summary>
		/// <param name="line">Legt den Text fest, der ausgegeben werden soll.</param>
		/// <param name="arg0">Ein Objekt das in der Ausgabe formatiert werden soll.</param>
		/// <param name="arg1">Das zweite Objekt das in der Ausgabe formatiert werden soll.</param>
		/// <param name="arg2">Das dritte Objekt das in der Ausgabe formatiert werden soll.</param>
		public static void WriteLine(string line, object arg0, object arg1, object arg2) {
			WriteLine (string.Format (line, arg0, arg1,arg2));
		}

		/// <summary>
		/// Schreibt eine Textzeile an das in LogMode festgelegte Ziel.
		/// </summary>
		/// <param name="line">Legt den Text fest, der ausgegeben werden soll.</param>
		/// <param name="arg0">Ein Objekt das in der Ausgabe formatiert werden soll.</param>
		/// <param name="arg1">Das zweite Objekt das in der Ausgabe formatiert werden soll.</param>
		public static void WriteLine(string line, object arg0, object arg1) {
			WriteLine (string.Format (line, arg0, arg1));
		}

		/// <summary>
		/// Schreibt eine Textzeile an das in LogMode festgelegte Ziel.
		/// </summary>
		/// <param name="line">Legt den Text fest, der ausgegeben werden soll.</param>
		/// <param name="arg0">Ein Objekt das in der Ausgabe formatiert werden soll.</param>
		public static void WriteLine(string line, object arg0) {
			WriteLine (string.Format (line, arg0));
		}

		/// <summary>
		/// Schreibt eine Textzeile an das in LogMode festgelegte Ziel.
		/// </summary>
		/// <param name="line">Legt den Text fest, der ausgegeben werden soll.</param>
		public static void WriteLine(string line) {
			if(Mode == LogMode.Console || Mode == LogMode.ConsoleAndLog) {
				Console.WriteLine ("["+System.DateTime.Now.ToString("T")+"] "+ line);
			}
			if(Mode == LogMode.LogFile || Mode == LogMode.ConsoleAndLog) {
				lock(thisLock) {
					if (System.IO.Directory.Exists("./logs/") == false) System.IO.Directory.CreateDirectory("./logs/");
					System.IO.File.AppendAllText ("./logs/" + System.DateTime.Now.ToString("dd-MM-yyyy") + ".log" , "["+System.DateTime.Now.ToString("T")+"] "+ line + "\r\n");
				}
			}
		}

		/// <summary>
		/// Schreibt Text an das in LogMode festgelegte Ziel.
		/// </summary>
		/// <param name="line">Legt den Text fest, der ausgegeben werden soll.</param>
		/// <param name="arg0">Ein Objekt das in der Ausgabe formatiert werden soll.</param>
		/// <param name="arg1">Das zweite Objekt das in der Ausgabe formatiert werden soll.</param>
		/// <param name="arg2">Das dritte Objekt das in der Ausgabe formatiert werden soll.</param>
		/// <param name="arg3">Das vierte Objekt das in der Ausgabe formatiert werden soll.</param>
		public static void Write(string line, object arg0, object arg1, object arg2, object arg3) {
			Write (string.Format (line, arg0, arg1,arg2,arg3));
		}

		/// <summary>
		/// Schreibt Text an das in LogMode festgelegte Ziel.
		/// </summary>
		/// <param name="line">Legt den Text fest, der ausgegeben werden soll.</param>
		/// <param name="arg0">Ein Objekt das in der Ausgabe formatiert werden soll.</param>
		/// <param name="arg1">Das zweite Objekt das in der Ausgabe formatiert werden soll.</param>
		/// <param name="arg2">Das dritte Objekt das in der Ausgabe formatiert werden soll.</param>
		public static void Write(string line, object arg0, object arg1, object arg2) {
			Write (string.Format (line, arg0, arg1,arg2));
		}

		/// <summary>
		/// Schreibt Text an das in LogMode festgelegte Ziel.
		/// </summary>
		/// <param name="line">Legt den Text fest, der ausgegeben werden soll.</param>
		/// <param name="arg0">Ein Objekt das in der Ausgabe formatiert werden soll.</param>
		/// <param name="arg1">Das zweite Objekt das in der Ausgabe formatiert werden soll.</param>
		public static void Write(string line, object arg0, object arg1) {
			Write (string.Format (line, arg0, arg1));
		}

		/// <summary>
		/// Schreibt Text an das in LogMode festgelegte Ziel.
		/// </summary>
		/// <param name="line">Legt den Text fest, der ausgegeben werden soll.</param>
		/// <param name="arg0">Ein Objekt das in der Ausgabe formatiert werden soll.</param>
		public static void Write(string line, object arg0) {
			Write (string.Format (line, arg0));
		}

		/// <summary>
		/// Schreibt Text an das in LogMode festgelegte Ziel.
		/// </summary>
		/// <param name="line">Legt den Text fest, der ausgegeben werden soll.</param>
		public static void Write(string line) {
			Object thisLock = new Object();
			if(Mode == LogMode.Console || Mode == LogMode.ConsoleAndLog) {
				Console.Write(line);
			}
			if(Mode == LogMode.LogFile || Mode == LogMode.ConsoleAndLog) {
				lock (thisLock) {
                    if (System.IO.Directory.Exists("./logs/") == false) System.IO.Directory.CreateDirectory("./logs/");
					System.IO.File.AppendText("./logs/" + System.DateTime.Now.ToString ("dd-MM-yyyy") + ".log").Write(line);
				}
			}
		}
	}
}
