using System;
using System.Net;
using HttpServer;
using HttpServer.Addons.CGI;
using HttpServer.Addons.Files;
using HttpServer.Addons.Modules;
using HttpServer.Addons.Routing;
using FeuerwehrCloud.Plugin;
namespace FeuerwehrCloud.General.WebServer
{
	public class WebServer : FeuerwehrCloud.Plugin.IPlugin
	{

		private static System.Collections.Generic.Dictionary<string, string> WebConfig = new System.Collections.Generic.Dictionary<string, string> ();
		static HttpServer.HttpListener listener;
		static System.Threading.Thread WSThread;
		private static System.Threading.ManualResetEvent ThreadExitEvent = new System.Threading.ManualResetEvent(false);
		private IHost My;

		#region IPlugin implementation

		public event FeuerwehrCloud.Plugin.PluginEvent Event;

		public bool Initialize (IHost hostApplication)
		{
			My = hostApplication;
			if(!System.IO.File.Exists("webserver.cfg")) {
				WebServer.WebConfig.Add ("RootWebDir", "./web/");
				WebServer.WebConfig.Add ("MaxPostSize", "102400");
				WebServer.WebConfig.Add ("PathToPHP", "/Applications/MAMP/bin/php/php5.4.19/bin/php");
				WebServer.WebConfig.Add ("IndexFile", "index.php,index.fritz");
				WebServer.WebConfig.Add ("Port", "80");
				de.SYStemiya.de.SYStemiya.Helper.AppSettings.Save(WebServer.WebConfig,"webserver.cfg");
			} 
			WebServer.WebConfig = de.SYStemiya.de.SYStemiya.Helper.AppSettings.Load ("webserver.cfg");
			de.SYStemiya.de.SYStemiya.Helper.Logger.WriteLine ("| ["+System.DateTime.Now.ToString("T") +"] |-| [WebServer] *** Start listening on port: " + WebServer.WebConfig["Port"]);

			WebServer.WSThread = new System.Threading.Thread ( delegate() {
				WebServer.listener = HttpServer.HttpListener.Create (IPAddress.Any, int.Parse(WebServer.WebConfig["Port"]));
				var server = new Server();
				server.MaxContentSize = int.Parse(WebServer.WebConfig["MaxPostSize"]);
				var cgiService = new  CgiService(WebServer.WebConfig["PathToPHP"], "php");
				var cgiModule = new CgiModule(WebServer.WebConfig["RootWebDir"], cgiService);
				server.Add(cgiModule);
				var avmModule = new AVMModule(WebServer.WebConfig["RootWebDir"], hostApplication);
				server.Add(avmModule);
				var fileService = new DiskFileService("/", WebServer.WebConfig["RootWebDir"]);
				var fileModule = new GzipFileModule(fileService) { EnableGzip = true };
				server.Add(fileModule);
				var router = new DefaultIndexRouter(WebServer.WebConfig["RootWebDir"]);
				router.AddIndexFile(WebServer.WebConfig["IndexFile"]);
				server.Add(router);
				var dirModule = new DirectoryModule(fileService);
				server.Add(dirModule);
				server.Add(listener);

				server.Start(10);
				ThreadExitEvent.WaitOne();
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
			WSThread.Abort ();
			//throw new NotImplementedException ();
		}

		#endregion

		public WebServer ()
		{
		}
	}
}

