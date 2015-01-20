using System;
using System.Net;
using System.Net.NetworkInformation;
using FeuerwehrCloud.Plugin;
using System.Net.Sockets;
using System.Text;

namespace FeuerwehrCloud.Input.FRITZCallMonitor
{
	public class FRITZCallMonitor: IPlugin
	{
		#region IPlugin implementation
		private IHost My;

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

		private static async void ConnectAsTcpClient()
		{
			try {
				System.Net.WebClient WC = new WebClient ();
				string RESULT = WC.DownloadString ("http://" + FRITZCallMonitor.Gw.ToString ());
				if (RESULT.IndexOf ("FRITZ!Box", StringComparison.OrdinalIgnoreCase) > 0) {
					FBC = new System.Net.Sockets.TcpClient ();

					using (var tcpClient = new TcpClient ()) {
						de.SYStemiya.Helper.Logger.WriteLine ("| [" + System.DateTime.Now.ToString ("T") + "] |-< [FRITZCallMonitor] *** FRITZ!Box found at " + FRITZCallMonitor.Gw.ToString () + ", trying to attach to CallMonitor");
						await tcpClient.ConnectAsync (FRITZCallMonitor.Gw, 1012);
						de.SYStemiya.Helper.Logger.WriteLine ("| [" + System.DateTime.Now.ToString ("T") + "] |-< [FRITZCallMonitor] *** CallMonitor attached, waiting for calls...");
						using (var networkStream = tcpClient.GetStream ()) {
							do {
								var buffer = new byte[4096];
								var byteCount = await networkStream.ReadAsync (buffer, 0, buffer.Length);
								var response = Encoding.UTF8.GetString (buffer, 0, byteCount);
								string[] rLine = response.Split (new [] { ";" }, StringSplitOptions.None);
								switch (rLine [1]) {
								case "CALL":
									de.SYStemiya.Helper.Logger.WriteLine ("| [" + System.DateTime.Now.ToString ("T") + "] |-< [FRITZCallMonitor] *** Outgoing call with " + rLine [3] + "/" + rLine [4] + " to: " + rLine [5]);
									break;
								case "RING":
									de.SYStemiya.Helper.Logger.WriteLine ("| [" + System.DateTime.Now.ToString ("T") + "] |-< [FRITZCallMonitor] *** Incoming call from: " + rLine [3] + " on number " + rLine [4]);
									if(BOSConfig.ContainsValue(rLine [3])) {
										de.SYStemiya.Helper.Logger.WriteLine ("| [" + System.DateTime.Now.ToString ("T") + "] |-< [FRITZCallMonitor] *** Incoming call FAX FROM ILS! Forcing printer tp warm up...");
										try {
											System.Diagnostics.Process.Start("/usr/bin/lp wakeup.txt");
										} catch (Exception ex) {
											
										}
									}
									break;
								case "CONNECT":
									de.SYStemiya.Helper.Logger.WriteLine ("| [" + System.DateTime.Now.ToString ("T") + "] |-< [FRITZCallMonitor] *** Pickup call: " + rLine [4]);
									break;
								case "DISCONNECT":
									de.SYStemiya.Helper.Logger.WriteLine ("| [" + System.DateTime.Now.ToString ("T") + "] |-< [FRITZCallMonitor] *** Hangup call after " + rLine [3]);
									break;
								default:
									de.SYStemiya.Helper.Logger.WriteLine ("| [" + System.DateTime.Now.ToString ("T") + "] |-< [FRITZCallMonitor] *** response was {0}", response);
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
			de.SYStemiya.Helper.Logger.WriteLine ("| ["+System.DateTime.Now.ToString("T") +"] |-< [FRITZCallMonitor] *** Searching for AVM FRITZ!Box...");
			if(!System.IO.File.Exists("ILS.cfg")) {
				BOSConfig.Add ("Target", "");
				de.SYStemiya.Helper.AppSettings.Save(BOSConfig,"ILS.cfg");
			} 
			BOSConfig = de.SYStemiya.Helper.AppSettings.Load ("ILS.cfg");

			FRITZCallMonitor.Gw = FRITZCallMonitor.GetDefaultGateway ();
			if (FRITZCallMonitor.Gw == null)
				FRITZCallMonitor.Gw = IPAddress.Parse ("192.168.172.1");
			try {
				M = new System.Threading.Thread ((object obj) => ConnectAsTcpClient ());
				M.Start ();
				return true;
			} catch (Exception ex) {
				de.SYStemiya.Helper.Logger.WriteLine ("| [" + System.DateTime.Now.ToString ("T") + "] |-< [FRITZCallMonitor] *** Connection to FRITZ!Box timed out...");
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