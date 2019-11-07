using System;
using FeuerwehrCloud.Plugin;
using System.IO;
using System.Reflection;

namespace FeuerwehrCloud.Input
{
	public class FMSFuG : Plugin.IPlugin
	{
		#region IPlugin implementation

		public event PluginEvent Event;

		public bool Initialize (IHost hostApplication)
		{
			FeuerwehrCloud.Helper.Logger.WriteLine ("|  *** FMSFuG plugin loaded...");
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
				return ServiceType.input;
			}
		}

		public string Name {
			get {
				return "FMSFuG";
			}
		}

		public string FriendlyName {
			get {
				return "Funkmeldesystem (über FuG b8)";
			}
		}

		public Guid GUID {
			get {
				return new Guid (Name);
			}
		}

		public byte[] Icon {
			get {
				var assembly = typeof(FeuerwehrCloud.Input.FMSFuG).GetTypeInfo().Assembly;
				string[] resources = assembly.GetManifestResourceNames();
				Stream stream = assembly.GetManifestResourceStream("icon.ico");
				return ((MemoryStream)stream).ToArray();
			}
		}

		#endregion

		#region IDisposable implementation

		public void Dispose ()
		{
			FeuerwehrCloud.Helper.Logger.WriteLine ("|  *** FMSFuG plugin unloaded...");
		}

		#endregion

		public FMSFuG ()
		{
		}
	}
}

