using System;
using System.Diagnostics;
using FeuerwehrCloud.Plugin;

namespace FeuerwehrCloud.Processor.OCR
{
	public class Tesseract : IPlugin
	{
		public Tesseract ()
		{
		}

		public bool Initialize(IHost hostApplication) {
			de.SYStemiya.Helper.Logger.WriteLine ("| ["+System.DateTime.Now.ToString("T") +"] |-+ [Parser: ILS-Rosenheim] *** Initializing...");
			return true;
		}

		public event PluginEvent Event;

		public bool IsAsync
		{
			get { return true; }
		}

		public ServiceType ServiceType
		{
			get { return ServiceType.processor; }
		}

		public void Dispose() {
			de.SYStemiya.Helper.Logger.WriteLine ("| ["+System.DateTime.Now.ToString("T") +"] |-+ [Parser: ILS-Rosenheim] *** Unloading...");
		}

		public void RaiseFinish(params object[] list) {
			PluginEvent messageSent = Event;
			if (messageSent != null)
			{
				messageSent(this, list);
			}
		}

		public void Execute(params object[] list) {
			string[] Pages = (string[])list [1];
			string CompleteOCROutput = "";
			Console.Write ("| ["+System.DateTime.Now.ToString("T") +"] |-+ [Parser: ILS-Rosenheim] *** Do OCR for: ");
			foreach (string item in Pages) {
				de.SYStemiya.Helper.Logger.WriteLine (item);
				System.Diagnostics.Process P = new System.Diagnostics.Process();
				P.StartInfo.FileName = "/usr/bin/tesseract";
				P.StartInfo.Arguments = "/tmp/"+item+" /tmp/"+(item)+"de.SYStemiya.Helper. -l eng";
				P.StartInfo.UseShellExecute = false;
				P.StartInfo.RedirectStandardOutput = true;
				P.StartInfo.RedirectStandardError = true;
				P.Start(); P.WaitForExit();
				String XContent = System.IO.File.ReadAllText("/tmp/"+(item)+".txt");
				CompleteOCROutput =XContent;
			}

			// Detect Address from Text
			string CurrentSection = "";
			string[] Lines = CompleteOCROutput.Split(new [] {"\n"}, StringSplitOptions.RemoveEmptyEntries);
			bool SectionStarted = false;

			System.Collections.Generic.Dictionary<string, System.Collections.Generic.Dictionary<string,string>> EinsatzMittel = new System.Collections.Generic.Dictionary<string, System.Collections.Generic.Dictionary<string, string>>();
			de.SYStemiya.Helper.Logger.WriteLine ("| ["+System.DateTime.Now.ToString("T") +"] |-+ [Parser: ILS-Rosenheim] *** Extrade.SYStemiya.Helper.agedetails from OCR");

			String EinsatzOrt = "";
			String EinsatzStrasse = "";
			String EinsatzAbsch = "";
			String EinsatzObjekt = "";
			String EinsatzKreuz = "";
			String EinsatzNr = "";
			String Schlagwort = "";
			string Stichwort = "";
			String Mitteiler = "";
			String Prio = "";
			String Bemerkung = "";

			for(int i =0; i<Lines.Length; i++) {
				if(Lines[i].StartsWith("-", StringComparison.InvariantCultureIgnoreCase) & SectionStarted==true) {
					CurrentSection = Lines[i].Substring(Lines[i].IndexOf("- ", StringComparison.InvariantCultureIgnoreCase)+2);
				try {

					CurrentSection = CurrentSection.Substring(0,CurrentSection.IndexOf("--", StringComparison.InvariantCultureIgnoreCase)).Trim();
				} catch (Exception ex) {
					de.SYStemiya.Helper.Logger.WriteLine (ex.ToString ());
				}
				if(Lines[i].StartsWith("-", StringComparison.InvariantCultureIgnoreCase) & !SectionStarted) {
					SectionStarted = true;

					CurrentSection = Lines[i].Substring(Lines[i].IndexOf("-", StringComparison.InvariantCultureIgnoreCase)+2);
					CurrentSection = CurrentSection.Substring(0,CurrentSection.IndexOf("--", StringComparison.InvariantCultureIgnoreCase)).Trim();
				} else if( SectionStarted == true & Lines[i].StartsWith("Stra", StringComparison.InvariantCultureIgnoreCase) & CurrentSection=="EINSATZORT") {
					//+8
					EinsatzStrasse = Lines[i].Substring(8).Trim();
				} else if(SectionStarted == true & Lines[i].StartsWith("Absch", StringComparison.InvariantCultureIgnoreCase) & CurrentSection=="EINSATZOR") {
					if(EinsatzAbsch.Replace(" ","")==":") 
						EinsatzAbsch ="";
				} else if( SectionStarted == true & Lines[i].StartsWith("Ortst", StringComparison.InvariantCultureIgnoreCase) & CurrentSection=="EINSATZORT") {
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
				} else if(SectionStarted == true & Lines[i].StartsWith("Kreuz", StringComparison.InvariantCultureIgnoreCase) & CurrentSection=="EINSATZORT") {
					//+8
					EinsatzKreuz = Lines[i].Substring(10).Trim();
					if(EinsatzKreuz.Replace(" ","")==":") EinsatzKreuz ="";
				} else if( SectionStarted == true & Lines[i].StartsWith("Ob", StringComparison.InvariantCultureIgnoreCase) & CurrentSection=="EINSATZORT") {
					//+6
					EinsatzObjekt = Lines[i].Substring(6).Trim();
					if(EinsatzObjekt.Replace(" ","")==":") EinsatzObjekt ="";
				} else if( SectionStarted == true & Lines[i].StartsWith("Schlagw", StringComparison.InvariantCultureIgnoreCase) & CurrentSection=="EINSATZGRUND") {
					//+8
					Schlagwort= Lines[i].Substring(10).Trim();
				} else if( SectionStarted == true & Lines[i].StartsWith("Stichw", StringComparison.InvariantCultureIgnoreCase) & CurrentSection=="EINSATZGRUND") {
					//+11
					Stichwort= Lines[i].Substring(11).Trim();
				} else if( SectionStarted == true & Lines[i].StartsWith("Prior", StringComparison.InvariantCultureIgnoreCase) & CurrentSection=="EINSATZGRUND") {
					//+11
					Prio= Lines[i].Substring(11).Trim();
				} else if(SectionStarted == true & Lines[i].StartsWith("Einsatz", StringComparison.InvariantCultureIgnoreCase) & CurrentSection.StartsWith("Alarmfax der ", StringComparison.InvariantCultureIgnoreCase)) {
					//+12
					EinsatzNr = Lines[i].Substring(12).Trim();
				} else if( SectionStarted == true & Lines[i].StartsWith("Name", StringComparison.InvariantCultureIgnoreCase) & CurrentSection=="MITTEILER") {
					//+6
					Mitteiler= Lines[i].Substring(6).Trim();
				} else if( SectionStarted == true & Lines[i].StartsWith("Name", StringComparison.InvariantCultureIgnoreCase) & SectionStarted == true & CurrentSection=="EINSATZMITTEL") {

					System.Collections.Generic.Dictionary<string, string> Section = new	 System.Collections.Generic.Dictionary<string, string> ();
					Section.Add ("Name", Lines [i].Substring (6).Trim ());
					Section.Add ("Alarmiert", Lines [i+1].Substring (12).Trim ());
					Section.Add ("Gerät", Lines [i+2].Substring (12).Trim ());
					EinsatzMittel.Add (Lines [i].Substring (6).Trim (), Section);
					// Alarmiert
					// gef. Ger
				} else if( SectionStarted == true & CurrentSection=="BEMERKUNG") {
					Bemerkung +=  Lines[i]+"\r\n";
				} else {
				}
			}
		}

		EinsatzAbsch = EinsatzAbsch.Replace(EinsatzOrt,"");
		if(EinsatzAbsch.EndsWith("-", StringComparison.InvariantCultureIgnoreCase)) EinsatzAbsch = EinsatzAbsch.Substring(0,EinsatzAbsch.Length-1);

		System.Collections.Generic.Dictionary<string, string> Result = new System.Collections.Generic.Dictionary<string, string> ();

		Result.Add ("EinsatzNr", EinsatzNr);
		Result.Add ("EinsatzStrasse", EinsatzStrasse);
		Result.Add ("EinsatzOrt", EinsatzOrt);
		Result.Add ("EinsatzAbschnitt", EinsatzAbsch);
		Result.Add ("EinsatzKreuzung", EinsatzKreuz);
		Result.Add ("EinsatzObjekt", EinsatzObjekt);
		Result.Add ("EinsatzBemerkung", Bemerkung);
		Result.Add ("EinsatzPrioritaet", Prio);
		Result.Add ("EinsatzStichwort", Stichwort);
		Result.Add ("EinsatzSchlagwort", Schlagwort);

		RaiseFinish ("text", Result);
	}
}
}

