using System;
using FeuerwehrCloud.Plugin;
using TextApp.SMS.www.textapp.net;
using System.IO;
using System.Reflection;

namespace FeuerwehrCloud.Output
{
	public class TextApp : IPlugin
	{
		#region IPlugin implementation

		IHost My;

		public event PluginEvent Event;

		public string Name {
			get {
				return "TextApp";
			}
		}
		public string FriendlyName {
			get {
				return "TextApp SMS-Modul";
			}
		}

		public Guid GUID {
			get {
				return new Guid ("A");
			}
		}

		public byte[] Icon {
			get {
				var assembly = typeof(FeuerwehrCloud.Output.TextApp).GetTypeInfo().Assembly;
				string[] resources = assembly.GetManifestResourceNames();
				Stream stream = assembly.GetManifestResourceStream("icon.ico");
				return ((MemoryStream)stream).ToArray();
			}
		}


		public bool Initialize (IHost hostApplication)
		{
			My = hostApplication;
			FeuerwehrCloud.Helper.Logger.WriteLine ("|  *** TextApp SMS loaded...");
			return true;	
		}

		public void Execute (params object[] list)
		{

			Service mySMS = new Service ();
			string Message = ((string)list [1]);
			string x = mySMS.TestSendSMS (false, "benedikt.huebschen@systemiya.de", "01012000", "Feuerwehr Neubeuern", "", "+4980353749", "+491739561779", Message, Message.Length, 2, 4, "", "");
			FeuerwehrCloud.Helper.Logger.WriteLine (">>" + x);
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

		#endregion

		#region IDisposable implementation

		public void Dispose ()
		{
			throw new NotImplementedException ();
		}

		#endregion

		public TextApp ()
		{
		}
	}
}

