using System;
using D.Net.EmailInterfaces;
using D.Net.EmailClient;
using System.IO;

namespace FeuerwehrCloud.Input
{
	public class IMAP4 : FeuerwehrCloud.Plugin.IPlugin
	{
		#region IPlugin implementation

		public event FeuerwehrCloud.Plugin.PluginEvent Event;
		private System.Collections.Generic.Dictionary<string, string> IMAPConfig = new System.Collections.Generic.Dictionary<string, string> ();
		FileSystemWatcher watcher;

		public bool Initialize(FeuerwehrCloud.Plugin.IHost hostApplication)
		{
			if(!System.IO.File.Exists("imap.cfg")) {
				IMAPConfig.Add ("user", "");
				IMAPConfig.Add ("password", "");
                IMAPConfig.Add ("server", "");
                IMAPConfig.Add ("trash", "");
				FeuerwehrCloud.Helper.AppSettings.Save(IMAPConfig,"imap.cfg");
			} 
			IMAPConfig = FeuerwehrCloud.Helper.AppSettings.Load ("imap.cfg");

			watcher = new FileSystemWatcher ();
			watcher.Path = System.Environment.CurrentDirectory;
			watcher.IncludeSubdirectories = false;
			watcher.Filter = "imap.cfg";
			watcher.NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.LastAccess | NotifyFilters.DirectoryName | NotifyFilters.FileName;
			watcher.Changed += new FileSystemEventHandler((object sender, FileSystemEventArgs e) => IMAPConfig = FeuerwehrCloud.Helper.AppSettings.Load("imap.cfg"));
			watcher.EnableRaisingEvents = true;


            new System.Threading.Thread(() => {
				while (true) {
					try {
						IEmailClient ImapClient = EmailClientFactory.GetClient(EmailClientEnum.IMAP); 
						ImapClient.Connect(IMAPConfig["server"], IMAPConfig["user"], IMAPConfig["password"],143, false);  
						ImapClient.SetCurrentFolder("INBOX"); 
						ImapClient.LoadMessages();

						if(!ImapClient.IsConnected) {
							ImapClient.Connect(IMAPConfig["server"], IMAPConfig["user"], IMAPConfig["password"],143, false);  
							ImapClient.SetCurrentFolder("INBOX"); 
						}
						ImapClient.SetCurrentFolder("INBOX"); 
						ImapClient.LoadMessages();
						System.Diagnostics.Debug.WriteLine("MSG: "  + ImapClient.Messages.Count.ToString());
						for (int i = 0; i < ImapClient.Messages.Count; i++)
						{
							IEmail msm = (IEmail)ImapClient.Messages[i];
							// Load all infos include attachments
							msm.LoadInfos();
                            #if _ASTERISK_
                            if(msm.Subject.StartsWith("Facsimile received from")) {
                                FeuerwehrCloud.Helper.Logger.WriteLine ("|  < [IMAP4] *** FAX-Mail found!");
                                try {
                                    msm.SetDeleted("");
                                } catch (Exception ex2) {
                                    if(ex2.Message.Contains("Mailbox doesn't exist: INBOX.Trash")) {
                                    }
                                }
                                System.IO.File.WriteAllBytes("/var/spool/hylafax/recvq/"+msm.Attachments[0].Text,msm.Attachments[0].Body);
                            //gs -dNOPAUSE -q -g300x300 -sDEVICE=tiffg4 -dBATCH -sOutputFile=output_file_name.tif input_file_name.pdf

                                string FName = msm.Attachments[0].Text.Replace(".pdf",".tif");
                                FeuerwehrCloud.Helper.Helper.exec("/usr/bin/gs"," -dNOPAUSE -q -sDEVICE=tiffg4 -dBATCH -sOutputFile=/var/spool/hylafax/recvq/"+FName+" "+"/var/spool/hylafax/recvq/"+msm.Attachments[0].Text);
                                System.Net.Sockets.TcpClient PP = new System.Net.Sockets.TcpClient();
                                PP.Connect("localhost",10112);
                                string Telefonnummer = "08031900900"; //msm.TextBody.Substring(msm.TextBody.IndexOf("Rufnummer  : "));
                                //Telefonnummer = Telefonnummer.Substring(Telefonnummer.IndexOf(":")+2);
                                //Telefonnummer = Telefonnummer.Substring(0,Telefonnummer.IndexOf("\r\n"));
                                string Fax = Telefonnummer+"|recvq/"+FName+"\r\n"; 
                                PP.GetStream().Write(System.Text.Encoding.Default.GetBytes(Fax),0,Fax.Length);
                                PP.Close();
                            }
                            #else
                            if(msm.Subject == "Neue Faxnachricht") {
								FeuerwehrCloud.Helper.Logger.WriteLine ("|  < [IMAP4] *** FAX-Mail found!");
								try {
                                    msm.SetDeleted(IMAPConfig["trash"]);
								} catch (Exception ex2) {
									if(ex2.Message.Contains("Mailbox doesn't exist: INBOX.Trash")) {
										//ImapClient.m
									}
								}
								System.IO.File.WriteAllBytes("/var/spool/hylafax/recvq/"+msm.Attachments[0].Text,msm.Attachments[0].Body);
								System.Net.Sockets.TcpClient PP = new System.Net.Sockets.TcpClient();
								PP.Connect("localhost",10112);
								string Telefonnummer = msm.TextBody.Substring(msm.TextBody.IndexOf("Rufnummer  : "));
								Telefonnummer = Telefonnummer.Substring(Telefonnummer.IndexOf(":")+2);
								Telefonnummer = Telefonnummer.Substring(0,Telefonnummer.IndexOf("\r\n"));
								string Fax = Telefonnummer+"|recvq/"+msm.Attachments[0].Text+"\r\n"; 
								PP.GetStream().Write(System.Text.Encoding.Default.GetBytes(Fax),0,Fax.Length);
								PP.Close();
								//System.Threading.Thread.Sleep(10000);
							}
                            #endif

						}
						try {
							ImapClient.Disconnect();
						} catch (Exception ex) {

						}
									System.Threading.Thread.Sleep(1000*20);
					} catch (Exception ex) {									
							System.Diagnostics.Debug.WriteLine(">>"+ex.Message);
					}
																																	System.Threading.Thread.Sleep(1000*20);
																																																																																																																																																																																																																																																																																																																																																																																																																																																																																																																																																																																																																																																																																																																																																																																																																																																																																																																																																																																																																																																																																																																																																																																																																																																																																																																																																																																																																																																																																																																																}


			}).Start();

			FeuerwehrCloud.Helper.Logger.WriteLine ("|  *** IMAP4 loaded for address: " + IMAPConfig["user"]);
			return true;
		}

		public void Execute(params object[] list)
		{
		}

		public bool IsAsync {
			get {
				return true;
			}
		}

		public FeuerwehrCloud.Plugin.ServiceType ServiceType {
			get {
				return FeuerwehrCloud.Plugin.ServiceType.input;
			}
		}

		public string Name {
			get {
				return "IMAP4";
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

		public IMAP4()
		{
		}
	}
}

