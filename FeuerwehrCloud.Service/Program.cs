using System;
using FeuerwehrCloud.Plugin;
using FeuerwehrCloud;
using System.IO;
using System.Linq;
using System.Globalization;
using System.Runtime.ExceptionServices;

#if __WINDOWS__
#else
using Mono.Unix;
using Mono.Unix.Native;
#endif
using System.Runtime.InteropServices;
using System.Reflection;
using NLua;
using FeuerwehrCloud;
using System.Drawing;
using System.Xml;
using System.Windows.Forms;
using System.Drawing.Drawing2D;
using System.Collections.Generic;
using OpenSource.UPnP;

namespace FeuerwehrCloud.Service
{
	public class Service :  IHost {

		public void Execute (string library, params object[] list)
		{
			PlugIns [library].Execute (list);
		}
            
        public DateTime LastAlert
        {
            get;
            set;
        }

        public string LastENumber
        {
            get;
            set;
        }

		public System.Collections.Generic.Dictionary<string, IPlugin> PlugIns = new System.Collections.Generic.Dictionary<string, Plugin.IPlugin>();
		public NLua.Lua lua;

		public Service() {
            this.LastAlert = new DateTime(1970,01,01,0,0,0);
            this.LastENumber = "";
			try {
				FeuerwehrCloud.Helper.Logger.WriteLine ("|  *** Initializing NLua...");
				lua = new NLua.Lua ();
				lua.LoadCLRPackage ();
				lua ["Service"] = this;
			} catch (Exception ex) {
				if (ex.GetType().ToString() == "System.DllNotFoundException" && System.Environment.OSVersion.Platform == PlatformID.Win32NT)
				{
					//
				}
				else
				{
					//
				}
				FeuerwehrCloud.Helper.Logger.WriteLine(ex.ToString());
				return;
			}


			System.IO.File.Delete ("versions.txt");
			System.IO.File.AppendAllText("versions.txt", System.IO.Path.GetFileNameWithoutExtension (Assembly.GetExecutingAssembly().GetName().CodeBase)+":"+Assembly.GetExecutingAssembly().GetName().Version.Major.ToString()+"."+Assembly.GetExecutingAssembly().GetName().Version.Minor.ToString()+"."+Assembly.GetExecutingAssembly().GetName().Version.Build.ToString()+"."+Assembly.GetExecutingAssembly().GetName().Version.Revision.ToString()+"\r\n");

			var plugins = System.IO.Directory.EnumerateFiles ("./plugins", "FeuerwehrCloud.*.*.dll");
			//FeuerwehrCloud.Plugin.IPlugin c = new  FeuerwehrCloud.Input.SMTP.SMTPSever ();
			foreach (string currentFile in plugins)
			{
				try {
					if(!System.IO.File.Exists(currentFile+".disabled")) {
						var DLL = Assembly.LoadFile (currentFile);
						var dllVer = DLL.GetName().Version;
						System.IO.File.AppendAllText("versions.txt", System.IO.Path.GetFileNameWithoutExtension (currentFile)+":"+dllVer.Major.ToString()+"."+dllVer.Minor.ToString()+"."+dllVer.Build.ToString()+"."+dllVer.Revision.ToString()+"\r\n");
						foreach(Type type in DLL.GetExportedTypes())
						{
							FeuerwehrCloud.Plugin.IPlugin c; 
							try {
								c = (FeuerwehrCloud.Plugin.IPlugin)Activator.CreateInstance(type);
								lua[System.IO.Path.GetFileNameWithoutExtension (currentFile).Replace(".","_")] = c;
								//lua[System.IO.Path.GetFileNameWithoutExtension (currentFile)] = c;
								lua.DebugHook += HandleDebugHook;
								lua.RegisterFunction(System.IO.Path.GetFileNameWithoutExtension (currentFile).Replace(".","_")+"_Execute", c, c.GetType().GetMethod("Execute"));
								if (c.Initialize (this)) {
									//FeuerwehrCloud.Helper.Logger.WriteLine(">>>>"+System.IO.Path.GetFileNameWithoutExtension (currentFile));
									PlugIns.Add (System.IO.Path.GetFileNameWithoutExtension (currentFile), c);
									c.Event += delegate(object sender, object[] list) {

										/*Xamarin.Insights.Track ("Event", new System.Collections.Generic.Dictionary<string, string> {
											{"ModuleName", c.Name },
											{"ServiceType", c.ServiceType.ToString()},
											{"NextAction", sender.ToString() + ".lua" }
										});*/

										lua["response"] = list;
										switch (c.ServiceType) {
										case FeuerwehrCloud.Plugin.ServiceType.input:
											if (list [0] == "pictures") {
												try {
													//FeuerwehrCloud.Helper.Logger.WriteLine("3||||"+Application.StartupPath +"/"+ sender.ToString () + ".lua");
													lua.DoFile (Application.StartupPath +"/"+ sender.ToString () + ".lua");
													//FeuerwehrCloud.Helper.Logger.WriteLine("3||||"+Application.StartupPath +"/"+ sender.ToString () + ".lua");
												}
												catch (Exception ex) {
													FeuerwehrCloud.Helper.Logger.WriteLine (FeuerwehrCloud.Helper.Helper.GetExceptionDescription(ex));
												}
											}
											else
												if (list [0] == "text") {
													try {
														//FeuerwehrCloud.Helper.Logger.WriteLine("4||||"+Application.StartupPath +"/"+ sender.ToString () + ".lua");
														lua.DoFile (Application.StartupPath +"/"+sender.ToString () + ".lua");
														//FeuerwehrCloud.Helper.Logger.WriteLine("4||||"+Application.StartupPath +"/"+ sender.ToString () + ".lua");
													}
													catch (Exception ex) {
														FeuerwehrCloud.Helper.Logger.WriteLine (FeuerwehrCloud.Helper.Helper.GetExceptionDescription(ex));
													}
												}
											break;
										case FeuerwehrCloud.Plugin.ServiceType.processor:
											if (list [0] == "pictures") {
												try {
													//FeuerwehrCloud.Helper.Logger.WriteLine("1||||"+Application.StartupPath +"/"+ sender.ToString () + ".lua");
													lua.DoFile (Application.StartupPath +"/"+sender.ToString () + ".lua");
													//FeuerwehrCloud.Helper.Logger.WriteLine("1||||"+Application.StartupPath +"/"+ sender.ToString () + ".lua");
												}
												catch (Exception ex) {
													FeuerwehrCloud.Helper.Logger.WriteLine (FeuerwehrCloud.Helper.Helper.GetExceptionDescription(ex));
												}
											}
											else
												if (list [0] == "text") {
													try {
														//FeuerwehrCloud.Helper.Logger.WriteLine("5||||"+Application.StartupPath +"/"+ sender.ToString () + ".lua");
														lua.DoFile (Application.StartupPath +"/"+sender.ToString () + ".lua");
														//FeuerwehrCloud.Helper.Logger.WriteLine("5||||"+Application.StartupPath +"/"+ sender.ToString () + ".lua");
													}
													catch (Exception ex) {
														FeuerwehrCloud.Helper.Logger.WriteLine (FeuerwehrCloud.Helper.Helper.GetExceptionDescription(ex));
														try {
														}
														catch (Exception ex2) {
															var smtpClient = new System.Net.Mail.SmtpClient ();
															smtpClient.Host = "feuerwehrcloud.de";
															smtpClient.Port = 25;
															smtpClient.EnableSsl = false;
															smtpClient.DeliveryMethod = System.Net.Mail.SmtpDeliveryMethod.Network;
															var smtp = smtpClient;
															var mailMessage = new System.Net.Mail.MailMessage ("bug@raspberry.pi", "benedikt.huebschen@sysemiya.de");
															mailMessage.Subject = "[FeuerwehrCloud] EXCEPTION AT " + System.Environment.MachineName;
															mailMessage.Body = FeuerwehrCloud.Helper.Helper.GetExceptionDescription (ex);
															using (var message = mailMessage)
																smtp.Send (message);
														}
													}
												}
											break;
										case FeuerwehrCloud.Plugin.ServiceType.output:
											try {
												//FeuerwehrCloud.Helper.Logger.WriteLine("2||||"+Application.StartupPath +"/"+ sender.ToString () + ".lua");
												lua.DoFile (sender.ToString () + ".lua");
												//FeuerwehrCloud.Helper.Logger.WriteLine("2||||"+Application.StartupPath +"/"+ sender.ToString () + ".lua");
											} catch (Exception ex) {
												FeuerwehrCloud.Helper.Logger.WriteLine (FeuerwehrCloud.Helper.Helper.GetExceptionDescription(ex));
											}
											break;
										}
										//FeuerwehrCloud.Helper.Logger.WriteLine(list[0].ToString());
									};
								}				

							} catch (Exception ex) {
								FeuerwehrCloud.Helper.Logger.WriteLine (FeuerwehrCloud.Helper.Helper.GetExceptionDescription(ex));
							}
						}
					}
				} catch (Exception ex) {
					FeuerwehrCloud.Helper.Logger.WriteLine ("************ ERRROR ***************");
					FeuerwehrCloud.Helper.Logger.WriteLine (FeuerwehrCloud.Helper.Helper.GetExceptionDescription(ex));
				}
			}
			FeuerwehrCloud.Helper.Logger.WriteLine ("|  Loaded plugins: "+PlugIns.Count.ToString());

			lua ["PlugIns"] = PlugIns;
			lua.DoFile ("init_complete.lua");
			FeuerwehrCloud.Helper.Logger.WriteLine ("|  Initialisation complete. System started.");
			FeuerwehrCloud.Helper.Logger.WriteLine ("`·--- Startup complete. ----------------------- ----- --  -");
		}

		void HandleDebugHook (object sender, NLua.Event.DebugHookEventArgs e)
		{
			Console.Out.WriteLine("LUA EXCEPTION:");
			Console.Out.Write(String.Format("FT {0} ", e.LuaDebug.eventCode.ToString()));
			Console.Out.WriteLine(e.LuaDebug.currentline);
			KeraLua.LuaDebug luaDebug = e.LuaDebug;
			if (luaDebug.source.Length > 0 && luaDebug.source[0] == '@')
			{
				Console.Out.WriteLine(String.Format("   n:{0} nw:{1} s:{2} ss:{3} ld:{4} lld:{5} cl:{6}",
					luaDebug.name, luaDebug.namewhat,
					luaDebug.source, luaDebug.shortsrc,
					luaDebug.linedefined, luaDebug.lastlinedefined, luaDebug.currentline));
			}
		}


		protected virtual void Dispose(bool disposing)
		{
		}

		public void Dispose()
		{
			FeuerwehrCloud.Helper.Logger.WriteLine("¸.--- Unloading System -------------------------------------------------- ----- --  -");
			try {
				(new System.Net.WebClient()).DownloadStringAsync(new Uri("http://www.feuerwehrcloud.de/deiva/stop.php?hn="+System.Environment.MachineName));
			} catch (Exception ex) {

			}
			try {
				foreach (FeuerwehrCloud.Plugin.IPlugin item in PlugIns.Values) {
					try {
						item.Dispose();
					} catch (Exception ex2) {
						ex2.ToString();
					}
				}
			} catch (Exception ex) {
				ex.ToString();
			}
			FeuerwehrCloud.Helper.Logger.WriteLine("|  Removed plugins.");
			FeuerwehrCloud.Helper.Logger.WriteLine("|  *** System Shutdown initiated");
			FeuerwehrCloud.Helper.Logger.WriteLine("|  *** Thank you for using an FeuerwehrCloud DEIVA! C-Ya!");
			FeuerwehrCloud.Helper.Logger.WriteLine("|  ");
			FeuerwehrCloud.Helper.Logger.WriteLine("`·--- Good bye. ---------------------------------------------------------- ----- --  -");

			Dispose(true);

		}

		~Service()
		{
			Dispose(false);
		}


	}


	public class MainClass
	{
		static string Level = "|";
		static UPnPDevice _objUPnPDevice;
		static System.Threading.Timer DailyTimer;
		static System.Threading.Timer TrackTimer;

		public static String GetRPiData() {
			const string filePath = "/proc/cpuinfo";
			var settings = new Dictionary<string, string>();
			var cpuInfo = File.ReadAllLines (filePath);
			var suffix = string.Empty;
			foreach(var l in cpuInfo)
			{
				var separator = l.IndexOf(':');
				if (!string.IsNullOrWhiteSpace(l) && separator > 0)
				{
					var key = l.Substring(0, separator).Trim();
					var val = l.Substring(separator + 1).Trim();
					if (string.Equals(key, "processor", StringComparison.InvariantCultureIgnoreCase))
						suffix = "." + val;

					settings.Add(key + suffix, val);
				}
				else
					suffix = "";
			}


			string Model = "";
			string hardware;
			settings.TryGetValue ("Hardware", out hardware);
			if (string.Equals (hardware, "BCM2708", StringComparison.InvariantCultureIgnoreCase)) {
				Model = "Raspberry Pi Model ";
				int firmware;
				string revision;
				settings.TryGetValue ("Revision", out revision);
				int.TryParse (revision, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out firmware);
				switch (firmware & 0xFFFF) {
				case 0x7:
				case 0x8:
				case 0x9:
					Model += "A";
					break;
				case 0x2:
				case 0x3:
				case 0x4:
				case 0x5:
				case 0x6:
				case 0xd:
				case 0xe:
				case 0xf:
				case 0x10:
					Model += "B";
					break;
				}
				switch (firmware & 0xFFFF) {
				case 0x7:
				case 0x8:
				case 0x9:
					Model += " Rev.1";   // Model A, rev1
					break;
				case 0x2:
				case 0x3:
					Model += " Rev.1";   // Model A, rev1
					break;
				case 0x4:
				case 0x5:
				case 0x6:
				case 0xd:
				case 0xe:
				case 0xf:
					Model += " Rev.2";   // Model A, rev1
					break;

				case 0x10:
					Model += "+ Rev.3";   // Model A, rev1
					break;
				}
			}
			if (string.Equals (hardware, "BCM2709", StringComparison.InvariantCultureIgnoreCase)) {
				Model = "Raspberry Pi2 Model ";
				int firmware;
				string revision;
				settings.TryGetValue ("Revision", out revision); 
				int.TryParse (revision, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out firmware);
				switch (firmware & 0xFFFF) {
				case 0x0:
					Model += "A";
					break;
				case 0x1041:
					Model += "B";
					break;
				}
				switch (firmware & 0xFFFF) {
				case 0x1041:
					Model += " Rev.1";   // Pi2 Model B, rev1
					break;
				}
			}
			return Model;
		}

		public static Service StartServer() {
			bool isIntel = true;
			//System.Environment.CurrentDirectory = "/Volumes/Macintosh HD/FeuerwehrCloud - Source/FeuerwehrCloud.Service/bin/Debug";
			//System.IO.Directory.SetCurrentDirectory ("/Volumes/Macintosh HD/FeuerwehrCloud - Source/FeuerwehrCloud.Service/bin/Debug");
			string platformType = "intel";
			FeuerwehrCloud.Helper.Logger.WriteLine ("¸+*°´^`°*+-> FeuerwehrCloud DEIVA 1.0 Startup <-+*°´^`°*+¸");
			string OSText = "";
			bool Is64 = false;
			string Device = "";
			string cpuType = "";
			var platID = System.Environment.OSVersion.Platform;
			if (platID == PlatformID.Unix) {
				string uname = FeuerwehrCloud.Helper.Helper.exec ("uname", "-a").Replace("\n","").Replace("\r","");
				cpuType = FeuerwehrCloud.Helper.Helper.exec ("uname", "-m").Replace("\n","").Replace("\r","");
				if (platID == PlatformID.Unix && System.Environment.OSVersion.Version.Major == 14) {
					OSText = System.IO.File.ReadAllText ("/System/Library/CoreServices/SystemVersion.plist");
					OSText = OSText.Substring (OSText.IndexOf ("ProductVersion"));
					OSText = OSText.Substring (OSText.IndexOf ("<string>") + 8);
					OSText = "MacOS X " + OSText.Substring (0, OSText.IndexOf ("<"));
					Is64 = (uname.IndexOf("x86_64")>-1);
				} else if (platID == PlatformID.Unix) {
					OSText = uname.Substring(0,uname.IndexOf(" ")) + System.Environment.OSVersion.Version.ToString ();

					string Distri = "";
					if (System.IO.File.Exists ("/etc/SUSE-release")) {
						Distri = "Novell SUSE";
					} else if (System.IO.File.Exists ("/etc/redhat-release") || System.IO.File.Exists ("/etc/redhat_version")) {
						Distri = "Red Hat";
					} else if (System.IO.File.Exists ("/etc/centos-release")) {
						Distri = "CentOS";
					} else if (System.IO.File.Exists ("/etc/arch-release")) {
						Distri = "ArchLinux";
					} else if (System.IO.File.Exists ("/etc/meego-release")) {
						Distri = "MeeGo";
					} else if (System.IO.File.Exists ("/etc/enterprise-release")) {
						Distri = "Oracle Enterprise";
					} else if (System.IO.File.Exists ("/etc/alpine-release")) {
						Distri = "Red Hat";
					} else if (System.IO.File.Exists ("/etc/system-release")) {
						Distri = "Red Hat";
					} else if (System.IO.File.Exists ("/etc/enterprise-release")) {
						Distri = "Red Hat";
					} else if (System.IO.File.Exists ("/etc/ovs-release")) {
						Distri = "Open Virtual Switch";
					} else if (System.IO.File.Exists ("/etc/vmware-release")) {
						Distri = "VMware ESX";
					} else if (System.IO.File.Exists ("/etc/bluewhite64-version")) {
						Distri = "Bluewhite64";
					} else if (System.IO.File.Exists ("/etc/slamd64-version")) {
						Distri = "Slamd64";
					} else if (System.IO.File.Exists ("/etc/oracle-release")) {
						Distri = "Oracle";
					} else if (System.IO.File.Exists ("/etc/slackware-release") || System.IO.File.Exists ("/etc/slackware-version")) {
						Distri = "Slackware";
					} else if (System.IO.File.Exists ("/etc/debian_release") || System.IO.File.Exists ("/etc/debian_version")) {
						Distri = "debian";
					} else if (System.IO.File.Exists ("/etc/mandrake-release")) {
						Distri = "Mandrage";
					} else if (System.IO.File.Exists ("/etc/yellowdog-release")) {
						Distri = "Yellow Dog";
					} else if (System.IO.File.Exists ("/etc/sun-release")) {
						Distri = "Sun JDS";
					} else if (System.IO.File.Exists ("/etc/release")) {
						Distri = "Solaris/Sparc";
					} else if (System.IO.File.Exists ("/etc/gentoo-release")) {
						Distri = "Gentoo";
					} else if (System.IO.File.Exists ("/etc/UnitedLinux-release")) {
						Distri = "UnitedLinux";
					} else if (System.IO.File.Exists ("/etc/ulsb-release")) {
						Distri = "ubuntu";
					}
					if (System.IO.File.Exists ("/etc/os-release")) {
						string[] Z = System.IO.File.ReadAllLines ("/etc/os-release");
						foreach (var item in Z) {
							if (item.StartsWith ("NAME=")) {
								Distri = item.Substring (6).Replace ("\"", "").Replace ("'", "");
							}
						}
					}
					OSText = OSText+" ("+Distri+")";
					Is64 = (uname.IndexOf("x86_64")>-1);
					if (uname.IndexOf ("armv6l") > -1 || uname.IndexOf ("armv7l") > -1) {
						platformType = "arm";
						isIntel = false;
						Device = GetRPiData ();
					}
				}
			} else {
				if (platID == PlatformID.Win32NT) {
					if (System.Environment.OSVersion.Version.Major == 4 && System.Environment.OSVersion.Version.Major == 0) {
						OSText = "Windows NT 4.0 " + System.Environment.OSVersion.ServicePack;
					} else if (System.Environment.OSVersion.Version.Major == 5 &&  System.Environment.OSVersion.Version.Major == 0) {
						OSText = "Windows 2000 " + System.Environment.OSVersion.ServicePack;
					} else if (System.Environment.OSVersion.Version.Major == 5 && System.Environment.OSVersion.Version.Major == 1) {
						OSText = "Windows XP " + System.Environment.OSVersion.ServicePack;
					} else if (System.Environment.OSVersion.Version.Major == 5 && System.Environment.OSVersion.Version.Major == 2) {
						OSText = "Windows Server 2003 " + System.Environment.OSVersion.ServicePack;
					} else if (System.Environment.OSVersion.Version.Major == 6 && System.Environment.OSVersion.Version.Major == 0) {
						OSText = "Windows Vista " + System.Environment.OSVersion.ServicePack;
					} else if (System.Environment.OSVersion.Version.Major == 6 && System.Environment.OSVersion.Version.Major == 1) {
						OSText = "Windows 7 " + System.Environment.OSVersion.ServicePack;
					} else if (System.Environment.OSVersion.Version.Major == 6 && System.Environment.OSVersion.Version.Major == 2) {
						OSText = "Windows 8 " + System.Environment.OSVersion.ServicePack;
					} else if (System.Environment.OSVersion.Version.Major == 4 && System.Environment.OSVersion.Version.Major == 3) {
						OSText = "Windows 8.1 " + System.Environment.OSVersion.ServicePack;
					} else {
						OSText = "Windows NT " + System.Environment.OSVersion.Version.Major.ToString() + "." + System.Environment.OSVersion.Version.Minor.ToString() + "." + System.Environment.OSVersion.Version.Build.ToString() + "." + System.Environment.OSVersion.Version.Revision.ToString() + ", " + System.Environment.OSVersion.ServicePack;
					}
				} else if (platID == PlatformID.WinCE) {
					OSText = "Windows CE " + System.Environment.OSVersion.Version.Major.ToString() + "." + System.Environment.OSVersion.Version.Minor.ToString() + "." + System.Environment.OSVersion.Version.Build.ToString() + "." + System.Environment.OSVersion.Version.Revision.ToString() + ", " + System.Environment.OSVersion.ServicePack;
				} else if (platID == PlatformID.Xbox) {
					OSText = "XBOX " + System.Environment.OSVersion.Version.Major.ToString() + "." + System.Environment.OSVersion.Version.Minor.ToString() + "." + System.Environment.OSVersion.Version.Build.ToString() + "." + System.Environment.OSVersion.Version.Revision.ToString() + ", " + System.Environment.OSVersion.ServicePack;
				} else if (platID == PlatformID.Win32S) {
					OSText = "Windows 3.x mit WIN32S " + System.Environment.OSVersion.Version.Major.ToString() + "." + System.Environment.OSVersion.Version.Minor.ToString() + "." + System.Environment.OSVersion.Version.Build.ToString() + "." + System.Environment.OSVersion.Version.Revision.ToString() + ", " + System.Environment.OSVersion.ServicePack;
				} else if (platID == PlatformID.Win32Windows) {
					if (System.Environment.OSVersion.Version.Major == 4 && System.Environment.OSVersion.Version.Major == 0) {
						OSText = "Windows 95 " + System.Environment.OSVersion.ServicePack;
					} else {
						OSText = "Windows NT " + System.Environment.OSVersion.Version.Major.ToString() + "." + System.Environment.OSVersion.Version.Minor.ToString() + "." + System.Environment.OSVersion.Version.Build.ToString() + "." + System.Environment.OSVersion.Version.Revision.ToString() + ", " + System.Environment.OSVersion.ServicePack;
					}
				}
			}
			OSText = OSText + (System.Environment.Is64BitOperatingSystem||Is64 ? " (" + (isIntel?"x86_64":cpuType) : " ("+(isIntel?"i386":cpuType)) + ", executed in " + (System.Environment.Is64BitProcess?"64 bit mode)":"32 bit mode)");
			FeuerwehrCloud.Helper.Logger.WriteLine ("|  FeuerwehrCloud DEIVA " + Application.ProductVersion.ToString() + ", running on " +  OSText);
			FeuerwehrCloud.Helper.Logger.WriteLine ("|  "+Device);
			_objUPnPDevice = OpenSource.UPnP.UPnPDevice.CreateRootDevice(900, 1, "");
			_objUPnPDevice.HasPresentation = true;
			_objUPnPDevice.StandardDeviceType = "DEIVA";
			//_objUPnPDevice.Icon = 
			//_objUPnPDevice.Icon2 = 
			_objUPnPDevice.AddVirtualDirectory("link", delegate(UPnPDevice sender, HTTPMessage msg, HTTPSession WebSession, string VirtualDir) {

			}, delegate(UPnPDevice sender, HTTPMessage msg, HTTPSession WebSession, string VirtualDir) {
				OpenSource.UPnP.HTTPMessage response = new HTTPMessage();
				response.StatusCode = 200;
				response.StatusData = "OK";
				switch (msg.DirectiveObj) {
				default:
					if(System.IO.File.Exists("."+msg.DirectiveObj)) {
						response.BodyBuffer = System.IO.File.ReadAllBytes("."+msg.DirectiveObj);
					} else {
						if(System.IO.Directory.Exists("."+msg.DirectiveObj)) {
							string[] files = System.IO.Directory.GetFiles("."+msg.DirectiveObj, "*", SearchOption.TopDirectoryOnly);
							string ResultStr = "";
							foreach (var item in files) {
									if(System.IO.Directory.Exists("."+msg.DirectiveObj + item)) {
										ResultStr += "<directory item='dir' href='/link/"+item+"'>"+System.IO.Path.GetFileName(item)+"</directory><br>";

									} else {
										ResultStr += "<file item='file' href='/link/"+item+"'>"+System.IO.Path.GetFileName(item)+"</file><br>";
									}
							}
							response.BodyBuffer = System.Text.Encoding.Default.GetBytes(ResultStr);
						} else {
							if(msg.DirectiveObj.StartsWith("/PUSHSCRIPT")) { 
								//ScriptData
								try {
									string fName = System.IO.Path.GetFileName(new System.Uri(msg.DirectiveObj).LocalPath);
									System.IO.File.WriteAllText(fName,System.Text.Encoding.UTF8.GetString(System.Convert.FromBase64String(msg.StringBuffer)));
								} catch (Exception ex) {
									response.StatusCode = 500;
									response.StatusData = ex.ToString();
								}
							} else if(msg.DirectiveObj.StartsWith("/SaveConfig")) {
							} else if(msg.DirectiveObj.StartsWith("/SaveForm")) {
								try {
									string fName = System.IO.Path.GetFileName(new System.Uri(msg.DirectiveObj).LocalPath);
									System.IO.File.WriteAllText(fName,System.Text.Encoding.UTF8.GetString(System.Convert.FromBase64String(msg.StringBuffer)));
								} catch (Exception ex) {
										response.StatusCode = 500;
										response.StatusData = ex.ToString();
								}
							} else {
								response.StatusCode = 404;
								response.StatusData = "NOT_FOUND";
							}
						}
					}
					break;
				}
				WebSession
					.Send(response);

			});
			_objUPnPDevice.UniqueDeviceName = System.Guid.NewGuid().ToString();
			_objUPnPDevice.User = Environment.UserName + "@" + Environment.UserDomainName;
			_objUPnPDevice.Manufacturer = "FeuerwehrCloud";
			_objUPnPDevice.StandardDeviceType = "DEIVA";
			_objUPnPDevice.ManufacturerURL = "http://www.feuerwehrcloud.de";
			_objUPnPDevice.ModelName = "DEIVA";
			_objUPnPDevice.ModelURL = new Uri("http://www.feuerwehrcloud.de/");
			_objUPnPDevice.ModelNumber = Application.ProductVersion.ToString();
			_objUPnPDevice.SerialNumber = "";
			_objUPnPDevice.ProductCode = "";
			_objUPnPDevice.ModelDescription = "FeuerwehrCloud DEIVA " + Application.ProductVersion.ToString() + ", running on " + OSText; //(System.Environment.OSVersion.Platform==PlatformID.MacOSX?"MacOS X":(System.Environment.OSVersion.Platform==PlatformID.Unix?"lINUX":(System.Environment.OSVersion.Platform==PlatformID.Win32NT?"Windows NT":(System.Environment.OSVersion.Platform==PlatformID.WinCE?"Windows CE":(System.Environment.OSVersion.Platform==PlatformID.Xbox?"XBOX?!!?":"Unbekannt"))))) + " " + (System.Environment.OSVersion.VersionString);
			_objUPnPDevice.PresentationURL = "/";
			_objUPnPDevice.FriendlyName = "DEIVA: " + Environment.MachineName.ToString();
			_objUPnPDevice.StartDevice(19270);
			FeuerwehrCloud.Helper.Logger.WriteLine ("|  Initialized UPnP-Server");
			new System.Threading.Thread (delegate(object obj) {
				try {
					(new System.Net.WebClient()).DownloadStringAsync(new Uri("http://www.feuerwehrcloud.de/deiva/start.php?hn="+System.Environment.MachineName));
				} catch (Exception ex) {

				}
			}).Start ();
			AppDomain currentDomain = default(AppDomain);
			currentDomain = AppDomain.CurrentDomain;
			/*currentDomain.FirstChanceException += delegate(object sender, FirstChanceExceptionEventArgs e) {
				FeuerwehrCloud.Helper.Logger.WriteLine("**************** FirstChanceException *****************");
				FeuerwehrCloud.Helper.Logger.WriteLine(((Exception)e.Exception).Message);
				FeuerwehrCloud.Helper.Logger.WriteLine(((Exception)e.Exception).StackTrace);
				FeuerwehrCloud.Helper.Logger.WriteLine("**************** FirstChanceException *****************");
				//Xamarin.Insights.Report (e.Exception, Insights.Severity.Warning);
			};
*/

			currentDomain.ProcessExit+= delegate(object sender, EventArgs e) {


			};
			currentDomain.UnhandledException += delegate(object sender, UnhandledExceptionEventArgs e) {
				FeuerwehrCloud.Helper.Logger.WriteLine("**************** UnhandledException *****************");
				FeuerwehrCloud.Helper.Logger.WriteLine(((Exception)e.ExceptionObject).Message);
				FeuerwehrCloud.Helper.Logger.WriteLine(((Exception)e.ExceptionObject).StackTrace);
				FeuerwehrCloud.Helper.Logger.WriteLine("**************** UnhandledException *****************");
				//Xamarin.Insights.Report((Exception)e.ExceptionObject, Insights.Severity.Error);
				/*var smtpClient = new System.Net.Mail.SmtpClient ();
				smtpClient.Host = "feuerwehrcloud.de";
				smtpClient.Port = 25;
				smtpClient.EnableSsl = false;
				smtpClient.DeliveryMethod = System.Net.Mail.SmtpDeliveryMethod.Network;
				var smtp = smtpClient;
				var mailMessage = new System.Net.Mail.MailMessage ("bug@raspberry.pi", "info@feuerwehrcloud.de");
				mailMessage.Subject = "[FeuerwehrCloud] EXCEPTION AT " + System.Environment.MachineName;
				mailMessage.Body = e.ToString ();
				using (var message = mailMessage)
					smtp.Send (message);
				*/
			};
			DailyTimer = new System.Threading.Timer (delegate {
				System.Diagnostics.Debug.WriteLine(">> Daily Timer");
				FeuerwehrCloud.Helper.Logger.WriteLine("|  ["+System.DateTime.Now.ToString("T")+"] --- " + System.DateTime.Now.ToString("dddd, dd.MM.yyyy"));
				FeuerwehrCloud.Helper.Logger.WriteLine("|  ["+System.DateTime.Now.ToString("T")+"] *** Switching logs...");
				FeuerwehrCloud.Helper.Logger.WriteLine("|  ["+System.DateTime.Now.ToString("T")+"] *** Done.");
				DailyTimer.Change(24*60*60*1000,0);
			});
			DateTime Morgen = (new DateTime(System.DateTime.Now.Year, System.DateTime.Now.Month, System.DateTime.Now.Day, 23, 59, 59)).AddSeconds(2);
			TimeSpan ZeitBisMorgen = (new TimeSpan(1,0,0,1)) - System.DateTime.Now.TimeOfDay;
			DailyTimer.Change(((int)ZeitBisMorgen.TotalMilliseconds),0);

			/*TrackTimer = new System.Threading.Timer (delegate {
				Insights.Track("HeartBeat");
			});*/
			//TrackTimer.Change(new TimeSpan(0,5,0), new TimeSpan(0,5,0));
			return new Service ();
		}

		public static void StopServer() {
		}

		[MTAThread]
		public static void Main (string[] args)
		{
			//FeuerwehrCloud.Helper.Logger.WriteLine ("*****************************************************");
			//FeuerwehrCloud.Helper.Logger.WriteLine (">> Insights.Initialize(...)");
			//Xamarin.Insights.Initialize ("6a06545960cad8ccb572bb228b21470f3e1d5c09"); //,Application.ProductVersion,"FeuerwehrCloud.DEIVA"); //, , "FeuerwehrCloud.DEIVA", null,);
			//FeuerwehrCloud.Helper.Logger.WriteLine (">> Insights.Identify("+System.Environment.MachineName+")");
			//Insights.Identify (System.Environment.MachineName, new System.Collections.Generic.Dictionary<string, string> {
			//	{ Insights.Traits.Name,  System.Environment.MachineName }
			//});
			//FeuerwehrCloud.Helper.Logger.WriteLine (">> Insights.ForceDataTransmission = true;");
			//Insights.ForceDataTransmission = true;
			//FeuerwehrCloud.Helper.Logger.WriteLine ("Insights.Track (\"StartUp\");");
			//Insights.Track ("StartUp");
			//FeuerwehrCloud.Helper.Logger.WriteLine ("*****************************************************");
			Service P;
			using (P = StartServer())
			{

				//System.Environment.CurrentDirectory = "/Volumes/Macintosh HD/FeuerwehrCloud - Source/FeuerwehrCloud.Service/bin/Debug";
				//System.IO.Directory.SetCurrentDirectory ("/Volumes/Macintosh HD/FeuerwehrCloud - Source/FeuerwehrCloud.Service/bin/Debug");
				#if __WINDOWS__

				for (bool exit = false; !exit; )
				{
				System.Threading.Thread.Sleep(0);
				Application.DoEvents();
				}

				#else
				UnixSignal [] signals = new UnixSignal[] { 
					new UnixSignal(Signum.SIGINT), 
					new UnixSignal(Signum.SIGTERM), 
					new UnixSignal(Signum.SIGHUP), 
					new UnixSignal(Signum.SIGUSR2),
					new UnixSignal(Signum.SIGQUIT)
					//new UnixSignal(Signum.SIGSEGV),
					//
				};

				for (bool exit = false; !exit; )
				{
					int id = UnixSignal.WaitAny(signals);

					if (id >= 0 && id < signals.Length)
					{
						if (signals[id].IsSet) {
							exit = true;
							P.Dispose();
							System.Environment.Exit(1);
						}
					}
				}
				#endif
			}
		}
	}
}
