using System;
using FeuerwehrCloud.Plugin;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Reflection;

namespace FeuerwehrCloud.Input
{
	public class Fax : FeuerwehrCloud.Plugin.IPlugin
	{
		private FeuerwehrCloud.Plugin.IHost My;
		public bool IsTerminating = false;
		public event PluginEvent Event;
		private System.Collections.Generic.Dictionary<string, string> FAXConfig = new System.Collections.Generic.Dictionary<string, string> ();
		private System.Collections.Generic.Dictionary<string, string> CopiesConfig = new System.Collections.Generic.Dictionary<string, string> ();
		System.IO.FileSystemWatcher FSW;
		static System.Net.Sockets.TcpListener TCL;
		System.Threading.Thread ListenSockThread;
		static int DEFAULT_SIZE = 1024;           //size of receive buffer
		static byte[] buffer = new byte[DEFAULT_SIZE];  
		static int dataRecieved = 0;

		public string Name {
			get {
				return "Fax";
			}
		}
		public string FriendlyName {
			get {
				return "Faxeingangsmodul";
			}
		}

		public Guid GUID {
			get {
				return new Guid (Name);
			}
		}

		public byte[] Icon {
			get {
				var assembly = typeof(FeuerwehrCloud.Input.Fax).GetTypeInfo().Assembly;
				string[] resources = assembly.GetManifestResourceNames();
				Stream stream = assembly.GetManifestResourceStream("icon.ico");
				return ((MemoryStream)stream).ToArray();
			}
		}

		public void Dispose() {
			FeuerwehrCloud.Helper.Logger.WriteLine ("|  *** Terminating FAX-Plugin");
			IsTerminating = true;
			try {
				TCL.Stop();
			} catch (Exception ex) {
				
			}
			try {
				ListenSockThread.Abort ();
			} catch (Exception ex) {
				
			}
		}

		public void RaiseFinish(params object[] list) {
			PluginEvent messageSent = Event;
			if (messageSent != null)
			{
				messageSent(this, list);

			}
		}

		public bool IsAsync
		{
			get { return true; }
		}

		public ServiceType ServiceType
		{
			get { return FeuerwehrCloud.Plugin.ServiceType.input; }
		}

		public void Execute(params object[] list) {
		}

		public bool Initialize(IHost hostApplication) {

			My = hostApplication;

			if(!System.IO.File.Exists("ILS.cfg")) {
				FAXConfig.Add ("Target", "");
				FeuerwehrCloud.Helper.AppSettings.Save(FAXConfig,"ILS.cfg");
			} 
			FAXConfig = FeuerwehrCloud.Helper.AppSettings.Load ("ILS.cfg");
			if(!System.IO.File.Exists("copies.cfg")) {
				CopiesConfig.Add ("count", "1");
				FeuerwehrCloud.Helper.AppSettings.Save(CopiesConfig,"copies.cfg");
			} 
			CopiesConfig = FeuerwehrCloud.Helper.AppSettings.Load ("copies.cfg");

			FeuerwehrCloud.Helper.Logger.WriteLine ("|  < [FAX] *** Waiting for Fax from:");
			foreach (var item in FAXConfig.Values) {
				FeuerwehrCloud.Helper.Logger.WriteLine ("|  < [FAX]     " + item);
			}

			//System.Environment.MachineName + 
			ListenSockThread = new System.Threading.Thread(new System.Threading.ThreadStart(HandleThreadStart));
			ListenSockThread.Start ();
			return true;
		}
			


		public Fax () : base ()
		{
			FeuerwehrCloud.Helper.Logger.WriteLine ("|  *** Loading FAX-Plugin");
		}

		void HandleThreadStart ()
		{
			try {
				TCL = new System.Net.Sockets.TcpListener(new System.Net.IPEndPoint(System.Net.IPAddress.Any,10112));
				TCL.Start ();
				do {
					try {
						System.Net.Sockets.Socket S =  TCL.AcceptSocket();
						FeuerwehrCloud.Helper.Logger.WriteLine ("|  < [FAX] *** Incoming notification");
						S.Blocking = false;
						S.BeginReceive(buffer, 0, DEFAULT_SIZE - dataRecieved,
							System.Net.Sockets.SocketFlags.None, new AsyncCallback(HandleAsyncCallback), S);
						if(IsTerminating) break;

					} catch (Exception e2x) {

					}
				} while (true);

			} catch (Exception ex) {
				if (ex.Message.Contains("already in use")) {
					FeuerwehrCloud.Helper.Logger.WriteLine("Kann FeuerwehrCloud-Server FaxModul nicht starten!");
				}
			}
		}

		void HandleAsyncCallback (IAsyncResult ar)
		{
			Socket socket = ar.AsyncState as Socket;
			dataRecieved = socket.EndReceive(ar);
			(new System.Threading.Thread(delegate() {
				string output = Encoding.ASCII.GetString(buffer);
				buffer = new byte[DEFAULT_SIZE];  
				FeuerwehrCloud.Helper.Logger.WriteLine ("|  < [FAX] *** Received FAX from Hylafax");
				string[] Pages = output.Split(new [] {"|"}, StringSplitOptions.RemoveEmptyEntries);
				if (Pages == null) {
				} else {
					FeuerwehrCloud.Helper.Logger.WriteLine ("|  < [FAX] ***** Is FAX-Number from ILS? " + (Pages[0]==""?"No number supplied...":Pages[0]));
				}
				if (Pages == null || FAXConfig.ContainsValue (Pages [0]) ) {
					FeuerwehrCloud.Helper.Logger.WriteLine ("|  < [FAX] ******* ILS-Number detected!");
					if (Pages [1].IndexOf ("\\") > -1) {
						Pages [1] = Pages [1].Substring (0, Pages [1].IndexOf ("\\"));
					}
					if (Pages [1].IndexOf ("\n") > -1) {
						Pages [1] = Pages [1].Substring (0, Pages [1].IndexOf ("\n"));
					}
					if (Pages [1].IndexOf ("\r") > -1) {
						Pages [1] = Pages [1].Substring (0, Pages [1].IndexOf ("\r"));
					}
					FeuerwehrCloud.Helper.Logger.WriteLine ("|  < [FAX] ******* Printing "+CopiesConfig["count"]+" copies ");
					string TF = System.IO.Path.GetTempFileName ();
					System.IO.File.WriteAllText(TF,"/usr/bin/tiff2ps -a -h 12 -w 7.7 "+"/var/spool/hylafax/" + Pages [1]+" | lpr  -o orientation-requested=3 -o position=top-left -#"+CopiesConfig["count"]);
					FeuerwehrCloud.Helper.Helper.exec ("/bin/bash", " "+TF);
					System.IO.File.Delete (TF);
					FeuerwehrCloud.Helper.Logger.WriteLine ("|  < [FAX] ******* Safe TIFF to " + "/tmp/" + Pages [1].Replace ("recvq/", ""));
					System.IO.File.Copy ("/var/spool/hylafax/" + Pages [1], "/tmp/" + Pages [1].Replace ("recvq/", ""), true);
					if (!System.IO.Directory.Exists ("public/fax")) {
						System.IO.Directory.CreateDirectory ("public/fax");
					}
					System.IO.File.Copy ("/var/spool/hylafax/" + Pages [1], "public/fax/" + Pages [1].Replace ("recvq/", ""), true);
					string[] P = { Pages [1].Replace ("recvq/", "") };
					RaiseFinish ("pictures", P);
					FeuerwehrCloud.Helper.Logger.WriteLine ("|  < [FAX] *** Done!");
				} else {

					FeuerwehrCloud.Helper.Logger.WriteLine ("|  < [FAX] ******* Does not look like a FAX from ILS...");
					if (Pages [1].IndexOf ("\\") > -1) {
						Pages [1] = Pages [1].Substring (0, Pages [1].IndexOf ("\\"));
					}
					if (Pages [1].IndexOf ("\n") > -1) {
						Pages [1] = Pages [1].Substring (0, Pages [1].IndexOf ("\n"));
					}
					if (Pages [1].IndexOf ("\r") > -1) {
						Pages [1] = Pages [1].Substring (0, Pages [1].IndexOf ("\r"));
					}
					FeuerwehrCloud.Helper.Logger.WriteLine ("|  < [FAX] ******* Printing 1 copy ");
					string TF = System.IO.Path.GetTempFileName ();
					System.IO.File.WriteAllText(TF,"/usr/bin/tiff2ps -a -h 12 -w 7.7 "+"/var/spool/hylafax/" + Pages [1]+" | lpr  -o orientation-requested=3 -o position=top-left");
					FeuerwehrCloud.Helper.Helper.exec ("/bin/bash", " "+TF);
					System.IO.File.Delete (TF);
				}				
			})).Start();
			try {				
				socket.Shutdown(SocketShutdown.Both);
				socket.Close();
			} catch (Exception ex) {
				
			}
		}
	}


}

