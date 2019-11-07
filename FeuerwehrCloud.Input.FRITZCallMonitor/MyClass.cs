using System;
using System.Net;
using System.Net.NetworkInformation;
using FeuerwehrCloud.Plugin;
using System.Net.Sockets;
using System.Text;
using System.IO;
using System.Reflection;

namespace FeuerwehrCloud.Input
{
	public class FRITZCallMonitor: IPlugin
	{
		#region IPlugin implementation
		private FeuerwehrCloud.Plugin.IHost My;


		public string Name {
			get {
				return "FRITZCallMonitor";
			}
		}
		public string FriendlyName {
			get {
				return "FRITZ!Box Anrufmonitor";
			}
		}

		public Guid GUID {
			get {
				return new Guid (Name);
			}
		}

		public byte[] Icon {
			get {
				var assembly = typeof(FeuerwehrCloud.Input.FRITZCallMonitor).GetTypeInfo().Assembly;
				string[] resources = assembly.GetManifestResourceNames();
				Stream stream = assembly.GetManifestResourceStream("icon.ico");
				return ((MemoryStream)stream).ToArray();
			}
		}


		public event PluginEvent Event;
		public static System.Net.Sockets.TcpClient FBC;
		public static IPAddress Gw;
		public static bool Disposing = false;
		public System.Threading.Thread M;
		public static System.Collections.Generic.Dictionary<string, string> BOSConfig = new System.Collections.Generic.Dictionary<string, string> ();

		public static IPAddress GetDefaultGateway()
		{
			foreach (var item in NetworkInterface.GetAllNetworkInterfaces ()) {
				try {
					foreach (var item2 in ((NetworkInterface)item).GetIPProperties().GatewayAddresses) {
						if(item2!=null) {
							return item2.Address;
						}
					}
				} catch (Exception ex) {
					
				}
			}

			return null;

		}

		public void RaiseFinish(params object[] list) {
			PluginEvent messageSent = Event;
			if (messageSent != null)
			{
				messageSent(this, list);

			}
		}

		private static async void ConnectAsTcpClient(object iHost)
		{
			try {
				System.Net.WebClient WC = new WebClient ();
				string RESULT = WC.DownloadString ("http://" + FRITZCallMonitor.Gw.ToString ());
				if (RESULT.IndexOf ("FRITZ!Box", StringComparison.OrdinalIgnoreCase) > 0) {
					FBC = new System.Net.Sockets.TcpClient ();
					FRITZCallMonitor iH = (FRITZCallMonitor)iHost;
					using (var tcpClient = new TcpClient ()) {
						FeuerwehrCloud.Helper.Logger.WriteLine ("|  < [FRITZCallMonitor] *** FRITZ!Box found at " + FRITZCallMonitor.Gw.ToString () + ", trying to attach to CallMonitor");
						await tcpClient.ConnectAsync (FRITZCallMonitor.Gw, 1012);
						FeuerwehrCloud.Helper.Logger.WriteLine ("|  < [FRITZCallMonitor] *** CallMonitor attached, waiting for calls...");
						using (var networkStream = tcpClient.GetStream ()) {
							do {
								var buffer = new byte[4096];
								var byteCount = await networkStream.ReadAsync (buffer, 0, buffer.Length);
								var response = Encoding.UTF8.GetString (buffer, 0, byteCount);
								string[] rLine = response.Split (new [] { ";" }, StringSplitOptions.None);
								switch (rLine [1]) {
								case "CALL":
									FeuerwehrCloud.Helper.Logger.WriteLine ("|  < [FRITZCallMonitor] *** Outgoing call with " + rLine [3] + "/" + rLine [4] + " to: " + rLine [5]);
									iH.RaiseFinish("text", rLine);
									break;
								case "RING":
									FeuerwehrCloud.Helper.Logger.WriteLine ("|  < [FRITZCallMonitor] *** Incoming call from: " + rLine [3] + " on number " + rLine [4]);
									if(BOSConfig.ContainsValue(rLine [3])) {
										FeuerwehrCloud.Helper.Logger.WriteLine ("|  < [FRITZCallMonitor] *** Incoming call FAX FROM ILS! Forcing printer tp warm up...");
										try {
											// There's a new Method wo wake up the printer -> FeuerwehrCloud.Generic.PrinterStatus
											//System.Diagnostics.Process.Start("/usr/bin/lp wakeup.txt");
										} catch (Exception ex) {
											
										}
									}
									break;
									iH.RaiseFinish("text", rLine);
								case "CONNECT":
									FeuerwehrCloud.Helper.Logger.WriteLine ("|  < [FRITZCallMonitor] *** Pickup call: " + rLine [4]);
									iH.RaiseFinish("text", rLine);
									break;
								case "DISCONNECT":
									FeuerwehrCloud.Helper.Logger.WriteLine ("|  < [FRITZCallMonitor] *** Hangup call after " + rLine [3]);
									iH.RaiseFinish("text", rLine);
									break;
								default:
									FeuerwehrCloud.Helper.Logger.WriteLine ("|  < [FRITZCallMonitor] *** response was {0}", response);
									break;
								}						
							} while (Disposing == false);
						}
					}
				}	
			} catch (Exception ex) {
				
			}
		}

		public bool Initialize (IHost hostApplication)
		{
			My = hostApplication;
			FeuerwehrCloud.Helper.Logger.WriteLine ("|  *** FRITZCallMonitor loaded...");
			FeuerwehrCloud.Helper.Logger.WriteLine ("|  < [FRITZCallMonitor] *** Searching for AVM FRITZ!Box...");
			if(!System.IO.File.Exists("ILS.cfg")) {
				BOSConfig.Add ("Target", "");
				FeuerwehrCloud.Helper.AppSettings.Save(BOSConfig,"ILS.cfg");
			} 
			BOSConfig = FeuerwehrCloud.Helper.AppSettings.Load ("ILS.cfg");

			FRITZCallMonitor.Gw = FRITZCallMonitor.GetDefaultGateway ();
			if (FRITZCallMonitor.Gw == null)
				FRITZCallMonitor.Gw = IPAddress.Parse ("192.168.172.1");
			try {
				M = new System.Threading.Thread ((object obj) => ConnectAsTcpClient (obj));
				M.Start(this);
				return true;
			} catch (Exception ex) {
				FeuerwehrCloud.Helper.Logger.WriteLine ("|  < [FRITZCallMonitor] *** Connection to FRITZ!Box timed out...");
				return false;
			}
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
			M.Abort ();
			Disposing = true;
		}

		#endregion

		public FRITZCallMonitor ()
		{
		}
	}
}