using Simple.MailServer.Smtp;
using Simple.MailServer.Smtp.Config;
using System;
using System.IO;
using System.Text;
using System.Net;
using System.Xml;
using System.Xml.Linq;
using System.Net.Sockets;

namespace FeuerwehrCloud
{
	class BOSSMTPDataResponder : DefaultSmtpDataResponder<ISmtpServerConfiguration>
	{
		private readonly string _mailDir;
		System.IO.MemoryStream FS;
		FeuerwehrCloud.Input.SMTP.SMTPSever ParentPlugin;
		private System.Collections.Generic.Dictionary<string, string> BOSConfig = new System.Collections.Generic.Dictionary<string, string> ();

		public BOSSMTPDataResponder(ISmtpServerConfiguration configuration, string mailDir, FeuerwehrCloud.Input.SMTP.SMTPSever ServerObject)
			: base(configuration)
		{
			ParentPlugin = ServerObject;
			//byte[] macaddress = new byte[] {0x00,0x0E,0x7F,0xAA,0x1E,0x89};
			//byte[] macaddress = new byte[] {0x00,0x11,0x85,0xE3,0xA2,0xF2};
			//WakeOnLan(macaddress);

			if(!System.IO.File.Exists("ILS.cfg")) {
				BOSConfig.Add ("Target", "");
				de.SYStemiya.Helper.AppSettings.Save(BOSConfig,"ILS.cfg");
			} 
			BOSConfig = de.SYStemiya.Helper.AppSettings.Load ("ILS.cfg");

			de.SYStemiya.Helper.Logger.WriteLine ("| ["+System.DateTime.Now.ToString("T") +"] |-< [SMTPServer] *** Waiting for Fax from:");
			foreach (var item in BOSConfig.Values) {
				de.SYStemiya.Helper.Logger.WriteLine ("| ["+System.DateTime.Now.ToString("T") +"] |-< [SMTPServer]     " + item);
			}

			_mailDir = mailDir;
			EnsureDirExists(mailDir);
		}

		private static void EnsureDirExists(string directory)
		{
			if (!Directory.Exists(directory))
				Directory.CreateDirectory(directory);
			//Mono.Posix.Syscall.chmod(directory, Mono.Posix.FileMode.)
		}

		public override SmtpResponse DataStart(SmtpSessionInfo sessionInfo)
		{
			//de.SYStemiya.Helper.Logger.WriteLine("Start receiving mail: {0}", GetFileNameFromSessionInfo(sessionInfo));
			FS = new MemoryStream ();
			return SmtpResponse.DataStart;
		}

		private string GetFileNameFromSessionInfo(SmtpSessionInfo sessionInfo)
		{
			var fileName = sessionInfo.CreatedTimestamp.ToString("yyyy-MM-dd_HHmmss_fff") + ".eml";
			var fullName = Path.Combine(_mailDir, fileName);
			return fullName;

		}

		public override SmtpResponse DataLine(SmtpSessionInfo sessionInfo, byte[] lineBuf)
		{
			var fileName = GetFileNameFromSessionInfo(sessionInfo);
			FS.Write(lineBuf, 0, lineBuf.Length);
			FS.WriteByte(13);
			FS.WriteByte(10);
			return SmtpResponse.None;
		}

		public override SmtpResponse DataEnd(SmtpSessionInfo sessionInfo)
		{
			var fileName = GetFileNameFromSessionInfo(sessionInfo);
			using (var stream = File.OpenWrite (fileName)) {
				FS.WriteTo (stream);
				stream.Flush ();
				stream.Close ();
			}
			var size = FS.Length;
			var successMessage = String.Format("{0} bytes received", size);
			var response = SmtpResponse.OK.CloneAndChange(successMessage);
			de.SYStemiya.Helper.Logger.WriteLine ("| ["+System.DateTime.Now.ToString("T") +"] |-< [SMTPServer] *** Finished receiving eMail");
			System.Threading.Thread NWorker = new System.Threading.Thread (new System.Threading.ParameterizedThreadStart (delegate(object obj) {
				try {
					de.SYStemiya.Helper.Logger.WriteLine ("| ["+System.DateTime.Now.ToString("T") +"] |-< [SMTPServer] *** Pass Message to MimeLoader");
					FS.Seek(0, SeekOrigin.Begin);
					FeuerwehrCloud.Mime.Message M =  FeuerwehrCloud.Mime.Message.Load(FS); //new FileInfo((string)obj));
					System.Collections.Generic.List<FeuerwehrCloud.Mime.MessagePart> attachments = M.FindAllAttachments();
					de.SYStemiya.Helper.Logger.WriteLine ("| ["+System.DateTime.Now.ToString("T") +"] |-< [SMTPServer] ***** Loop trough Parts");
					foreach (FeuerwehrCloud.Mime.MessagePart attachment in attachments)
					{

						// Only extract Text if from ILS!
						// Has PDF-Attachment?
						de.SYStemiya.Helper.Logger.WriteLine ("| ["+System.DateTime.Now.ToString("T") +"] |-< [SMTPServer] ****** Has PDF-Attachment?");
						if(attachment.FileName.EndsWith(".pdf", StringComparison.InvariantCultureIgnoreCase)) {
							try {
								// Extract PDF to /tmp/
								de.SYStemiya.Helper.Logger.WriteLine ("| ["+System.DateTime.Now.ToString("T") +"] |-< [SMTPServer] ******* Safe PDF to " + "/tmp/"+attachment.FileName);
								attachment.Save(new FileInfo("/tmp/"+attachment.FileName));
								int count = 0;

								// Print PDF!
								System.Diagnostics.Process P = new System.Diagnostics.Process();
								P.StartInfo.FileName=FeuerwehrCloud.Input.SMTP.Properties.Settings.Default.lp_path;
								P.StartInfo.WorkingDirectory = "/tmp/";
								P.StartInfo.Arguments = " /tmp/"+attachment.FileName;
								P.StartInfo.UseShellExecute = false;
								P.StartInfo.CreateNoWindow=true;
								P.StartInfo.RedirectStandardOutput = true;
								P.Start();
								P.StandardOutput.ReadToEnd ();
								P.WaitForExit ();

								string XSubject = M.Headers.Subject;
								if(XSubject.StartsWith("=?UTF-8?B", StringComparison.InvariantCultureIgnoreCase)) {
									XSubject = System.Text.Encoding.Default.GetString(System.Convert.FromBase64String(XSubject.Substring(XSubject.IndexOf("?B?", StringComparison.InvariantCultureIgnoreCase)+3).Replace("?=","")));
								}
								if(XSubject.EndsWith(")", StringComparison.InvariantCultureIgnoreCase)) {
									XSubject = XSubject.Replace(" ("," ").Replace(")","");
								}

								if(XSubject.StartsWith("Fax von")) {
									XSubject = XSubject.Substring(XSubject.LastIndexOf(" ", StringComparison.InvariantCultureIgnoreCase)+1).Trim();
									de.SYStemiya.Helper.Logger.WriteLine ("| ["+System.DateTime.Now.ToString("T") +"] |-< [SMTPServer] ***** Is FAX-Number of ILS?");
									if(BOSConfig.ContainsValue(XSubject)) {
										// Extract Image from PDF
										new System.Threading.Thread(delegate() {
											try {
												System.Net.Sockets.TcpClient TC = new TcpClient();
												TC.Connect("localhost",27655);
												TC.GetStream().Write(System.Text.Encoding.Default.GetBytes("GET / HTTP/1.0\r\n\r\n"),0,18);
												TC.Close();
											} catch (Exception ex) {
												
											}
										}).Start();

										de.SYStemiya.Helper.Logger.WriteLine ("| ["+System.DateTime.Now.ToString("T") +"] |-< [SMTPServer] ******* Extract PNG from PDF!");
										P = new System.Diagnostics.Process();
										P.StartInfo.FileName=FeuerwehrCloud.Input.SMTP.Properties.Settings.Default.mutool_path;
										P.StartInfo.WorkingDirectory = "/tmp/";
										P.StartInfo.Arguments = " extract /tmp/"+attachment.FileName;
										P.StartInfo.UseShellExecute = false;
										P.StartInfo.CreateNoWindow=true;
										P.StartInfo.RedirectStandardOutput = true;
										P.Start();
										string output = P.StandardOutput.ReadToEnd ();
										P.WaitForExit ();

										string[] Pages = output.Split(new [] {"\n"}, StringSplitOptions.RemoveEmptyEntries);
										for(int i = 0; i<Pages.Length;i++) {
											Pages[i] =  Pages[i].Substring(Pages[i].IndexOf("image ", StringComparison.OrdinalIgnoreCase)+6);
										}
										ParentPlugin.RaiseFinish("pictures", Pages);
									}
										}
										} catch (Exception ex2) {
											Console.ForegroundColor = ConsoleColor.Red;
											Console.Write(ex2);
											Console.ForegroundColor = ConsoleColor.Gray;
										}
										}
										}

										FS = new MemoryStream ();
										} catch (Exception ex) {
											Console.ForegroundColor = ConsoleColor.Red;
											Console.Write(ex);
											Console.ForegroundColor = ConsoleColor.Gray;
										}
										de.SYStemiya.Helper.Logger.WriteLine("| ["+System.DateTime.Now.ToString("T") +"] |-< [SMTPServer] *** Done!");
										}));
									NWorker.Start(fileName);		
									return response;
								}

								private long GetFileSize(string fileName)
								{
									return new FileInfo(fileName).Length;
								}
							}
						}
