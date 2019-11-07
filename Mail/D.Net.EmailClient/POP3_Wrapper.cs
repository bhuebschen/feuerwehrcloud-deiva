using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LumiSoft.Net;
using LumiSoft.Net.Mail;
using LumiSoft.Net.MIME;
using LumiSoft.Net.POP3.Client;
using D.Net.EmailInterfaces;

namespace D.Net.EmailClient
{
    public class POP3_Wrapper : IEmailClient
    {
        public event EmailClient_OnMessagesLoaded OnMessagesLoaded;

        private POP3_Client Client = null;
        private List<IEmail> _Messages = new List<IEmail>();
        private bool _IsConnected = false;

        public bool IsConnected
        {
            get { return _IsConnected; }
            set { _IsConnected = value; }
        }

        public List<IEmail> Messages
        {
            get { return _Messages; }
            set { _Messages = value; }
        }

        public void Connect(String server, String User, String pass, int port, bool useSSl)
        {
            try
            {
                Client = new POP3_Client();
                Client.Connect(server, port, useSSl);
                Client.Login(User, pass);
                _IsConnected = true;
            }
            catch (Exception exe)
            {
                throw new EMailException { ExceptionType = EMAIL_EXCEPTION_TYPE.ERROR_ON_CONNECTION, InnerException = exe };
            }
        }

        public void Disconnect()
        {
            if (IsConnected)
            {
                Client.Disconnect();
            }
        }

        public int GetMessagesCount()
        {
            if (IsConnected)
            {
                return Client.Messages.Count;
            }
            else return 0;
        }

        public void LoadMessages()
        {
            LoadMessages("1", GetMessagesCount().ToString());
        }

        public void LoadMessages(String start, String end)
        {
            if (!_IsConnected) throw new EMailException { ExceptionType = EMAIL_EXCEPTION_TYPE.NOT_CONNECTED };
            
            int intStart = 0;
            int.TryParse(start, out intStart);
            int intEnd = Client.Messages.Count - 1;
            int.TryParse(end, out intEnd);

            int okEnd = (intEnd > Client.Messages.Count - 1 || intEnd < 1) ? Client.Messages.Count - 1 : intEnd;
            int okStart = (intStart < 0 || intStart > okEnd) ? 0 : intStart;
            for (int i = okStart; i <= okEnd; i++)
            {
                POP3_ClientMessage item = Client.Messages[i];
                POP3_Message_Wrapper wr = new POP3_Message_Wrapper();
                Mail_Message mime = Mail_Message.ParseFromByte(item.MessageToByte());

                string body = mime.BodyText;
                Mail_t_AddressList cc = mime.Cc;
                MIME_Entity[] atts = mime.Attachments;
                Mail_t_AddressList to = mime.To;

                wr.Date = mime.Date;
                foreach (var fr in mime.From)
                {
                    if (fr is Mail_t_Mailbox)
                    {
                        wr.From.Add(((Mail_t_Mailbox)fr).Address);
                    }
                }
                wr.UID = mime.MessageID;
                wr.SequenceNumber = item.SequenceNumber;
                wr.Subject = mime.Subject;
                wr.TextBody = String.IsNullOrWhiteSpace(mime.BodyText) ? mime.BodyHtmlText : mime.BodyText;

                foreach (MIME_Entity entity in mime.Attachments)
                {
                    POP3_Mail_Attachment att = new POP3_Mail_Attachment();
                    if (entity.ContentDisposition != null && entity.ContentDisposition.Param_FileName != null)
                    {
                        att.Text = entity.ContentDisposition.Param_FileName;
                    }
                    else
                    {
                        att.Text = "untitled";
                    }
                    att.Body = ((MIME_b_SinglepartBase)entity.Body).Data;
                    wr.Attachments.Add(att);
                }
                _Messages.Add(wr);
            }
        }

        public void SetCurrentFolder(string folder) { }

        public void LoadRecentMessages(Int32 lastSequenceNumber)
        {
            if (!_IsConnected) throw new EMailException { ExceptionType = EMAIL_EXCEPTION_TYPE.NOT_CONNECTED };
            List<POP3_ClientMessage> lista = new List<POP3_ClientMessage>();
            foreach (POP3_ClientMessage item in Client.Messages)
            {
                lista.Add(item);
            }
            lista = lista.OrderBy(x => x.SequenceNumber).ToList();
            var nuovi = (from n in lista
                         where n.SequenceNumber > lastSequenceNumber
                         select n).ToList();
            foreach (var item in nuovi)
            {
                POP3_Message_Wrapper wr = new POP3_Message_Wrapper();
                Mail_Message mime = Mail_Message.ParseFromByte(item.MessageToByte());

                string body = mime.BodyText;
                Mail_t_AddressList cc = mime.Cc;
                MIME_Entity[] atts = mime.Attachments;
                Mail_t_AddressList to = mime.To;

                wr.Date = mime.Date;
                foreach (var fr in mime.From)
                {
                    if (fr is Mail_t_Mailbox)
                    {
                        wr.From.Add(((Mail_t_Mailbox)fr).Address);
                    }
                }
                wr.UID = mime.MessageID;
                wr.SequenceNumber = item.SequenceNumber;
                wr.Subject = mime.Subject;
                wr.TextBody = String.IsNullOrWhiteSpace(mime.BodyText) ? mime.BodyHtmlText : mime.BodyText;

                foreach (MIME_Entity entity in mime.Attachments)
                {
                    POP3_Mail_Attachment att = new POP3_Mail_Attachment();
                    if (entity.ContentDisposition != null && entity.ContentDisposition.Param_FileName != null)
                    {
                        att.Text = entity.ContentDisposition.Param_FileName;
                    }
                    else
                    {
                        att.Text = "untitled";
                    }
                    att.Body = ((MIME_b_SinglepartBase)entity.Body).Data;
                    wr.Attachments.Add(att);
                }
                _Messages.Add(wr);
            }
        }
    }

    public class POP3_Message_Wrapper : IEmail
    {
        private string _Subject = null;
        private DateTime _Date;
        private long _Size = 0;
        private string _UID = null;
        private string _TextBody = null;
        private List<IEMailAttachment> _Attachments = new List<IEMailAttachment>();
        private List<String> _From = new List<string>();

        public List<String> From
        {
            get { return _From; }
            set { _From = value; }
        }

        public List<IEMailAttachment> Attachments
        {
            get { return _Attachments; }
            set { _Attachments = value; }
        }

        public string TextBody
        {
            get { return _TextBody; }
            set { _TextBody = value; }
        }

        public string UID
        {
            get { return _UID; }
            set { _UID = value; }
        }

        public long Size
        {
            get { return _Size; }
            set { _Size = value; }
        }

        public DateTime Date
        {
            get { return _Date; }
            set { _Date = value; }
        }

        public string Subject
        {
            get { return _Subject; }
            set { _Subject = value; }
        }

        public Int32 SequenceNumber
        {
            get;
            set;
        }

        public void LoadInfos() { }

		public void SetUnread()
		{
		}

		public void SetRead()
		{
		}

		public void SetDeleted(string tc)
		{
		}

		public void SetAnswered()
		{
		}

		public void SetUnrecent()
		{
		}

    }

    public class POP3_Mail_Attachment: IEMailAttachment
    {
        public String Text { get; set; }
        public Byte[] Body { get; set; }
    }
}
