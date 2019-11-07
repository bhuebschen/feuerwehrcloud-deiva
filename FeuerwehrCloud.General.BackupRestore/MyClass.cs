using System;
using FeuerwehrCloud.Plugin;
using Ionic.BZip2;
using Ionic.Zip;
using System.IO;
using System.Reflection;

namespace FeuerwehrCloud.General
{
	public class BackupRestore : Plugin.IPlugin
	{
		#region IPlugin implementation

		public event PluginEvent Event;

		private FeuerwehrCloud.Plugin.IHost My;
		static System.Threading.Timer WSThread;

		public bool Initialize (IHost hostApplication)
		{
			My = hostApplication;
			WSThread = new System.Threading.Timer(delegate(object state) {
				System.Diagnostics.Debug.WriteLine(">> Backup THREAD");
				try {
					using (ZipFile zip = new ZipFile())
					{
						zip.AlternateEncoding = System.Text.Encoding.UTF8;
						zip.AlternateEncodingUsage = ZipOption.AsNecessary;
						zip.AddFiles (System.IO.Directory.GetFiles ("./", "*.cfg"));
						zip.AddFiles (System.IO.Directory.GetFiles ("./", "*.lua"));
						zip.AddFiles (System.IO.Directory.GetFiles ("./", "*.txt"));
						zip.CompressionLevel = Ionic.Zlib.CompressionLevel.BestCompression;
						zip.Comment = "Backup created at " + System.DateTime.Now.ToString("G");
						string bFile = "backups/"+string.Format("backup{0}.zip", System.DateTime.Now.Ticks);
						zip.Save(bFile);
						using(var WC = new System.Net.WebClient()) {
							WC.UploadFile(new Uri("http://www.feuerwehrcloud.de/deiva/backup.php?HID="+System.Environment.MachineName),"POST",bFile);
						}
					}
				} catch (Exception ex) {
					ex.ToString();
				}
				WSThread.Change (0, 86400000);
			});
			WSThread.Change (0, 86400000);
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
				return "BackupRestore";
			}
		}

		public string FriendlyName {
			get {
				return "Backup & Restore-Modul";
			}
		}

		public Guid GUID {
			get {
				return new Guid (Name);
			}
		}

		public byte[] Icon {
			get {
				var assembly = typeof(FeuerwehrCloud.General.BackupRestore).GetTypeInfo().Assembly;
				string[] resources = assembly.GetManifestResourceNames();
				Stream stream = assembly.GetManifestResourceStream("icon.ico");
				return ((MemoryStream)stream).ToArray();
			}
		}

		#endregion

		#region IDisposable implementation

		public void Dispose ()
		{
			
		}

		#endregion

		public BackupRestore ()
		{
		}
	}
}

