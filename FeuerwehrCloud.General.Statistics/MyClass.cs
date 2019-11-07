using System;
using FeuerwehrCloud.Plugin;
using System.IO;
using System.Reflection;

namespace FeuerwehrCloud.General
{
	public class Statistics : IPlugin
	{
		#region IPlugin implementation

		public event PluginEvent Event;
		private static System.Collections.Generic.Dictionary<string, string> DDNSConfig = new System.Collections.Generic.Dictionary<string, string> ();
		static System.Threading.Timer WSThread;
		private static System.Threading.ManualResetEvent ThreadExitEvent = new System.Threading.ManualResetEvent(false);
		private FeuerwehrCloud.Plugin.IHost My;

		public bool Initialize (IHost hostApplication)
		{
			My = hostApplication;
			FeuerwehrCloud.Helper.Logger.WriteLine ("|  *** Statistics loaded...");
			WSThread = new System.Threading.Timer(delegate(object state) {
				System.Diagnostics.Debug.WriteLine(">> Statistics THREAD");
				try {

					string Temp = FeuerwehrCloud.Helper.Helper.exec("/usr/bin/vcgencmd"," measure_temp");
					string memory = FeuerwehrCloud.Helper.Helper.exec("/usr/bin/vcgencmd"," get_mem arm");
					string uptime = FeuerwehrCloud.Helper.Helper.exec("/usr/bin/uptime","");
					using (System.Net.WebClient wc = new System.Net.WebClient())
					{
						wc.Headers[System.Net.HttpRequestHeader.ContentType] = "application/x-www-form-urlencoded";
						string HtmlResult = wc.UploadString(new Uri("http://www.feuerwehrcloud.de/deiva/runtime.php"), ("HID="+System.Environment.MachineName+"&"+Temp+"&"+memory+"&utime="+uptime));

					}
				} catch (Exception ex) {
					FeuerwehrCloud.Helper.Logger.WriteLine(FeuerwehrCloud.Helper.Helper.GetExceptionDescription(ex));
				}
			});
			WSThread.Change (0, 7200000);
			return true;
		}

		public void Execute (params object[] list)
		{
		}

		public bool IsAsync {
			get {
				return true;
			}
		}

		public ServiceType ServiceType {
			get {
				return ServiceType.general;
			}
		}

		public string Name {
			get {
				return "Statistics";
			}
		}

		public string FriendlyName {
			get {
				return "General Statistics Module";
			}
		}

		public Guid GUID {
			get {
				return new Guid ("DDNS");
			}
		}

		public byte[] Icon {
			get {
				var assembly = typeof(FeuerwehrCloud.General.Statistics).GetTypeInfo().Assembly;
				string[] resources = assembly.GetManifestResourceNames();
				Stream stream = assembly.GetManifestResourceStream("icon.ico");
				return ((MemoryStream)stream).ToArray();
			}
		}

		#endregion

		#region IDisposable implementation

		public void Dispose ()
		{
			try {
				WSThread.Dispose();
			} catch (Exception ex) {
				ex.ToString ();
			}
		}

		#endregion

		public Statistics ()
		{
		}
	}
}

