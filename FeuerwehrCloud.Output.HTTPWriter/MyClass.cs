using System;
using FeuerwehrCloud.Plugin;
using System.IO;
using System.Reflection;

namespace FeuerwehrCloud.Output
{
	public class HTTPWriter : IPlugin
	{
		public event PluginEvent Event;

		private FeuerwehrCloud.Plugin.IHost My;

		public string Name {
			get {
				return "HTTPWriter";
			}
		}
		public string FriendlyName {
			get {
				return "HTTP Ausgabemodul";
			}
		}

		public Guid GUID {
			get {
				return new Guid ("A");
			}
		}

		public byte[] Icon {
			get {
				var assembly = typeof(FeuerwehrCloud.Output.HTTPWriter).GetTypeInfo().Assembly;
				string[] resources = assembly.GetManifestResourceNames();
				Stream stream = assembly.GetManifestResourceStream("icon.ico");
				return ((MemoryStream)stream).ToArray();
			}
		}

		public bool Initialize(IHost hostApplication) {
			My = hostApplication;
			FeuerwehrCloud.Helper.Logger.WriteLine ("|  *** HTTPWriter loaded...");
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
			FeuerwehrCloud.Helper.Logger.WriteLine ("|  > [HTTPWriter] *** Unloading...");
		}

		public void RaiseFinish(params object[] list) {
			PluginEvent messageSent = Event;
			if (messageSent != null)
			{
				messageSent(this, list);
			}
		}

		public void Execute(params object[] list) {
			try {
				string[] f = Array.ConvertAll (list, p => (string)p);
				string OutPut = string.Join ("\n", f, 1, f.Length - 1);
				string r = (string)(list [0]);
				System.IO.File.WriteAllText (r, OutPut);
				using(var Writer = new System.Net.WebClient()) {
					string O = Writer.DownloadString(r);
					RaiseFinish(new []{O});
				}
				FeuerwehrCloud.Helper.Logger.WriteLine ("|  > [HTTPWriter] *** "+r.ToUpper()+" executed...");

			} catch (Exception ex) {
				FeuerwehrCloud.Helper.Logger.WriteLine(FeuerwehrCloud.Helper.Helper.GetExceptionDescription(ex));

			}

		}

		public HTTPWriter ()
		{
		}


	}
}

