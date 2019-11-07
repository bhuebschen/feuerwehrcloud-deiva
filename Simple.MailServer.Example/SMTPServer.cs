using SMTPd.Logging;
using SMTPd.Smtp;
using SMTPd.Smtp.Config;
using System;
using System.Net;
using FeuerwehrCloud.Plugin;
using System.IO;
using System.Reflection;

namespace FeuerwehrCloud.Input
{

	public class SMTPServer : IPlugin
	{
		private SmtpServer smtpServer;
		private System.Collections.Generic.Dictionary<string, string> SMTPConfig = new System.Collections.Generic.Dictionary<string, string> ();
		private FeuerwehrCloud.Plugin.IHost My;

		public event PluginEvent Event;

		public string Name {
			get {
				return "SMTP";
			}
		}
		public string FriendlyName {
			get {
				return "SMTP Faxeingang (AVM)";
			}
		}

		public Guid GUID {
			get {
				return new Guid ("1");
			}
		}

		public byte[] Icon {
			get {
				var assembly = typeof(FeuerwehrCloud.Input.SMTPServer).GetTypeInfo().Assembly;
				string[] resources = assembly.GetManifestResourceNames();
				Stream stream = assembly.GetManifestResourceStream("icon.ico");
				return ((MemoryStream)stream).ToArray();
			}
		}

		public void Dispose() {
			smtpServer.Dispose ();
			smtpServer = null;
		}

		public void RaiseFinish(params object[] list) {
			PluginEvent messageSent = Event;
			if (messageSent != null)
			{
				messageSent(this, list);

			}
		}

		public bool IsAsync
		{
			get { return true; }
		}

		public ServiceType ServiceType
		{
			get { return FeuerwehrCloud.Plugin.ServiceType.input; }
		}

		public void Execute(params object[] list) {
		}

		public bool Initialize(IHost hostApplication) {

			My = hostApplication;

			if(!System.IO.File.Exists("SMTP.cfg")) {
				SMTPConfig.Add ("SMTPPort", "25");
				SMTPConfig.Add ("RootMailDir", "./mail/");
				FeuerwehrCloud.Helper.AppSettings.Save(SMTPConfig,"SMTP.cfg");
			} 
			SMTPConfig = FeuerwehrCloud.Helper.AppSettings.Load ("SMTP.cfg");
			//System.Environment.MachineName + 



			smtpServer = new SmtpServer
			{
				Configuration =
				{
					DefaultGreeting = System.Environment.MachineName +  ".feuerwehrcloud.de FeuerwehrCloud DEIVA BOSMTP MAIL service ready at"
				}
			};
			smtpServer.DefaultResponderFactory = 
				new SmtpResponderFactory(smtpServer.Configuration)
			{
				DataResponder = new BOSSMTPDataResponder(smtpServer.Configuration, SMTPConfig["RootMailDir"], this)
			};
			FeuerwehrCloud.Helper.Logger.WriteLine ("|  *** SMTPServer loaded: listening on port: " + SMTPConfig["SMTPPort"]);
			smtpServer.BindAndListenTo (IPAddress.Any, int.Parse(SMTPConfig["SMTPPort"]));
			return true;
		}

		public SMTPServer() : base() {
			MailServerLogger.Set(new MailServerConsoleLogger(MailServerLogLevel.None));
		}
	}
}
