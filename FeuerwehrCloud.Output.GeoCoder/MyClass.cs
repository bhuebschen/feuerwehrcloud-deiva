using System;
using System.Linq;
using FeuerwehrCloud.Plugin;
using System.ComponentModel;


namespace FeuerwehrCloud.Processor.GeoCoder
{
	public class GeoCoder: FeuerwehrCloud.Plugin.IPlugin
	{
		#region IPlugin implementation
		public event FeuerwehrCloud.Plugin.PluginEvent Event;
		private IHost My;

		public bool Initialize (IHost hostApplication)
		{
			My = hostApplication;
			de.SYStemiya.Helper.Logger.WriteLine ("| ["+System.DateTime.Now.ToString("T") +"] |-+ [GeoCoder] *** Initializing...");
			return true;
		}

		public void RaiseFinish(params object[] list) {
			FeuerwehrCloud.Plugin.PluginEvent messageSent = Event;
			if (messageSent != null)
			{
				messageSent(this, list);
			}
		}

		public void Execute (params object[] list)
		{
			Console.Write ("| ["+System.DateTime.Now.ToString("T") +"] |-+ [GeoCoder] *** Trying to get coordinates from: " + (string)list[0]);
			try {
				Geocoding.Address[] addresses = GeC.Geocode( (string)list[0]  ).ToArray();
				System.Collections.Generic.Dictionary<string, string> Result = new System.Collections.Generic.Dictionary<string, string> ();

				Result.Add("lat",(addresses [0].Coordinates.Latitude * 100000).ToString () );
				Result.Add("lng",(addresses [0].Coordinates.Longitude * 100000).ToString () );
				RaiseFinish ("text", Result);
			} catch (Exception ex) {
				
			}
		}

		public bool IsAsync {
			get {
				return true;
			}
		}
		public FeuerwehrCloud.Plugin.ServiceType ServiceType {
			get {
				return FeuerwehrCloud.Plugin.ServiceType.processor;
			}
		}
		#endregion

		#region IDisposable implementation
		public void Dispose ()
		{
			GeC = null;
			de.SYStemiya.Helper.Logger.WriteLine ("| ["+System.DateTime.Now.ToString("T") +"] |-+ [GeoCoder] *** Unloading...");
		}
		#endregion

		Geocoding.IGeocoder GeC = new Geocoding.Google.GoogleGeocoder();

		public GeoCoder ()
		{

		}
	}

	#region Actitity
	public class GeoCoderActivityValidator : System.Workflow.ComponentModel.Compiler.ActivityValidator
	{
	}

	[System.Workflow.ComponentModel.Compiler.ActivityValidator(typeof(GeoCoderActivityValidator))]
	[System.Drawing.ToolboxBitmap(typeof(GeoCoderActivity), "geocoder.ico")]
	public class GeoCoderActivity : System.Workflow.ComponentModel.Activity
	{
		public GeoCoderActivity ()  {
			this.Name = "GeoCoderActivity";
		}

		public static System.Workflow.ComponentModel.DependencyProperty AddressProperty = System.Workflow.ComponentModel.DependencyProperty.Register(
			"Dateiname", typeof(string), typeof(GeoCoderActivity));
		[Description("Legt die Adresse fest, aus der die Koordinaten ermittelt werden sollen")]
		[Category("Activity")]
		[Browsable(true)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		public string Address
		{
			get
			{
				return ((string)base.GetValue(AddressProperty));
			}
			set
			{
				base.SetValue(AddressProperty, value);
			}
		}
	}
	#endregion

}

