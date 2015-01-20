using Simple.MailServer.Logging;
using Simple.MailServer.Smtp;
using Simple.MailServer.Smtp.Config;
using System;
using System.Net;
using FeuerwehrCloud.Plugin;

namespace FeuerwehrCloud.Input.SMTP
{

	public class SMTPSever : IPlugin
	{
		private SmtpServer smtpServer;
		private System.Collections.Generic.Dictionary<string, string> SMTPConfig = new System.Collections.Generic.Dictionary<string, string> ();
		private IHost My;

		public event PluginEvent Event;

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
				de.SYStemiya.Helper.AppSettings.Save(SMTPConfig,"SMTP.cfg");
			} 
			SMTPConfig = de.SYStemiya.Helper.AppSettings.Load ("SMTP.cfg");
			//System.Environment.MachineName + 



			smtpServer = new SmtpServer
			{
				Configuration =
				{
					DefaultGreeting = "ffwronbpi.feuerwehrcloud.de SYStemiya FeuerwehrCloud (FFW-Neubeuern) ESMTP MAIL service ready at"
				}
			};
			smtpServer.DefaultResponderFactory = 
				new DefaultSmtpResponderFactory<ISmtpServerConfiguration>(smtpServer.Configuration)
			{
				DataResponder = new BOSSMTPDataResponder(smtpServer.Configuration, SMTPConfig["RootMailDir"], this)
			};
			de.SYStemiya.Helper.Logger.WriteLine ("| ["+System.DateTime.Now.ToString("T") +"] |-< [SMTPServer] *** Start listening on port: " + SMTPConfig["SMTPPort"]);
			smtpServer.BindAndListenTo (IPAddress.Any, int.Parse(SMTPConfig["SMTPPort"]));
			return true;
		}

		public SMTPSever() : base() {
			MailServerLogger.Set(new MailServerConsoleLogger(MailServerLogLevel.None));
			de.SYStemiya.Helper.Logger.WriteLine ("| ["+System.DateTime.Now.ToString("T") +"] |-- Loading SMTPServer-Plugin");

		}
	}
}
