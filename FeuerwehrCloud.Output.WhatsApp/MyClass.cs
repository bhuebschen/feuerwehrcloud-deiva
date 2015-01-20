using System;
using System.Text;
using WhatsAppApi;
using WhatsAppApi.Account;
using WhatsAppApi.Helper;
using WhatsAppApi.Register;
using FeuerwehrCloud.Plugin;
using System.ComponentModel;

namespace FeuerwehrCloud.Output.WhatsApp
{
	public class WhatsAppOutput : FeuerwehrCloud.Plugin.IPlugin 
	{
	
		private WhatsAppApi.WhatsApp wa;
		private System.Collections.Generic.Dictionary<string, string> WAConfig = new System.Collections.Generic.Dictionary<string, string> ();
		public event PluginEvent Event;

		public bool IsAsync
		{
			get { return true; }
		}

		public ServiceType ServiceType
		{
			get { return ServiceType.output; }
		}

		public void Dispose() {
			wa.Disconnect ();
			wa = null;
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
					wa.SendMessageLocation(WAConfig["Target"], (double)(list [1]), (double)(list [2]), (string)(list [3]), "");
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
				de.SYStemiya.Helper.Logger.WriteLine ("| ["+System.DateTime.Now.ToString("T") +"] |-> [WhatsApp] *** Message("+((string)(list [0])).ToUpper()+") sent: " + list[1]);
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

		private IHost My;
		public bool Initialize(IHost hostApplication) {
			My = hostApplication;
			de.SYStemiya.Helper.Logger.WriteLine ("| ["+System.DateTime.Now.ToString("T") +"] |-> [WhatsApp] *** Initializing...");


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
			return true;
		}

		private void wa_OnLoginSuccess(string phonenum, byte[] data)
		{
			de.SYStemiya.Helper.Logger.WriteLine ("| ["+System.DateTime.Now.ToString("T") +"] |-> [WhatsApp] *** Login success");
		}

		void wa_OnConnectSuccess ()
		{
			de.SYStemiya.Helper.Logger.WriteLine ("| ["+System.DateTime.Now.ToString("T") +"] |-> [WhatsApp] *** Connection success");
		}

		void wa_LoginFailed (string data)
		{
			de.SYStemiya.Helper.Logger.WriteLine ("| ["+System.DateTime.Now.ToString("T") +"] |-> [WhatsApp] *** Login failed! " + data);
		}

		void wa_ConnectFailed (Exception ex)
		{
			de.SYStemiya.Helper.Logger.WriteLine ("| ["+System.DateTime.Now.ToString("T") +"] |-> [WhatsApp] *** Connection failed!");
			de.SYStemiya.Helper.Logger.WriteLine (ex.ToString());
		}

		void wa_OnError (string id, string from, int code, string text)
		{
			de.SYStemiya.Helper.Logger.WriteLine ("| ["+System.DateTime.Now.ToString("T") +"] |-> [WhatsApp] *** ERROR! " + text);
		}

		public WhatsAppOutput()
		{
		}

	}

	#region Actitity
	public class WhatsAppActivityValidator : System.Workflow.ComponentModel.Compiler.ActivityValidator
	{
	}

	[System.Workflow.ComponentModel.Compiler.ActivityValidator(typeof(WhatsAppActivityValidator))]
	[System.Drawing.ToolboxBitmap(typeof(WhatsAppActivity), "whatsapp.ico")]
	public class WhatsAppActivity : System.Workflow.ComponentModel.Activity
	{
		public WhatsAppActivity ()  {
			this.Name = "WhatsAppActivity";
		}

		public static System.Workflow.ComponentModel.DependencyProperty ReceipientProperty = System.Workflow.ComponentModel.DependencyProperty.Register(
			"Receipient", typeof(string), typeof(WhatsAppActivity));
		[Description("Legt den Empfänger fest, in den die Daten geschrieben werden sollen")]
		[Category("Activity")]
		[Browsable(true)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		public string Receipient
		{
			get
			{
				return ((string)base.GetValue(ReceipientProperty));
			}
			set
			{
				base.SetValue(ReceipientProperty, value);
			}
		}

		public enum MessageTypeEnum
		{
			Text = 0,
			Picture = 1,
			Location = 2,
			Video = 3,
			Audio = 4
		}

		public static System.Workflow.ComponentModel.DependencyProperty MessageTypeProperty = System.Workflow.ComponentModel.DependencyProperty.Register(
			"Receipient", typeof(MessageTypeEnum), typeof(WhatsAppActivity));
		[Description("Legt den den Nachrichtentyp fest, mit dem die Nachricht geschickt werden sollen")]
		[Category("Activity")]
		[Browsable(true)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		public MessageTypeEnum MessageType
		{
			get
			{
				return ((MessageTypeEnum)base.GetValue(MessageTypeProperty));
			}
			set
			{
				base.SetValue(MessageTypeProperty, value);
			}
		}

	}
	#endregion

}

