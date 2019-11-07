using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace D.Net.EmailInterfaces
{
    public interface IEmail
    {
        List<String> From
        {
            get;
            set;
        }

        List<IEMailAttachment> Attachments
        {
            get;
            set;
        }

        String TextBody
        {
            get;
            set;
        }

        String UID
        {
            get;
            set;
        }

        Int32 SequenceNumber { get; set; }

        long Size
        {
            get;
            set;
        }

        DateTime Date
        {
            get;
            set;
        }

        String Subject
        {
            get;
            set;
        }

        void LoadInfos();
		void SetUnread();
		void SetRead();
        void SetDeleted(string TC);
		void SetAnswered();
		void SetUnrecent();

    }
}
