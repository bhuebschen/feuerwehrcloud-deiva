using System;
using Prowlin;
using FeuerwehrCloud.Plugin;
using Prowlin.Interfaces;
using System.IO;
using System.Reflection;

namespace FeuerwehrCloud.Output
{
	public class Prowl : Plugin.IPlugin
	{
		#region IPlugin implementation
		public event PluginEvent Event;
		private FeuerwehrCloud.Plugin.IHost My;

		public string Name {
			get {
				return "Prowl";
			}
		}

		public string FriendlyName {
			get {
				return "Prowl";
			}
		}

		public Guid GUID {
			get {
				return new Guid ("A");
			}
		}

		public byte[] Icon {
			get {
				var assembly = typeof(FeuerwehrCloud.Output.Prowl).GetTypeInfo().Assembly;
				string[] resources = assembly.GetManifestResourceNames();
				Stream stream = assembly.GetManifestResourceStream("icon.ico");
				return ((MemoryStream)stream).ToArray();
			}
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

		}

		public void Execute(params object[] list) {
			INotification notification = new Prowlin.Notification()
			{
				Application = "FeuerwehrCloud",
				Description = (string)list[1],
				Event = (string)list[0],
				Priority = NotificationPriority.Emergency,
				Url = "https://"+System.Environment.MachineName + ".feuerwehrcloud.de:10443/alert.php?alerid="+(string)list[2]
			};
			notification.AddApiKey("6b9ea4e98c84381c2606dd3ec48ea1033a4d127a");
			ProwlClient prowlClient = new ProwlClient();
			NotificationResult notificationResult = prowlClient.SendNotification(notification);
		}

		public bool Initialize(IHost hostApplication) {
			My = hostApplication;
			FeuerwehrCloud.Helper.Logger.WriteLine ("|  *** Prowl loaded...");
			return true;
		}


		#endregion

		public Prowl ()
		{
		}
	}
}

