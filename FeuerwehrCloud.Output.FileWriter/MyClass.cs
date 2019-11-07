using System;
using System.Text;
using FeuerwehrCloud.Plugin;
using System.ComponentModel;
using System.IO;
using System.Reflection;

namespace FeuerwehrCloud.Output
{
	public class FileWriter : FeuerwehrCloud.Plugin.IPlugin 
	{
	
		public event PluginEvent Event;
		private FeuerwehrCloud.Plugin.IHost My;


		public string Name {
			get {
				return "FileWriter";
			}
		}
		public string FriendlyName {
			get {
				return "Dateierzeugungsplugin";
			}
		}

		public Guid GUID {
			get {
				return new Guid ("9");
			}
		}

		public byte[] Icon {
			get {
				var assembly = typeof(FeuerwehrCloud.Output.FileWriter).GetTypeInfo().Assembly;
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
			try {
				string[] f = Array.ConvertAll (list, p => (string)p);
				string OutPut = string.Join ("\n", f, 1, f.Length - 1);
				string r = (string)(list [0]);
				System.IO.File.WriteAllText (r, OutPut);
				FeuerwehrCloud.Helper.Logger.WriteLine ("|  > [FileWriter] *** File ("+r.ToUpper()+") written...");

			} catch (Exception ex) {
				FeuerwehrCloud.Helper.Logger.WriteLine(FeuerwehrCloud.Helper.Helper.GetExceptionDescription(ex));

			}
		}

		public bool Initialize(IHost hostApplication) {
			My = hostApplication;
			FeuerwehrCloud.Helper.Logger.WriteLine ("|  *** FileWriter loaded");
			return true;
		}
			
		public FileWriter()
		{
		}

	}

}

