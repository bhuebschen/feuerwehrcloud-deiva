using System;
using FeuerwehrCloud.Plugin;
using System.IO;
using System.Reflection;

namespace FeuerwehrCloud.Input
{
	public class BosMon : Plugin.IPlugin
	{
		#region IPlugin implementation

		public event PluginEvent Event;
		public bool Initialize (IHost hostApplication)
		{
			FeuerwehrCloud.Helper.Logger.WriteLine ("|  *** BosMon-Input plugin loaded...");
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
				return "BosMonInput";
			}
		}

		public string FriendlyName {
			get {
				return "BosMon-Eingangsmodul";
			}
		}

		public Guid GUID {
			get {
				return new Guid (Name);
			}
		}

		public byte[] Icon {
			get {
				var assembly = typeof(FeuerwehrCloud.Input.BosMon).GetTypeInfo().Assembly;
				string[] resources = assembly.GetManifestResourceNames();
				Stream stream = assembly.GetManifestResourceStream("icon.ico");
				return ((MemoryStream)stream).ToArray();
			}
		}

		#endregion

		#region IDisposable implementation

		public void Dispose ()
		{
			FeuerwehrCloud.Helper.Logger.WriteLine ("|  *** BosMon-Input plugin unloaded...");
		}
		#endregion
		public BosMon ()
		{
		}
	}
}

