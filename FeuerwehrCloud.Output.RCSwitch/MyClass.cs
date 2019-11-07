using System;
using System.IO;
using System.Reflection;

namespace FeuerwehrCloud.Output
{
	public class RCSwitch : Plugin.IPlugin
	{
		#region IPlugin implementation

		public event FeuerwehrCloud.Plugin.PluginEvent Event;

		public string Name {
			get {
				return "RCSwitch";
			}
		}
		public string FriendlyName {
			get {
				return "Funkschalter-Modul";
			}
		}

		public Guid GUID {
			get {
				return new Guid ("A");
			}
		}

		public byte[] Icon {
			get {
				var assembly = typeof(FeuerwehrCloud.Output.RCSwitch).GetTypeInfo().Assembly;
				string[] resources = assembly.GetManifestResourceNames();
				Stream stream = assembly.GetManifestResourceStream("icon.ico");
				return ((MemoryStream)stream).ToArray();
			}
		}


		public bool Initialize (FeuerwehrCloud.Plugin.IHost hostApplication)
		{
			return true;
		}

		public void Execute (params object[] list)
		{
			FeuerwehrCloud.Helper.Helper.exec ("/bin/send", (string)(list [1]) + " " + (string)(list [2]) + " " + (string)(list [3]));
		}

		public bool IsAsync {
			get {
				return true;
			}
		}

		public FeuerwehrCloud.Plugin.ServiceType ServiceType {
			get {
				return FeuerwehrCloud.Plugin.ServiceType.output;
			}
		}

		#endregion

		#region IDisposable implementation

		public void Dispose ()
		{
		}

		#endregion

		public RCSwitch ()
		{
		}
	}
}

