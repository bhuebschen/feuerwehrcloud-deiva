using System;
using FeuerwehrCloud.Plugin;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Reflection;
using System.IO;

namespace FeuerwehrCloud.Input
{
	public class FMS32: Plugin.IPlugin
	{
		#region IPlugin implementation

		public event PluginEvent Event;
		public static System.Net.Sockets.TcpClient FBC;
		public static bool Disposing = false;
		public System.Threading.Thread M;
        public static System.Collections.Generic.Dictionary<string, string> FMSConfig = new System.Collections.Generic.Dictionary<string, string> ();
        FileSystemWatcher watcher;

		public bool Initialize (IHost hostApplication)
		{
			FeuerwehrCloud.Helper.Logger.WriteLine ("|  *** FMS32 plugin loaded...");
            if(!System.IO.File.Exists("fms32.cfg")) {
                FMSConfig.Add ("hostname", "localhost");
                FMSConfig.Add ("port", "9300");
                FMSConfig.Add ("zvei", "9300");
                FMSConfig.Add ("pocsag", "9300");
                FMSConfig.Add ("fms", "9300");
                FeuerwehrCloud.Helper.AppSettings.Save(FMSConfig,"fms32.cfg");
            } 
            FMSConfig = FeuerwehrCloud.Helper.AppSettings.Load ("fms32.cfg");

            watcher = new FileSystemWatcher ();
            watcher.Path = System.Environment.CurrentDirectory;
            watcher.IncludeSubdirectories = false;
            watcher.Filter = "fms32.cfg";
            watcher.NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.LastAccess | NotifyFilters.DirectoryName | NotifyFilters.FileName;
            watcher.Changed += new FileSystemEventHandler((object sender, FileSystemEventArgs e) => FMSConfig = FeuerwehrCloud.Helper.AppSettings.Load("fms32.cfg"));
            watcher.EnableRaisingEvents = true;

			try {
				M = new System.Threading.Thread ((object obj) => ConnectAsTcpClient (obj));
				M.Start(this);
				return true;
			} catch (Exception ex) {
				FeuerwehrCloud.Helper.Logger.WriteLine ("|  < [FMS32] *** Connection to FMS32 timed out...");
				return false;
			}
			return true;
		}

		private static async void ConnectAsTcpClient(object iHost)
		{
			try {
				FBC = new System.Net.Sockets.TcpClient ();
				FMS32 iH = (FMS32)iHost;
				using (var tcpClient = new TcpClient ()) {
                    await tcpClient.ConnectAsync (FMSConfig["hostname"], int.Parse(FMSConfig["port"]));
					FeuerwehrCloud.Helper.Logger.WriteLine ("|  < [FMS32] *** Connected.. waiting for data");
					using (var networkStream = tcpClient.GetStream ()) {
						do {
							var buffer = new byte[4096];
							var byteCount = await networkStream.ReadAsync (buffer, 0, buffer.Length);
							var response = Encoding.UTF8.GetString (buffer, 0, byteCount);
							string[] Z = response.Split (new [] { "\r\n" }, StringSplitOptions.None);
							foreach (var item in Z) {
								string[] rLine = response.Split (new [] { "\t" }, StringSplitOptions.None);
								if(item.StartsWith("#")) {
									//
								} else {
									switch (rLine [0]) {
									case "FMSTlg":
										/*
										 * 1  = FZid
										 * 2  = Status
										 * 3  = Land
										 * 4  = BOS-, Landes- und Orts-Kennung in hexadezimaler Schreibweise
										 * 5  = die Fahrzeug-Kennung in hexadezimaler Schreibweise.
										 * 6  = die Statusnummer in dezimaler Schreibweise (Wertebereich=0-15).
										 * 7  = die Baustufe in dezimaler Schreibweise (Wertebereich=0-1).
										 * 8  = die Richtung in dezimaler Schreibweise (Wertebereich=0-1)
										 * 9  = die TKI in dezimaler Schreibweise (Wertebereich=0-4, wobei 4 für eine manuelle Statusänderung am Server steht).
										 * 10 = Statusflag, ob das Fahrzeug ein Folgetelegramm gesendet hat.
										 * 11 = Statusflag, ob die Leitstelle ein Folgetelegramm gesendet hat.
										 * 12 = die Folgenummer in dezimaler Schreibweise wenn Feld 10 den Wert 1 enthält
										 * 13 = der Text oder die Daten eines Folgetelegramms entsprechend den Werten in den Feldern 10 und 11.
										 * 14 = die Nummer der Soundkarte vom Server, mit der das Telegramm empfangen wurde
										 * 15 = die Nummer des Kanals der Soundkarte vom Server, mit der das Telegramm empfangen wurde
										 */
                                            //1|6D816111  2|6   3|13  4|6D81    5|6111    6|1   7|1   8|0   9|4   10|0   11|0   12|0   13|    14|0   15|0

										string[] FMSItems = rLine [0].Split(new[]{"\t"},StringSplitOptions.None);
                                        new System.Threading.Thread(delegate() {
                                                System.Net.WebClient WC = new WebClient();
                                                FeuerwehrCloud.PhpSerializer s = new PhpSerializer();

                                                WC.DownloadStringAsync(new Uri("https://www.feuerwehrcloud.de/deiva/fms.php?data="+s.Serialize(FMSItems) ));
                                        }).Start();
										break;
									case "POC":
										/*
										 * 1  = die RIC des DME in dezimaler Schreibweise. Zu beachten ist, das die RIC ohne führenden Nullen übertragen wird (Wertebereich=0–2097152)
										 * 2  = die Funktionsnummer des DME in dezimaler Schreibweise (Wertebereich=1–4).
										 * 3  = der Text der Alarmierung.
										 * 4  = die Nummer der Soundkarte vom Server, mit der die Alarmierung empfangen wurde
										 * 5  = die Nummer des Kanals der Soundkarte vom Server, mit der das Telegramm empfangen wurde
										 */
										string[] POCItems = rLine [0].Split(new[]{"\t"},StringSplitOptions.None);
										break;
									case "ZVEI":
										/*
										 * 1  = die ZVEI-Nummer des Empfängers in alphadezimaler Schreibweise. Zu beachten ist, dass die ZVEI-Nummer mit führenden Nullen übertragen wird. (Wertebereich=00000–99999).
										 * 2  = die Nummer der Soundkarte vom Server, mit der das Telegramm empfangen wurde
										 * 3  = die Nummer des Kanals der Soundkarte vom Server, mit der das Telegramm empfangen wurde
										 */
										string[] ZVEIItems = rLine [0].Split(new[]{"\t"},StringSplitOptions.None);
										break;
									case "FMSALL":
										break;
									case "AN1":
									case "AN2":
									case "AN3":
										break;
										
									}
								}

							}					
						} while (Disposing == false);
					}
				}
				//}	
			} catch (Exception ex) {

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

		public string Name {
			get {
				return "FMS32";
			}
		}

		public string FriendlyName {
			get {
				return "Funkmeldesystem (FMS32pro)";
			}
		}

		public Guid GUID {
			get {
				return new Guid (Name);
			}
		}

		public byte[] Icon {
			get {
				var assembly = typeof(FeuerwehrCloud.Input.FMS32).GetTypeInfo().Assembly;
				string[] resources = assembly.GetManifestResourceNames();
				Stream stream = assembly.GetManifestResourceStream("icon.ico");
				return ((MemoryStream)stream).ToArray();
			}
		}

		#endregion

		#region IDisposable implementation

		public void Dispose ()
		{
			FeuerwehrCloud.Helper.Logger.WriteLine ("|  *** FMS32 plugin unloaded...");
		}

		#endregion

		public FMS32 ()
		{
		}
	}
}

