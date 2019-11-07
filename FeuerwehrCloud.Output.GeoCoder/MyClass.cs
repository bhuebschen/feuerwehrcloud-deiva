using System;
using System.Linq;
using FeuerwehrCloud.Plugin;
using System.ComponentModel;
using System.Net;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using Newtonsoft.Json;
using GeoUtility.GeoSystem;
using System.Reflection;

namespace FeuerwehrCloud.Processor
{
	public class GeoCoder: FeuerwehrCloud.Plugin.IPlugin
	{
		#region IPlugin implementation
		public event FeuerwehrCloud.Plugin.PluginEvent Event;
		private FeuerwehrCloud.Plugin.IHost My;


		public string Name {
			get {
				return "GeoCoder";
			}
		}
		public string FriendlyName {
			get {
				return "GeoCoder Koordinatenermittlung";
			}
		}

		public Guid GUID {
			get {
				return new Guid (Name);
			}
		}

		public byte[] Icon {
			get {
				var assembly = typeof(FeuerwehrCloud.Processor.GeoCoder).GetTypeInfo().Assembly;
				string[] resources = assembly.GetManifestResourceNames();
				Stream stream = assembly.GetManifestResourceStream("icon.ico");
				return ((MemoryStream)stream).ToArray();
			}
		}

		public bool Initialize (IHost hostApplication)
		{
			My = hostApplication;
			FeuerwehrCloud.Helper.Logger.WriteLine ("|  *** GeoCoder loaded...");
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


			string addr = (string)list [0];

			if (addr.Substring (0, 5) == "COORD") {
				addr = addr.Substring (addr.IndexOf (":") + 1);
				FeuerwehrCloud.Helper.Logger.WriteLine ("|  + [GeoCoder] *** Converting Gauss-Krüger to Latitude/Longitude: " + addr);
				GaussKrueger gauss = new GaussKrueger(int.Parse(addr.Substring(0,addr.IndexOf(" ")).Trim()), int.Parse(addr.Substring(addr.IndexOf(" ")+1).Trim()));
				MGRS mgrs = (MGRS)gauss;
				Geographic geo = (Geographic)mgrs;
				double lon = geo.Longitude;
				double lat = geo.Latitude;

				string key = "AsSta3yGavJt7O8i-SokTyANtvF1HfBoPK5GdD_xgl4ul6ZBmBB3Ru7gX7pof0_T";
				Uri geocodeRequest = new Uri (string.Format ("http://dev.virtualearth.net/REST/v1/Locations?q={0}&key={1}", addr, key));
				System.Collections.Generic.Dictionary<string, string> Result;
				Result = new System.Collections.Generic.Dictionary<string, string> ();
				System.Net.WebClient WCC = new WebClient ();
				Result.Add ("lat", (System.Math.Round (lat, 5)).ToString (System.Globalization.CultureInfo.InvariantCulture.NumberFormat));
				Result.Add ("lng", (System.Math.Round (lon, 5)).ToString (System.Globalization.CultureInfo.InvariantCulture.NumberFormat));
				try {
					System.Net.ServicePointManager.ServerCertificateValidationCallback = (a, b, c, d) => {
						return true;
					};
					System.Net.WebClient WC = new System.Net.WebClient ();
					FeuerwehrCloud.Helper.Logger.WriteLine ("|  + [GeoCoder] *** downloading map...");
					WC.DownloadFile ("http://dev.virtualearth.net/REST/V1/Imagery/Map/AerialWithLabels/" + (lat).ToString (System.Globalization.CultureInfo.InvariantCulture.NumberFormat) + "," + (lon).ToString (System.Globalization.CultureInfo.InvariantCulture.NumberFormat) + "/16?pp=" + (lat).ToString (System.Globalization.CultureInfo.InvariantCulture.NumberFormat) + "," + (lon).ToString (System.Globalization.CultureInfo.InvariantCulture.NumberFormat) + ";10;&mapSize=600,700&mapLayer=TrafficFlow&key=AsSta3yGavJt7O8i-SokTyANtvF1HfBoPK5GdD_xgl4ul6ZBmBB3Ru7gX7pof0_T", "/tmp/map.jpg");
					//WC.DownloadFile("https://maps.googleapis.com/maps/api/staticmap?center="+(lat).ToString (System.Globalization.CultureInfo.InvariantCulture.NumberFormat)+","+(lon ).ToString (System.Globalization.CultureInfo.InvariantCulture.NumberFormat)+"&zoom=13&scale=2&size=400x900&maptype=roadmap&format=png&visual_refresh=true&markers=size:big%7Ccolor:red%7Clabel:Einsatzort%7C"+(lat ).ToString(System.Globalization.CultureInfo.InvariantCulture.NumberFormat)+","+(lon ).ToString (System.Globalization.CultureInfo.InvariantCulture.NumberFormat),"/tmp/map.png");
					string XFile = System.IO.File.ReadAllText ("web/"+((string)list [1]).Replace (" ", "_")+".xml");
					XFile = XFile.Replace ("<Property name=\"Tag\">MAP</Property>", "<Property name=\"Tag\">MAP</Property>\r\n      <Property name=\"Image\">\r\n        <Binary>" + System.Convert.ToBase64String (System.IO.File.ReadAllBytes ("/tmp/map.jpg")) + "</Binary>\r\n</Property>");
					FeuerwehrCloud.Helper.Logger.WriteLine ("|  + [GeoCoder] *** put map to panel-xml...");
					System.IO.File.WriteAllText ("web/panel.xml", XFile);
					System.IO.File.WriteAllText ("web/"+((string)list [1]).Replace (" ", "_")+".xml", XFile);		
				} catch (Exception exx) {
					FeuerwehrCloud.Helper.Logger.WriteLine (exx.ToString ());
				}
				RaiseFinish ("text", Result);
			} else {
				FeuerwehrCloud.Helper.Logger.WriteLine ("|  + [GeoCoder] *** Trying to get coordinates from: " + addr);
				try {

					System.Collections.Generic.Dictionary<string, string> Result;
					#if GOOGLE

					Geocoding.Address[] addresses = GeC.Geocode( (string)list[0]  ).ToArray();
					Result = new System.Collections.Generic.Dictionary<string, string> ();
					Result.Add("lat",(addresses [0].Coordinates.Latitude).ToString (System.Globalization.CultureInfo.InvariantCulture.NumberFormat) );
					Result.Add("lng",(addresses [0].Coordinates.Longitude).ToString (System.Globalization.CultureInfo.InvariantCulture.NumberFormat) );
					try {
					System.Net.ServicePointManager.ServerCertificateValidationCallback = (a, b, c, d) => { return true; };
					System.Net.WebClient WC = new System.Net.WebClient();
					WC.DownloadFile("https://maps.googleapis.com/maps/api/staticmap?center="+(addresses [0].Coordinates.Latitude).ToString (System.Globalization.CultureInfo.InvariantCulture.NumberFormat)+","+(addresses [0].Coordinates.Longitude ).ToString (System.Globalization.CultureInfo.InvariantCulture.NumberFormat)+"&zoom=13&scale=2&size=400x900&maptype=roadmap&format=png&visual_refresh=true&markers=size:big%7Ccolor:red%7Clabel:Einsatzort%7C"+(addresses [0].Coordinates.Latitude ).ToString(System.Globalization.CultureInfo.InvariantCulture.NumberFormat)+","+(addresses [0].Coordinates.Longitude ).ToString (System.Globalization.CultureInfo.InvariantCulture.NumberFormat),"/tmp/map.png");
					string XFile = System.IO.File.ReadAllText("/FeuerwehrCloud/DEIVA/panel.xml");
					XFile = XFile.Replace("<Property name=\"Tag\">MAP</Property>","<Property name=\"Tag\">MAP</Property>\r\n      <Property name=\"Image\">\r\n        <Binary>"+  System.Convert.ToBase64String(System.IO.File.ReadAllBytes("/tmp/map.png")) +"</Binary>\r\n</Property>");
					System.IO.File.WriteAllText("/FeuerwehrCloud/DEIVA/panel.xml", XFile);
					} catch (Exception ex) {
					FeuerwehrCloud.Helper.Logger.WriteLine(ex.ToString());
					}
					RaiseFinish ("text", Result);
					#else
					// StaticMap with Routes!
					//http://dev.virtualearth.net/REST/v1/Imagery/Map/Road/Routes?mapSize=800,800&wp.0=Sonnenstr.3,Rosenheim;64;1&wp.1=Hoffeldstr.10,Rosenheim;66;2&key=AsSta3yGavJt7O8i-SokTyANtvF1HfBoPK5GdD_xgl4ul6ZBmBB3Ru7gX7pof0_T

					string key = "AsSta3yGavJt7O8i-SokTyANtvF1HfBoPK5GdD_xgl4ul6ZBmBB3Ru7gX7pof0_T";
					Uri geocodeRequest = new Uri (string.Format ("http://dev.virtualearth.net/REST/v1/Locations?q={0}&key={1}", addr, key));
					Result = new System.Collections.Generic.Dictionary<string, string> ();
					System.Net.WebClient WCC = new WebClient ();
					try {
						string str = WCC.DownloadString (geocodeRequest);
						if (str.IndexOf ("\"Point\",\"coordinates\":") > -1) {
							FeuerwehrCloud.Helper.Logger.WriteLine ("|  + [GeoCoder] *** Address found!");
							dynamic jsonDe = JsonConvert.DeserializeObject (str);
							float lat = jsonDe.resourceSets [0].resources [0].point.coordinates [0];
							float lon = jsonDe.resourceSets [0].resources [0].point.coordinates [1];
							Result.Add ("lat", (System.Math.Round (lat, 5)).ToString (System.Globalization.CultureInfo.InvariantCulture.NumberFormat));
							Result.Add ("lng", (System.Math.Round (lon, 5)).ToString (System.Globalization.CultureInfo.InvariantCulture.NumberFormat));
							try {
								System.Net.ServicePointManager.ServerCertificateValidationCallback = (a, b, c, d) => {
									return true;
								};
								System.Net.WebClient WC = new System.Net.WebClient ();
								FeuerwehrCloud.Helper.Logger.WriteLine ("|  + [GeoCoder] *** downloading map...");
								//WC.DownloadFile ("http://dev.virtualearth.net/REST/V1/Imagery/Map/Road/" + (lat).ToString (System.Globalization.CultureInfo.InvariantCulture.NumberFormat) + "," + (lon).ToString (System.Globalization.CultureInfo.InvariantCulture.NumberFormat) + "/14?pp=" + (lat).ToString (System.Globalization.CultureInfo.InvariantCulture.NumberFormat) + "," + (lon).ToString (System.Globalization.CultureInfo.InvariantCulture.NumberFormat) + ";47;!&mapSize=400,700&mapLayer=TrafficFlow&key=AsSta3yGavJt7O8i-SokTyANtvF1HfBoPK5GdD_xgl4ul6ZBmBB3Ru7gX7pof0_T", "/tmp/map.jpg");
								WC.DownloadFile ("http://dev.virtualearth.net/REST/V1/Imagery/Map/AerialWithLabels/" + (lat).ToString (System.Globalization.CultureInfo.InvariantCulture.NumberFormat) + "," + (lon).ToString (System.Globalization.CultureInfo.InvariantCulture.NumberFormat) + "/16?pp=" + (lat).ToString (System.Globalization.CultureInfo.InvariantCulture.NumberFormat) + "," + (lon).ToString (System.Globalization.CultureInfo.InvariantCulture.NumberFormat) + ";10;&mapSize=500,700&mapLayer=TrafficFlow&key=AsSta3yGavJt7O8i-SokTyANtvF1HfBoPK5GdD_xgl4ul6ZBmBB3Ru7gX7pof0_T", "/tmp/map.jpg");
								//WC.DownloadFile("https://maps.googleapis.com/maps/api/staticmap?center="+(lat).ToString (System.Globalization.CultureInfo.InvariantCulture.NumberFormat)+","+(lon ).ToString (System.Globalization.CultureInfo.InvariantCulture.NumberFormat)+"&zoom=13&scale=2&size=400x900&maptype=roadmap&format=png&visual_refresh=true&markers=size:big%7Ccolor:red%7Clabel:Einsatzort%7C"+(lat ).ToString(System.Globalization.CultureInfo.InvariantCulture.NumberFormat)+","+(lon ).ToString (System.Globalization.CultureInfo.InvariantCulture.NumberFormat),"/tmp/map.png");
								string XFile = System.IO.File.ReadAllText ("web/panel.xml");
								XFile = XFile.Replace ("<Property name=\"Tag\">MAP</Property>", "<Property name=\"Tag\">MAP</Property>\r\n      <Property name=\"Image\">\r\n        <Binary>" + System.Convert.ToBase64String (System.IO.File.ReadAllBytes ("/tmp/map.jpg")) + "</Binary>\r\n</Property>");
								FeuerwehrCloud.Helper.Logger.WriteLine ("|  + [GeoCoder] *** put map to panel-xml...");
								System.IO.File.WriteAllText ("web/panel.xml", XFile);
							} catch (Exception exx) {
								FeuerwehrCloud.Helper.Logger.WriteLine (exx.ToString ());
							}
							FeuerwehrCloud.Helper.Logger.WriteLine ("|  + [GeoCoder] *** done...!");
							RaiseFinish ("text", Result);
						} else {
							FeuerwehrCloud.Helper.Logger.WriteLine ("|  + [GeoCoder] *** Address not found ... :/");
							FeuerwehrCloud.Helper.Logger.WriteLine (str);
							// Keine Koordinaten gefunden... :/
						}
					} catch (Exception ex) {
						if (ex.GetType ().ToString () == "System.Net.WebException") {
							if (ex.Message == "NameResolutionFailure") {
								// PANIC! We don't have internet!!!!
							}
						}
						FeuerwehrCloud.Helper.Logger.WriteLine ("|  + [GeoCoder] *** Exception... :/");
						FeuerwehrCloud.Helper.Logger.WriteLine (FeuerwehrCloud.Helper.Helper.GetExceptionDescription (ex));
					}
					#endif
				} catch (Exception ex) {
					FeuerwehrCloud.Helper.Logger.WriteLine (FeuerwehrCloud.Helper.Helper.GetExceptionDescription (ex));

				}
			}

		}

		private Response GetResponse(Uri uri)
		{
			WebClient wc = new WebClient();
			DataContractJsonSerializer ser = new DataContractJsonSerializer (typeof(Response));
			return ser.ReadObject(wc.OpenRead(uri)) as Response;
		}

		private void GetPOSTResponse(Uri uri, string data, Action<Response> callback)
		{
			HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(uri);

			request.Method = "POST";
			request.ContentType = "text/plain;charset=utf-8";

			System.Text.UTF8Encoding encoding = new System.Text.UTF8Encoding();
			byte[] bytes = encoding.GetBytes(data);

			request.ContentLength = bytes.Length;

			using (Stream requestStream = request.GetRequestStream())
			{
				// Send the data.
				requestStream.Write(bytes, 0, bytes.Length);
			}

			request.BeginGetResponse((x) =>
				{
					using (HttpWebResponse response = (HttpWebResponse)request.EndGetResponse(x))
					{
						if (callback != null)
						{
							DataContractJsonSerializer ser = new DataContractJsonSerializer(typeof(Response));
							callback(ser.ReadObject(response.GetResponseStream()) as Response);
						}
					}
				}, null);
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
			FeuerwehrCloud.Helper.Logger.WriteLine ("|  + [GeoCoder] *** Unloading...");
		}
		#endregion

		Geocoding.IGeocoder GeC = new Geocoding.Google.GoogleGeocoder();

		public GeoCoder ()
		{

		}
	}

	[DataContract]
	internal class Address
	{
		[DataMember(Name = "addressLine", EmitDefaultValue = false)]
		public string AddressLine { get; set; }

		[DataMember(Name = "adminDistrict", EmitDefaultValue = false)]
		public string AdminDistrict { get; set; }

		[DataMember(Name = "adminDistrict2", EmitDefaultValue = false)]
		public string AdminDistrict2 { get; set; }

		[DataMember(Name = "countryRegion", EmitDefaultValue = false)]
		public string CountryRegion { get; set; }

		[DataMember(Name = "countryRegionIso2", EmitDefaultValue = false)]
		public string CountryRegionIso2 { get; set; }

		[DataMember(Name = "formattedAddress", EmitDefaultValue = false)]
		public string FormattedAddress { get; set; }

		[DataMember(Name = "locality", EmitDefaultValue = false)]
		public string Locality { get; set; }

		[DataMember(Name = "postalCode", EmitDefaultValue = false)]
		public string PostalCode { get; set; }

		[DataMember(Name = "neighborhood", EmitDefaultValue = false)]
		public string Neighborhood { get; set; }

		[DataMember(Name = "landmark", EmitDefaultValue = false)]
		public string Landmark { get; set; }
	}

	[DataContract(Namespace = "http://schemas.microsoft.com/search/local/ws/rest/v1")]
	internal class BirdseyeMetadata : ImageryMetadata
	{
		[DataMember(Name = "orientation", EmitDefaultValue = false)]
		public double Orientation { get; set; }

		[DataMember(Name = "tilesX", EmitDefaultValue = false)]
		public int TilesX { get; set; }

		[DataMember(Name = "tilesY", EmitDefaultValue = false)]
		public int TilesY { get; set; }
	}

	[DataContract]
	internal class BoundingBox
	{
		[DataMember(Name = "southLatitude", EmitDefaultValue = false)]
		public double SouthLatitude { get; set; }

		[DataMember(Name = "westLongitude", EmitDefaultValue = false)]
		public double WestLongitude { get; set; }

		[DataMember(Name = "northLatitude", EmitDefaultValue = false)]
		public double NorthLatitude { get; set; }

		[DataMember(Name = "eastLongitude", EmitDefaultValue = false)]
		public double EastLongitude { get; set; }
	}

	[DataContract]
	internal class Detail
	{
		[DataMember(Name = "compassDegrees", EmitDefaultValue = false)]
		public int CompassDegrees { get; set; }

		[DataMember(Name = "maneuverType", EmitDefaultValue = false)]
		public string ManeuverType { get; set; }

		[DataMember(Name = "startPathIndices", EmitDefaultValue = false)]
		public int[] StartPathIndices { get; set; }

		[DataMember(Name = "endPathIndices", EmitDefaultValue = false)]
		public int[] EndPathIndices { get; set; }

		[DataMember(Name = "roadType", EmitDefaultValue = false)]
		public string RoadType { get; set; }

		[DataMember(Name = "locationCodes", EmitDefaultValue = false)]
		public string[] LocationCodes { get; set; }

		[DataMember(Name = "names", EmitDefaultValue = false)]
		public string[] Names { get; set; }

		[DataMember(Name = "mode", EmitDefaultValue = false)]
		public string Mode { get; set; }

		[DataMember(Name = "roadShieldRequestParameters", EmitDefaultValue = false)]
		public RoadShield roadShieldRequestParameters { get; set; }
	}

	[DataContract]
	internal class Generalization
	{
		[DataMember(Name = "pathIndices", EmitDefaultValue = false)]
		public int[] PathIndices { get; set; }

		[DataMember(Name = "latLongTolerance", EmitDefaultValue = false)]
		public double LatLongTolerance { get; set; }
	}

	[DataContract]
	internal class Hint
	{
		[DataMember(Name = "hintType", EmitDefaultValue = false)]
		public string HintType { get; set; }

		[DataMember(Name = "text", EmitDefaultValue = false)]
		public string Text { get; set; }
	}

	[DataContract(Namespace = "http://schemas.microsoft.com/search/local/ws/rest/v1")]
	[KnownType(typeof(StaticMapMetadata))]
	[KnownType(typeof(BirdseyeMetadata))]
	internal class ImageryMetadata : Resource
	{
		[DataMember(Name = "imageHeight", EmitDefaultValue = false)]
		public string ImageHeight { get; set; }

		[DataMember(Name = "imageWidth", EmitDefaultValue = false)]
		public string ImageWidth { get; set; }

		[DataMember(Name = "imageUrl", EmitDefaultValue = false)]
		public string ImageUrl { get; set; }

		[DataMember(Name = "imageUrlSubdomains", EmitDefaultValue = false)]
		public string[] ImageUrlSubdomains { get; set; }

		[DataMember(Name = "vintageEnd", EmitDefaultValue = false)]
		public string VintageEnd { get; set; }

		[DataMember(Name = "vintageStart", EmitDefaultValue = false)]
		public string VintageStart { get; set; }

		[DataMember(Name = "zoomMax", EmitDefaultValue = false)]
		public int ZoomMax { get; set; }

		[DataMember(Name = "zoomMin", EmitDefaultValue = false)]
		public int ZoomMin { get; set; }
	}

	[DataContract]
	internal class Instruction
	{
		[DataMember(Name = "maneuverType", EmitDefaultValue = false)]
		public string ManeuverType { get; set; }

		[DataMember(Name = "text", EmitDefaultValue = false)]
		public string Text { get; set; }
	}

	[DataContract]
	internal class ItineraryItem
	{
		[DataMember(Name = "childItineraryItems", EmitDefaultValue = false)]
		public ItineraryItem[] ChildItineraryItems { get; set; }

		[DataMember(Name = "compassDirection", EmitDefaultValue = false)]
		public string CompassDirection { get; set; }

		[DataMember(Name = "details", EmitDefaultValue = false)]
		public Detail[] Details { get; set; }

		[DataMember(Name = "exit", EmitDefaultValue = false)]
		public string Exit { get; set; }

		[DataMember(Name = "hints", EmitDefaultValue = false)]
		public Hint[] Hints { get; set; }

		[DataMember(Name = "iconType", EmitDefaultValue = false)]
		public string IconType { get; set; }

		[DataMember(Name = "instruction", EmitDefaultValue = false)]
		public Instruction Instruction { get; set; }

		[DataMember(Name = "maneuverPoint", EmitDefaultValue = false)]
		public Point ManeuverPoint { get; set; }

		[DataMember(Name = "sideOfStreet", EmitDefaultValue = false)]
		public string SideOfStreet { get; set; }

		[DataMember(Name = "signs", EmitDefaultValue = false)]
		public string[] Signs { get; set; }

		[DataMember(Name = "time", EmitDefaultValue = false)]
		public string Time { get; set; }

		[DataMember(Name = "tollZone", EmitDefaultValue = false)]
		public string TollZone { get; set; }

		[DataMember(Name = "towardsRoadName", EmitDefaultValue = false)]
		public string TowardsRoadName { get; set; }

		[DataMember(Name = "transitLine", EmitDefaultValue = false)]
		public TransitLine TransitLine { get; set; }

		[DataMember(Name = "transitStopId", EmitDefaultValue = false)]
		public int TransitStopId { get; set; }

		[DataMember(Name = "transitTerminus", EmitDefaultValue = false)]
		public string TransitTerminus { get; set; }

		[DataMember(Name = "travelDistance", EmitDefaultValue = false)]
		public double TravelDistance { get; set; }

		[DataMember(Name = "travelDuration", EmitDefaultValue = false)]
		public double TravelDuration { get; set; }

		[DataMember(Name = "travelMode", EmitDefaultValue = false)]
		public string TravelMode { get; set; }

		[DataMember(Name = "warning", EmitDefaultValue = false)]
		public Warning[] Warning { get; set; }
	}

	[DataContract]
	internal class Line
	{
		[DataMember(Name = "type", EmitDefaultValue = false)]
		public string Type { get; set; }

		[DataMember(Name = "coordinates", EmitDefaultValue = false)]
		public double[][] Coordinates { get; set; }
	}

	[DataContract(Namespace = "http://schemas.microsoft.com/search/local/ws/rest/v1")]
	internal class Location : Resource
	{
		[DataMember(Name = "name", EmitDefaultValue = false)]
		public string Name { get; set; }

		[DataMember(Name = "point", EmitDefaultValue = false)]
		public Point Point { get; set; }

		[DataMember(Name = "entityType", EmitDefaultValue = false)]
		public string EntityType { get; set; }

		[DataMember(Name = "address", EmitDefaultValue = false)]
		public Address Address { get; set; }

		[DataMember(Name = "confidence", EmitDefaultValue = false)]
		public string Confidence { get; set; }

		[DataMember(Name = "matchCodes", EmitDefaultValue = false)]
		public string[] MatchCodes { get; set; }

		[DataMember(Name = "geocodePoints", EmitDefaultValue = false)]
		public Point[] GeocodePoints { get; set; }

		[DataMember(Name = "queryParseValues", EmitDefaultValue = false)]
		public QueryParseValue[] QueryParseValues { get; set; }
	}

	[DataContract]
	internal class QueryParseValue
	{
		[DataMember(Name = "property", EmitDefaultValue = false)]
		public string Property { get; set; }

		[DataMember(Name = "value", EmitDefaultValue = false)]
		public string Value { get; set; }
	}

	[DataContract(Namespace = "http://schemas.microsoft.com/search/local/ws/rest/v1")]
	internal class PinInfo
	{
		[DataMember(Name = "anchor", EmitDefaultValue = false)]
		public Pixel Anchor { get; set; }

		[DataMember(Name = "bottomRightOffset", EmitDefaultValue = false)]
		public Pixel BottomRightOffset { get; set; }

		[DataMember(Name = "topLeftOffset", EmitDefaultValue = false)]
		public Pixel TopLeftOffset { get; set; }

		[DataMember(Name = "point", EmitDefaultValue = false)]
		public Point Point { get; set; }
	}

	[DataContract(Namespace = "http://schemas.microsoft.com/search/local/ws/rest/v1")]
	internal class Pixel
	{
		[DataMember(Name = "x", EmitDefaultValue = false)]
		public string X { get; set; }

		[DataMember(Name = "y", EmitDefaultValue = false)]
		public string Y { get; set; }
	}

	[DataContract]
	internal class Point : Shape
	{
		[DataMember(Name = "type", EmitDefaultValue = false)]
		public string Type { get; set; }

		/// <summary>
		/// Latitude,Longitude
		/// </summary>
		[DataMember(Name = "coordinates", EmitDefaultValue = false)]
		public double[] Coordinates { get; set; }

		[DataMember(Name = "calculationMethod", EmitDefaultValue = false)]
		public string CalculationMethod { get; set; }

		[DataMember(Name = "usageTypes", EmitDefaultValue = false)]
		public string[] UsageTypes { get; set; }
	}

	[DataContract]
	[KnownType(typeof(Location))]
	[KnownType(typeof(Route))]
	[KnownType(typeof(TrafficIncident))]
	[KnownType(typeof(ImageryMetadata))]
	[KnownType(typeof(ElevationData))]
	[KnownType(typeof(SeaLevelData))]    
	[KnownType(typeof(CompressedPointList))]
	internal class Resource
	{
		[DataMember(Name = "bbox", EmitDefaultValue = false)]
		public double[] BoundingBox { get; set; }

		[DataMember(Name = "__type", EmitDefaultValue = false)]
		public string Type { get; set; }
	}

	[DataContract]
	internal class ResourceSet
	{
		[DataMember(Name = "estimatedTotal", EmitDefaultValue = false)]
		public long EstimatedTotal { get; set; }

		[DataMember(Name = "resources", EmitDefaultValue = false)]
		public Resource[] Resources { get; set; }
	}

	[DataContract]
	internal class Response
	{
		[DataMember(Name = "copyright", EmitDefaultValue = false)]
		public string Copyright { get; set; }

		[DataMember(Name = "brandLogoUri", EmitDefaultValue = false)]
		public string BrandLogoUri { get; set; }

		[DataMember(Name = "statusCode", EmitDefaultValue = false)]
		public int StatusCode { get; set; }

		[DataMember(Name = "statusDescription", EmitDefaultValue = false)]
		public string StatusDescription { get; set; }

		[DataMember(Name = "authenticationResultCode", EmitDefaultValue = false)]
		public string AuthenticationResultCode { get; set; }

		[DataMember(Name = "errorDetails", EmitDefaultValue = false)]
		public string[] errorDetails { get; set; }

		[DataMember(Name = "traceId", EmitDefaultValue = false)]
		public string TraceId { get; set; }

		[DataMember(Name = "resourceSets", EmitDefaultValue = false)]
		public ResourceSet[] ResourceSets { get; set; }
	}

	[DataContract]
	internal class RoadShield
	{
		[DataMember(Name = "bucket", EmitDefaultValue = false)]
		public int Bucket { get; set; }

		[DataMember(Name = "shields", EmitDefaultValue = false)]
		public Shield[] Shields { get; set; }
	}

	[DataContract(Namespace = "http://schemas.microsoft.com/search/local/ws/rest/v1")]
	internal class Route : Resource
	{
		[DataMember(Name = "id", EmitDefaultValue = false)]
		public string Id { get; set; }

		[DataMember(Name = "distanceUnit", EmitDefaultValue = false)]
		public string DistanceUnit { get; set; }

		[DataMember(Name = "durationUnit", EmitDefaultValue = false)]
		public string DurationUnit { get; set; }

		[DataMember(Name = "travelDistance", EmitDefaultValue = false)]
		public double TravelDistance { get; set; }

		[DataMember(Name = "travelDuration", EmitDefaultValue = false)]
		public double TravelDuration { get; set; }

		[DataMember(Name = "routeLegs", EmitDefaultValue = false)]
		public RouteLeg[] RouteLegs { get; set; }

		[DataMember(Name = "routePath", EmitDefaultValue = false)]
		public RoutePath RoutePath { get; set; }
	}

	[DataContract]
	internal class RouteLeg
	{
		[DataMember(Name = "travelDistance", EmitDefaultValue = false)]
		public double TravelDistance { get; set; }

		[DataMember(Name = "travelDuration", EmitDefaultValue = false)]
		public double TravelDuration { get; set; }

		[DataMember(Name = "actualStart", EmitDefaultValue = false)]
		public Point ActualStart { get; set; }

		[DataMember(Name = "actualEnd", EmitDefaultValue = false)]
		public Point ActualEnd { get; set; }

		[DataMember(Name = "startLocation", EmitDefaultValue = false)]
		public Location StartLocation { get; set; }

		[DataMember(Name = "endLocation", EmitDefaultValue = false)]
		public Location EndLocation { get; set; }

		[DataMember(Name = "itineraryItems", EmitDefaultValue = false)]
		public ItineraryItem[] ItineraryItems { get; set; }
	}

	[DataContract]
	internal class RoutePath
	{
		[DataMember(Name = "line", EmitDefaultValue = false)]
		public Line Line { get; set; }

		[DataMember(Name = "generalizations", EmitDefaultValue = false)]
		public Generalization[] Generalizations { get; set; }
	}

	[DataContract]
	[KnownType(typeof(Point))]
	internal class Shape
	{
		[DataMember(Name = "boundingBox", EmitDefaultValue = false)]
		public double[] BoundingBox { get; set; }
	}

	[DataContract]
	internal class Shield
	{
		[DataMember(Name = "labels", EmitDefaultValue = false)]
		public string[] Labels { get; set; }

		[DataMember(Name = "roadShieldType", EmitDefaultValue = false)]
		public int RoadShieldType { get; set; }
	}

	[DataContract(Namespace = "http://schemas.microsoft.com/search/local/ws/rest/v1")]
	internal class StaticMapMetadata : ImageryMetadata
	{
		[DataMember(Name = "mapCenter", EmitDefaultValue = false)]
		public Point MapCenter { get; set; }

		[DataMember(Name = "pushpins", EmitDefaultValue = false)]
		public PinInfo[] Pushpins { get; set; }

		[DataMember(Name = "zoom", EmitDefaultValue = false)]
		public string Zoom { get; set; }
	}

	[DataContract(Namespace = "http://schemas.microsoft.com/search/local/ws/rest/v1")]
	internal class TrafficIncident : Resource
	{
		[DataMember(Name = "point", EmitDefaultValue = false)]
		public Point Point { get; set; }

		[DataMember(Name = "congestion", EmitDefaultValue = false)]
		public string Congestion { get; set; }

		[DataMember(Name = "description", EmitDefaultValue = false)]
		public string Description { get; set; }

		[DataMember(Name = "detour", EmitDefaultValue = false)]
		public string Detour { get; set; }

		[DataMember(Name = "start", EmitDefaultValue = false)]
		public string Start { get; set; }

		[DataMember(Name = "end", EmitDefaultValue = false)]
		public string End { get; set; }

		[DataMember(Name = "incidentId", EmitDefaultValue = false)]
		public long IncidentId { get; set; }

		[DataMember(Name = "lane", EmitDefaultValue = false)]
		public string Lane { get; set; }

		[DataMember(Name = "lastModified", EmitDefaultValue = false)]
		public string LastModified { get; set; }

		[DataMember(Name = "roadClosed", EmitDefaultValue = false)]
		public bool RoadClosed { get; set; }

		[DataMember(Name = "severity", EmitDefaultValue = false)]
		public int Severity { get; set; }

		[DataMember(Name = "toPoint", EmitDefaultValue = false)]
		public Point ToPoint { get; set; }

		[DataMember(Name = "locationCodes", EmitDefaultValue = false)]
		public string[] LocationCodes { get; set; }

		[DataMember(Name = "type", EmitDefaultValue = false)]
		public int Type { get; set; }

		[DataMember(Name = "verified", EmitDefaultValue = false)]
		public bool Verified { get; set; }
	}

	[DataContract]
	internal class TransitLine
	{
		[DataMember(Name = "verboseName", EmitDefaultValue = false)]
		public string verboseName { get; set; }

		[DataMember(Name = "abbreviatedName", EmitDefaultValue = false)]
		public string abbreviatedName { get; set; }

		[DataMember(Name = "agencyId", EmitDefaultValue = false)]
		public long AgencyId { get; set; }

		[DataMember(Name = "agencyName", EmitDefaultValue = false)]
		public string agencyName { get; set; }

		[DataMember(Name = "lineColor", EmitDefaultValue = false)]
		public long lineColor { get; set; }

		[DataMember(Name = "lineTextColor", EmitDefaultValue = false)]
		public long lineTextColor { get; set; }

		[DataMember(Name = "uri", EmitDefaultValue = false)]
		public string uri { get; set; }

		[DataMember(Name = "phoneNumber", EmitDefaultValue = false)]
		public string phoneNumber { get; set; }

		[DataMember(Name = "providerInfo", EmitDefaultValue = false)]
		public string providerInfo { get; set; }
	}

	[DataContract]
	internal class Warning
	{
		[DataMember(Name = "warningType", EmitDefaultValue = false)]
		public string WarningType { get; set; }

		[DataMember(Name = "severity", EmitDefaultValue = false)]
		public string Severity { get; set; }

		[DataMember(Name = "text", EmitDefaultValue = false)]
		public string Text { get; set; }
	}

	[DataContract(Namespace = "http://schemas.microsoft.com/search/local/ws/rest/v1")]
	internal class CompressedPointList : Resource
	{
		[DataMember(Name = "value", EmitDefaultValue = false)]
		public string Value { get; set; }
	}

	[DataContract(Namespace = "http://schemas.microsoft.com/search/local/ws/rest/v1")]
	internal class ElevationData : Resource
	{
		[DataMember(Name = "elevations", EmitDefaultValue = false)]
		public int[] Elevations { get; set; }

		[DataMember(Name = "zoomLevel", EmitDefaultValue = false)]
		public int ZoomLevel { get; set; }
	}

	[DataContract(Namespace = "http://schemas.microsoft.com/search/local/ws/rest/v1")]
	internal class SeaLevelData : Resource
	{
		[DataMember(Name = "offsets", EmitDefaultValue = false)]
		public int[] Offsets { get; set; }

		[DataMember(Name = "zoomLevel", EmitDefaultValue = false)]
		public int ZoomLevel { get; set; }
	}

}

