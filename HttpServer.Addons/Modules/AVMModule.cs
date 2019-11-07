using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using HttpServer.Headers;
using HttpServer.Messages;
using HttpServer.Modules;
using HttpServer.Addons.Files;
using FritzTR064;
using FritzTR064.Generated;
using System.Web;
using System.Net.Security;
using FeuerwehrCloud.Helper;

namespace HttpServer.Addons.Modules
{
	/// <summary>
	/// Executes CGI applications and serves the output to the client.
	/// </summary>
	public class AVMModule : IModule
	{
		private readonly string _homeDirectory;
		private readonly FeuerwehrCloud.Plugin.IHost _my;

		private readonly object _readLock = new object();

		/// <summary>
		/// Initializes a new instance of the <see cref="CgiModule"/> class.
		/// </summary>
		/// <param name="homeDirectory">The directory containing the CGI scripts.</param>
		/// <exception cref="ArgumentNullException"><c>baseUri</c> or <c>basePath</c> is <c>null</c>.</exception>
		public AVMModule(string homeDirectory, FeuerwehrCloud.Plugin.IHost hostApplication)
		{
			_my = hostApplication;

			if (!Directory.Exists(homeDirectory))
				throw new ArgumentException("Directory does not exist: " + homeDirectory, "homeDirectory");

			_homeDirectory = homeDirectory;
		}

		#region IModule Members


		/// <summary>
		/// Process a request.
		/// </summary>
		/// <param name="context">Request information</param>
		/// <returns>What to do next.</returns>
		/// <exception cref="InternalServerException">Failed to find file extension</exception>
		/// <exception cref="ForbiddenException">Forbidden file type.</exception>
		public ProcessingResult Process(RequestContext context)
		{

			IRequest request = context.Request;
			IResponse response = context.Response;

			IFileService _fileService = new DiskFileService("/", _homeDirectory);
			var fileContext = new FileContext(context.Request,System.DateTime.Now);
			_fileService.GetFile(fileContext);
			var scriptName = fileContext.Filename;
			if (!fileContext.IsFound)
			{
				response.Status = HttpStatusCode.NotFound;
				return ProcessingResult.SendResponse;
			}


			try
			{
				string ext = Path.GetExtension(scriptName).TrimStart('.');

				if (!ext.Equals("fritz", StringComparison.OrdinalIgnoreCase))
					return ProcessingResult.Continue;

				// TODO: is this a good place to lock?
				lock (_readLock)
				{
					string fileContent = System.IO.File.ReadAllText(scriptName);


					string[] XInfo = fileContent.Split(new []{"{%"}, StringSplitOptions.RemoveEmptyEntries);
					for(int i = 1; i<XInfo.Length;i++) {
						if(XInfo[i].Contains("%}")) {
							XInfo[i] = XInfo[i].Substring(0,XInfo[i].IndexOf("%}"));

						}
					}
					XInfo = XInfo.RemoveAt (0);


//					Wanpppconn1 service = new Wanpppconn1("https://192.168.178.1");
//					service.SoapHttpClientProtocol.Credentials = new NetworkCredential("dslf-config", "Viking2011");
//					string ip;
//					service.GetExternalIPAddress(out ip);
					//service.AddPortMapping
					//service.DeletePortMapping
					//service.ForceTermination
					//service.SetUserName
					//service.SetPassword
					//service.SetAutoDisconnectTimeSpan
					//service.SetConnectionType
					//service.SetConnectionTrigger
					//service.SetIdleDisconnectTime
					//service.GetUserName (out ip);
					//service.GetConnectionTypeInfo (out ip, out ip2);
					//service.SetUserName ("ar15636672741");
					//service.SetPassword ("internet");

//					/* Anrufbeantworter */
//					Tam service2 = new Tam("https://l7dmkoihvym5jghm.myfritz.net:499");
//					service2.SoapHttpClientProtocol.Credentials = new NetworkCredential("dslf-config", "Viking2011");
//					string url;
//					service2.GetMessageList(0, out url); // Get Tam message List
//					//service2.SetEnable
//					//service2.DeleteMessage
//					//service2.MarkMessage
//					FritzTR064.Generated.Voip service4;
//					//service4.AddVoIPAccount
//					//service4.DelVoIPAccount
//					//service4.DialSetConfig
//					FritzTR064.Generated.Voip f;

//				    System.Net.WebClient Anrufbeantworter = new WebClient ();
//					Anrufbeantworter.Credentials = service2.SoapHttpClientProtocol.Credentials;
//					string AB = Anrufbeantworter.DownloadString (url);


					//Deviceconfig service3 = new Deviceconfig("https://fritz.box");
					//service3.

					/* Telefonbuch */
					//Contact service = new Contact("https://fritz.box");
					//service.AddPhonebook("ExtraID", "Name"); // Create a new Phonebook
					//string callListUrl;
					//service.GetCallList(out callListUrl);

					ServicePointManager.ServerCertificateValidationCallback += (sender, certificate, chain, sslPolicyErrors) => true;
					System.Xml.XmlDocument CallList = new System.Xml.XmlDocument();
					CallList.Load("dyndata/calllist.xml");
					System.Xml.XmlDocument TamsList = new System.Xml.XmlDocument();
					TamsList.Load("dyndata/tam1.xml");

					System.Collections.Generic.Dictionary<string, string> DSL = FeuerwehrCloud.Helper.AppSettings.Load("dyndata/wandsl.info");
					System.Collections.Generic.Dictionary<string, string> WLAN = FeuerwehrCloud.Helper.AppSettings.Load("dyndata/wlan.info");
					System.Collections.Generic.Dictionary<string, string> TAM = FeuerwehrCloud.Helper.AppSettings.Load("dyndata/tam.info");
					System.Collections.Generic.Dictionary<string, string> DEVI = FeuerwehrCloud.Helper.AppSettings.Load("dyndata/Deviceinfo.info");
					System.Collections.Generic.Dictionary<string, string> WAN = FeuerwehrCloud.Helper.AppSettings.Load("dyndata/wanppp.info");
					System.Collections.Generic.Dictionary<string, string> TEL = FeuerwehrCloud.Helper.AppSettings.Load("dyndata/phone.info");

					foreach (var XP in XInfo) {
						switch (XP) {
						case "INCLUDE_HEADER":
							fileContent = fileContent.Replace("{%"+XP+"%}",System.IO.File.ReadAllText("web/header.html"));
							break;
						case "INCLUDE_FOOTER":
							fileContent = fileContent.Replace("{%"+XP+"%}",System.IO.File.ReadAllText("web/footer.html"));
							break;
						case "FAX_ENABLED":
							fileContent = fileContent.Replace("{%"+XP+"%}","Alarmfax-System aktiviert");
							break;
						case "INFO_LED_TYPE":
							fileContent = fileContent.Replace("{%"+XP+"%}","blinkt bei neuen Nachrichten/Anrufen");
							break;
							//
						case "TAM_DATA":
							int iTamz = 0;
							string CLL2="";

							foreach (System.Xml.XmlElement item in TamsList["Root"].SelectNodes("//Message")) {
								//if(item.Name =="Call") {

								CLL2+="<tr><td>"+item["Date"].InnerText+"<td>"+ item ["Number"].InnerText +"</td></tr>";
								iTamz++;

								//}
								if(iTamz>8) break;
							}
							fileContent = fileContent.Replace("{%"+XP+"%}",CLL2);
							break;
						case "TAM_COUNT":
							fileContent = fileContent.Replace("{%"+XP+"%}",(TamsList["Root"].SelectNodes("//Message").Count).ToString());
							break;
						case "CALL_COUNT":
							fileContent = fileContent.Replace("{%"+XP+"%}",(CallList["root"].SelectNodes("//Call").Count).ToString());
							break;
						case "CALL_LIST":
							int iCallZ = 0;
							string CLL="";

							foreach (System.Xml.XmlElement item in CallList["root"].SelectNodes("//Call")) {
								//if(item.Name =="Call") {

								CLL+="<tr><td"+ (item ["Type"].InnerText=="2"?" style='color:red'":(item ["Type"].InnerText=="3"?" style='color:#00AA00'":" style='color:#0000CC'")) +">"+item["Date"].InnerText+"<td"+ (item ["Type"].InnerText=="2"?" style='color:red'":(item ["Type"].InnerText=="3"?" style='color:#00AA00'":" style='color:#0000CC'")) +">"+ (item ["Name"].InnerText !=""?item ["Name"].InnerText +" ("+item ["Called"].InnerText+")" : item ["Called"].InnerText)+"</td></tr>";
									iCallZ++;

								//}
								if(iCallZ>8) break;
							}
							fileContent = fileContent.Replace("{%"+XP+"%}",CLL);
							break;
						case "DSL_CONNECTED":
							fileContent = fileContent.Replace("{%"+XP+"%}",(bool.Parse(DSL["wandslIFEnable"])==true?"bereit, "+(int.Parse(DSL["DownstreamCurrRate"])/1024)+" Mbit/s (down), "+ (int.Parse(DSL["UpstreamCurrRate"]))+" kbit/s (up)":"getrennt"));
							break;
						case "NETWORK_DEVICE_COUNT":
							string[] result1 = System.IO.File.ReadAllLines(System.IO.Path.Combine(System.Environment.CurrentDirectory,"network.csv"));
							int iDeviceCount = 0;
							for(int i=0; i<result1.Length;i++) {
								string[] Columns = result1[i].Split(new []{";"},StringSplitOptions.None);
								if(Columns[2]=="up") iDeviceCount++;
							}
							fileContent = fileContent.Replace("{%"+XP+"%}",(iDeviceCount.ToString()));
							break;
						case "NETWORK_DEVICE_LIST":
							string[] result2 = System.IO.File.ReadAllLines(System.IO.Path.Combine(System.Environment.CurrentDirectory,"network.csv"));
							string HTMLTable="";
							int idev=0;
							for(int i=0; i<result2.Length;i++) {
								string[] Columns = result2[i].Split(new []{";"},StringSplitOptions.None);
								if(Columns[2]=="up") {
									HTMLTable+="<tr><td>"+(Columns[4]==""?"PC-"+Columns[0].Replace(".","-"):Columns[4]).Replace(".fritz.box","")+"</td><td>LAN</td></tr>";
									idev++;
									if(idev==9) break;
								}
							}
							string dHTM = "";
							for(int i=0; i<result2.Length;i++) {
								string[] Columns = result2[i].Split(new []{";"},StringSplitOptions.None);
								if(Columns[2]=="up")
									dHTM+="<tr><td><a href='http://"+Columns[0]+"' target='_blank'>"+(Columns[4]==""?"PC-"+Columns[0].Replace(".","-"):Columns[4]).Replace(".fritz.box","")+"</a></td><td>"+Columns[0]+"</td><td>"+Columns[5]+"</td><td>"+Columns[6]+"</td></tr>";
							}
							System.IO.File.WriteAllText("web/devices_online.htm", dHTM);
							dHTM = "";
							for(int i=0; i<result2.Length;i++) {
								string[] Columns = result2[i].Split(new []{";"},StringSplitOptions.None);
								if(Columns[2]=="down")
									dHTM+="<tr><td>"+(Columns[4]==""?"PC-"+Columns[0].Replace(".","-"):Columns[4]).Replace(".fritz.box","")+"</td><td>"+Columns[0]+"</td><td>"+Columns[5]+"</td><td>"+Columns[6]+"</td></tr>";
							}
							System.IO.File.WriteAllText("web/devices_offline.htm", dHTM);
										fileContent = fileContent.Replace("{%"+XP+"%}",HTMLTable);
							break;
						case "WLAN_CONNECTED":
							fileContent = fileContent.Replace("{%"+XP+"%}",(bool.Parse(WLAN["Enable"])?"An, "+WLAN["Status"]:"Deaktiviert"));
							break;
						case "DECT_CONNECTED":
							fileContent = fileContent.Replace("{%"+XP+"%}","Deaktiviert");
							break;
						case "USB_CONNECTED":
							fileContent = fileContent.Replace("{%"+XP+"%}",((int.Parse(FeuerwehrCloud.Helper.AppSettings.Load("dyndata/usb.info")["Count"]))-5) + " Geräte angeschlossen");
							break;
						case "I2C_CONNECTED":
							fileContent = fileContent.Replace("{%"+XP+"%}",((int.Parse(FeuerwehrCloud.Helper.AppSettings.Load("dyndata/i2c.info")["Count"]))) + " Module angeschlossen");
							break;
						case "CONNECTED_SINCE4":
							if(WAN["ConnectionStatus"] == "Connected") {
								fileContent = fileContent.Replace("{%"+XP+"%}","verbunden seit "+WAN["Uptime"]);
							} else {
								fileContent = fileContent.Replace("{%"+XP+"%}","nicht verbunden");
							}
							break;
						case "CONNECTED_IP4":
							fileContent = fileContent.Replace("{%"+XP+"%}",WAN["ExternalIPAddress"]);
							break;
						case "CONNECTED_SINCE6":
							fileContent = fileContent.Replace("{%"+XP+"%}","");
							break;
						case "CONNECTED_IP6":
							fileContent = fileContent.Replace("{%"+XP+"%}","");
							break;
						case "PHONE_LINES_STRING":
							fileContent = fileContent.Replace("{%"+XP+"%}",TEL["NumberList"]);
							break;
						case "PHONE_LINE_COUNT":
							fileContent = fileContent.Replace("{%"+XP+"%}",TEL["NumberOfNumbers"]);
							break;
						default:
							fileContent = fileContent.Replace("{%"+XP+"%}","");
							break;
						}
					}
					//fileContent = fileContent.Replace("%")


					string output = fileContent;

					// Set default content type
					response.ContentType = new ContentTypeHeader("text/html");

					//ProcessHeaders(response, CgiFeuerwehrCloud.Helper.ParseCgiHeaders(ref output));

					response.ContentLength.Value = output.Length;

					ResponseWriter generator = new ResponseWriter();
					generator.SendHeaders(context.HttpContext, response);
					generator.Send(context.HttpContext, output, System.Text.Encoding.UTF8);
				}

				return ProcessingResult.Abort;
			}
			catch (FileNotFoundException err)
			{
				throw new InternalServerException("Failed to process file '" + request.Uri + "'.", err);
			}
			catch (Exception err)
			{
				throw new InternalServerException("Failed to process file '" + request.Uri + "'.", err);
			}
		}

		#endregion

		private void ProcessHeaders(IResponse response, Dictionary<string, string> headers)
		{
			foreach (var item in headers)
			{
				if (item.Key.Equals("status", StringComparison.OrdinalIgnoreCase))
				{
					SetStatusCode(response, item.Key, item.Value);
				}
				else if (item.Key.Equals("content-type", StringComparison.OrdinalIgnoreCase))
				{
					response.ContentType = new ContentTypeHeader(item.Value);
				}
				else
				{
					response.Add(new StringHeader(item.Key, item.Value));
				}
			}
		}

		private void SetStatusCode(IResponse response, string key, string value)
		{
			if (value.StartsWith("404"))
			{
				response.Status = HttpStatusCode.NotFound;
			}
			else if (value.StartsWith("401"))
			{
				response.Status = HttpStatusCode.Unauthorized;
			}
			else if (value.StartsWith("302"))
			{
				response.Status = HttpStatusCode.Found;
			}
		}
	}
}
