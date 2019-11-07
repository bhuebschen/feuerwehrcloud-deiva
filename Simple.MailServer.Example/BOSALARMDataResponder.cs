using SMTPd.Smtp;
using SMTPd.Smtp.Config;
using System;
using System.IO;
using System.Text;
using System.Net;
using System.Xml;
using System.Xml.Linq;
using System.Net.Sockets;

namespace FeuerwehrCloud
{
	class BOSSMTPDataResponder : SmtpDataResponder
	{
		private readonly string _mailDir;
		System.IO.MemoryStream FS;
		FeuerwehrCloud.Input.SMTPServer ParentPlugin;
		private System.Collections.Generic.Dictionary<string, string> BOSConfig = new System.Collections.Generic.Dictionary<string, string> ();

		public BOSSMTPDataResponder(ISmtpServerConfiguration configuration, string mailDir, FeuerwehrCloud.Input.SMTPServer ServerObject)
			: base(configuration)
		{
			ParentPlugin = ServerObject;
			//byte[] macaddress = new byte[] {0x00,0x0E,0x7F,0xAA,0x1E,0x89};
			//byte[] macaddress = new byte[] {0x00,0x11,0x85,0xE3,0xA2,0xF2};
			//WakeOnLan(macaddress);

			if(!System.IO.File.Exists("ILS.cfg")) {
				BOSConfig.Add ("Target", "");
				FeuerwehrCloud.Helper.AppSettings.Save(BOSConfig,"ILS.cfg");
			} 
			BOSConfig = FeuerwehrCloud.Helper.AppSettings.Load ("ILS.cfg");

			FeuerwehrCloud.Helper.Logger.WriteLine ("|  < [SMTPServer] *** Waiting for Fax from:");
			foreach (var item in BOSConfig.Values) {
				FeuerwehrCloud.Helper.Logger.WriteLine ("|  < [SMTPServer]     " + item);
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

		public override SmtpResponse DataStart(ISmtpSessionInfo sessionInfo)
		{
			//FeuerwehrCloud.Helper.Logger.WriteLine("Start receiving mail: {0}", GetFileNameFromSessionInfo(sessionInfo));
			FS = new MemoryStream ();
			return SmtpResponse.DataStart;
		}

		private string GetFileNameFromSessionInfo(ISmtpSessionInfo sessionInfo)
		{
			var fileName = sessionInfo.CreatedTimestamp.ToString("yyyy-MM-dd_HHmmss_fff") + ".eml";
			var fullName = Path.Combine(_mailDir, fileName);
			return fullName;

		}

		public override SmtpResponse DataLine(ISmtpSessionInfo sessionInfo, byte[] lineBuf)
		{
			var fileName = GetFileNameFromSessionInfo(sessionInfo);
			FS.Write(lineBuf, 0, lineBuf.Length);
			FS.WriteByte(13);
			FS.WriteByte(10);
			return SmtpResponse.None;
		}

		public override SmtpResponse DataEnd(ISmtpSessionInfo sessionInfo)
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
			FeuerwehrCloud.Helper.Logger.WriteLine ("|  < [SMTPServer] *** Finished receiving eMail");
			System.Threading.Thread NWorker = new System.Threading.Thread (new System.Threading.ParameterizedThreadStart (delegate(object obj) {
				try {
					FeuerwehrCloud.Helper.Logger.WriteLine ("|  < [SMTPServer] *** Pass Message to MimeLoader");
					FS.Seek(0, SeekOrigin.Begin);
					FeuerwehrCloud.Mime.Message M =  FeuerwehrCloud.Mime.Message.Load(FS); //new FileInfo((string)obj));
					System.Collections.Generic.List<FeuerwehrCloud.Mime.MessagePart> attachments = M.FindAllAttachments();
					FeuerwehrCloud.Helper.Logger.WriteLine ("|  < [SMTPServer] ***** Loop trough Parts");
					foreach (FeuerwehrCloud.Mime.MessagePart attachment in attachments)
					{

						// Only extract Text if from ILS!
						// Has PDF-Attachment?
						FeuerwehrCloud.Helper.Logger.WriteLine ("|  < [SMTPServer] ****** Has PDF-Attachment?");
						if(attachment.FileName.EndsWith(".pdf", StringComparison.InvariantCultureIgnoreCase)) {
							try {
								// Extract PDF to /tmp/
								FeuerwehrCloud.Helper.Logger.WriteLine ("|  < [SMTPServer] ******* Safe PDF to " + "/tmp/"+attachment.FileName);
								attachment.Save(new FileInfo("/tmp/"+attachment.FileName));
								try {
									System.IO.File.Copy ("/tmp/"+attachment.FileName, "public/fax/" + attachment.FileName, true);
								} catch (Exception ex) {
									FeuerwehrCloud.Helper.Logger.WriteLine(FeuerwehrCloud.Helper.Helper.GetExceptionDescription(ex));
								}
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
									FeuerwehrCloud.Helper.Logger.WriteLine ("|  < [SMTPServer] ***** Is FAX-Number of ILS? " + (XSubject==""?"No number supplied...":XSubject));

									if(BOSConfig.ContainsValue(XSubject)) {
										FeuerwehrCloud.Helper.Logger.WriteLine ("|  < [SMTPServer] ******* ILS-Number detected!");
										// Notify running Display-Software!
										new System.Threading.Thread(delegate() {
											try {
												System.Net.Sockets.TcpClient TC = new TcpClient();
												TC.Connect("localhost",27655);
												TC.GetStream().Write(System.Text.Encoding.Default.GetBytes("GET / HTTP/1.0\r\n\r\n"),0,18);
												TC.Close();
											} catch (Exception ex) {

											}
										}).Start();

										// Extract Image from PDF
										FeuerwehrCloud.Helper.Logger.WriteLine ("|  < [SMTPServer] ********* Extract PNG from PDF!");
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
									} else {
										FeuerwehrCloud.Helper.Logger.WriteLine ("|  < [SMTPServer] ******* Does not look like a FAX from ILS...");
									}
								}
							} catch (Exception ex2) {
								Console.ForegroundColor = ConsoleColor.Red;
								FeuerwehrCloud.Helper.Logger.WriteLine(FeuerwehrCloud.Helper.Helper.GetExceptionDescription(ex2));
								Console.ForegroundColor = ConsoleColor.Gray;
							}
						}
					}

					FS = new MemoryStream ();
				} catch (Exception ex) {
					Console.ForegroundColor = ConsoleColor.Red;
					FeuerwehrCloud.Helper.Logger.WriteLine(FeuerwehrCloud.Helper.Helper.GetExceptionDescription(ex));
					Console.ForegroundColor = ConsoleColor.Gray;
				}
				FeuerwehrCloud.Helper.Logger.WriteLine("|  < [SMTPServer] *** Done!");
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
