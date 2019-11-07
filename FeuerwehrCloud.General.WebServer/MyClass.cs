using System;
using System.Net;
using HttpServer;
using HttpServer.Addons.CGI;
using HttpServer.Addons.Files;
using HttpServer.Addons.Modules;
using HttpServer.Addons.Routing;
using FeuerwehrCloud.Plugin;
using System.Threading;
using HttpServer.Addons;

using System.IO;
using System.Reflection;

namespace FeuerwehrCloud.General.WebServer
{
	public class WebServer : FeuerwehrCloud.Plugin.IPlugin
	{

		private static System.Collections.Generic.Dictionary<string, string> WebConfig = new System.Collections.Generic.Dictionary<string, string> ();
		static HttpServer.HttpListener listener;
		static System.Threading.Thread WSThread;
		private static System.Threading.ManualResetEvent ThreadExitEvent = new System.Threading.ManualResetEvent(false);
		private FeuerwehrCloud.Plugin.IHost My;

			#region IPlugin implementation

		public string Name {
			get {
				return "WebServer";
			}
		}
		public string FriendlyName {
			get {
				return "Webserver-Modul";
			}
		}

		public Guid GUID {
			get {
				return new Guid ("A");
			}
		}

		public byte[] Icon {
			get {
				var assembly = typeof(FeuerwehrCloud.General.WebServer.WebServer).GetTypeInfo().Assembly;
				string[] resources = assembly.GetManifestResourceNames();
				Stream stream = assembly.GetManifestResourceStream("icon.ico");
				return ((MemoryStream)stream).ToArray();
			}
		}


		public event FeuerwehrCloud.Plugin.PluginEvent Event;

		public bool Initialize (IHost hostApplication)
		{
			My = hostApplication;
			if(!System.IO.File.Exists("webserver.cfg")) {
				WebServer.WebConfig.Add ("RootWebDir", "./web/");
				WebServer.WebConfig.Add ("MaxPostSize", "102400");
				WebServer.WebConfig.Add ("PathToPHP", "/usr/bin/php");
				WebServer.WebConfig.Add ("IndexFile", "index.php,index.fritz");
				WebServer.WebConfig.Add ("Port", "80");
				FeuerwehrCloud.Helper.AppSettings.Save(WebServer.WebConfig,"webserver.cfg");
			} 
			WebServer.WebConfig = FeuerwehrCloud.Helper.AppSettings.Load ("webserver.cfg");
			FeuerwehrCloud.Helper.Logger.WriteLine ("|  *** WebServer loaded: listening on port: " + WebServer.WebConfig["Port"]);

			WebServer.WSThread = new System.Threading.Thread ( delegate() {
				WebServer.listener = HttpServer.HttpListener.Create (IPAddress.Any, int.Parse(WebServer.WebConfig["Port"]));
				var server = new Server();
				server.MaxContentSize = int.Parse(WebServer.WebConfig["MaxPostSize"]);
				var deivaModule = new DEIVAModule(hostApplication);
				server.Add(deivaModule);
				var cgiService = new  CgiService(WebServer.WebConfig["PathToPHP"], "php");
				var cgiModule = new CgiModule(WebServer.WebConfig["RootWebDir"], cgiService);
				server.Add(cgiModule);
				//var avmModule = new AVMModule(WebServer.WebConfig["RootWebDir"], hostApplication);
				//server.Add(avmModule);
				var fileService = new DiskFileService("/", WebServer.WebConfig["RootWebDir"]);
				var fileModule = new GzipFileModule(fileService) { EnableGzip = true };
				server.Add(fileModule);
				var router = new DefaultIndexRouter(WebServer.WebConfig["RootWebDir"]);
				router.AddIndexFile(WebServer.WebConfig["IndexFile"]);
				server.Add(router);
				var dirModule = new DirectoryModule(fileService);
				server.Add(dirModule);
				server.Add(listener);
				try {
					server.Start(10);
					ThreadExitEvent.WaitOne();
					server.Stop(true);
				} catch (System.Exception ex) {
					if(ex.Message.Contains("already in")) {
						FeuerwehrCloud.Helper.Logger.WriteLine("Kann FeuerwehrCloud-Server HTTP-Modul nicht starten!");
					}
				}
			});
			WebServer.WSThread.Start ();
			return true;
		}

		public void Execute (params object[] list)
		{
			//throw new NotImplementedException ();
		}

		public bool IsAsync {
			get {
				return true;
			}
		}

		public FeuerwehrCloud.Plugin.ServiceType ServiceType {
			get {
				return FeuerwehrCloud.Plugin.ServiceType.general;
			}
		}

		#endregion

		#region IDisposable implementation

		public void Dispose ()
		{
			ThreadExitEvent.Set ();
			//throw new NotImplementedException ();
		}

		#endregion

		public WebServer ()
		{
		}
	}
}

