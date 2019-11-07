using System;
using System.Text;
using WhatsAppApi;
using WhatsAppApi.Account;
using WhatsAppApi.Helper;
using WhatsAppApi.Register;
using FeuerwehrCloud.Plugin;

namespace FeuerwehrCloud.Output
{
	public class WhatsAppOutput : FeuerwehrCloud.Plugin.IPlugin 
	{
	
		private WhatsAppApi.WhatsApp wa;
		private System.Collections.Generic.Dictionary<string, string> WAConfig = new System.Collections.Generic.Dictionary<string, string> ();
		public event PluginEvent Event;


		public string Name {
			get {
				return "WhatsApp";
			}
		}
		public string FriendlyName {
			get {
				return "WhatsApp Nachrichtendienst";
			}
		}

		public Guid GUID {
			get {
				return new Guid ("3");
			}
		}

		public byte[] Icon {
			get {
				return System.IO.File.ReadAllBytes("");
			}
		}

		public bool IsAsync
		{
			get { return true; }
		}

		public ServiceType ServiceType
		{
			get { return ServiceType.output; }
		}

		public void Dispose() {
		}

		public void Execute(params object[] list) {
			try {
				var tmpEncoding = Encoding.UTF8;
				System.Console.OutputEncoding = Encoding.Default;
				System.Console.InputEncoding = Encoding.Default;

				wa = new WhatsAppApi.WhatsApp(WAConfig["Sender"], WAConfig["Password"], WAConfig["NickName"], false);
				wa.OnLoginSuccess += wa_OnLoginSuccess;
				wa.OnConnectSuccess += wa_OnConnectSuccess;
				wa.OnError += wa_OnError;
				wa.OnConnectFailed += wa_ConnectFailed;
				wa.OnLoginFailed += wa_LoginFailed;
				wa.Connect();
				wa.Login();
				string MType = ((string)list [0]);
				switch (MType) {
				case "location":
					wa.SendMessageLocation(WAConfig["Target"], double.Parse((string)list [1],System.Globalization.CultureInfo.InvariantCulture.NumberFormat), double.Parse((string)list [2],System.Globalization.CultureInfo.InvariantCulture.NumberFormat), (string)(list [3]), "");
					break;
				case "text":
					wa.SendMessage (WAConfig["Target"], ((string)list [1]));
					break;
				case "picture":
					wa.SendMessageImage (WAConfig["Target"], System.IO.File.ReadAllBytes(((string)list [1])), WhatsAppApi.ApiBase.ImageType.PNG);
					break;
				case "audio":
					wa.SendMessageAudio (WAConfig["Target"], System.IO.File.ReadAllBytes(((string)list [1])), ApiBase.AudioType.MP3);
					break;
				case "video":
					wa.SendMessageVideo (WAConfig["Target"], System.IO.File.ReadAllBytes(((string)list [1])), ApiBase.VideoType.MP4);
					break;
				}
				de.SYStemiya.Helper.Logger.WriteLine ("|  > [WhatsApp] *** Message("+((string)(list [0])).ToUpper()+") sent: " + list[1]);
				PluginEvent messageSent = Event;
				if (messageSent != null)
					messageSent(this, 2,2,2,2,2,2);
			} catch (Exception ex) {
				de.SYStemiya.Helper.Logger.WriteLine (ex.Message);
				de.SYStemiya.Helper.Logger.WriteLine (ex.StackTrace);
				if (ex.InnerException != null) {
					de.SYStemiya.Helper.Logger.WriteLine (ex.InnerException.Message);
					de.SYStemiya.Helper.Logger.WriteLine (ex.InnerException.StackTrace);
				}
			}
		}

		private FeuerwehrCloud.Plugin.IHost My;
		public bool Initialize(IHost hostApplication) {
			My = hostApplication;


			if(!System.IO.File.Exists("WhatsApp.cfg")) {
				WAConfig.Add ("Target", "");
				WAConfig.Add ("NickName", "");
				WAConfig.Add ("Sender", "");
				WAConfig.Add ("Password", "");
				de.SYStemiya.Helper.AppSettings.Save(WAConfig,"WhatsApp.cfg");
			} 
			WAConfig = de.SYStemiya.Helper.AppSettings.Load ("WhatsApp.cfg");

			//string nickname = "FFW-Neubeuern";
			//string sender = "4980353749";
			//string password = "9QhpZsVLKJAqg7RQdr0hpmqpA5Q=";
			//"491739561779"; //"4980353749-1391848501"
			// Prepare WhatsApp
			de.SYStemiya.Helper.Logger.WriteLine ("|  *** WhatsApp loaded...");

			return true;
		}

		private void wa_OnLoginSuccess(string phonenum, byte[] data)
		{
			de.SYStemiya.Helper.Logger.WriteLine ("|  > [WhatsApp] *** Login success");
		}

		void wa_OnConnectSuccess ()
		{
			de.SYStemiya.Helper.Logger.WriteLine ("|  > [WhatsApp] *** Connection success");
		}

		void wa_LoginFailed (string data)
		{
			de.SYStemiya.Helper.Logger.WriteLine ("|||  > [WhatsApp] *** Login failed! " + data);
		}

		void wa_ConnectFailed (Exception ex)
		{
			de.SYStemiya.Helper.Logger.WriteLine ("|||  > [WhatsApp] *** Connection failed!");
			de.SYStemiya.Helper.Logger.WriteLine (ex.ToString());
		}

		void wa_OnError (string id, string from, int code, string text)
		{
			de.SYStemiya.Helper.Logger.WriteLine ("|||  > [WhatsApp] *** ERROR! " + text);
		}

		public WhatsAppOutput()
		{
		}

	}


}

