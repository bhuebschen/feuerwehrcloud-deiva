using System;
using FeuerwehrCloud.Plugin;
using System.IO;
using System.Reflection;

namespace FeuerwehrCloud.Output
{
	public class Mail : Plugin.IPlugin
	{
		#region IPlugin implementation

		public event PluginEvent Event;
		private FeuerwehrCloud.Plugin.IHost My;
		public static bool Disposing = false;
		private System.Collections.Generic.Dictionary<string, string> SMTPConfig = new System.Collections.Generic.Dictionary<string, string> ();
		FileSystemWatcher watcher;

		public bool Initialize (IHost hostApplication)
		{
			FeuerwehrCloud.Helper.Logger.WriteLine ("|  *** Mail plugin loaded...");
			if(!System.IO.File.Exists("smtp.cfg")) {
				SMTPConfig.Add ("host", "");
				SMTPConfig.Add ("ssl", "");
				SMTPConfig.Add ("username", "");
				SMTPConfig.Add ("password", "");
				SMTPConfig.Add ("port", "");
				FeuerwehrCloud.Helper.AppSettings.Save(SMTPConfig,"smtp.cfg");
			} 
			SMTPConfig = FeuerwehrCloud.Helper.AppSettings.Load ("smtp.cfg");
			watcher = new FileSystemWatcher ();
			watcher.Path = System.Environment.CurrentDirectory;
			watcher.IncludeSubdirectories = false;
			watcher.Filter = "smtp.cfg";
			watcher.NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.LastAccess | NotifyFilters.DirectoryName | NotifyFilters.FileName;
			watcher.Changed += new FileSystemEventHandler((object sender, FileSystemEventArgs e) => SMTPConfig = FeuerwehrCloud.Helper.AppSettings.Load("smtp.cfg"));
			watcher.EnableRaisingEvents = true;
			return true;
		}

		public void Execute (params object[] list)
		{
			var t = new System.Threading.Thread (delegate() {
				try {
					string To = ((string)list [0]);
					string Subject = ((string)list [1]);
					string Content = ((string)list [2]);
					string[] Receipients = To.Split (new []{ ',' }, StringSplitOptions.RemoveEmptyEntries);
					foreach (var Empfaenger in Receipients) {
						System.Net.Mail.SmtpClient SMTP = new System.Net.Mail.SmtpClient ();
						SMTP.Host = SMTPConfig ["host"];
						SMTP.Port = (SMTPConfig ["port"] != "" ? int.Parse (SMTPConfig ["port"]) : 25);
						SMTP.EnableSsl = bool.Parse (SMTPConfig ["ssl"]);
						SMTP.Credentials = new System.Net.NetworkCredential (SMTPConfig ["username"], SMTPConfig ["password"]);
						System.Net.Mail.MailMessage Msg = new System.Net.Mail.MailMessage ("alarmfax@" + System.Environment.MachineName + ".feuerwehrcloud.de", Empfaenger);
						Msg.Body = Content;
						Msg.Subject = Subject;
						if (list.Length > 3) {
							string AttachFile = ((string)list [3]);
							Msg.Attachments.Add (new System.Net.Mail.Attachment (AttachFile));
						}
						FeuerwehrCloud.Helper.Logger.WriteLine ("|  > [Mail] *** Sending E-Mail to: " + Empfaenger);
						SMTP.Send (Msg);
					}
				} catch (Exception ex) {
					FeuerwehrCloud.Helper.Logger.WriteLine (FeuerwehrCloud.Helper.Helper.GetExceptionDescription (ex));
				}
			});
			t.Start ();
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
				return "Mail";
			}
		}

		public string FriendlyName {
			get {
				return "eMail-Plugin";
			}
		}

		public Guid GUID {
			get {
				return new Guid (Name);
			}
		}

		public byte[] Icon {
			get {
				var assembly = typeof(FeuerwehrCloud.Output.Mail).GetTypeInfo().Assembly;
				string[] resources = assembly.GetManifestResourceNames();
				Stream stream = assembly.GetManifestResourceStream("icon.ico");
				return ((MemoryStream)stream).ToArray();
			}
		}

		#endregion

		#region IDisposable implementation

		public void Dispose ()
		{
			FeuerwehrCloud.Helper.Logger.WriteLine ("|  *** Mail plugin unloaded...");
		}

		#endregion

		public Mail ()
		{
		}
	}
}

