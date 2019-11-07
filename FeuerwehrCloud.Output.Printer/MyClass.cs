using System;
using System.IO;
using FeuerwehrCloud.Plugin;
using System.Reflection;

namespace FeuerwehrCloud.Output
{
	public class ImagePrinter :  IPlugin
	{
		public event PluginEvent Event;

		private FeuerwehrCloud.Plugin.IHost My;


		public string Name {
			get {
				return "ImagePrinter";
			}
		}
		public string FriendlyName {
			get {
				return "Druckt ein Bild auf dem Standarddrucker aus";
			}
		}

		public Guid GUID {
			get {
				return new Guid ("2");
			}
		}

		public byte[] Icon {
			get {
				var assembly = typeof(FeuerwehrCloud.Output.ImagePrinter).GetTypeInfo().Assembly;
				string[] resources = assembly.GetManifestResourceNames();
				Stream stream = assembly.GetManifestResourceStream("icon.ico");
				return ((MemoryStream)stream).ToArray();
			}
		}

		public bool Initialize(IHost hostApplication) {
			My = hostApplication;
			FeuerwehrCloud.Helper.Logger.WriteLine ("|  *** ImagePrinter loaded...");
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
			FeuerwehrCloud.Helper.Logger.WriteLine ("|  > [ImagePrinter] *** Unloading...");
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
			FeuerwehrCloud.Helper.Logger.WriteLine ("|  > [ImagePrinter] *** Print FAX!");
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
				FeuerwehrCloud.Helper.Logger.WriteLine(FeuerwehrCloud.Helper.Helper.GetExceptionDescription(ex2));
				RaiseFinish ("ERROR", ex2.ToString ());
			}
		}

		public ImagePrinter ()
		{
		}
	}
}

