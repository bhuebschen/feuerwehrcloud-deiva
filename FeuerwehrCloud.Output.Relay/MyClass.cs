using System;
using FeuerwehrCloud.Plugin;
using System.IO;
using System.Reflection;

namespace FeuerwehrCloud.Output
{
	public class Relay : Plugin.IPlugin
	{
		#region IPlugin implementation

		public event PluginEvent Event;
		private FeuerwehrCloud.Plugin.IHost My;
		public static bool Disposing = false;
		private System.Collections.Generic.Dictionary<string, string> Configuration = new System.Collections.Generic.Dictionary<string, string> ();

		public bool Initialize (FeuerwehrCloud.Plugin.IHost hostApplication)
		{
			FeuerwehrCloud.Helper.Logger.WriteLine ("|  *** Relay plugin loaded...");
			if(!System.IO.File.Exists("relay.cfg")) {
				Configuration.Add ("relaisnum", "");
				FeuerwehrCloud.Helper.AppSettings.Save(Configuration,"relay.cfg");
			} 
			Configuration = FeuerwehrCloud.Helper.AppSettings.Load ("relay.cfg");
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
				return ServiceType.output;
			}
		}

		public string Name {
			get {
				return "Relay";
			}
		}

		public string FriendlyName {
			get {
				return "Plugin zum Schalten von Relais";
			}
		}

		public Guid GUID {
			get {
				return new Guid (Name);
			}
		}

		public byte[] Icon {
			get {
				var assembly = typeof(FeuerwehrCloud.Output.Relay).GetTypeInfo().Assembly;
				string[] resources = assembly.GetManifestResourceNames();
				Stream stream = assembly.GetManifestResourceStream("icon.ico");
				return ((MemoryStream)stream).ToArray();
			}
		}

		#endregion

		#region IDisposable implementation

		public void Dispose ()
		{
			FeuerwehrCloud.Helper.Logger.WriteLine ("|  *** Relay plugin unloaded...");
		}

		#endregion


		public Relay ()
		{
		}
	}
}

