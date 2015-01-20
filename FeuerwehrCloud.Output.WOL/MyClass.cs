using System;
using System.Net;
using System.Net.Sockets;
using FeuerwehrCloud.Plugin;
using System.Collections;
using System.Collections.Generic;


/*


*/
using System.ComponentModel;


namespace FeuerwehrCloud.Output.WOL
{
	public class MagicPacket : IPlugin
	{

		public event PluginEvent Event;

		private IHost My;

		public bool Initialize(IHost hostApplication) {
			My = hostApplication;
			de.SYStemiya.Helper.Logger.WriteLine ("| ["+System.DateTime.Now.ToString("T") +"] |-> [WakeUp On LAN] *** Initializing...");
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
			de.SYStemiya.Helper.Logger.WriteLine ("| ["+System.DateTime.Now.ToString("T") +"] |-> [WakeUp On LAN] *** Unloading...");
		}

		public void RaiseFinish(params object[] list) {
			PluginEvent messageSent = Event;
			if (messageSent != null)
			{
				messageSent(this, list);
			}
		}

		public void Execute(params object[] list) {
			WakeOnLan ((string)(list[0]));
		}


		public MagicPacket ()
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
		private void WakeOnLan(string mac)
		{
			WakeOnLan (StrToByteArray (mac));
			de.SYStemiya.Helper.Logger.WriteLine ("| ["+System.DateTime.Now.ToString("T") +"] |-> [WakeUp On LAN] *** Waking Up " + mac);
		}

		private void WakeOnLan(byte[] mac)
		{
			// WOL packet is sent over UDP 255.255.255.0:40000.
			UdpClient client = new UdpClient();
			client.Connect(IPAddress.Broadcast, 40000);

			// WOL packet contains a 6-bytes trailer and 16 times a 6-bytes sequence containing the MAC address.
			byte[] packet = new byte[17*6];

			// Trailer of 6 times 0xFF.
			for (int i = 0; i < 6; i++)
				packet[i] = 0xFF;

			// Body of magic packet contains 16 times the MAC address.
			for (int i = 1; i <= 16; i++)
				for (int j = 0; j < 6; j++)
					packet[i*6 + j] = mac[j];

			// Send WOL packet.
			client.Send(packet, packet.Length);
		}
	}

	#region Actitity
	public class WakeUpOnLANActivityValidator : System.Workflow.ComponentModel.Compiler.ActivityValidator
	{
	}

	[System.Workflow.ComponentModel.Compiler.ActivityValidator(typeof(WakeUpOnLANActivityValidator))]
	[System.Drawing.ToolboxBitmap(typeof(WakeUpOnLANActivity), "wol.ico")]
	public class WakeUpOnLANActivity : System.Workflow.ComponentModel.Activity
	{
		public WakeUpOnLANActivity ()  {
			this.Name = "WakeUpOnLANActivity";
		}

		public static System.Workflow.ComponentModel.DependencyProperty MacAddressProperty = System.Workflow.ComponentModel.DependencyProperty.Register(
			"Dateiname", typeof(string), typeof(WakeUpOnLANActivity));
		[Description("Legt die MAC-Adresse des Systems fest das geweckt werden soll")]
		[Category("Activity")]
		[Browsable(true)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		public string MacAddress
		{
			get
			{
				return ((string)base.GetValue(MacAddressProperty));
			}
			set
			{
				base.SetValue(MacAddressProperty, value);
			}
		}
	}
	#endregion
}

