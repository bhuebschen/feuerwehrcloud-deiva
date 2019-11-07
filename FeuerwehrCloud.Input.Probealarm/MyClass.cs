using System;
using FeuerwehrCloud.Plugin;
using System.Net.Sockets;
using System.Text;
using System.IO;
using System.Reflection;

namespace FeuerwehrCloud.Input
{
	public class Probealarm : FeuerwehrCloud.Plugin.IPlugin
	{
		private FeuerwehrCloud.Plugin.IHost My;
		public static bool Disposing = false;
		static System.Net.Sockets.TcpListener TCL;
		System.Threading.Thread ListenSockThread;
		static int DEFAULT_SIZE = 1024;           //size of receive buffer
		static byte[] buffer = new byte[DEFAULT_SIZE];  
		static int dataRecieved = 0;
		private System.Collections.Generic.Dictionary<string, string> FAXConfig = new System.Collections.Generic.Dictionary<string, string> ();
		private System.Collections.Generic.Dictionary<string, string> CopiesConfig = new System.Collections.Generic.Dictionary<string, string> ();

		#region IPlugin implementation


		public string Name {
			get {
				return "Probealarm";
			}
		}
		public string FriendlyName {
			get {
				return "Probealarm-Modul";
			}
		}

		public Guid GUID {
			get {
				return new Guid ("A");
			}
		}

		public byte[] Icon {
			get {
				var assembly = typeof(FeuerwehrCloud.Input.Probealarm).GetTypeInfo().Assembly;
				string[] resources = assembly.GetManifestResourceNames();
				Stream stream = assembly.GetManifestResourceStream("icon.ico");
				return ((MemoryStream)stream).ToArray();
			}
		}

		public event PluginEvent Event;

		public bool Initialize (IHost hostApplication)
		{
			My = hostApplication;

			if(!System.IO.File.Exists("probealarm.cfg")) {
				FAXConfig.Add ("Target", "");
				FeuerwehrCloud.Helper.AppSettings.Save(FAXConfig,"probealarm.cfg");
			} 
			FAXConfig = FeuerwehrCloud.Helper.AppSettings.Load ("probealarm.cfg");
			if(!System.IO.File.Exists("copies.cfg")) {
				CopiesConfig.Add ("count", "1");
				FeuerwehrCloud.Helper.AppSettings.Save(CopiesConfig,"copies.cfg");
			} 
			CopiesConfig = FeuerwehrCloud.Helper.AppSettings.Load ("copies.cfg");

			//System.Environment.MachineName + 
			ListenSockThread = new System.Threading.Thread(new System.Threading.ThreadStart(HandleThreadStart));
			ListenSockThread.Start ();
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

		#endregion

		#region IDisposable implementation

		public void Dispose ()
		{
			try {
				ListenSockThread.Abort();
			} catch (Exception ex) {
				
			}
		}

		#endregion

		public Probealarm ()
		{
			FeuerwehrCloud.Helper.Logger.WriteLine ("|  *** Probealarm loaded...");
		}

		void HandleThreadStart ()
		{
			try {
				TCL = new System.Net.Sockets.TcpListener(new System.Net.IPEndPoint(System.Net.IPAddress.Any,20112));
				TCL.Start ();
				do {
					System.Net.Sockets.Socket S =  TCL.AcceptSocket();
					FeuerwehrCloud.Helper.Logger.WriteLine ("|  < [Probealarm] *** Incoming Probealarm");

					if(!System.IO.File.Exists("/tmp/probealarm.png")) {
						try {
							System.IO.File.Copy("probealarm.png","/tmp/probealarm.png",true);
						} catch (Exception ex) {
							FeuerwehrCloud.Helper.Logger.WriteLine("[Probealarm] "+FeuerwehrCloud.Helper.Helper.GetExceptionDescription(ex));

						}
					}

					// Notify running Display-Software!
					new System.Threading.Thread(delegate() {
						try {
							System.Net.Sockets.TcpClient TC = new TcpClient();
							TC.Connect("localhost",27655);
							TC.GetStream().Write(System.Text.Encoding.Default.GetBytes("GET / HTTP/1.0\r\n\r\n"),0,18);
							TC.Close();
						} catch (Exception ex) {
							FeuerwehrCloud.Helper.Logger.WriteLine("[Probealarm] "+FeuerwehrCloud.Helper.Helper.GetExceptionDescription(ex));

						}
					}).Start();

					try {
						S.BeginReceive(buffer, dataRecieved, DEFAULT_SIZE - dataRecieved,
							System.Net.Sockets.SocketFlags.None, new AsyncCallback(HandleAsyncCallback), S);
					} catch (Exception ex) {
						FeuerwehrCloud.Helper.Logger.WriteLine(FeuerwehrCloud.Helper.Helper.GetExceptionDescription(ex));


					}
				} while (true);				
			} catch (Exception ex) {
				if(ex.Message.Contains("already in")) {
					FeuerwehrCloud.Helper.Logger.WriteLine("Kann FeuerwehrCloud-Server Probealarm-Modul nicht starten!");
				}

			}


		}

		public void RaiseFinish(params object[] list) {
			PluginEvent messageSent = Event;
			if (messageSent != null)
			{
				messageSent(this, list);

			}
		}
			
		void HandleAsyncCallback (IAsyncResult ar)
		{
			try {
				Socket socket = ar.AsyncState as Socket;
				dataRecieved = socket.EndReceive(ar);
				string output = Encoding.ASCII.GetString(buffer);
				FeuerwehrCloud.Helper.Logger.WriteLine ("|  < [Probealarm] *** Received Probealarm-request");
				if(!System.IO.File.Exists("/tmp/probealarm.png")) {
					System.IO.File.Copy("./probealarm.png","/tmp/probealarm.png");
				}
				FeuerwehrCloud.Helper.Helper.exec("/usr/bin/lpr","/tmp/probealarm.png -#"+CopiesConfig["count"]);
				string[] P = {"probealarm.png"};
				RaiseFinish("pictures", P);

			} catch (Exception ex) {
				FeuerwehrCloud.Helper.Logger.WriteLine(FeuerwehrCloud.Helper.Helper.GetExceptionDescription(ex));
			}
		}
	}
}

