using System;
using System.Collections.Generic;
using System.IO;
using FeuerwehrCloud.Plugin;

namespace FeuerwehrCloud.Input.USBFrontPanel
{
	public class USBFrontPanel : FeuerwehrCloud.Plugin.IPlugin
	{
		#region IPlugin implementation

		public event FeuerwehrCloud.Plugin.PluginEvent Event;
		System.Threading.Thread BGThread;
		private IHost My;

		void JoypadReader (object obj)
		{
			try {
				using (FileStream fs = new FileStream("/dev/input/js0", FileMode.Open))
				{

					byte[] buff = new byte [8];
					Joystick j = new Joystick();

					while (true)
					{
						// Read 8 bytes from file and analyze.
						fs.Read(buff, 0, 8);
						j.DetectChange(buff);

						// Prints Axis values
						//foreach (byte key in j.Axis.Keys)
						//{
						//	writeLine(top, string.Format("Axis{0}: {1}", key, j.Axis[key]));
						//}

						foreach (byte key in j.Button.Keys)
						{
							de.SYStemiya.Helper.Logger.WriteLine(string.Format("Button{0}: {1}", key, j.Button[key]));
						}

						if(j.Button[0] == true) {
							My.Execute ("FeuerwehrCloud.Output.LCDProc","1", "1", "1. Hauptmenü");
							My.Execute ("FeuerwehrCloud.Output.LCDProc","1", "2", "2. Service");
						}

					}
				}				
			} catch (Exception ex) {
				//Helper.Logger.WriteLine (ex.ToString ());
			}

		}

		public bool Initialize (IHost hostApplication)
		{
			My = hostApplication;
			BGThread = new System.Threading.Thread(JoypadReader);
			BGThread.Start ();
			return true;
		}

		public void Execute (params object[] list)
		{
		}

		public bool IsAsync {
			get {
				return true;
			}
		}

		public FeuerwehrCloud.Plugin.ServiceType ServiceType {
			get {
				return FeuerwehrCloud.Plugin.ServiceType.input;
			}
		}

		#endregion

		#region IDisposable implementation

		public void Dispose ()
		{
		}

		#endregion

		public USBFrontPanel ()
		{
		}

	
	
		internal class Joystick
		{
			public Joystick()
			{
				Button = new Dictionary<byte, bool>();
				Axis   = new Dictionary<byte, short>();
			}

			enum STATE : byte { PRESSED = 0x01, RELEASED = 0x00 }
			enum TYPE  : byte { AXIS = 0x02, BUTTON = 0x01 }
			enum MODE  : byte { CONFIGURATION = 0x80, VALUE = 0x00 }

			/// <summary>
			/// Buttons collection, key: address, bool: value
			/// </summary>
			public Dictionary<byte, bool> Button;

			/// <summary>
			/// Axis collection, key: address, short: value
			/// </summary>
			public Dictionary<byte, short> Axis;

			/// <summary>
			/// Function recognizes flags in buffer and modifies value of button, axis or configuration.
			/// Every new buffer changes only one value of one button/axis. Joystick object have to remember all previous values.
			/// </summary>
			public void DetectChange(byte[] buff)
			{
				// If configuration
				if (checkBit(buff[6], (byte)MODE.CONFIGURATION))
				{
					if (checkBit(buff[6], (byte)TYPE.AXIS))
					{
						// Axis configuration, read address and register axis
						byte key = (byte)buff[7];
						if (!Axis.ContainsKey(key))
						{
							Axis.Add(key, 0);
							return;
						}
					}
					else if (checkBit(buff[6], (byte)TYPE.BUTTON))
					{
						// Button configuration, read address and register button
						byte key = (byte)buff[7];
						if (!Button.ContainsKey(key))
						{
							Button.Add((byte)buff[7], false);
							return;
						}
					}
				}

				// If new button/axis value
				if (checkBit(buff[6], (byte)TYPE.AXIS))
				{
					// Axis value, decode U2 and save to Axis dictionary.
					short value = BitConverter.ToInt16(new byte[2] { buff[4], buff[5] }, 0);
					Axis[(byte)buff[7]] = value;
					return;
				}
				else if (checkBit(buff[6], (byte)TYPE.BUTTON))
				{
					// Bytton value, decode value and save to Button dictionary.
					Button[(byte)buff[7]] = buff[4] == (byte)STATE.PRESSED;
					return;
				}
			}

			/// <summary>
			/// Checks if bits that are set in flag are set in value.
			/// </summary>
			bool checkBit(byte value, byte flag)
			{
				byte c = (byte)(value & flag);
				return c == (byte)flag;
			}
		}
	}
}
