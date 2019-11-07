using System;
using FeuerwehrCloud.Plugin;
using System.IO;
using System.Reflection;

namespace FeuerwehrCloud.General
{
	public class DDNS : Plugin.IPlugin
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
			if(!System.IO.File.Exists("ddns.cfg")) {
				DDNSConfig.Add ("Username", "CHANGEME");
				DDNSConfig.Add ("Password", "CHANGEME\t");
				FeuerwehrCloud.Helper.AppSettings.Save(DDNSConfig,"ddns.cfg");
			} 
			DDNSConfig = FeuerwehrCloud.Helper.AppSettings.Load ("ddns.cfg");
			FeuerwehrCloud.Helper.Logger.WriteLine ("|  *** DDNS loaded...");
			WSThread = new System.Threading.Timer(delegate(object state) {
				System.Diagnostics.Debug.WriteLine(">> DDNS THREAD");
				try {
					System.Net.WebClient WC = new System.Net.WebClient();
					WC.DownloadStringAsync(new Uri("http://www.feuerwehrcloud.de/deiva/ddns.php?HID="+System.Environment.MachineName+"&username="+DDNSConfig["Username"]+"&password="+DDNSConfig["Password"]));
				} catch (Exception ex) {
					ex.ToString();
				}
			});
			WSThread.Change (0, 3600000);
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
				return "DDNS";
			}
		}

		public string FriendlyName {
			get {
				return "Dynamic DNS Client";
			}
		}

		public Guid GUID {
			get {
				return new Guid ("DDNS");
			}
		}

		public byte[] Icon {
			get {
				var assembly = typeof(FeuerwehrCloud.General.DDNS).GetTypeInfo().Assembly;
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
			}
		}

		#endregion

		public DDNS ()
		{

		}
	}
}

