using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LumiSoft.Net.IMAP.Client;
using LumiSoft.Net.IMAP;
using LumiSoft.Net;
using LumiSoft.Net.Mail;
using LumiSoft.Net.MIME;
using D.Net.EmailInterfaces;

namespace D.Net.EmailClient
{
    public class IMAP_Wrapper : IEmailClient
    {
        public event EmailClient_OnMessagesLoaded OnMessagesLoaded;

        private IMAP_Client Client = null;
        private List<IEmail> _Messages = new List<IEmail>();
        private String _CurrentFolder;
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
                Client = new IMAP_Client();
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

        public void SetCurrentFolder(String folder)
        {
            if (!_IsConnected) throw new EMailException { ExceptionType = EMAIL_EXCEPTION_TYPE.NOT_CONNECTED };
            Client.SelectFolder(folder);
            _CurrentFolder = folder;
        }

        /// <summary>
        /// Examples:
        ///		2        -> seq-number (2)
        ///		2:4      -> seq-range  (from 2 - 4)
        ///		2:*      -> seq-range  (from 2 to last)
        ///		2,3,10:* -> sequence-set (seq-number,seq-number,seq-range)
        ///		                       (2,3, 10 - last)
        /// </summary>
        /// <param name="start"></param>
        /// <param name="end"></param>
        public void LoadMessages(String start, String end)
        {
			Messages.Clear();
            if (!_IsConnected) throw new EMailException { ExceptionType = EMAIL_EXCEPTION_TYPE.NOT_CONNECTED };
            if (!String.IsNullOrWhiteSpace(_CurrentFolder))
            {
                Client.Fetch(
                    false,

                    IMAP_t_SeqSet.Parse(start + ":" + end),
                    new IMAP_t_Fetch_i[]
                    {
                        new IMAP_t_Fetch_i_Envelope(),
                        new IMAP_t_Fetch_i_Flags(),
                        new IMAP_t_Fetch_i_InternalDate(),
                        new IMAP_t_Fetch_i_Rfc822Size(),
                        new IMAP_t_Fetch_i_Uid()
                    },
                    (s, e) =>
                    {
                        try
                        {
                            IMAP_r_u_Fetch fetchResp = (IMAP_r_u_Fetch)e.Value;

                            string from = "";
							if (fetchResp.Envelope.From != null)
                            {
                                for (int i = 0; i < fetchResp.Envelope.From.Length; i++)
                                {
                                    // Don't add ; for last item
                                    if (i == fetchResp.Envelope.From.Length - 1)
                                    {
                                        from += fetchResp.Envelope.From[i].ToString();
                                    }
                                    else
                                    {
                                        from += fetchResp.Envelope.From[i].ToString() + ";";
                                    }
                                }
                            }
                            else
                            {
                                from = "<none>";
                            }
                            string Subject = fetchResp.Envelope.Subject != null ? fetchResp.Envelope.Subject : "<none>";
                            string Date = fetchResp.InternalDate.Date.ToString("dd.MM.yyyy HH:mm");
                            string size = ((decimal)(fetchResp.Rfc822Size.Size / (decimal)1000)).ToString("f2") + " kb";

                            Mail_t_Address[] froms = fetchResp.Envelope.From;
                            
                            string Tag = fetchResp.UID.UID.ToString();

                            IMAP_Message_Wrapper wr = new IMAP_Message_Wrapper { Client = Client, SequenceNumber = (int)fetchResp.UID.UID, UID = Tag, Date = fetchResp.InternalDate.Date, Size = fetchResp.Rfc822Size.Size, Subject = Subject };
                            foreach (var item in froms)
                            {
                                if (item is Mail_t_Mailbox)
                                {
                                    wr.From.Add(((Mail_t_Mailbox)item).Address);
                                }
                            }
							//var F = fetchResp.Flags.Flags;
							//if(!F.Contains("\\Deleted"))
								_Messages.Add(wr);
                            
                        }
                        catch (Exception exe)
                        {
                            throw new EMailException
                            {
                                ExceptionType = EMAIL_EXCEPTION_TYPE.ERROR_ON_GET_MESSAGE,
                                InnerException = exe
                            };
                        }
                    }
                );
                if (OnMessagesLoaded != null) OnMessagesLoaded(this);
            }
        }

        public void LoadMessages()
        {
            LoadMessages("1", "*");
        }

        public int GetMessagesCount()
        {
            if (IsConnected)
            {
                return Messages.Count;
            }
            else return 0;
        }


        public void LoadRecentMessages(Int32 lastSequenceNumber)
        {
            LoadMessages();
            List<IEmail> lista = _Messages.OrderBy(x => x.SequenceNumber).ToList();
            var nuovi = (from n in lista
                         where n.SequenceNumber > lastSequenceNumber
                         select n).ToList();
            _Messages = nuovi;
        }
    }

    public class IMAP_Message_Wrapper: IEmail
    {
        private IMAP_Client _Client;
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

        public IMAP_Client Client
        {
            get { return _Client; }
            set { _Client = value; }
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

        public void LoadInfos()
        {
            if (!Client.IsConnected) throw new EMailException { ExceptionType = EMAIL_EXCEPTION_TYPE.NOT_CONNECTED };
            Client.Fetch(
                true,
                IMAP_t_SeqSet.Parse(UID),
                new IMAP_t_Fetch_i[]{
                    new IMAP_t_Fetch_i_Rfc822()
                },
                (sender, e) =>
                {
                    if (e.Value is IMAP_r_u_Fetch)
                    {
                        IMAP_r_u_Fetch fetchResp = (IMAP_r_u_Fetch)e.Value;
                        try
                        {
                            if (fetchResp.Rfc822 != null)
                            {
                                fetchResp.Rfc822.Stream.Position = 0;
                                Mail_Message mime = Mail_Message.ParseFromStream(fetchResp.Rfc822.Stream);
                                fetchResp.Rfc822.Stream.Dispose();

                                if (String.IsNullOrWhiteSpace(mime.BodyText)) _TextBody = mime.BodyHtmlText;
                                else _TextBody = mime.BodyText;
                                Attachments.Clear();
                                foreach (MIME_Entity entity in mime.Attachments)
                                {
                                    IMAP_Mail_Attachment att = new IMAP_Mail_Attachment();
                                    if (entity.ContentDisposition != null && entity.ContentDisposition.Param_FileName != null)
                                    {
                                        att.Text = entity.ContentDisposition.Param_FileName;
                                    }
                                    else
                                    {
                                        att.Text = "untitled";
                                    }
                                    att.Body = ((MIME_b_SinglepartBase)entity.Body).Data;
                                    Attachments.Add(att);
                                }
                            }
                        }
                        catch (Exception exe)
                        {
                            throw new EMailException { ExceptionType = EMAIL_EXCEPTION_TYPE.ERROR_ON_GET_MESSAGE, InnerException = exe };
                        }
                    }
                }
            );
        }

        public Int32 SequenceNumber
        {
            get;
            set;
        }

		public void SetUnread()
		{
			IMAP_t_SeqSet seq = IMAP_t_SeqSet.Parse(_UID + ":" + _UID);
			Client.StoreMessageFlags(true, seq, IMAP_Flags_SetType.Remove, new IMAP_t_MsgFlags(new string[] { IMAP_t_MsgFlags.Seen }));
		}

		public void SetRead()
		{
			IMAP_t_SeqSet seq = IMAP_t_SeqSet.Parse(_UID + ":" + _UID);
			Client.StoreMessageFlags(true, seq, IMAP_Flags_SetType.Add, new IMAP_t_MsgFlags(new string[] { IMAP_t_MsgFlags.Seen }));
		}

        public void SetDeleted(string TC)
		{
			IMAP_t_SeqSet seq = IMAP_t_SeqSet.Parse(_UID + ":" + _UID);
			Client.StoreMessageFlags(true, seq, IMAP_Flags_SetType.Add, new IMAP_t_MsgFlags(new string[] { IMAP_t_MsgFlags.Deleted }));
            Client.Expunge();
		}

		public void SetAnswered()
		{
			IMAP_t_SeqSet seq = IMAP_t_SeqSet.Parse(_UID + ":" + _UID);
			Client.StoreMessageFlags(true, seq, IMAP_Flags_SetType.Add, new IMAP_t_MsgFlags(new string[] { IMAP_t_MsgFlags.Answered }));
		}

		public void SetUnrecent()
		{
			IMAP_t_SeqSet seq = IMAP_t_SeqSet.Parse(_UID + ":" + _UID);
			Client.StoreMessageFlags(true, seq, IMAP_Flags_SetType.Remove, new IMAP_t_MsgFlags(new string[] { IMAP_t_MsgFlags.Recent }));
		}

    }

    public class IMAP_Mail_Attachment : IEMailAttachment
    {
        public String Text { get; set; }
        public Byte[] Body { get; set; }
    }
}
