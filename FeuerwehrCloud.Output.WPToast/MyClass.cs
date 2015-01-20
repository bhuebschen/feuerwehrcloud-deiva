
using System;
using Butterfly.PushNotification;
using System.Net;
using System.IO;
using System.Net.Sockets;
using System.Net.NetworkInformation;
using System.Collections;
using System.Xml;
using System.Xml.Serialization;
using System.Collections;
using System.Collections.Generic;
using FeuerwehrCloud.Plugin;

namespace FeuerwehrCloud.Output.BosMon
{
	public class BosMon: IPlugin
	{
		#region IPlugin implementation

		private List<BosAlarm.Alarm> _alarme;
		private IHost My;


		protected internal class TelegramSerialize<T>
		{
			public static T FromString(string data)
			{
				int index = data.IndexOf("<EOF>");
				if (index != -1)
				{
					data = data.Substring(0, index);
				}
				T local = default(T);
				try
				{
					XmlSerializer serializer = new XmlSerializer(typeof(T));
					StringReader textReader = new StringReader(data);
					local = (T) serializer.Deserialize(textReader);
					textReader.Close();
				}
				catch
				{
				}
				return local;
			}

			public static string ToString(T obj)
			{
				string str = "";
				try {
					XmlSerializer serializer = new XmlSerializer (typeof(T));
					StringWriter writer = new StringWriter ();
					serializer.Serialize ((TextWriter)writer, obj);
					str = writer.ToString ();
					writer.Close ();
				} catch {
				}
				return str;
			}
		}

		[XmlIgnore]
		internal List<BosAlarm.Alarm> Alarme
		{
			get
			{
				return this._alarme;
			}
			set
			{
				this._alarme = value;
			}
		}



		System.Net.Sockets.TcpListener serverSocket;
		System.Collections.Generic.Dictionary<string, System.Net.Sockets.Socket> clientSocketList = new System.Collections.Generic.Dictionary<string, System.Net.Sockets.Socket> ();
		byte[] buffer = new byte[2048];

		internal class MyToast : Butterfly.PushNotification.WindowsPhoneToastPushNotificationMessage {
			public override PushNotificationType PushNotificationType {
				get {
					return PushNotificationType.Toast;
				}
			}

			internal MyToast() {
				MessagePriority = MessagePriority.Realtime;
				EnableValidation=false;
			}
		}

		internal class MyTile : Butterfly.PushNotification.WindowsPhoneToastPushNotificationMessage {
			public override PushNotificationType PushNotificationType {
				get {
					return PushNotificationType.Tile;
				}
			}

			internal MyTile() {
				MessagePriority = MessagePriority.Realtime;
				EnableValidation=false;
			}
		}

		public event FeuerwehrCloud.Plugin.PluginEvent Event;

		void AcceptCallback (IAsyncResult ar)
		{

			Socket clientSocket = serverSocket.EndAcceptSocket(ar); 
			clientSocketList.Add(clientSocket.GetHashCode().ToString(), clientSocket);
			clientSocket.BeginReceive(buffer, 0, buffer.Length, SocketFlags.None, new AsyncCallback(ReceiveCallback), clientSocket);
			serverSocket.BeginAcceptSocket(new AsyncCallback(AcceptCallback), null);
		}

		void ReceiveCallback (IAsyncResult ar)
		{
			int received = 0;
			Socket current = (Socket)ar.AsyncState;
			received = current.EndReceive (ar);
			byte[] data = new byte[received];

			if (received == 0) {
				return;
			}

			Array.Copy (buffer, data, received);
			string text = System.Text.Encoding.Default.GetString (data);
			de.SYStemiya.Helper.Logger.WriteLine (text);
			text = text.Replace ("<Step>0</Step>","<Step>1</Step>");

			System.Xml.XmlDocument d = new System.Xml.XmlDocument ();
			d.LoadXml (text.Replace("<EOF>",""));
			d ["Handshake"] ["Step"].InnerText  = "1";
			d ["Handshake"] ["Alarme"].InnerXml = "<Alarm><Name>N1</Name><Trigger>T1</Trigger><Type>T2</Type></Alarm>";
			text = d.OuterXml + "<EOF>";
			current.Send (System.Text.Encoding.Default.GetBytes (text), System.Text.Encoding.Default.GetBytes(text).Length, SocketFlags.None);
			buffer = null;
			Array.Resize(ref buffer, current.ReceiveBufferSize);
			current.BeginReceive(buffer, 0, buffer.Length, SocketFlags.None, new AsyncCallback(ReceiveCallback), current);
		}

		public bool Initialize (IHost hostApplication)
		{
			My = hostApplication;
			de.SYStemiya.Helper.Logger.WriteLine ("| ["+System.DateTime.Now.ToString("T") +"] |-> [BosMon-Compatibility] *** Initializing...");
			de.SYStemiya.Helper.Logger.WriteLine ("| ["+System.DateTime.Now.ToString("T") +"] |-> [BosMon-Compatibility] *** Listening on Port 3334 (for BOS Alarm)");
			serverSocket = new TcpListener (IPAddress.Any, 3334);

			serverSocket.Start ();
			serverSocket.BeginAcceptSocket(new AsyncCallback(AcceptCallback), null);   


			return true;
		}

		public void Execute (params object[] list)
		{
//			var toastMessage = new MyToast();
//			toastMessage.Text1 = ((string)list[1]);
//			toastMessage.Text2 = ((string)list[2]);
//			toastMessage.NotificationUri = (string)list [0];
//			toastMessage.CreatePayload ();
//			var result = PushNotifier.SendPushNotificationMessage(toastMessage);
			var tileMessage = new MyTile();
			tileMessage.Text1 = ((string)list[1]);
			tileMessage.Text2 = ((string)list[2]);
			tileMessage.NotificationUri = (string)list [0];
			tileMessage.CreatePayload ();
			var result2 = PushNotifier.SendPushNotificationMessage(tileMessage);

			//throw new NotImplementedException ();
		}

		public bool IsAsync {
			get { return true; }
		}

		public FeuerwehrCloud.Plugin.ServiceType ServiceType {
			get { return FeuerwehrCloud.Plugin.ServiceType.output; }
		}

		#endregion

		#region IDisposable implementation

		public void Dispose ()
		{

		}

		#endregion

	}
}

