using System;
using FeuerwehrCloud.Plugin;

namespace FeuerwehrCloud.Output.SMS
{
	public class SMS : IPlugin
	{
		#region IPlugin implementation

		IHost My;

		public event PluginEvent Event;

		public bool Initialize (IHost hostApplication)
		{
			My = hostApplication;
			de.SYStemiya.Helper.Logger.WriteLine ("| ["+System.DateTime.Now.ToString("T") +"] |-> [TextApp SMS] *** Initializing...");
			return true;	
		}

		public void Execute (params object[] list)
		{

			TextApp.SMS.www.textapp.net.Service mySMS = new TextApp.SMS.www.textapp.net.Service ();
			string Message = ((string)list [1]);
			string x = mySMS.TestSendSMS (false, "benedikt.huebschen@systemiya.de", "01012000", "Feuerwehr Neubeuern", "", "+4980353749", "+491739561779", Message, Message.Length, 2, 4, "", "");
			de.SYStemiya.Helper.Logger.WriteLine (">>" + x);
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

		public SMS ()
		{
		}
	}
}

