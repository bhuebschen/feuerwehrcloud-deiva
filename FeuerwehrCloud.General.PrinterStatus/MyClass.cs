using System;
using FeuerwehrCloud.Plugin;
using System.IO;
using System.Reflection;

namespace FeuerwehrCloud.General
{
	public class PrinterStatus : Plugin.IPlugin
	{
		#region IPlugin implementation

		public event PluginEvent Event;
		private static System.Collections.Generic.Dictionary<string, string> PrinterStatusConfig = new System.Collections.Generic.Dictionary<string, string> ();
		static System.Threading.Timer WSThread;
		private static System.Threading.ManualResetEvent ThreadExitEvent = new System.Threading.ManualResetEvent(false);
		private FeuerwehrCloud.Plugin.IHost My;
		private string Template = "";
		private System.Collections.Generic.Dictionary<string, string> SMTPConfig = new System.Collections.Generic.Dictionary<string, string> ();
		FileSystemWatcher watcher;

		public string StatusToText(string Status) {
			switch (Status) {
			case "40005":
				Template = "toner.html";
				return "Fehler beim Ausrichten der Tonerkassette";
			case "40010":
				Template = "toner.html";
				return "Tonerkassette nicht geladen";
			case "40019":
				return "REMOVE PAPER";
			case "40020":
				Template = "toner.html";
				return " NO MICR TONER or INSTALL MICR TONER CARTRIDGE";
			case "40022":
				Template = "papier.html";
				return "Papierstau oder Papierfach leer";
			case "10006":
				Template = "toner.html";
				return "Wenig Toner";
			case "40038":
				Template = "toner.html";
				return "Kein Toner";
			case "40050":
			case "40051":
			case "40052": 
			case "40053":
			case "40054":
			case "40055":
			case "40056":
			case "40057":
			case "40058":
			case "40059":
			case "40060":
			case "40061":
			case "40062": 
			case "40063":
			case "40064":
			case "40065":
			case "40066":
			case "40067":
			case "40068":
			case "40069":
			case "40070":
			case "40071":
			case "40092":
				return "FEHLER - BITTE DEN DRUCKER AUS- UND WIEDER EINSCHALTEN";
			case "40079":
				return "Drucker wurde manuell in den OFFLINE-Modus versetzt";
			case "40080":
				return "EE oder LC Imkompatibel";
			case "40090":
				Template = "toner.html";
				return "Inkompatible Entwicklereinheit installiert";
			case "40093":
				return "Papierstau in der Duplexeinheit";
			case "40096":
				Template = "papier.html";
				return "Falsche Papiergröße";
			case "40102":
				Template = "toner.html";
				return "Ausrichtungsfehler beim FINISHER";
			case "40103":
				Template = "toner.html";
				return "FINISHER hat sein Limit erreicht";
			case "40104":
				return "Papierfach offen";
			case "40105":
				return "Ablage offen";
			case "40120":
			case "40121":
			case "40021":
				return "Gehäuse offen";
			case "40122":
				return "Duplexeinheit erforderlich";
			case "40123":
				return "Fehler in der Duplexeinheit - Bitte entfernen";
			case "40124":
				return "Fehler in der Verbindung zur Duplexeinheit";
			case "40128":
				Template = "toner.html";
				return "Trommelfehler - Trommel ersetzen";
			case "40129":
				Template = "toner.html";
				return "Trommel hat sein Limit erreicht";
			case "40130":
				Template = "toner.html";
				return "Trommellebensdauer gering";
			case "40131":
				Template = "toner.html";
				return "Transfereinheit hat sein Limit erreicht";
			case "40132":
				Template = "toner.html";
				return "Lebensdauer der Transfereinheit gering";
			case "40141":
				Template = "toner.html";
				return "Tonersammelbehälter voll";
			case "40142":
				Template = "toner.html";
				return "Trommeleinheit fehlt";
			case "40177":
				return "CSI Hardwarefehler";
			case "40178":
				return "CSI Fehler";
			case "40194":
				return "Dekompressionsfehler";
			case "40205":
				return "Controller Hardwarefehler";
			case "40206":
				return "Controller Softwarefehler";
			case "40218":
				return "Nicht genügend Rasterspeicher";
			case "40220":
				return "Nicht genügend E/A-Speicher";
			case "40223":
				Template = "papier.html";
				return "Falscher Papiertyp geladen";
			case "50000":
				Template = "papier.html";
				return "Genereller Hardwarefehler / evtl. kein Papier";
			case "50001":
				return "Hardwarefehler - ROM ERROR!";
			case "50002":
				return "Hardwarefehler - RAM ERROR";
			case "50007":
				return "Hardwarefehler - Fehler bei der Kommunikation mit dem Motor";
			case "50026":
				return "Hardwarefehler - GERÄTEFEHLER";
			default:
				if (Status.StartsWith ("41") || Status.StartsWith ("43") ) {
					Template = "papier.html";
					return "Kein Papier";
				}
				return "";
				break;
			}
		}

		static string WritePJL(string Type, string Address, string Command) {
			System.IO.Stream T = null;
			try {
				System.Net.Sockets.TcpClient TC;
				if(Type == "usb") {
					T = System.IO.File.Open (Address, System.IO.FileMode.Open, System.IO.FileAccess.ReadWrite,System.IO.FileShare.ReadWrite);
				} else if(Type == "serial") {
					T = System.IO.File.Open (Address, System.IO.FileMode.Open, System.IO.FileAccess.ReadWrite,System.IO.FileShare.ReadWrite);
				} else if(Type == "parallel") {
					T = System.IO.File.Open (Address, System.IO.FileMode.Open, System.IO.FileAccess.ReadWrite,System.IO.FileShare.ReadWrite);
				} else if(Type == "ethernet") {
					TC = new System.Net.Sockets.TcpClient ();
					TC.Connect(Address, 9100);
					T = TC.GetStream();
				}
				byte[] b = System.Text.Encoding.Default.GetBytes (Command);
				T.Write (b, 0, b.Length);
				byte[] b2 = new byte[1024]; int xLen = T.Read(b2, 0, 1024);
				T.Close();
				return System.Text.Encoding.Default.GetString(b2).Substring(0,xLen);

			} catch (Exception ex) {
				if (T != null)
					T.Close (); 
				return "";
				// TODO: Fehler-E-Mail wenn der Drucker nicht erreichbar war 
			}
		}

		public bool Initialize (IHost hostApplication)
		{
			My = hostApplication;
			if(!System.IO.File.Exists("printerstatus.cfg")) {
				PrinterStatusConfig.Add ("email", "CHANGEME");
				PrinterStatusConfig.Add ("interval", "CHANGEME");
				PrinterStatusConfig.Add ("lastnotificated", "0");
				PrinterStatusConfig.Add ("printers", "1");
				PrinterStatusConfig.Add ("printertype1", "CHANGEME");
				PrinterStatusConfig.Add ("address1", "CHANGEME");
				FeuerwehrCloud.Helper.AppSettings.Save(PrinterStatusConfig,"printerstatus.cfg");
			} 
			if(!System.IO.File.Exists("smtp.cfg")) {
				SMTPConfig.Add ("host", "");
				SMTPConfig.Add ("ssl", "");
				SMTPConfig.Add ("username", "");
				SMTPConfig.Add ("password", "");
				SMTPConfig.Add ("port", "");
				FeuerwehrCloud.Helper.AppSettings.Save(SMTPConfig,"smtp.cfg");
			} 
			SMTPConfig = FeuerwehrCloud.Helper.AppSettings.Load ("smtp.cfg");
			PrinterStatusConfig = FeuerwehrCloud.Helper.AppSettings.Load ("printerstatus.cfg");


			watcher = new FileSystemWatcher ();
			watcher.Path = System.Environment.CurrentDirectory;
			watcher.IncludeSubdirectories = false;
			watcher.Filter = "smtp.cfg;printerstatus.cfg";
			watcher.NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.LastAccess | NotifyFilters.DirectoryName | NotifyFilters.FileName;
			watcher.Changed += new FileSystemEventHandler(delegate(object sender, FileSystemEventArgs e) {
				SMTPConfig = FeuerwehrCloud.Helper.AppSettings.Load ("smtp.cfg");
				PrinterStatusConfig = FeuerwehrCloud.Helper.AppSettings.Load ("printerstatus.cfg");
			});
			watcher.EnableRaisingEvents = true;

			FeuerwehrCloud.Helper.Logger.WriteLine ("|  *** PrinterStatus loaded...");
			FeuerwehrCloud.Helper.Logger.WriteLine ("|  = [PrinterStatus] active for:");
			for (int i=1; i<=int.Parse(PrinterStatusConfig["printers"]);i++) {
				FeuerwehrCloud.Helper.Logger.WriteLine ("|  = [PrinterStatus]     "+PrinterStatusConfig["printertype"+i.ToString()]+": "+PrinterStatusConfig["address"+i.ToString()]);
			}
			WSThread = new System.Threading.Timer(delegate(object state) {
				System.Diagnostics.Debug.WriteLine(">> PrinterStatus THREAD");
				if(Helper.Helper.exec("/usr/bin/lpq","").IndexOf("no entries")>-1) {
					for (int i=1; i<=int.Parse(PrinterStatusConfig["printers"]);i++) {
						try {
							string StatusText = WritePJL(((string)PrinterStatusConfig["printertype"+i.ToString()]).ToLower(),((string)PrinterStatusConfig["address"+i.ToString()]),String.Format ("\x1B%-12345X@PJL INFO STATUS \r\n\x1B%-12345X\r\n"));
							//string PAGECOUNT = WritePJL(((string)PrinterStatusConfig["printertype"+i.ToString()]).ToLower(),((string)PrinterStatusConfig["address"+i.ToString()]),String.Format ("\x1B%-12345X@PJL INFO PAGECOUNT \r\n\x1B%-12345X\r\n"));
							//string SERIAL = WritePJL(((string)PrinterStatusConfig["printertype"+i.ToString()]).ToLower(),((string)PrinterStatusConfig["address"+i.ToString()]),String.Format ("\x1B%-12345X@PJL INQUIRE SERIALNUMBER \r\n\x1B%-12345X\r\n"));
							string ID = WritePJL(((string)PrinterStatusConfig["printertype"+i.ToString()]).ToLower(),((string)PrinterStatusConfig["address"+i.ToString()]),String.Format ("\x1B%-12345X@PJL INFO ID \r\n\x1B%-12345X\r\n"));
							//string SELFTEXT1 = WritePJL(((string)PrinterStatusConfig["printertype"+i.ToString()]).ToLower(),((string)PrinterStatusConfig["address"+i.ToString()]),String.Format ("\x1B%-12345X@PJL INFO VARIABLES \r\n\x1B%-12345X\r\n"));
							//string SELFTEXT2 = WritePJL(((string)PrinterStatusConfig["printertype"+i.ToString()]).ToLower(),((string)PrinterStatusConfig["address"+i.ToString()]),String.Format ("\x1B%-12345X@PJL INFO USTATUS \r\n\x1B%-12345X\r\n"));
							//string SELFTEXT3 = WritePJL(((string)PrinterStatusConfig["printertype"+i.ToString()]).ToLower(),((string)PrinterStatusConfig["address"+i.ToString()]),String.Format ("\x1B%-12345X@PJL INQUIRE RESOLUTION \r\n\x1B%-12345X\r\n"));
							//string SELFTEXT4 = WritePJL(((string)PrinterStatusConfig["printertype"+i.ToString()]).ToLower(),((string)PrinterStatusConfig["address"+i.ToString()]),String.Format ("\x1B%-12345X@PJL INFO PRODINFO \r\n\x1B%-12345X\r\n"));
							//string SELFTEXT5 = WritePJL(((string)PrinterStatusConfig["printertype"+i.ToString()]).ToLower(),((string)PrinterStatusConfig["address"+i.ToString()]),String.Format ("\x1B%-12345X@PJL INFO FILESYS \r\n\x1B%-12345X\r\n"));
							//string SELFTEXT6 = WritePJL(((string)PrinterStatusConfig["printertype"+i.ToString()]).ToLower(),((string)PrinterStatusConfig["address"+i.ToString()]),String.Format ("\x1BZ\r\n\x1BE\r\n"));
							//StatusText = "CODE=41038\nDISPLAY=ONLINE\nONLINE=TRUE\n";
							string XCode = "";
							if(StatusText.IndexOf("CODE=")>-1) {
								XCode = StatusText.Substring(StatusText.IndexOf("CODE=")+5);
								XCode = XCode.Substring(0,XCode.IndexOf("\n")).Replace("\r","");
								if(StatusToText(XCode)!="") {
									if(Int32.Parse(PrinterStatusConfig["lastnotificated"+i.ToString()])+Int32.Parse(PrinterStatusConfig["interval"])<(Int32)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds) {
										PrinterStatusConfig.Remove("lastnotificated"+i.ToString());
										PrinterStatusConfig.Add("lastnotificated"+i.ToString(),((Int32)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds).ToString());
										FeuerwehrCloud.Helper.AppSettings.Save(PrinterStatusConfig,"printerstatus.cfg");

										System.Net.Mail.SmtpClient SMTP = new System.Net.Mail.SmtpClient ();
										SMTP.Host = SMTPConfig ["host"];
										SMTP.Port = (SMTPConfig ["port"]!=""? int.Parse(SMTPConfig ["port"]):25);
										SMTP.EnableSsl = bool.Parse (SMTPConfig ["ssl"]);
										SMTP.Credentials = new System.Net.NetworkCredential (SMTPConfig ["username"], SMTPConfig ["password"]);
										System.Net.Mail.MailMessage Msg = new System.Net.Mail.MailMessage ("printeralert@"+System.Environment.MachineName+".feuerwehrcloud.de", PrinterStatusConfig["email"]);
										if(Template != "") {
											Msg.IsBodyHtml = true;
											Msg.Body = System.IO.File.ReadAllText("plugins/"+Template).Replace("%STATUS%",StatusToText(XCode));
										} else {
											Msg.Body = StatusToText(XCode);
										}
										Msg.Subject = "PRINTER IS IN ALERT STATE";
										SMTP.Send (Msg);

									}
								}
							}
							if(ID.IndexOf("\n")>-1) {
								ID=ID.Substring(ID.IndexOf("\n")+2).Trim().Replace("\0","");
								if(ID.StartsWith("\"")) ID=ID.Substring(1);
								if(ID.EndsWith("\"")) ID=ID.Substring(0,ID.Length-1);
							} else {
								ID="Generic Printer";
							}
							using (System.Net.WebClient wc = new System.Net.WebClient())
							{
								wc.Headers[System.Net.HttpRequestHeader.ContentType] = "application/x-www-form-urlencoded";
								string HtmlResult = wc.UploadString(new Uri("http://www.feuerwehrcloud.de/deiva/printerstate.php"), ("HID="+System.Environment.MachineName+"&ptype="+((string)PrinterStatusConfig["printertype"+i.ToString()]).ToLower()+"&paddr="+((string)PrinterStatusConfig["address"+i.ToString()])+"&model="+ID+"&error="+XCode));

							}

						} catch (Exception ex) {
							FeuerwehrCloud.Helper.Logger.WriteLine(FeuerwehrCloud.Helper.Helper.GetExceptionDescription(ex));
						}
					}
				}
			});
			WSThread.Change (0, 600*1000);
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
				return ServiceType.general;
			}
		}

		public string Name {
			get {
				return "PrinterStatus";
			}
		}

		public string FriendlyName {
			get {
				return "Druckerstatusüberwachung";
			}
		}

		public Guid GUID {
			get {
				return new Guid (Name);
			}
		}

		public byte[] Icon {
			get {
				var assembly = typeof(FeuerwehrCloud.General.PrinterStatus).GetTypeInfo().Assembly;
				string[] resources = assembly.GetManifestResourceNames();
				Stream stream = assembly.GetManifestResourceStream("icon.ico");
				return ((MemoryStream)stream).ToArray();
			}
		}

		#endregion

		#region IDisposable implementation

		public void Dispose ()
		{
			try {
				WSThread.Dispose();
			} catch (Exception ex) {
				ex.ToString ();
			}
		}

		#endregion
		public PrinterStatus ()
		{

		}
	}
}

