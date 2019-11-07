using SMTPd.Smtp;
using SMTPd.Smtp.Config;
using System;
using System.IO;
using System.Text;

namespace FeuerwehrCloud.SMTP
{
	class ExampleDataResponder : SmtpDataResponder
    {
        private readonly string _mailDir;

        public ExampleDataResponder(ISmtpServerConfiguration configuration, string mailDir)
            : base(configuration)
        {
            _mailDir = mailDir;
            EnsureDirExists(mailDir);
        }

        private static void EnsureDirExists(string directory)
        {
            if (!Directory.Exists(directory))
                Directory.CreateDirectory(directory);
        }

        public override SmtpResponse DataStart(ISmtpSessionInfo sessionInfo)
        {
			FeuerwehrCloud.Helper.Logger.WriteLine("|  < [SMTPServer] *** Start receiving mail: {0}", GetFileNameFromSessionInfo(sessionInfo));
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

			//FeuerwehrCloud.Helper.Logger.WriteLine("{0} <<< {1}", fileName, Encoding.UTF8.GetString(lineBuf));

            using (var stream = File.OpenWrite(fileName))
            {
                stream.Seek(0, SeekOrigin.End);
                stream.Write(lineBuf, 0, lineBuf.Length);

                stream.WriteByte(13);
                stream.WriteByte(10);
            }

            return SmtpResponse.None;
        }

        public override SmtpResponse DataEnd(ISmtpSessionInfo sessionInfo)
        {
            var fileName = GetFileNameFromSessionInfo(sessionInfo);
            var size = GetFileSize(fileName);

			FeuerwehrCloud.Helper.Logger.WriteLine("|  < [SMTPServer] *** Mail received ({0} bytes): {1}", size, fileName);

            var successMessage = String.Format("{0} bytes received", size);
            var response = SmtpResponse.OK.CloneAndChange(successMessage);

            return response;
        }

        private long GetFileSize(string fileName)
        {
            return new FileInfo(fileName).Length;
        }
    }
}
