using System;
using System.Windows;
using System.ComponentModel;
using FeuerwehrCloud.Plugin;
using System.Reflection;
using System.IO;

namespace FeuerwehrCloud.Output
{
	public class eLION : IPlugin
	{
		#region IPlugin implementation

		public event PluginEvent Event;
		IHost My;
		private System.Collections.Generic.Dictionary<string, string> Config = new System.Collections.Generic.Dictionary<string, string> ();
		FileSystemWatcher watcher;

		public bool Initialize(IHost hostApplication)
		{
			My = hostApplication;
			if(!System.IO.File.Exists("elion.cfg")) {
				Config.Add ("serverpath", "");
				FeuerwehrCloud.Helper.AppSettings.Save(Config,"elion.cfg");
			} 
			Config = FeuerwehrCloud.Helper.AppSettings.Load ("elion.cfg");
			FeuerwehrCloud.Helper.Logger.WriteLine ("|  *** e:LION loaded...");
			watcher = new FileSystemWatcher ();
			watcher.Path = System.Environment.CurrentDirectory;
			watcher.IncludeSubdirectories = false;
			watcher.Filter = "elion.cfg";
			watcher.NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.LastAccess | NotifyFilters.DirectoryName | NotifyFilters.FileName;
			watcher.Changed += new FileSystemEventHandler(delegate(object sender, FileSystemEventArgs e) {
				Config = FeuerwehrCloud.Helper.AppSettings.Load ("elion.cfg");
			});
			watcher.EnableRaisingEvents = true;
			return true;
		}

		public void Execute(params object[] list)
		{
			FeuerwehrCloud.Helper.Logger.WriteLine ("|  > [e:LION] *** Writing XML-File...");
			System.Collections.Generic.Dictionary<string, string> Daten = list[0] as System.Collections.Generic.Dictionary<string, string>;
			string Result = "<?xml version=\"1.0\" encoding=\"iso-8859-1\"?>\r\n";
			Result += "<Einsatz>\r\n";
			Result += "<Ort>"+ Daten["EinsatzOrt"] + "</Ort>\r\n";
			string Ortsteil = "";
			if (Daten["EinsatzAbschnitt"] != "-") {
				Ortsteil = Daten["EinsatzAbschnitt"];
			}
			if (Daten["EinsatzKreuzung"] != "-") {
				if (Ortsteil != "") {
					Ortsteil += ", "+Daten["EinsatzKreuzung"];
				} else {
					Ortsteil = Daten["EinsatzKreuzung"];
				}
			}
			if (Daten["EinsatzObjekt"] != "-") {
				if (Ortsteil != "") {
					Ortsteil += ", "+Daten["EinsatzObjekt"];
				} else {
					Ortsteil = Daten["EinsatzObjekt"];
				}
			}
			Result += "<Ortsteil>"+ Ortsteil  +"</Ortsteil>\r\n";	
			Result += "<Strasse>"+ Daten["EinsatzStrasse"] + "</Strasse>\r\n";
			Result += "<Hausnummer></Hausnummer>\r\n";
			Result += "<GK_X>"+ Daten["EinsatzCoords"].Split(' ')[0] + "</GK_X>\r\n";
			Result += "<GK_Y>"+ Daten["EinsatzCoords"].Split(' ')[1] + "</GK_Y>\r\n";
			Result += "<WGS></WGS>\r\n";
			Result += "<Zusatzinformationen>"+ Daten["EinsatzNr"] + "</Zusatzinformationen>\r\n";
			Result += "<Einsatzart>"+ Daten["EinsatzSchlagwort"] + "</Einsatzart>\r\n";
			Result += "<Stichwort>"+ Daten["EinsatzStichwort"] + "</Stichwort>\r\n";
			Result += "<Sondersignal></Sondersignal>\r\n";
			Result += "<E-Key></E-Key>\r\n";
			Result += "<Meldender></Meldender>\r\n";
			Result += "<Telefon></Telefon>\r\n";
			Result += "<Bemerkung>"+ Daten["EinsatzBemerkung"] + "</Bemerkung>\r\n";
			Result += "<Alarmierungen>\r\n";
			Result += "<Alarmzeit>"+System.DateTime.Now.ToString("dd.MM.yyyy hh:mm:ss")+"</Alarmzeit>  <Codierung>08031900900 FAX   </Codierung>\r\n";
			Result += "</Alarmierungen>\r\n";
			Result += "<Leistungsnehmer>\r\n";
			Result += "</Leistungsnehmer>\r\n";
			Result += "<Datum>"+ System.DateTime.Now.ToString("dd.MM.yyyy") +"</Datum>\r\n";
			Result += "<Zeit>"+System.DateTime.Now.ToString("hh:mm:ss")+"</Zeit>\r\n";
			Result += "<Referenz>"+ Daten["EinsatzNr"] +"</Referenz>\r\n";
			Result += "</Einsatz>\r\n";
			//(string)list [2]
			System.IO.File.WriteAllText(Config["serverpath"] + "/"+ Daten["EinsatzNr"] +".xml", Result, System.Text.Encoding.Default);
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

		public string Name {
			get {
				return "eLION";
			}
		}

		public string FriendlyName {
			get {
				return "e:LION Ausgabemodul";
			}
		}

		public Guid GUID {
			get {
				return new Guid(Name);
			}
		}

		public byte[] Icon {
			get {
				var assembly = typeof(FeuerwehrCloud.Output.eLION).GetTypeInfo().Assembly;
				string[] resources = assembly.GetManifestResourceNames();
				Stream stream = assembly.GetManifestResourceStream("icon.ico");
				return ((MemoryStream)stream).ToArray();
			}
		}

		#endregion

		#region IDisposable implementation

		public void Dispose()
		{
			FeuerwehrCloud.Helper.Logger.WriteLine ("|  > [e:LION] *** Unloading...");
		}

		#endregion

		public eLION()
		{
		}
	}
}

