using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace D.Net.EmailInterfaces
{
    public delegate void EmailClient_OnMessagesLoaded(object sender);

    public interface IEmailClient
    {
        event EmailClient_OnMessagesLoaded OnMessagesLoaded;

        bool IsConnected
        {
            get;
            set;
        }

        List<IEmail> Messages
        {
            get;
            set;
        }

        void Connect(String server, String User, String pass, int port, bool useSSl);

        void Disconnect();

        void SetCurrentFolder(String folder);

        int GetMessagesCount();

        void LoadMessages();

        void LoadMessages(String start, String end);

        void LoadRecentMessages(Int32 lastSequenceNumber);
    }
}
