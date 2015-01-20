using System;
using FeuerwehrCloud.Plugin;
using de.SYStemiya;

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

		public System.Collections.Generic.Dictionary<string, IPlugin> PlugIns = new System.Collections.Generic.Dictionary<string, Plugin.IPlugin>();
		public NLua.Lua lua;

		public Service() {
			try {
				de.SYStemiya.Helper.Logger.WriteLine ("| ["+System.DateTime.Now.ToString("T") +"] *** Initializing NLua...");
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
                de.SYStemiya.Helper.Logger.WriteLine(ex.ToString());
				return;
			}

			var plugins = System.IO.Directory.EnumerateFiles ("./plugins", "FeuerwehrCloud*.dll");
			//FeuerwehrCloud.Plugin.IPlugin c = new  FeuerwehrCloud.Input.SMTP.SMTPSever ();
			foreach (string currentFile in plugins)
			{
				var DLL = Assembly.LoadFile (currentFile);
				foreach(Type type in DLL.GetExportedTypes())
				{
					FeuerwehrCloud.Plugin.IPlugin c; 
					try {
						c = (FeuerwehrCloud.Plugin.IPlugin)Activator.CreateInstance(type);
						lua[System.IO.Path.GetFileNameWithoutExtension (currentFile).Replace(".","_")] = c;
						lua.DebugHook += HandleDebugHook;
						lua.RegisterFunction(System.IO.Path.GetFileNameWithoutExtension (currentFile).Replace(".","_")+"_Execute", c, c.GetType().GetMethod("Execute"));
						if (c.Initialize (this)) {

							PlugIns.Add (System.IO.Path.GetFileNameWithoutExtension (currentFile), c);
							c.Event += delegate(object sender, object[] list) {
								lua["response"] = list;
								switch (c.ServiceType) {
								case FeuerwehrCloud.Plugin.ServiceType.input:
									if (list [0] == "pictures") {
										try {
											lua.DoFile (sender.ToString () + ".lua");
										}
										catch (Exception ex) {
											de.SYStemiya.Helper.Logger.WriteLine (ex.InnerException.ToString ());
										}
									}
									else
										if (list [0] == "text") {
											try {
												lua.DoFile (sender.ToString () + ".lua");
											}
											catch (Exception ex) {
												de.SYStemiya.Helper.Logger.WriteLine (ex.InnerException.ToString ());
											}
										}
									break;
								case FeuerwehrCloud.Plugin.ServiceType.processor:
									if (list [0] == "pictures") {
										try {
											lua.DoFile (sender.ToString () + ".lua");
										}
										catch (Exception ex) {
											de.SYStemiya.Helper.Logger.WriteLine (ex.InnerException.ToString ());
										}
									}
									else
										if (list [0] == "text") {
											try {
												lua.DoFile (sender.ToString () + ".lua");
											}
											catch (Exception ex) {
												de.SYStemiya.Helper.Logger.WriteLine (ex.ToString ());
												try {
												}
												catch (Exception ex2) {
													var smtpClient = new System.Net.Mail.SmtpClient ();
													smtpClient.Host = "systemiya.de";
													smtpClient.Port = 25;
													smtpClient.EnableSsl = false;
													smtpClient.DeliveryMethod = System.Net.Mail.SmtpDeliveryMethod.Network;
													var smtp = smtpClient;
													var mailMessage = new System.Net.Mail.MailMessage ("bug@raspberry.pi", "benedikt.huebschen@sysemiya.de");
													mailMessage.Subject = "[FeuerwehrCloud] EXCEPTION AT " + System.Environment.MachineName;
													mailMessage.Body = ex.ToString ();
													using (var message = mailMessage)
														smtp.Send (message);
												}
											}
										}
									break;
								case FeuerwehrCloud.Plugin.ServiceType.output:
									try {
										lua.DoFile (sender.ToString () + ".lua");
									} catch (Exception ex) {
										de.SYStemiya.Helper.Logger.WriteLine (ex.ToString ());
									}
									break;
								}
								de.SYStemiya.Helper.Logger.WriteLine(list[0].ToString());
							};
						}				
											
					} catch (Exception ex) {
						//System.Diagnostics.Debug.WriteLine(ex);
					}
				}
				lua ["PlugIns"] = PlugIns;
			}
			lua.DoFile ("init_complete.lua");
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
			Dispose(true);
		}

		~Service()
		{
			Dispose(false);
		}


	}


	class MainClass
	{
		static string Level = "|";
		static UPnPDevice _objUPnPDevice;

		public static Service StartServer() {
			de.SYStemiya.Helper.Logger.WriteLine (",---------------------------------------------------------.");
			de.SYStemiya.Helper.Logger.WriteLine ("|                                                         |");
			de.SYStemiya.Helper.Logger.WriteLine ("|  FeuerwehrCloud 1.0 DEIVA                               |");
			de.SYStemiya.Helper.Logger.WriteLine ("|                                                         |");
			de.SYStemiya.Helper.Logger.WriteLine ("|  Copyright (C) 2013 - 2015                              |");
			de.SYStemiya.Helper.Logger.WriteLine ("|  FeuerwehrCloud mGmbH Haftungsbeschränkt                |");
			de.SYStemiya.Helper.Logger.WriteLine ("|                                                         |");
			de.SYStemiya.Helper.Logger.WriteLine ("|                                                         |");
			de.SYStemiya.Helper.Logger.WriteLine ("|---------------------------------------------------------´");
			de.SYStemiya.Helper.Logger.WriteLine ("|");
			de.SYStemiya.Helper.Logger.WriteLine ("| FeuerwehrCloud DEIVA " + Application.ProductVersion.ToString() + ", running on " +  (System.Environment.OSVersion.VersionString));
			de.SYStemiya.Helper.Logger.WriteLine ("| *** Starting UPnP-Server");
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
							string[] files = System.IO.Directory.GetFiles("."+msg.DirectiveObj, "*");
							string ResultStr = "";
							foreach (var item in files) {
								ResultStr += "<a href='/link/"+item+"'>"+System.IO.Path.GetFileName(item)+"</a><br>";
							}
							response.BodyBuffer = System.Text.Encoding.Default.GetBytes(ResultStr);
						} else {
							response.StatusCode = 404;
							response.StatusData = "NOT_FOUND";
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
			_objUPnPDevice.ManufacturerURL = "http://www.feuerwehrcloud.de";
			_objUPnPDevice.ModelName = "DEIVA";
			_objUPnPDevice.ModelURL = new Uri("http://www.feuerwehrcloud.de/");
			_objUPnPDevice.ModelNumber = Application.ProductVersion.ToString();
			_objUPnPDevice.SerialNumber = "";
			_objUPnPDevice.ProductCode = "";
			_objUPnPDevice.ModelDescription = "FeuerwehrCloud DEIVA " + Application.ProductVersion.ToString() + ", running on " + (System.Environment.OSVersion.Platform==PlatformID.MacOSX?"MacOS X":(System.Environment.OSVersion.Platform==PlatformID.Unix?"lINUX":(System.Environment.OSVersion.Platform==PlatformID.Win32NT?"Windows NT":(System.Environment.OSVersion.Platform==PlatformID.WinCE?"Windows CE":(System.Environment.OSVersion.Platform==PlatformID.Xbox?"XBOX?!!?":"Unbekannt"))))) + " " + (System.Environment.OSVersion.VersionString);
			_objUPnPDevice.PresentationURL = "/";
			_objUPnPDevice.FriendlyName = "DEIVA: " + Environment.MachineName.ToString();
			_objUPnPDevice.StartDevice(19270);
			de.SYStemiya.Helper.Logger.WriteLine ("| *** Advertising UPnP-Services and DeviceInformations");
			AppDomain currentDomain = default(AppDomain);
			currentDomain = AppDomain.CurrentDomain;
/*			currentDomain.UnhandledException += delegate(object sender, UnhandledExceptionEventArgs e) {
				var smtpClient = new System.Net.Mail.SmtpClient ();
				smtpClient.Host = "systemiya.de";
				smtpClient.Port = 25;
				smtpClient.EnableSsl = false;
				smtpClient.DeliveryMethod = System.Net.Mail.SmtpDeliveryMethod.Network;
				var smtp = smtpClient;
				var mailMessage = new System.Net.Mail.MailMessage ("bug@raspberry.pi", "benedikt.huebschen@sysemiya.de");
				mailMessage.Subject = "[FeuerwehrCloud] EXCEPTION AT " + System.Environment.MachineName;
				mailMessage.Body = e.ToString ();
				using (var message = mailMessage)
					smtp.Send (message);
			};*/
			return new Service ();
		}

		[MTAThread]
		public static void Main (string[] args)
		{
			using (StartServer())
			{

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
				};

				for (bool exit = false; !exit; )
				{
					int id = UnixSignal.WaitAny(signals);

					if (id >= 0 && id < signals.Length)
					{
						if (signals[id].IsSet) exit = true;
					}
				}
#endif
			}
		}
	}
}
