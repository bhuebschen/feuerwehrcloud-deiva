using System;
using FeuerwehrCloud.Plugin;
using RaspberryPiDotNet;
using RaspberryPiDotNet.MicroLiquidCrystal;
using RaspberryPiDotNet.TM16XX;

namespace FeuerwehrCloud.Output.GPIO
{
	public class GPIO : FeuerwehrCloud.Plugin.IPlugin
	{
		public event PluginEvent Event;

		private IHost My;

		public bool Initialize(IHost hostApplication) {
			My = hostApplication;
			de.SYStemiya.Helper.Logger.WriteLine ("| ["+System.DateTime.Now.ToString("T") +"] |-> [GPIO] *** Initializing...");
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
			de.SYStemiya.Helper.Logger.WriteLine ("| ["+System.DateTime.Now.ToString("T") +"] |-> [GPIO] *** Unloading...");
		}

		public void RaiseFinish(params object[] list) {
			PluginEvent messageSent = Event;
			if (messageSent != null)
			{
				messageSent(this, list);
			}
		}

		public void Execute(params object[] list) {
			GPIOMem led = new GPIOMem (GPIOPins.V2_GPIO_22);
				while(true)
				{
					led.Write(PinState.High);
					System.Threading.Thread.Sleep(500);
					led.Write(PinState.Low);
					System.Threading.Thread.Sleep(500);
				}
		}

		public GPIO () 
		{
		}
	}
}

