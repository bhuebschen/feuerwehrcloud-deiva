using System;
using System.IO;
using System.Net;

namespace FeuerwehrCloud.Processor.ELDIS
{
	public class ELDIS : FeuerwehrCloud.Plugin.IPlugin
	{
		FeuerwehrCloud.Plugin.IHost my;
		private System.Collections.Generic.Dictionary<string, string> ELDISConfig = new System.Collections.Generic.Dictionary<string, string> ();
		#region IPlugin implementation

		public event FeuerwehrCloud.Plugin.PluginEvent Event;

		public bool Initialize (FeuerwehrCloud.Plugin.IHost hostApplication)
		{
			my = hostApplication;
			de.SYStemiya.Helper.Logger.WriteLine ("| ["+System.DateTime.Now.ToString("T") +"] |-+ [ELDIS] *** Initializing...");
			if(!System.IO.File.Exists("ELDIS.cfg")) {
				de.SYStemiya.Helper.Logger.WriteLine ("| ["+System.DateTime.Now.ToString("T") +"] |-+ [ELDIS] *** ERROR! NO CONFIGURATION FOUND!");
				ELDISConfig.Add ("IP", "");
				ELDISConfig.Add ("Username", "");
				ELDISConfig.Add ("Password", "");
				de.SYStemiya.Helper.AppSettings.Save(ELDISConfig,"ELDIS.cfg");
			} 
			ELDISConfig = de.SYStemiya.Helper.AppSettings.Load ("ELDIS.cfg");
			return true;
		}

		public void Execute (params object[] list)
		{
			Console.Write ("| ["+System.DateTime.Now.ToString("T") +"] |-+ [ELDIS] *** Fetching data... ");

			CookieContainer cookieJar = new CookieContainer();
			HttpWebRequest request = (HttpWebRequest)WebRequest.Create("https://" + ELDISConfig["IP"] + "/login");
			request.CookieContainer = cookieJar;
			request.UserAgent ="Mozilla/5.0 (U; Linux armv6l; de_DE) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/36 Safari/537.36 Gecko/20140101 FeuerwehrCloud/1.0";
			HttpWebResponse response = (HttpWebResponse)request.GetResponse();
			int cookieCount = cookieJar.Count;
			System.IO.Stream GRS = response.GetResponseStream ();
			StreamReader SR = new StreamReader (GRS);
			string Result = SR.ReadToEnd ();

			string authenticityToken = Result.Substring (Result.IndexOf ("authenticityToken", StringComparison.InvariantCultureIgnoreCase));
			authenticityToken = authenticityToken.Substring (authenticityToken.IndexOf ("value=", StringComparison.InvariantCultureIgnoreCase) + 7);
			authenticityToken = authenticityToken.Substring (0, authenticityToken.IndexOf ("\"", StringComparison.InvariantCultureIgnoreCase));

			GRS.Close ();
			request.Abort ();
			request = (HttpWebRequest)WebRequest.Create("https://" + ELDISConfig["IP"] + "/login");
			request.CookieContainer = cookieJar;
			request.UserAgent ="Mozilla/5.0 (U; Linux armv6l; de_DE) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/36 Safari/537.36 Gecko/20140101 FeuerwehrCloud/1.0";
			string PostData = "authenticityToken=" + authenticityToken + "&username=" + ELDISConfig["Username"] + "&password="+System.Web.HttpUtility.UrlEncode(ELDISConfig["Password"])+"&submit=Anmeldung";
			request.ContentLength = PostData.Length;
			request.ContentType = "application/x-www-form-urlencoded";
			request.Method = "POST";
			Stream dataStream = request.GetRequestStream ();
			dataStream.Write (System.Text.Encoding.Default.GetBytes(PostData), 0, PostData.Length);
			dataStream.Close ();
			response = (HttpWebResponse)request.GetResponse();
			GRS = response.GetResponseStream ();
			SR = new StreamReader (GRS);
			Result = SR.ReadToEnd ();

			string mID = response.ResponseUri.LocalPath;

			string nUri = "https://" + ELDISConfig["IP"] + "/einsatznachbearbeitungberichtbycommonds/berichtliststandarddaten?sEcho=2&iColumns=14&sColumns=&iDisplayStart=0&iDisplayLength=1&mDataProp_0=elrEinsatzNummer&mDataProp_1=formularBoolean1&mDataProp_2=einsatzDatum&mDataProp_3=berichtsart&mDataProp_4=string100n1&mDataProp_5=statusBearbeitung&mDataProp_6=statusVerrechnung&mDataProp_7=string100n3&mDataProp_8=emsEnMandant_kurzzeichen&mDataProp_9=string100n2&mDataProp_10=string100n4&mDataProp_11=string100n5&mDataProp_12=string100n6&mDataProp_13=naechsterBearbeiter&sSearch=&bRegex=false&sSearch_0=&bRegex_0=false&bSearchable_0=true&sSearch_1=&bRegex_1=false&bSearchable_1=true&sSearch_2=&bRegex_2=false&bSearchable_2=true&sSearch_3=&bRegex_3=false&bSearchable_3=true&sSearch_4=&bRegex_4=false&bSearchable_4=true&sSearch_5=&bRegex_5=false&bSearchable_5=true&sSearch_6=&bRegex_6=false&bSearchable_6=true&sSearch_7=&bRegex_7=false&bSearchable_7=true&sSearch_8=&bRegex_8=false&bSearchable_8=true&sSearch_9=&bRegex_9=false&bSearchable_9=true&sSearch_10=&bRegex_10=false&bSearchable_10=true&sSearch_11=&bRegex_11=false&bSearchable_11=true&sSearch_12=&bRegex_12=false&bSearchable_12=true&sSearch_13=&bRegex_13=false&bSearchable_13=true&iSortingCols=1&iSortCol_0=2&sSortDir_0=desc&bSortable_0=true&bSortable_1=true&bSortable_2=true&bSortable_3=true&bSortable_4=true&bSortable_5=true&bSortable_6=true&bSortable_7=true&bSortable_8=true&bSortable_9=true&bSortable_10=true&bSortable_11=true&bSortable_12=true&bSortable_13=true&menueId=3140&konto=" + mID.Substring (1);
			request.Abort ();
			request = (HttpWebRequest)WebRequest.Create(nUri);
			request.CookieContainer = cookieJar;
			request.UserAgent ="Mozilla/5.0 (U; Linux armv6l; de_DE) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/36 Safari/537.36 Gecko/20140101 FeuerwehrCloud/1.0";
			response = (HttpWebResponse)request.GetResponse();
			GRS = response.GetResponseStream ();
			SR = new StreamReader (GRS);
			Result = SR.ReadToEnd ();

			dynamic Berichte = Newtonsoft.Json.JsonConvert.DeserializeObject(Result);
			int Records = Berichte.iTotalRecords;

			System.Collections.Generic.Dictionary<string, string> DataResult = new System.Collections.Generic.Dictionary<string, string> ();

			foreach (var item in Berichte.aaData) {
				DataResult.Add ("EinsatzOrt", item.string100n2 + " " + item.string100n3);
				DataResult.Add ("EinsatzAbschnitt", item.string100n4);
				DataResult.Add ("EinsatzKreuzung", "");
				DataResult.Add ("EinsatzObjekt", item.string100n6);
				DataResult.Add ("EinsatzPrioritaet", "0");
				DataResult.Add ("EinsatzStichwort", item.string100n1);
				DataResult.Add ("EinsatzSchlagwort", item.string100n1);
				DataResult.Add ("EinsatzNr", item.elrEinsatzNummer);
				System.Text.RegularExpressions.Regex RegEx = new System.Text.RegularExpressions.Regex(@"^(?<name>\w[\s\w]+?)\s*(?<num>\d+\s*[a-z]?)$", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
				string Z = item.string100n5;
				var match = RegEx.Split(Z);
				if (match.Length > 1) {
					DataResult.Add ("EinsatzStrasse",  match[1] + " " + match[2]);
				} else {
					DataResult.Add ("EinsatzStrasse",  match[0]);
				}
				request.Abort ();
				request = (HttpWebRequest)WebRequest.Create("https://" + ELDISConfig["IP"] + "/einsatznachbearbeitungberichtbyds/formularinhaltmeldungshow?formulardatenGruppe=23&berichtId="+item.DT_RowId+"&menueId=3140&konto="+mID.Substring(1));
				request.CookieContainer = cookieJar;
				request.UserAgent ="Mozilla/5.0 (U; Linux armv6l; de_DE) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/36 Safari/537.36 Gecko/20140101 FeuerwehrCloud/1.0";
				response = (HttpWebResponse)request.GetResponse();
				GRS = response.GetResponseStream ();
				SR = new StreamReader (GRS);
				Result = SR.ReadToEnd ();
				Result = Result.Substring (Result.IndexOf ("id=\"inhaltMeldung\"", StringComparison.InvariantCulture));
				Result = Result.Substring (Result.IndexOf (">", StringComparison.InvariantCulture) + 1);
				Result = Result.Substring (0, Result.IndexOf ("</te", StringComparison.InvariantCulture) );

				DataResult.Add ("EinsatzBemerkung", Result);


			}
			RaiseFinish ("text", Result);

		}

		public void RaiseFinish(params object[] list) {
			FeuerwehrCloud.Plugin.PluginEvent messageSent = Event;
			if (messageSent != null)
			{
				messageSent(this, list);
			}
		}

		public bool IsAsync {
			get {
				return true;
			}
		}

		public FeuerwehrCloud.Plugin.ServiceType ServiceType {
			get {
				return FeuerwehrCloud.Plugin.ServiceType.processor;
			}
		}

		#endregion

		#region IDisposable implementation

		public void Dispose ()
		{

		}

		#endregion

		public ELDIS ()
		{
		}
	}
}

