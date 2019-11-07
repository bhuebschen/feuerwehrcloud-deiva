using System;
using FeuerwehrCloud;
using System.Diagnostics;
using FeuerwehrCloud.Plugin;
using System.IO;
using System.Reflection;
using System.Text;

namespace FeuerwehrCloud.Processor
{
	public class Parser : FeuerwehrCloud.Plugin.IPlugin
	{
		#region IPlugin implementation

		public event FeuerwehrCloud.Plugin.PluginEvent Event;
		private FeuerwehrCloud.Plugin.IHost My;

		public bool Initialize(FeuerwehrCloud.Plugin.IHost hostApplication)
		{
			My = hostApplication;
			FeuerwehrCloud.Helper.Logger.WriteLine ("|  *** Parser: Rosenheim loaded...");
			return true;
		}

		public void Execute(params object[] list)
		{
			//
			string CompleteOCROutput = ((string)list[0]);
            string OriginalFile = ((string)list[1]);
			// Detect Address from Text
			string CurrentSection  = string.Empty;
			string[] Lines = CompleteOCROutput.Split(new [] {"\n"}, StringSplitOptions.RemoveEmptyEntries);
			System.Collections.Generic.Dictionary<string, System.Collections.Generic.Dictionary<string,string>> EinsatzMittel = new System.Collections.Generic.Dictionary<string, System.Collections.Generic.Dictionary<string, string>>();
			bool SectionStarted = false;

			String EinsatzMitteiler  = string.Empty;
			String EinsatzMitteilerNummer  = string.Empty;
			String EinsatzLeitstelle  = string.Empty;
			String EinsatzGemeinde = string.Empty;
			String EinsatzOrtCoords  = string.Empty;
			String EinsatzOrt  = string.Empty;
			String EinsatzStrasse  = string.Empty;
			String EinsatzAbsch  = string.Empty;
			String EinsatzObjekt  = string.Empty;
			String EinsatzKreuz  = string.Empty;
			String EinsatzNr  = string.Empty;
			String Schlagwort  = string.Empty;
			string Stichwort  = string.Empty;
			String Mitteiler  = string.Empty;
			String Prio  = string.Empty;
			String Bemerkung  = string.Empty;
			String EinsatzPlan = string.Empty;
			String EinsatzHausnummer = string.Empty;

			for(int i = 0; i<Lines.Length; i++) {
				if (Lines [i].StartsWith ("-", StringComparison.InvariantCultureIgnoreCase) == true && SectionStarted == true) {
					CurrentSection = Lines [i].Substring (Lines [i].IndexOf ("- ", StringComparison.InvariantCultureIgnoreCase) + 2);
					try {
						if(CurrentSection.Substring(0,10)=="EINSATZORT" && CurrentSection.IndexOf("----") ==-1) {
							if(CurrentSection.Substring(CurrentSection.LastIndexOf(" ")+1).Length == 7) {
								EinsatzOrtCoords = CurrentSection.Substring(CurrentSection.LastIndexOf(" ")+1);
								string XC = CurrentSection.Substring(0,CurrentSection.LastIndexOf(" "));
								if(XC.Substring(XC.LastIndexOf(" ")+1).Length == 7) {
									EinsatzOrtCoords = XC.Substring(XC.LastIndexOf(" ")+1) + " " + EinsatzOrtCoords;
								}

							}
							CurrentSection = CurrentSection.Substring (0, CurrentSection.IndexOf ("--", StringComparison.InvariantCultureIgnoreCase)).Trim ();
						} else {
							CurrentSection = CurrentSection.Substring (0, CurrentSection.IndexOf ("--", StringComparison.InvariantCultureIgnoreCase)).Trim ();
						}
						if (CurrentSection == "BEMERKUNG")
							i++;
					} catch (Exception ex) {
						FeuerwehrCloud.Helper.Logger.WriteLine(FeuerwehrCloud.Helper.Helper.GetExceptionDescription(ex));
					}
				}
				if(Lines[i].StartsWith("-", StringComparison.InvariantCultureIgnoreCase)==true && SectionStarted==false) {
					SectionStarted = true;

					CurrentSection = Lines[i].Substring(Lines[i].IndexOf("-", StringComparison.InvariantCultureIgnoreCase)+2);
					try {
						CurrentSection = CurrentSection.Substring(0,CurrentSection.IndexOf("--", StringComparison.InvariantCultureIgnoreCase)).Trim();

					} catch (Exception ex) {
						CurrentSection = CurrentSection.Substring(0,CurrentSection.IndexOf("-", StringComparison.InvariantCultureIgnoreCase)).Trim();

					}


				} else if( SectionStarted == true && Lines[i].StartsWith("Name", StringComparison.InvariantCultureIgnoreCase) == true  && CurrentSection=="MITTEILER") {
					EinsatzMitteiler= Lines[i].Substring(6).Trim();
				} else if( SectionStarted == true && Lines[i].StartsWith("Rufnum", StringComparison.InvariantCultureIgnoreCase) == true  && CurrentSection=="MITTEILER") {
					EinsatzMitteilerNummer= Lines[i].Substring(11).Trim();
				} else if( SectionStarted == true && Lines[i].StartsWith("Stra", StringComparison.InvariantCultureIgnoreCase) == true  && CurrentSection=="EINSATZORT") {
					//+8
					EinsatzStrasse = Lines[i].Substring(8).Trim();
					EinsatzHausnummer = EinsatzStrasse.Split(' ')[EinsatzStrasse.Split(' ').Length - 1];
				} else if( SectionStarted == true && Lines[i].StartsWith("Gemeind", StringComparison.InvariantCultureIgnoreCase) == true  && CurrentSection=="EINSATZORT") {
					//+8
					EinsatzGemeinde = Lines[i].Substring(10).Trim();
				} else if(SectionStarted == true && Lines[i].StartsWith("Absch", StringComparison.InvariantCultureIgnoreCase) == true  && CurrentSection=="EINSATZORT") {
					if(EinsatzAbsch.Replace(" ","")==":") 
						EinsatzAbsch ="";
				} else if( SectionStarted == true && Lines[i].StartsWith("Ortst", StringComparison.InvariantCultureIgnoreCase) == true  && CurrentSection=="EINSATZORT") {
					//+8
					EinsatzOrt = Lines[i].Substring(10).Trim();
					EinsatzOrt = EinsatzOrt.Substring(EinsatzOrt.IndexOf("-", StringComparison.InvariantCultureIgnoreCase)+1).Trim();
					string [] EO = EinsatzOrt.Split(new [] {' '});
					int XCount = EO.Length/2;
					if(XCount>1) {
						EinsatzOrt="";
						for(int o=0;o<XCount;o++) {
							EinsatzOrt += EO[o]+" ";
						}
						EinsatzOrt = EinsatzOrt.Trim();
					} else {
						EinsatzOrt = EO[0];
					}
				} else if(SectionStarted == true && Lines[i].StartsWith("Kreuz", StringComparison.InvariantCultureIgnoreCase) == true  && CurrentSection=="EINSATZORT") {
					//+8
					if (Lines [i].Length > 8) {
						EinsatzKreuz = Lines[i].Substring(10).Trim();
						if(EinsatzKreuz.Replace(" ","")==":") EinsatzKreuz ="";
					} else {
						EinsatzKreuz  = string.Empty;
					}
				} else if( SectionStarted == true && Lines[i].StartsWith("Ob", StringComparison.InvariantCultureIgnoreCase) == true  && CurrentSection=="EINSATZORT") {
					//+6
					EinsatzObjekt = Lines[i].Substring(6).Trim();
					if(EinsatzObjekt.Replace(" ","")==":") EinsatzObjekt ="";
				} else if( SectionStarted == true && Lines[i].StartsWith("Schlagw", StringComparison.InvariantCultureIgnoreCase) == true  && CurrentSection=="EINSATZGRUND") {
					//+8
					Schlagwort= Lines[i].Substring(10).Trim();
				} else if( SectionStarted == true && Lines[i].StartsWith("Stichw", StringComparison.InvariantCultureIgnoreCase) == true  && CurrentSection=="EINSATZGRUND") {
					//+11
					if (string.IsNullOrEmpty(Stichwort)) {
						Stichwort = Lines[i].Substring(11).Trim();
					} else {
						try {
							FeuerwehrCloud.Helper.Helper.exec("textout.sh", "\"Bei der Verarbeitung ist ein Fehler aufgetreten:\r\nVon der Leitstelle wurde bereits in Einsatzstichwort vergeben: '" + Stichwort + "', weiteres Stichwort: '"+Lines[i].Substring(11).Trim()+"'\"");

						} catch (Exception ex) {
							
						}
						Stichwort = Stichwort + ", " + Lines[i].Substring(11).Trim();
					}
				} else if( SectionStarted == true && Lines[i].StartsWith("Prior", StringComparison.InvariantCultureIgnoreCase) == true  && CurrentSection=="EINSATZGRUND") {
					//+11
					Prio= Lines[i].Substring(11).Trim();
				} else if(SectionStarted == true && Lines[i].StartsWith("Einsatz", StringComparison.InvariantCultureIgnoreCase) == true  && (CurrentSection.StartsWith("Alarmfax der ", StringComparison.InvariantCultureIgnoreCase) || CurrentSection.StartsWith("Alarmfax der ", StringComparison.InvariantCultureIgnoreCase))) {
					//+12
					EinsatzNr = Lines[i].Substring(12).Trim();
					EinsatzLeitstelle = CurrentSection.Substring(13);
				} else if( SectionStarted == true && Lines[i].StartsWith("Name", StringComparison.InvariantCultureIgnoreCase) == true  && CurrentSection=="MITTEILER") {
					//+6
					Mitteiler= Lines[i].Substring(6).Trim();
				} else if( SectionStarted == true && Lines[i].StartsWith("Name", StringComparison.InvariantCultureIgnoreCase) == true  && SectionStarted == true && CurrentSection=="EINSATZMITTEL") {

					System.Collections.Generic.Dictionary<string, string> Section = new	 System.Collections.Generic.Dictionary<string, string> ();
					Section.Add ("Name", Lines [i].Substring (6).Trim ());
					string Alarmiert = "";
					try {
						Alarmiert = Lines[i + 1].Substring(12).Trim();
					} catch (Exception ex) {
							
					}


					string BereitsAusger = "";
					if (Alarmiert.Length > (Alarmiert.IndexOf("Aus :") + 5))
					{
						BereitsAusger = Alarmiert.Substring(Alarmiert.IndexOf("Aus :") + 5);
					}
					else {
						Alarmiert = Alarmiert.Replace("Aus :", "");
					}
					Section.Add("Alarmiert", Alarmiert);
					Section.Add("Ausgerückt", BereitsAusger);
					if (Lines[i + 2].Length > 10) {
						Section.Add("Gerät", Lines[i + 2].Substring(12).Trim());
					} else {
						Section.Add("Gerät", "-");
					}
					try {
						EinsatzMittel.Add (Lines [i].Substring (6).Trim (), Section);
					} catch (Exception ex) {
						FeuerwehrCloud.Helper.Logger.WriteLine(ex.ToString());
						try {
							if (ex.GetType().ToString() == "System.ArgumentException") {
								if (ex.Message.IndexOf(" with the same key ") > -1) {
									FeuerwehrCloud.Helper.Helper.exec("textout.sh", "\"Bei der Verarbeitung ist ein Fehler aufgetreten:\r\nDie Leitstelle hat ein gefordertes Fahrzeug ist bereits gefordert: " + Lines[i].Substring(6).Trim() + "\"");
								} else {
								}
							}

						} catch (Exception e2x) {
						}
					}
					// Alarmiert
					// gef. Ger
				} else if( SectionStarted == true && CurrentSection=="BEMERKUNG") {
					Bemerkung +=  Lines[i]+"\r\n";
				} else {
				}
			}


			if (Bemerkung.Length > 81) {
				if(Bemerkung.Substring(80,2)=="\r\n") {
					Bemerkung = Bemerkung.Substring(0,80) + Bemerkung.Substring(82);
				}
			}
			//Bemerkung = Bemerkung.Replace("\r\n\r\n", "!°$").Replace("\r\n", "").Replace("!°$", "\r\n\r\n");
			if (!string.IsNullOrEmpty(EinsatzOrt)) {
				EinsatzAbsch = EinsatzAbsch.Replace(EinsatzOrt,"");
			} else {
			}
			if (string.IsNullOrEmpty(EinsatzAbsch))
				EinsatzAbsch = " ";
			if(EinsatzAbsch.EndsWith("-", StringComparison.InvariantCultureIgnoreCase)) EinsatzAbsch = EinsatzAbsch.Substring(0,EinsatzAbsch.Length-1);

			System.Collections.Generic.Dictionary<string, string> Result = new System.Collections.Generic.Dictionary<string, string> ();
			if (string.IsNullOrEmpty(EinsatzOrtCoords))
				EinsatzOrtCoords = " ";
			if (string.IsNullOrEmpty(EinsatzObjekt))
				EinsatzObjekt = " ";
			if (string.IsNullOrEmpty(EinsatzGemeinde))
				EinsatzGemeinde = " ";
			if (string.IsNullOrEmpty(EinsatzStrasse))
				EinsatzStrasse = " ";
			if (string.IsNullOrEmpty(EinsatzMitteilerNummer))
				EinsatzMitteilerNummer = " ";
			if (string.IsNullOrEmpty(EinsatzHausnummer))
				EinsatzHausnummer = " ";
			if (string.IsNullOrEmpty(EinsatzPlan))
				EinsatzPlan = " ";
			if (string.IsNullOrEmpty(EinsatzOrt) || EinsatzOrt == " ")
				if (EinsatzGemeinde != " ") {
					EinsatzOrt = EinsatzGemeinde;
				} else {
					EinsatzOrt = " ";
				}
			if (string.IsNullOrEmpty(EinsatzMitteilerNummer))
				EinsatzMitteilerNummer = " ";
			if (string.IsNullOrEmpty( EinsatzAbsch))
				EinsatzAbsch = " ";
			if (string.IsNullOrEmpty(EinsatzKreuz)) 
				EinsatzKreuz = " ";
			if (Prio.IndexOf (" ") > -1) {
				Prio = Prio.Substring (0,Prio.IndexOf (" ")).Trim ();
			}
			try {
				System.IO.File.WriteAllText ("public/alarms/" + EinsatzNr.Replace (" ", "_") + ".txt", CompleteOCROutput);
			} catch (Exception ex) {
				FeuerwehrCloud.Helper.Logger.WriteLine (FeuerwehrCloud.Helper.Helper.GetExceptionDescription (ex));
			}

			Result.Add ("EinsatzMitteiler", EinsatzMitteiler);
			Result.Add ("EinsatzMitteilerNummer", EinsatzMitteiler);

			Result.Add ("EinsatzLeitstelle", EinsatzLeitstelle);
			Result.Add ("EinsatzGemeinde", EinsatzGemeinde);



			Result.Add ("EinsatzCoords", EinsatzOrtCoords);
			Result.Add ("EinsatzNr", EinsatzNr);
			Result.Add ("EinsatzStrasse", EinsatzStrasse);
			Result.Add ("EinsatzHausnummer", EinsatzStrasse);
			Result.Add ("EinsatzOrt", EinsatzOrt);
			Result.Add ("EinsatzAbschnitt", EinsatzAbsch);
			Result.Add ("EinsatzKreuzung", EinsatzKreuz);
			Result.Add ("EinsatzObjekt", EinsatzObjekt);
			Result.Add ("EinsatzBemerkung", Bemerkung);
			Result.Add ("EinsatzPrioritaet", Prio);
			Result.Add ("EinsatzStichwort", Stichwort);
			StringBuilder X = new StringBuilder();
			foreach (System.Collections.Generic.KeyValuePair<string,System.Collections.Generic.Dictionary<string,string> > item in EinsatzMittel) {
				X.AppendLine(
					item.Value["Name"]+"∆"+item.Value["Alarmiert"]+"∆"+item.Value["Ausgerückt"]+"∆"+item.Value["Gerät"]
				);
			}



			Result.Add ("EinsatzMittel", X.ToString());
			Result.Add ("EinsatzSchlagwort", Schlagwort);
			foreach (var item in Result) {
				System.Environment.SetEnvironmentVariable (item.Key, item.Value); //System.Text.Encoding.Default.GetString(System.Text.Encoding.UTF8.GetBytes(item.Value)));
			}
			Result.Add ("RAW", CompleteOCROutput);
            Result.Add ("FILENAME", OriginalFile);
			string XMLFIle  = string.Empty;
			try {
				if (Stichwort.StartsWith ("B ")) {
					XMLFIle=System.IO.File.ReadAllText ("brand.fct", System.Text.Encoding.UTF8);
				} else if (Stichwort.StartsWith ("BOMBEN")) {
					XMLFIle=System.IO.File.ReadAllText ("bombe.fct", System.Text.Encoding.UTF8);
				} else if (Stichwort.StartsWith ("THL")) {
					XMLFIle=System.IO.File.ReadAllText ("thl.fct", System.Text.Encoding.UTF8);
				} else if (Stichwort.StartsWith ("GEBAUDEEINSTURZ") || Stichwort.StartsWith ("GEBÄUDEEINSTURZ")) {
					XMLFIle=System.IO.File.ReadAllText ("gebaeude.fct", System.Text.Encoding.UTF8);
				} else if (Stichwort.StartsWith ("VU ")) {
					XMLFIle=System.IO.File.ReadAllText ("vu.fct", System.Text.Encoding.UTF8);
				} else if (Stichwort.StartsWith ("P ")) {
					XMLFIle=System.IO.File.ReadAllText ("person.fct", System.Text.Encoding.UTF8);
				} else if (Stichwort.StartsWith ("SICHER")) {
					XMLFIle=System.IO.File.ReadAllText ("sicher.fct", System.Text.Encoding.UTF8);
				} else if (Stichwort.StartsWith ("ÖL")) {
					XMLFIle=System.IO.File.ReadAllText ("oel.fct", System.Text.Encoding.UTF8);
				} else if (Stichwort.StartsWith ("BENZIN")) {
					XMLFIle=System.IO.File.ReadAllText ("oel.fct", System.Text.Encoding.UTF8);
				} else  {
					XMLFIle=System.IO.File.ReadAllText ("oel.fct", System.Text.Encoding.UTF8);
				}
			} catch (Exception ex) {
				FeuerwehrCloud.Helper.Logger.WriteLine(ex.ToString());
				if (EinsatzNr.StartsWith("T")) {
					XMLFIle=System.IO.File.ReadAllText ("thl.fct", System.Text.Encoding.UTF8);
				} else if (EinsatzNr.StartsWith("B")) {
					XMLFIle=System.IO.File.ReadAllText ("brand.fct", System.Text.Encoding.UTF8);
				} else {
					XMLFIle=System.IO.File.ReadAllText ("oel.fct", System.Text.Encoding.UTF8);
				}
			}
			XMLFIle = System.Environment.ExpandEnvironmentVariables (XMLFIle);
			try {
				System.Xml.XmlDocument doc = new System.Xml.XmlDocument();
				doc.LoadXml(XMLFIle);

				foreach (System.Collections.Generic.KeyValuePair<string,System.Collections.Generic.Dictionary<string,string> > item in EinsatzMittel) {

					System.Xml.XmlNodeList L = doc.FirstChild.SelectNodes("/Object/Object/Property[@name='FahrzeugID']");
					foreach (System.Xml.XmlNode item2 in L) {
						if(item2.InnerText == item.Value["Name"]) {
							item2.ParentNode.SelectSingleNode("Property[@name='Data']").InnerText = item.Value["Name"]+"∆"+item.Value["Alarmiert"]+"∆"+item.Value["Ausgerückt"]+"∆"+item.Value["Gerät"];
						}
					}
					//X.AppendLine(
					//	
					//);
				}



                doc.Save("web/"+EinsatzNr.Replace (" ", "_")+".xml");       
				if (!System.IO.Directory.Exists ("web")) {
					System.IO.Directory.CreateDirectory ("web");
				}


                if(My.LastENumber != EinsatzNr && ((System.DateTime.Now - My.LastAlert).Hours<1)) {
                    string P = System.IO.File.ReadAllText("panel1x2.fct");
                    P=P.Replace("#t1#","http://localhost:19270/web/"+My.LastENumber.Replace (" ", "_")+".xml").Replace("#t2#","http://localhost:19270/web/"+EinsatzNr.Replace (" ", "_")+".xml");
                    System.IO.File.WriteAllText("web/panel.xml",P);
                } else {
                    doc.Save("web/panel.xml");  
                }

					
			} catch (Exception ex) {
				FeuerwehrCloud.Helper.Logger.WriteLine(ex.ToString());
			}
			RaiseFinish ("text", Result );
            My.LastENumber = EinsatzNr;
            My.LastAlert = System.DateTime.Now;
		}

		public void RaiseFinish(params object[] list) {
			PluginEvent messageSent = Event;
			if (messageSent != null)
			{
				messageSent(this, list);
			}
		}

		public bool IsAsync {
			get {
				return false;
			}
		}

		public FeuerwehrCloud.Plugin.ServiceType ServiceType {
			get {
				return ServiceType.processor;
			}
		}

		public string Name {
			get {
				return "Parser: ILS-Rosenheim";
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
				return new byte[]{ };
			}
		}

		#endregion

		#region IDisposable implementation

		public void Dispose()
		{
			
		}

		#endregion

		public Parser()
		{
		}
	}
}

