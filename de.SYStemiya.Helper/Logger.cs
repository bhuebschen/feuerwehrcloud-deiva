using System;

namespace de.SYStemiya.Helper
{

	public class Logger {

		public enum LogMode {
			Console = 0,
			LogFile = 1,
			ConsoleAndLog=2
		}

		System.Collections.Queue FileOutQueue = new System.Collections.Queue();
		static Object thisLock = new Object();

		public static LogMode Mode = LogMode.ConsoleAndLog;

		public static void WriteLine(string line, object arg0, object arg1, object arg2, object arg3) {
			WriteLine (string.Format (line, arg0, arg1,arg2,arg3));
		}

		public static void WriteLine(string line, object arg0, object arg1, object arg2) {
			WriteLine (string.Format (line, arg0, arg1,arg2));
		}

		public static void WriteLine(string line, object arg0, object arg1) {
			WriteLine (string.Format (line, arg0, arg1));
		}

		public static void WriteLine(string line, object arg0) {
			WriteLine (string.Format (line, arg0));
		}

		public static void WriteLine(string line) {
			if(Mode == LogMode.Console || Mode == LogMode.ConsoleAndLog) {
				Console.WriteLine (line);
			}
			if(Mode == LogMode.LogFile || Mode == LogMode.ConsoleAndLog) {
				lock(thisLock) {
					System.IO.File.AppendAllText ("./logs/" + System.DateTime.Now.ToString("dd-MM-yyyy") + ".log" , line + "\r\n");
				}
			}
		}

		public static void Write(string line, object arg0, object arg1, object arg2, object arg3) {
			Write (string.Format (line, arg0, arg1,arg2,arg3));
		}

		public static void Write(string line, object arg0, object arg1, object arg2) {
			Write (string.Format (line, arg0, arg1,arg2));
		}

		public static void Write(string line, object arg0, object arg1) {
			Write (string.Format (line, arg0, arg1));
		}

		public static void Write(string line, object arg0) {
			Write (string.Format (line, arg0));
		}

		public static void Write(string line) {
			Object thisLock = new Object();
			if(Mode == LogMode.Console || Mode == LogMode.ConsoleAndLog) {
				Console.Write(line);
			}
			if(Mode == LogMode.LogFile || Mode == LogMode.ConsoleAndLog) {
				lock (thisLock) {
                    if (System.IO.Directory.Exists("./logs/") == false) System.IO.Directory.CreateDirectory("./logs/");
					System.IO.File.AppendAllText ("./logs/" + System.DateTime.Now.ToString ("dd-MM-yyyy") + ".log", line);
				}
			}
		}
	}
}
