using System;
using FeuerwehrCloud.Plugin;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace FeuerwehrCloud.General
{
	public class Update : IPlugin
	{
		#region IPlugin implementation

		static string VersionHash;
		public event PluginEvent Event;
		private static System.Collections.Generic.Dictionary<string, string> DDNSConfig = new System.Collections.Generic.Dictionary<string, string> ();
		static System.Threading.Timer WSThread;
		private static System.Threading.ManualResetEvent ThreadExitEvent = new System.Threading.ManualResetEvent(false);
		private FeuerwehrCloud.Plugin.IHost My;

		public bool Initialize (IHost hostApplication)
		{
			My = hostApplication;
			FeuerwehrCloud.Helper.Logger.WriteLine ("|  *** Update loaded...");

	

			WSThread = new System.Threading.Timer(delegate(object state) {
				System.Diagnostics.Debug.WriteLine(">> UPDATE THREAD");
				try {
					using (System.Net.WebClient wc = new System.Net.WebClient())
					{
						System.Collections.Generic.Dictionary<string, string> Updates = new Dictionary<string, string>();
						var AllAssemblys = AppDomain.CurrentDomain.GetAssemblies();
						string HtmlResult = wc.DownloadString(new Uri("http://www.feuerwehrcloud.de/deiva/version.php"));
						string[] eachLine = HtmlResult.Replace("\r","").Split(new []{"\n"},StringSplitOptions.RemoveEmptyEntries);
						foreach (var item in eachLine) {
							string[] ALine = item.Split(new []{":"},StringSplitOptions.None);
							Version V=new Version(ALine[1]);
							foreach (var asmbly in AllAssemblys) {
								var cAssembly = asmbly.FullName.Split(new []{","},StringSplitOptions.RemoveEmptyEntries);
								if(cAssembly[0].Trim() == ALine[0]) {
									Version MV = new Version(cAssembly[1].Replace("Version=",""));
									if(V>MV) {
										// Es gibt ein Major-Update?!
										Updates.Add(cAssembly[0].Trim(), System.Web.HttpUtility.UrlDecode((new Uri(asmbly.CodeBase)).AbsolutePath));
									} 
								}
								//AssemblyName M  = AssemblyName.GetAssemblyName(asmbly.CodeBase);
								System.Diagnostics.Debug.Write("");
							}
						}

						if(Updates.Count>0) {
							foreach (var item in Updates) {
								using(var WC = new System.Net.WebClient()) {
									try {
										byte[] data = WC.DownloadData("http://www.feuerwehrcloud.de/deiva/update/"+System.IO.Path.GetFileName(item.Value));
										System.IO.File.WriteAllBytes(item.Value,data);
									} catch (Exception ex) {
										
									}
								}
							}
							System.Diagnostics.Process.Start("updatehelper.sh", System.Diagnostics.Process.GetCurrentProcess().Id.ToString());
							System.Environment.FailFast("ENDE");
						}
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
				return "Update";
			}
		}

		public string FriendlyName {
			get {
				return "Update Module";
			}
		}

		public Guid GUID {
			get {
				return new Guid ("UPDATE");
			}
		}

		public byte[] Icon {
			get {
				var assembly = typeof(FeuerwehrCloud.General.Update).GetTypeInfo().Assembly;
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

		public Update ()
		{
		}
	}
}

