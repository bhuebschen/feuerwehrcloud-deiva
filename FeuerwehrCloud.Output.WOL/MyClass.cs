using System;
using System.Net;
using System.Net.Sockets;
using FeuerwehrCloud.Plugin;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;


namespace FeuerwehrCloud.Output
{
    
    public class WakeOnLan : IPlugin
	{

		public event PluginEvent Event;

		private FeuerwehrCloud.Plugin.IHost My;


		public string Name {
			get {
				return "WakeOnLan";
			}
		}
		public string FriendlyName {
			get {
				return "WakeUp on LAN-Sender";
			}
		}

		public Guid GUID {
			get {
				return new Guid ("6");
			}
		}

		public byte[] Icon {
			get {
				var assembly = typeof(FeuerwehrCloud.Output.WakeOnLan).GetTypeInfo().Assembly;
				string[] resources = assembly.GetManifestResourceNames();
				Stream stream = assembly.GetManifestResourceStream("icon.ico");
				return ((MemoryStream)stream).ToArray();
			}
		}

		public bool Initialize(IHost hostApplication) {
			My = hostApplication;
			FeuerwehrCloud.Helper.Logger.WriteLine ("|  *** WakeUp On LAN loaded...");
			return true;
		}

		public bool IsAsync
		{
			get { return true; }
		}


		public ServiceType ServiceType
		{
			get { return ServiceType.output; }
		}

		public void Dispose() {
			FeuerwehrCloud.Helper.Logger.WriteLine ("|  > [WakeUp On LAN] *** Unloading...");
		}

		public void RaiseFinish(params object[] list) {
			PluginEvent messageSent = Event;
			if (messageSent != null)
			{
				messageSent(this, list);
			}
		}

		public void Execute(params object[] list) {
			WakeUp ((string)(list[0]));
		}


		public WakeOnLan ()
		{
		}
	
		public byte[] StrToByteArray(string str)
		{
			Dictionary<string, byte> hexindex = new Dictionary<string, byte>();
			for (byte i = 0; i < 255; i++)
				hexindex.Add(i.ToString("X2"), i);

			List<byte> hexres = new List<byte>();
			for (int i = 0; i < str.Length; i += 2)            
				hexres.Add(hexindex[str.Substring(i, 2)]);

			return hexres.ToArray();
		}
		private void WakeUp(string mac)
		{
			WakeUp (StrToByteArray (mac));
			FeuerwehrCloud.Helper.Logger.WriteLine ("|  > [WakeUp On LAN] *** Waking Up " + mac);
		}

		private void WakeUp(byte[] mac)
		{
			UdpClient client = new UdpClient();
			client.Connect(IPAddress.Broadcast, 40000);
			byte[] packet = new byte[17*6];
			for (int i = 0; i < 6; i++)
				packet[i] = 0xFF;
			for (int i = 1; i <= 16; i++)
				for (int j = 0; j < 6; j++)
					packet[i*6 + j] = mac[j];
			client.Send(packet, packet.Length);
		
			client = new UdpClient();
			client.Connect(IPAddress.Broadcast, 7);
			client.Send(packet, packet.Length);

			client = new UdpClient();
			client.Connect(IPAddress.Broadcast, 9);
			client.Send(packet, packet.Length);

		
		}

	}

	class MagicPackets : UdpClient {
		public MagicPackets() : base() {
		}

		public void SetClientToBrodcastMode() {
			if(this.Active) this.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Broadcast, 0);
		}
	}
}

