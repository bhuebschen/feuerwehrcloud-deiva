using System;
using System.IO;
using FeuerwehrCloud.Plugin;

namespace FeuerwehrCloud.Output.Printer
{
	public class ImagePrinter :  IPlugin
	{
		public event PluginEvent Event;

		private IHost My;

		public bool Initialize(IHost hostApplication) {
			My = hostApplication;
			de.SYStemiya.Helper.Logger.WriteLine ("| ["+System.DateTime.Now.ToString("T") +"] |-> [ImagePrinter] *** Initializing...");
			return true;
		}

		public bool IsAsync
		{
			get { return true; }
		}

		public ServiceType ServiceType
		{
			get { return ServiceType.output; }
		}

		public void Dispose() {
			de.SYStemiya.Helper.Logger.WriteLine ("| ["+System.DateTime.Now.ToString("T") +"] |-> [ImagePrinter] *** Unloading...");
		}

		public void RaiseFinish(params object[] list) {
			PluginEvent messageSent = Event;
			if (messageSent != null)
			{
				messageSent(this, list);
			}
		}

		public void Execute(params object[] list) {
			string[] Pages = (string[])list [1];
			de.SYStemiya.Helper.Logger.WriteLine ("| ["+System.DateTime.Now.ToString("T") +"] |-> [ImagePrinter] *** Print FAX!");
			try {
				int cPage = 0;

				System.Threading.Thread t = new System.Threading.Thread(new System.Threading.ParameterizedThreadStart(delegate(object obj) {
					System.Diagnostics.Process P = new System.Diagnostics.Process();
					P.StartInfo.FileName="printfile.sh";
					P.StartInfo.EnvironmentVariables.Add("LD_LIBRARY_PATH","/Library/Frameworks/Mono.framework/Libraries/:"+P.StartInfo.EnvironmentVariables["LD_LIBRARY_PATH"]);
					P.StartInfo.Arguments = String.Join(" ",(string[])(object)list[1]);
					P.Start(); P.WaitForExit();
					RaiseFinish ("SUCCESS");
				}));
				t.Start();
			} catch (Exception ex2) {
				de.SYStemiya.Helper.Logger.WriteLine ("[" + System.DateTime.Now.ToString ("T") + "] |-> [ImagePrinter] *** ERROR: " + ex2);
				RaiseFinish ("ERROR", ex2.ToString ());
			}
		}

		public ImagePrinter ()
		{
		}
	}
}

