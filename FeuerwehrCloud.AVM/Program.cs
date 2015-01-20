using System;
using FritzTR064.Generated;
using System.Net;
using System.Web.Security;
using System.Web.Services;


namespace FeuerwehrCloud.NetworkServices
{
	class MainClass
	{
		private static System.Collections.Generic.Dictionary<string, string> NetConfig = new System.Collections.Generic.Dictionary<string, string> ();
		private static System.Timers.Timer MyTimer = new System.Timers.Timer ();
		public static void Main (string[] args)
		{
			ServicePointManager.ServerCertificateValidationCallback += (sender, certificate, chain, sslPolicyErrors) => true;
			if(!System.IO.File.Exists("netconf.cfg")) {
				NetConfig.Add ("Gateway", "192.168.178.1");
				NetConfig.Add ("GW-SSL-Port", "443");
				NetConfig.Add ("FRITZPass", "");
				NetConfig.Add ("TR064-Username", "dslf-config");
				NetConfig.Add ("fing", "/usr/bin/fing");
				de.SYStemiya.Helper.AppSettings.Save(NetConfig,"netconf.cfg");
			} 
			NetConfig = de.SYStemiya.Helper.AppSettings.Load ("netconf.cfg");

			MyTimer.Interval = 60000;
			MyTimer.Elapsed += HandleElapsed;
			MyTimer.Enabled = true;
			HandleElapsed (null, null);
			do {
				System.Threading.Thread.Sleep(1);
			} while (true);
		}
		static void HandleElapsed (object sender, System.Timers.ElapsedEventArgs e)
		{
			System.Collections.Generic.Dictionary<string, string> dConfig = new System.Collections.Generic.Dictionary<string, string> ();

			if (!System.IO.Directory.Exists ("dyndata"))
				System.IO.Directory.CreateDirectory ("dyndata");

			bool Enable=false; string ConnectionStatus="Unbekannt"; string PossibleConnectionTypes="Unbekannt"; string ConnectionType="Unbekannt"; string Name="Unbekannt"; uint Uptime=0; uint UpstreamMaxBitRate=0; uint DownstreamMaxBitRate=0; string LastConnectionError=""; uint IdleDisconnectTime  = 0; bool RSIPAvailable=false; string UserName="Unbekannt"; bool NATEnabled = false; string ExternalIPAddress="0.0.0.0"; string DNSServers="0.0.0.0"; string MACAddress="::::"; string ConnectionTrigger="Unbekannt"; string LastAuthErrorInfo=""; ushort MaxCharsUsername=0; ushort MinCharsUsername=0; string AllowedCharsUsername="Unbekannt"; ushort MaxCharsPassword=0; ushort MinCharsPassword=0; string AllowedCharsPassword="Unbekannt"; string TransportType="Unbekannt"; string RouteProtocolRx="Unbekannt"; string PPPoEServiceName="Unbekannt"; string RemoteIPAddress="0.0.0.0"; string PPPoEACName="Unbekannt"; bool DNSEnabled=false; bool DNSOverrideAllowed=false;
			try {
				Wanpppconn1 service = new Wanpppconn1("https://"+NetConfig["Gateway"]+":"+NetConfig["GW-SSL-Port"]);
				service.SoapHttpClientProtocol.Credentials = new NetworkCredential(NetConfig["TR064-Username"], NetConfig["FRITZPass"]);
				service.GetInfo(out Enable, out ConnectionStatus, out PossibleConnectionTypes, out ConnectionType, out Name, out Uptime, out UpstreamMaxBitRate, out DownstreamMaxBitRate, out LastConnectionError, out IdleDisconnectTime, out RSIPAvailable, out UserName, out NATEnabled, out ExternalIPAddress, out DNSServers, out MACAddress, out ConnectionTrigger, out LastAuthErrorInfo, out MaxCharsUsername, out MinCharsUsername, out AllowedCharsUsername, out MaxCharsPassword, out MinCharsPassword, out AllowedCharsPassword, out TransportType, out RouteProtocolRx, out PPPoEServiceName, out RemoteIPAddress, out PPPoEACName, out DNSEnabled, out DNSOverrideAllowed);
				dConfig.Clear();
				dConfig.Add("Enable",Enable.ToString());
				dConfig.Add("ConnectionStatus",ConnectionStatus.ToString());
				dConfig.Add("PossibleConnectionTypes",PossibleConnectionTypes.ToString());
				dConfig.Add("ConnectionType",ConnectionType.ToString());
				dConfig.Add("Name",Name.ToString());
				dConfig.Add("Uptime",System.DateTime.Now.AddSeconds(Uptime).ToString());
				dConfig.Add("UpstreamMaxBitRate",UpstreamMaxBitRate.ToString());
				dConfig.Add("DownstreamMaxBitRate",DownstreamMaxBitRate.ToString());
				dConfig.Add("LastConnectionError",LastConnectionError.ToString());
				dConfig.Add("IdleDisconnectTime",IdleDisconnectTime.ToString());
				dConfig.Add("RSIPAvailable",RSIPAvailable.ToString());
				dConfig.Add("UserName",UserName.ToString());
				dConfig.Add("NATEnabled",NATEnabled.ToString());
				dConfig.Add("ExternalIPAddress",ExternalIPAddress.ToString());
				dConfig.Add("DNSServers",DNSServers.ToString());
				dConfig.Add("MACAddress",MACAddress.ToString());
				dConfig.Add("ConnectionTrigger",ConnectionTrigger.ToString());
				dConfig.Add("LastAuthErrorInfo",LastAuthErrorInfo.ToString());
				dConfig.Add("MaxCharsUsername",MaxCharsUsername.ToString());
				dConfig.Add("MinCharsUsername",MinCharsUsername.ToString());
				dConfig.Add("AllowedCharsUsername",AllowedCharsUsername.ToString());
				dConfig.Add("MaxCharsPassword",MaxCharsPassword.ToString());
				dConfig.Add("MinCharsPassword",MinCharsPassword.ToString());
				dConfig.Add("AllowedCharsPassword",AllowedCharsPassword.ToString());
				dConfig.Add("TransportType",TransportType.ToString());
				dConfig.Add("RouteProtocolRx",RouteProtocolRx.ToString());
				dConfig.Add("PPPoEServiceName",PPPoEServiceName.ToString());
				dConfig.Add("RemoteIPAddress",RemoteIPAddress.ToString());
				dConfig.Add("PPPoEACName",PPPoEACName.ToString());
				dConfig.Add("DNSEnabled",DNSEnabled.ToString());
				dConfig.Add("DNSOverrideAllowed",DNSOverrideAllowed.ToString());
				de.SYStemiya.Helper.AppSettings.Save(dConfig, "dyndata/wanppp.info");
			} catch (Exception ex) {
				ex.ToString();
			}




			System.Net.WebClient WC = new WebClient();

			FritzTR064.Generated.Tam cTam = new Tam ("https://"+NetConfig["Gateway"]+":"+NetConfig["GW-SSL-Port"]);
			cTam.SoapHttpClientProtocol.Credentials = new NetworkCredential(NetConfig["TR064-Username"], NetConfig["FRITZPass"]);
			string TamList;
			for (ushort i = 0; i < 5; i++) {
				ushort Index;
				bool abEnable;
				string abName;
				bool TAMRunning;
				ushort Stick;
				ushort Status;
				cTam.GetInfo (i, out abEnable, out abName, out TAMRunning, out Stick, out Status);
				dConfig.Clear ();
				dConfig.Add ("Enable", abEnable.ToString ());
				dConfig.Add ("Name", abName.ToString ());
				dConfig.Add ("TAMRunning", TAMRunning.ToString ());
				dConfig.Add ("Stick", Stick.ToString ());
				dConfig.Add ("Status", Status.ToString ());
				if (abEnable == true) {
					cTam.GetMessageList (i, out TamList);
					WC.Credentials = cTam.SoapHttpClientProtocol.Credentials;
					WC.DownloadFile (TamList, "dyndata/tam" + i.ToString () + ".xml");
				}
				de.SYStemiya.Helper.AppSettings.Save (dConfig, "dyndata/tam.info");

			}


			FritzTR064.Generated.Contact c = new Contact ("https://"+NetConfig["Gateway"]+":"+NetConfig["GW-SSL-Port"]);
			c.SoapHttpClientProtocol.Credentials = new NetworkCredential(NetConfig["TR064-Username"], NetConfig["FRITZPass"]);
			string calllist; string DectIDList; string HandsetName; ushort PhonebookID;  bool cEnable; string cStatus; string LastConnect; string Url; string ServiceId; string Username;
			dConfig.Clear();
			c.GetCallList (out calllist);
			WC.Credentials = c.SoapHttpClientProtocol.Credentials;
			WC.DownloadFile(calllist,"dyndata/calllist.xml");
			try {
				c.GetInfo (out cEnable, out cStatus, out LastConnect, out Url, out ServiceId, out UserName, out Name);
				dConfig.Add("Enable",cEnable.ToString());
				dConfig.Add("Status",cStatus.ToString());
				dConfig.Add("LastConnect",LastConnect.ToString());
				dConfig.Add("Url",Url.ToString());
				dConfig.Add("ServiceId",ServiceId.ToString());
				dConfig.Add("Name",Name.ToString());
				if(cEnable) {
					c.GetDECTHandsetList(out DectIDList);
					for(ushort DectID = 1; DectID < 10; DectID++) {
						c.GetDECTHandsetInfo(DectID, out HandsetName, out PhonebookID);
						dConfig.Add("DectID",DectID.ToString());
						dConfig.Add("HandsetName-"+DectID.ToString(),HandsetName.ToString());
						dConfig.Add("PhonebookID-"+DectID.ToString(),PhonebookID.ToString());
					}

				}
				//			c.GetInfoByIndex();
				//			c.GetNumberOfEntries();
				//			c.GetPhonebook();
				//			c.GetPhonebookList();


			} catch (Exception ex) {
				
			}


			FritzTR064.Generated.Wandslifconfig1 w1 = new Wandslifconfig1("https://"+NetConfig["Gateway"]+":"+NetConfig["GW-SSL-Port"]);
			w1.SoapHttpClientProtocol.Credentials = new NetworkCredential(NetConfig["TR064-Username"], NetConfig["FRITZPass"]);
			//FritzTR064.Generated.Wandsllinkconfig1 w2 = new Wandsllinkconfig1(());
			bool wandslIFEnable; string dslStatus; string DataPath; uint UpstreamCurrRate; uint DownstreamCurrRate; uint UpstreamMaxRate; uint DownstreamMaxRate; uint UpstreamNoiseMargin; uint DownstreamNoiseMargin; uint UpstreamAttenuation; uint DownstreamAttenuation; string ATURVendor; string ATURCountry; ushort UpstreamPower; ushort DownstreamPower;
			w1.GetInfo(out wandslIFEnable, out dslStatus, out DataPath, out UpstreamCurrRate, out DownstreamCurrRate, out UpstreamMaxRate, out DownstreamMaxRate, out UpstreamNoiseMargin, out DownstreamNoiseMargin, out UpstreamAttenuation, out DownstreamAttenuation, out ATURVendor, out ATURCountry, out UpstreamPower, out DownstreamPower);
			dConfig.Clear();
			dConfig.Add("wandslIFEnable",wandslIFEnable.ToString());
			dConfig.Add("Status",dslStatus.ToString());
			dConfig.Add("DataPath",DataPath.ToString());
			dConfig.Add("UpstreamCurrRate",UpstreamCurrRate.ToString());
			dConfig.Add("DownstreamCurrRate",DownstreamCurrRate.ToString());
			dConfig.Add("UpstreamMaxRate",UpstreamMaxRate.ToString());
			dConfig.Add("DownstreamMaxRate",DownstreamMaxRate.ToString());
			dConfig.Add("UpstreamNoiseMargin",UpstreamNoiseMargin.ToString());
			dConfig.Add("DownstreamNoiseMargin",DownstreamNoiseMargin.ToString());
			dConfig.Add("UpstreamAttenuation",UpstreamAttenuation.ToString());
			dConfig.Add("DownstreamAttenuation",DownstreamAttenuation.ToString());
			dConfig.Add("ATURVendor",ATURVendor.ToString());
			dConfig.Add("ATURCountry",ATURCountry.ToString());
			dConfig.Add("UpstreamPower",UpstreamPower.ToString());
			dConfig.Add("DownstreamPower",DownstreamPower.ToString());
			de.SYStemiya.Helper.AppSettings.Save(dConfig, "dyndata/wandsl.info");

			FritzTR064.Generated.Wlanconfig1 wlc = new Wlanconfig1("https://"+NetConfig["Gateway"]+":"+NetConfig["GW-SSL-Port"]);
			wlc.SoapHttpClientProtocol.Credentials = new NetworkCredential(NetConfig["TR064-Username"], NetConfig["FRITZPass"]);
			string wlStatus; bool wlEnable; string MaxBitRate; byte Channel; string SSID; string BeaconType; bool MACAddressControlEnabled; string Standard; string BSSID; string BasicEncryptionModes; string BasicAuthenticationMode; byte MaxCharsSSID; byte MinCharsSSID; string AllowedCharsSSID; byte MinCharsPSK; byte MaxCharsPSK; string AllowedCharsPSK;
			wlc.GetInfo(out  wlEnable, out  wlStatus, out  MaxBitRate, out  Channel, out  SSID, out  BeaconType, out  MACAddressControlEnabled, out  Standard, out  BSSID, out  BasicEncryptionModes, out  BasicAuthenticationMode, out  MaxCharsSSID, out  MinCharsSSID, out  AllowedCharsSSID, out  MinCharsPSK, out  MaxCharsPSK, out  AllowedCharsPSK);
			dConfig.Clear();
			dConfig.Add("Enable",wlEnable.ToString());
			dConfig.Add("Status",wlStatus.ToString());
			dConfig.Add("MaxBitRate",MaxBitRate.ToString());
			dConfig.Add("Channel",Channel.ToString());
			dConfig.Add("SSID",SSID.ToString());
			dConfig.Add("BeaconType",BeaconType.ToString());
			dConfig.Add("MACAddressControlEnabled",MACAddressControlEnabled.ToString());
			dConfig.Add("Standard",Standard.ToString());
			dConfig.Add("BSSID",BSSID.ToString());
			dConfig.Add("BasicEncryptionModes",BasicEncryptionModes.ToString());
			dConfig.Add("BasicAuthenticationMode",BasicAuthenticationMode.ToString());
			dConfig.Add("MaxCharsSSID",MaxCharsSSID.ToString());
			dConfig.Add("MinCharsSSID",MinCharsSSID.ToString());
			dConfig.Add("AllowedCharsSSID",AllowedCharsSSID.ToString());
			dConfig.Add("MinCharsPSK",MinCharsPSK.ToString());
			dConfig.Add("MaxCharsPSK",MaxCharsPSK.ToString());
			dConfig.Add("AllowedCharsPSK",AllowedCharsPSK.ToString());
			de.SYStemiya.Helper.AppSettings.Save(dConfig, "dyndata/wlan.info");

			FritzTR064.Generated.Voip voip1 = new Voip("https://"+NetConfig["Gateway"]+":"+NetConfig["GW-SSL-Port"]);
			voip1.SoapHttpClientProtocol.Credentials = new NetworkCredential(NetConfig["TR064-Username"], NetConfig["FRITZPass"]);
			string NumberList; bool FaxT38Enable; string VoiceCoding; string ClientList; ushort ExistingVoIPNumbers; string PhoneName; uint NumberOfNumbers; ushort NumberOfClients; ushort MaxVoipNumbers;
			voip1.GetInfo(out FaxT38Enable, out VoiceCoding);
			voip1.GetClients(out ClientList);
			voip1.GetExistingVoIPNumbers(out ExistingVoIPNumbers);
			voip1.DialGetConfig(out PhoneName);
			voip1.GetNumbers(out NumberList);
			voip1.GetNumberOfNumbers(out NumberOfNumbers);
			voip1.GetNumberOfClients(out NumberOfClients);
			voip1.GetMaxVoIPNumbers(out MaxVoipNumbers);
			dConfig.Clear();
			dConfig.Add("NumberList",NumberList.ToString());
			dConfig.Add("FaxT38Enable",FaxT38Enable.ToString());
			dConfig.Add("VoiceCoding",VoiceCoding.ToString());
			dConfig.Add("ClientList",ClientList.ToString());
			dConfig.Add("ExistingVoIPNumbers",ExistingVoIPNumbers.ToString());
			dConfig.Add("PhoneName",PhoneName.ToString());
			dConfig.Add("NumberOfNumbers",NumberOfNumbers.ToString());
			dConfig.Add("NumberOfClients",NumberOfClients.ToString());
			dConfig.Add("MaxVoipNumbers",MaxVoipNumbers.ToString());
			de.SYStemiya.Helper.AppSettings.Save(dConfig, "dyndata/phone.info");

			string ManufacturerName; string ManufacturerOUI; string ModelName; string Description; string ProductClass; string SerialNumber; string SoftwareVersion; string HardwareVersion; string SpecVersion; string ProvisioningCode; uint UpTime; string DeviceLog;
			FritzTR064.Generated.Deviceinfo DevInfo = new Deviceinfo ("https://"+NetConfig["Gateway"]+":"+NetConfig["GW-SSL-Port"]);
			DevInfo.SoapHttpClientProtocol.Credentials = new NetworkCredential(NetConfig["TR064-Username"], NetConfig["FRITZPass"]);
			DevInfo.GetInfo (out ManufacturerName, out ManufacturerOUI, out ModelName, out Description, out ProductClass, out SerialNumber, out SoftwareVersion, out HardwareVersion, out SpecVersion, out ProvisioningCode, out UpTime, out DeviceLog);
			dConfig.Clear ();
			dConfig.Add("ManufacturerName",ManufacturerName.ToString());
			dConfig.Add("ManufacturerOUI",ManufacturerOUI.ToString());
			dConfig.Add("ModelName",ModelName.ToString());
			dConfig.Add("Description",Description.ToString());
			dConfig.Add("ProductClass",ProductClass.ToString());
			dConfig.Add("SerialNumber",SerialNumber.ToString());
			dConfig.Add("SoftwareVersion",SoftwareVersion.ToString());
			dConfig.Add("HardwareVersion",HardwareVersion.ToString());
			dConfig.Add("SpecVersion",SpecVersion.ToString());
			dConfig.Add("ProvisioningCode",ProvisioningCode.ToString());
			dConfig.Add("UpTime",UpTime.ToString());
			dConfig.Add("DeviceLog",DeviceLog.ToString());
			de.SYStemiya.Helper.AppSettings.Save(dConfig, "dyndata/DeviceInfo.info");


			//FritzTR064.Generated.Contact

			//dConfig.Add("",xxxxxx.ToString());


			System.Diagnostics.Process P;

			P = new System.Diagnostics.Process () {
				StartInfo =  {
					FileName = NetConfig ["fing"],
					Arguments = " -n " + NetConfig ["Gateway"] + "/24 -r 1 --session " + System.IO.Path.Combine (System.Environment.CurrentDirectory, "network.fing") + " -o table,csv," + System.IO.Path.Combine (System.Environment.CurrentDirectory, "network.csv"),
					UseShellExecute = false,
					CreateNoWindow = true,
					RedirectStandardOutput = true
				}
			};
			P.Start();
			P.WaitForExit(10000);


			P = new System.Diagnostics.Process () {
				StartInfo =  {
					FileName = "/usr/local/bin/lsusb",
					WorkingDirectory = "/tmp/",
					UseShellExecute = false,
					CreateNoWindow = true,
					RedirectStandardOutput = true
				}
			};
			P.Start();
			string lsusb = P.StandardOutput.ReadToEnd ();
			int USBCount = lsusb.Split(new []{"\n"}, StringSplitOptions.RemoveEmptyEntries).Length;
			P.WaitForExit (10000);
			dConfig.Clear();
			dConfig.Add("Count",USBCount.ToString());
			de.SYStemiya.Helper.AppSettings.Save(dConfig, "dyndata/usb.info");

			P = new System.Diagnostics.Process () {
				StartInfo =  {
					FileName = "/Users/systemiya-apple/i2cdetect.sh",
					WorkingDirectory = "/tmp/",
					UseShellExecute = false,
					CreateNoWindow = true,
					RedirectStandardOutput = true
				}
			};
			P.Start();
			string lsi2c = P.StandardOutput.ReadToEnd ();
			P.WaitForExit (10000);
			dConfig.Clear();
			string[] i2C = lsi2c.Split(new []{"--"}, StringSplitOptions.RemoveEmptyEntries);
			int I2Ccount =116- (i2C.Length-2);
			dConfig.Add("Count",I2Ccount.ToString());
			de.SYStemiya.Helper.AppSettings.Save(dConfig, "dyndata/i2c.info");

		}
	}
}
