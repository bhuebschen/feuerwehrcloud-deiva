using System;
using FeuerwehrCloud;
using System.Net.Sockets;
using FeuerwehrCloud.Plugin;
using System.Text;

namespace FeuerwehrCloud.Input
{
	public class Demo : FeuerwehrCloud.Plugin.IPlugin
	{
		#region IPlugin implementation

		private FeuerwehrCloud.Plugin.IHost My;
		public static bool Disposing = false;
		static System.Net.Sockets.TcpListener TCL;
		System.Threading.Thread ListenSockThread;
		static int DEFAULT_SIZE = 1024;           //size of receive buffer
		static byte[] buffer = new byte[DEFAULT_SIZE];  
		static int dataRecieved = 0;
		private System.Collections.Generic.Dictionary<string, string> FAXConfig = new System.Collections.Generic.Dictionary<string, string> ();
		private System.Collections.Generic.Dictionary<string, string> CopiesConfig = new System.Collections.Generic.Dictionary<string, string> ();
		public event PluginEvent Event;

		public bool Initialize(FeuerwehrCloud.Plugin.IHost hostApplication)
		{
			ListenSockThread = new System.Threading.Thread(new System.Threading.ThreadStart(HandleThreadStart));
			ListenSockThread.Start ();

			return true;
		}


		void HandleThreadStart ()
		{
			try {
				TCL = new System.Net.Sockets.TcpListener(new System.Net.IPEndPoint(System.Net.IPAddress.Any,30112));
				TCL.Start ();
				do {
					System.Net.Sockets.Socket S =  TCL.AcceptSocket();
					FeuerwehrCloud.Helper.Logger.WriteLine ("|  < [DEMO-Alarm] *** Incoming DEMO-Alarm");

					try {
						S.BeginReceive(buffer, dataRecieved, DEFAULT_SIZE - dataRecieved,
							System.Net.Sockets.SocketFlags.None, new AsyncCallback(HandleAsyncCallback), S);
					} catch (Exception ex) {
						FeuerwehrCloud.Helper.Logger.WriteLine(FeuerwehrCloud.Helper.Helper.GetExceptionDescription(ex));


					}
				} while (true);

			} catch (Exception ex) {
				if (ex.Message.Contains("already in")) {
					FeuerwehrCloud.Helper.Logger.WriteLine("Kann DEMO-Port nicht binden - Läuft der FeuerwehrCloud-Server bereits?");
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
				try {
					socket.Shutdown(SocketShutdown.Both);
					socket.Close();
				} catch (Exception ex) {
					
				}
				string FX = Encoding.ASCII.GetString(buffer);
				buffer = new byte[DEFAULT_SIZE]; 
				FX = FX.Replace("\r","").Replace("\n","");
				FX = FX.Replace("\x0","");
				string output = System.IO.File.ReadAllText("public/"+FX+".txt");
				FeuerwehrCloud.Helper.Logger.WriteLine ("|  < [DEMO-Alarm] *** Received DEMO-Alarm-request");
				RaiseFinish("text", new string[]{output});

			} catch (Exception ex) {
				FeuerwehrCloud.Helper.Logger.WriteLine(FeuerwehrCloud.Helper.Helper.GetExceptionDescription(ex));
			}
		}


		public void Execute(params object[] list)
		{
			
		}

		public bool IsAsync {
			get {
				return true;
			}
		}

		public FeuerwehrCloud.Plugin.ServiceType ServiceType {
			get {
				return ServiceType.input;
			}
		}

		public string Name {
			get {
				return "DEMO-Alarm";
			}
		}

		public string FriendlyName {
			get {
				return Name;
			}
		}

		public Guid GUID {
			get {
				return new Guid(Name);
			}
		}

		public byte[] Icon {
			get {
				return new byte[] { };
			}
		}

		#endregion

		#region IDisposable implementation

		public void Dispose()
		{
			
		}

		#endregion

		public Demo()
		{
		}
	}
}

