using System;
using FeuerwehrCloud.Plugin;
using System.IO;
using System.Reflection;

namespace FeuerwehrCloud.Output
{
	public class MapsPrinter : Plugin.IPlugin
	{
		#region IPlugin implementation

		public event PluginEvent Event;
		private FeuerwehrCloud.Plugin.IHost My;
		public static bool Disposing = false;
		private System.Collections.Generic.Dictionary<string, string> Configuration = new System.Collections.Generic.Dictionary<string, string> ();
		private System.Collections.Generic.Dictionary<string, string> CopiesConfig = new System.Collections.Generic.Dictionary<string, string> ();
		FileSystemWatcher watcher;

		public bool Initialize (FeuerwehrCloud.Plugin.IHost hostApplication)
		{
			FeuerwehrCloud.Helper.Logger.WriteLine ("|  *** Maps plugin loaded...");
			if(!System.IO.File.Exists("copies.cfg")) {
				CopiesConfig.Add ("count", "1");
				FeuerwehrCloud.Helper.AppSettings.Save(CopiesConfig,"copies.cfg");
			} 
			CopiesConfig = FeuerwehrCloud.Helper.AppSettings.Load ("copies.cfg");
			if(!System.IO.File.Exists("mapprint.cfg")) {
				Configuration.Add ("zoom", "15");
				Configuration.Add ("start", "Feuerwehr");
				FeuerwehrCloud.Helper.AppSettings.Save(Configuration,"mapprint.cfg");
			} 
			Configuration = FeuerwehrCloud.Helper.AppSettings.Load ("mapprint.cfg");

			watcher = new FileSystemWatcher ();
			watcher.Path = System.Environment.CurrentDirectory;
			watcher.IncludeSubdirectories = false;
			watcher.Filter = "mapprint.cfg";
			watcher.NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.LastAccess | NotifyFilters.DirectoryName | NotifyFilters.FileName;
			watcher.Changed += new FileSystemEventHandler((object sender, FileSystemEventArgs e) => Configuration = FeuerwehrCloud.Helper.AppSettings.Load("mapprint.cfg"));
			watcher.EnableRaisingEvents = true;

			return true;
		}

		public void Execute (params object[] list)
		{
			/*if (Configuration ["provider"] == "google") {

			} else if (Configuration ["provider"] == "osm") {

			} else if (Configuration ["provider"] == "bing") {

			}*/
			FeuerwehrCloud.Helper.Logger.WriteLine ("|  +  [Maps] Preparing map...");
			string File = System.IO.File.ReadAllText ("map.html");
			string TF = System.IO.Path.GetTempFileName ()+".html";
			File = File.Replace("{STARTPOS}", Configuration["start"]);
			File = File.Replace("{ZOOM}", Configuration["zoom"]);
			File = File.Replace("{LOCATION}", string.Format("{0},{1}", (string)list[1], (string)list[2]));
			if (((string)list[2]).StartsWith("B")) {
				File = File.Replace("##PRE##", "");
				File = File.Replace("##POST##", "");
			} else {
				File = File.Replace("##PRE##", "/* KEIN BRANDEINSATZ !");
				File = File.Replace("##POST##", "*/");
			}

			System.IO.File.WriteAllText (TF+".html", File);
			//--disable-smart-shrinking 
			FeuerwehrCloud.Helper.Logger.WriteLine ("|  +  [Maps] Printing map...: " + TF + ".pdf");

			//string result1 = FeuerwehrCloud.Helper.Helper.exec ("/bin/wkhtmltopdf", " -O landscape --custom-header  \"Accept-Language\" \"de_DE,de;q=0.8,ja;q=0.6,en;q=0.4\" --enable-javascript --no-stop-slow-scripts --javascript-delay 15000 --no-header-line --no-footer-line --print-media-type --margin-top 2 -s A4 -d 300 "+TF+".html "+TF+".pdf");

			using (System.Net.WebClient wc = new System.Net.WebClient())
			{
				wc.Headers[System.Net.HttpRequestHeader.ContentType] = "application/x-www-form-urlencoded";
				string XData = File.Replace ("{LOCATION}", (string)list [1] + "," + (string)list [2]);
				XData = "data="+System.Web.HttpUtility.UrlEncode (XData);
				byte[] Result = wc.UploadData(new Uri("http://apps.soomba.de/api/html2pdf.php"),"POST", System.Text.Encoding.Default.GetBytes(XData));
				System.IO.File.WriteAllBytes (TF + ".pdf", Result);
			}



			if (System.IO.File.Exists (TF + ".pdf")) {
				string result2 = FeuerwehrCloud.Helper.Helper.exec ("/usr/bin/lpr", " -#"+CopiesConfig["count"]+" -o landscape  -o fit-to-page -o media=A4 " + TF + ".pdf");
			} else {
				FeuerwehrCloud.Helper.Logger.WriteLine("|  +  [Maps] *** MAP CREATION FAILED! ***");
			}
			//if(System.IO.File.Exists(TF+".html")) System.IO.File.Delete(TF+".html");
			//if(System.IO.File.Exists(TF+".pdf")) System.IO.File.Delete(TF+".pdf");
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
				return "Maps";
			}
		}

		public string FriendlyName {
			get {
				return "Plugin zum Drucken von Karten";
			}
		}

		public Guid GUID {
			get {
				return new Guid (Name);
			}
		}

		public byte[] Icon {
			get {
				var assembly = typeof(FeuerwehrCloud.Output.MapsPrinter).GetTypeInfo().Assembly;
				string[] resources = assembly.GetManifestResourceNames();
				Stream stream = assembly.GetManifestResourceStream("icon.ico");
				return ((MemoryStream)stream).ToArray();
			}
		}

		#endregion

		#region IDisposable implementation

		public void Dispose ()
		{
			FeuerwehrCloud.Helper.Logger.WriteLine ("|  *** Maps plugin unloaded...");
		}

		#endregion

		public MapsPrinter ()
		{
		}
	}
}

