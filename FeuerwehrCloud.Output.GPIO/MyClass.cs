using System;
using FeuerwehrCloud.Plugin;
using RaspberryPiDotNet;
using RaspberryPiDotNet.MicroLiquidCrystal;
using RaspberryPiDotNet.TM16XX;
using System.IO;
using System.Reflection;

namespace FeuerwehrCloud.Output
{
	public class GPIO : FeuerwehrCloud.Plugin.IPlugin
	{
		public event PluginEvent Event;

		private FeuerwehrCloud.Plugin.IHost My;

		public string Name {
			get {
				return "GPIO";
			}
		}
		public string FriendlyName {
			get {
				return "Raspberry Pi GPIO Ausgabemodul";
			}
		}

		public Guid GUID {
			get {
				return new Guid ("A");
			}
		}

		public byte[] Icon {
			get {
				var assembly = typeof(FeuerwehrCloud.Output.GPIO).GetTypeInfo().Assembly;
				string[] resources = assembly.GetManifestResourceNames();
				Stream stream = assembly.GetManifestResourceStream("icon.ico");
				return ((MemoryStream)stream).ToArray();
			}
		}

		public bool Initialize(IHost hostApplication) {
			My = hostApplication;
			FeuerwehrCloud.Helper.Logger.WriteLine ("|  *** GPIO loaded...");
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
			FeuerwehrCloud.Helper.Logger.WriteLine ("|  > [GPIO] *** Unloading...");
		}

		public void RaiseFinish(params object[] list) {
			PluginEvent messageSent = Event;
			if (messageSent != null)
			{
				messageSent(this, list);
			}
		}

		public void Execute(params object[] list) {
			string MType = ((string)list [0]);
			switch (MType) {
			case "READ_TEMPERATURE": 
				GPIOPins X1 = (GPIOPins)Enum.Parse (typeof(GPIOPins), (string)list [1], true);
				GPIOFile Sensor1 = new GPIOFile (X1);
				break;
			case "READ_LUMINANCE": 
				GPIOPins X2= (GPIOPins)Enum.Parse (typeof(GPIOPins), (string)list [1], true);
				GPIOFile Sensor2 = new GPIOFile(X2);
				break;
			case "READ_HUMIDITY": 
				GPIOPins X3 = (GPIOPins)Enum.Parse (typeof(GPIOPins), (string)list [1], true);
				GPIOFile Sensor3 = new GPIOFile(X3);
				break;
			case "READ_PRESSURE": 
				GPIOPins X4 = (GPIOPins)Enum.Parse (typeof(GPIOPins), (string)list [1], true);
				GPIOFile Sensor4 = new GPIOFile(X4);
				break;
			case "LEDON":
				new System.Threading.Thread (delegate() {
					GPIOPins X = (GPIOPins)Enum.Parse (typeof(GPIOPins), (string)list [1], true);
					GPIOMem led = new GPIOMem (X);
					led.Write (PinState.High);
				}).Start ();
				break;
			case "LEDOFF":
				new System.Threading.Thread (delegate() {
					GPIOPins X = (GPIOPins)Enum.Parse (typeof(GPIOPins), (string)list [1], true);
					GPIOMem led = new GPIOMem (X);
					led.Write (PinState.Low);
				}).Start ();
				break;
			case "LEDBLINK_SLOW":
				new System.Threading.Thread (delegate() {
					GPIOPins X = (GPIOPins)Enum.Parse (typeof(GPIOPins), (string)list [1], true);
					GPIOMem led = new GPIOMem (X);
					for (int i = 0; i < (int.Parse((string)list [2])); i++) {
						led.Write (PinState.High);
						System.Threading.Thread.Sleep (500);
						led.Write (PinState.Low);
						System.Threading.Thread.Sleep (500);
					}

				}).Start ();
				break;
			case "LEDBLINK_FAST":
				new System.Threading.Thread (delegate() {
					GPIOPins X = (GPIOPins)Enum.Parse (typeof(GPIOPins), (string)list [1], true);
					GPIOMem led = new GPIOMem (X);
					for (int i = 0; i < (int.Parse((string)list [2])); i++) {
						led.Write (PinState.High);
						System.Threading.Thread.Sleep (250);
						led.Write (PinState.Low);
						System.Threading.Thread.Sleep (250);
						led.Write (PinState.High);
						System.Threading.Thread.Sleep (250);
						led.Write (PinState.Low);
						System.Threading.Thread.Sleep (250);
					}

				}).Start ();
				break;			
			case "LEDBLINK_UFAST":
				new System.Threading.Thread (delegate() {
					GPIOPins X = (GPIOPins)Enum.Parse (typeof(GPIOPins), (string)list [1], true);
					GPIOMem led = new GPIOMem (X);
					for (int i = 0; i < (int.Parse((string)list [2])); i++) {
						led.Write (PinState.High);
						System.Threading.Thread.Sleep (125);
						led.Write (PinState.Low);
						System.Threading.Thread.Sleep (125);
						led.Write (PinState.High);
						System.Threading.Thread.Sleep (125);
						led.Write (PinState.Low);
						System.Threading.Thread.Sleep (125);
						led.Write (PinState.High);
						System.Threading.Thread.Sleep (125);
						led.Write (PinState.Low);
						System.Threading.Thread.Sleep (125);
						led.Write (PinState.High);
						System.Threading.Thread.Sleep (125);
						led.Write (PinState.Low);
						System.Threading.Thread.Sleep (125);
					}

				}).Start ();
				break;
			}
		}

		public GPIO () 
		{
		}
	}
}

