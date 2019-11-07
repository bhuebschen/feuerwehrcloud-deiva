using System;
using HttpServer.Modules;
using HttpServer.Messages;
using HttpServer.Headers;
using Newtonsoft.Json;

namespace HttpServer.Addons
{
	public class DEIVAModule : IModule
	{
		private readonly FeuerwehrCloud.Plugin.IHost _my;
		SQLite.SQLiteConnection dbConn;
		#region IModule implementation

		public ProcessingResult Process(RequestContext context)
		{
			IRequest request = context.Request;
			IResponse response = context.Response;
			var header = request.Headers["FWCID"].HeaderValue;
			var IsFWCHandy = header != null;
			bool IsAuthed = false;
			if (IsFWCHandy) {
				IsAuthed = (dbConn.Find<users>(x => x.mobile_id == header)).mobile_id == header;
			}
			string output = "";
			string R = System.IO.Path.GetFileName(request.Uri.LocalPath);

			if (R == "register") {
				var xUsr = new users {
					mobile_id = header,
					Nachname = request.Parameters["lname"],
					Vorname = request.Parameters["fname"],
					rights = ""
				};
				System.Net.WebClient WC = new System.Net.WebClient();
				WC.DownloadString("https://www.feuerwehrcloud.de/deiva/regalert.php?fname="+request.Parameters["fname"]+"&lname="+request.Parameters["lname"]+"&id="+header+"&fwid="+System.Environment.MachineName);

				dbConn.Insert(xUsr);
			} else if (R == "alarms" && IsAuthed) {
				System.Collections.Generic.Dictionary<string, string> Data = new System.Collections.Generic.Dictionary<string, string>();
				foreach (var item in System.IO.Directory.GetFiles("public/alarms/","*.txt")) {
					//var F = new System.Collections.Generic.Dictionary<string, string>();
					Data.Add(item,System.IO.File.ReadAllText(item.Replace(".txt",".xml")));
				}
				output = JsonConvert.SerializeObject(Data, Formatting.Indented);
			} else if (R == "mydata" && IsAuthed) {
				
			} else {
				return ProcessingResult.Continue;
			}
			// Set default content type
			response.ContentType = new ContentTypeHeader("text/html");

			//ProcessHeaders(response, CgiFeuerwehrCloud.Helper.ParseCgiHeaders(ref output));

			response.ContentLength.Value = output.Length;

			ResponseWriter generator = new ResponseWriter();
			generator.SendHeaders(context.HttpContext, response);
			generator.Send(context.HttpContext, output, System.Text.Encoding.UTF8);
			return ProcessingResult.Abort;
		}

		#endregion

		public DEIVAModule(FeuerwehrCloud.Plugin.IHost hostApplication)
		{
			_my = hostApplication;
			dbConn = new SQLite.SQLiteConnection("mobile.db", false);
		}
	}
}

