using System;
using System.IO;
using FeuerwehrCloud.Plugin;

namespace FeuerwehrCloud.Output
{
	public class Display :  IPlugin
	{
		public event PluginEvent Event;

		public static System.Net.Sockets.Socket S = new System.Net.Sockets.Socket(System.Net.Sockets.AddressFamily.InterNetwork, System.Net.Sockets.SocketType.Stream, System.Net.Sockets.ProtocolType.IP);
		public static int iTimer;
		private FeuerwehrCloud.Plugin.IHost My;


		public string Name {
			get {
				return "Display";
			}
		}
		public string FriendlyName {
			get {
				return "LCD Anzeigeplugin";
			}
		}

		public Guid GUID {
			get {
				return new Guid ("6");
			}
		}

		public byte[] Icon {
			get {
				return System.IO.File.ReadAllBytes("");
			}
		}

		void Connector ()
		{
			try {
				Display.S.Connect ("127.0.0.1", 13666);
				Display.S.Send (System.Text.Encoding.Default.GetBytes ("hello\r\n"));
				Display.S.Send (System.Text.Encoding.Default.GetBytes ("backlight on\r\n"));
				Display.S.Send (System.Text.Encoding.Default.GetBytes ("screen_add s1\r\n"));
				Display.S.Send (System.Text.Encoding.Default.GetBytes ("widget_add s1 w1 string\r\n"));
				Display.S.Send (System.Text.Encoding.Default.GetBytes ("widget_add s1 w2 string\r\n"));
				RaiseFinish ("text","SUCCESS");
			}
			catch (Exception ex) {
				FeuerwehrCloud.Helper.Logger.WriteLine ("| [" + System.DateTime.Now.ToString ("T") + "] |-> [LCDProc] *** UNABLE TO CONNECT TO LCDPROC SERVER ***");
			}
		}
		public bool Initialize(IHost hostApplication) {
			My = hostApplication;
			FeuerwehrCloud.Helper.Logger.WriteLine ("| ["+System.DateTime.Now.ToString("T") +"] |-> [LCDProc] *** Initializing...");
			try {
				System.Threading.Thread t = new System.Threading.Thread (Connector);
				t.Start();
			} catch (Exception ex2) {
				FeuerwehrCloud.Helper.Logger.WriteLine ("| [" + System.DateTime.Now.ToString ("T") + "] |-> [LCDProc] *** ERROR: " + ex2);
				RaiseFinish ("text","ERROR", ex2.ToString ());
			}
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
			S.Close ();
			FeuerwehrCloud.Helper.Logger.WriteLine ("| ["+System.DateTime.Now.ToString("T") +"] |-> [LCDProc] *** Unloading...");
		}

		public void RaiseFinish(params object[] list) {
			PluginEvent messageSent = Event;
			if (messageSent != null)
			{
				messageSent(this, list);
			}
		}

		public void Execute(params object[] list) {
			lock (S) {
				S.Send( System.Text.Encoding.Default.GetBytes("widget_set s1 w"+((string)list [1])+" "+((string)list [0])+" \""+((string)list [1])+" "+((string)list [2])+"\"\r\n"));
			}
		}

		public Display ()
		{
		}
	}
}

