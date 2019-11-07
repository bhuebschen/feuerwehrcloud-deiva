using System;
using System.Diagnostics;
using FeuerwehrCloud.Plugin;
using System.IO;
using System.Reflection;
using System.Text;

namespace FeuerwehrCloud.Processor.OCR
{
	public class Tesseract : IPlugin
	{
		private System.Collections.Generic.Dictionary<string, string> tesseractconfig = new System.Collections.Generic.Dictionary<string, string> ();
		FileSystemWatcher watcher;

		public Tesseract ()
		{
		}


		public string Name {
			get {
				return "Tesseract";
			}
		}
		public string FriendlyName {
			get {
				return "Tesseract Texterkennung";
			}
		}

		public Guid GUID {
			get {
				return new Guid (Name);
			}
		}

		public byte[] Icon {
			get {
				var assembly = typeof(FeuerwehrCloud.Processor.OCR.Tesseract).GetTypeInfo().Assembly;
				string[] resources = assembly.GetManifestResourceNames();
				Stream stream = assembly.GetManifestResourceStream("icon.ico");
				return ((MemoryStream)stream).ToArray();
			}
		}

		public bool Initialize(IHost hostApplication) {
			FeuerwehrCloud.Helper.Logger.WriteLine ("|  *** OCR loaded...");
			if(!System.IO.File.Exists("tesseract.cfg")) {
				FeuerwehrCloud.Helper.Logger.WriteLine ("|  + [OCR] *** ERROR! NO CONFIGURATION FOUND!");
				tesseractconfig.Add ("psm", "");
				tesseractconfig.Add ("lang", "eng");
				FeuerwehrCloud.Helper.AppSettings.Save(tesseractconfig,"tesseract.cfg");
			} 
			tesseractconfig = FeuerwehrCloud.Helper.AppSettings.Load ("tesseract.cfg");

			watcher = new FileSystemWatcher ();
			watcher.Path = System.Environment.CurrentDirectory;
			watcher.IncludeSubdirectories = false;
			watcher.Filter = "tesseract.cfg";
			watcher.NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.LastAccess | NotifyFilters.DirectoryName | NotifyFilters.FileName;
			watcher.Changed += new FileSystemEventHandler((object sender, FileSystemEventArgs e) => tesseractconfig = FeuerwehrCloud.Helper.AppSettings.Load("tesseract.cfg"));
			watcher.EnableRaisingEvents = true;

			return true;
		}

		public event PluginEvent Event;

		public bool IsAsync
		{
			get { return true; }
		}

		public ServiceType ServiceType
		{
			get { return ServiceType.processor; }
		}

		public void Dispose() {
			FeuerwehrCloud.Helper.Logger.WriteLine ("|  + [OCR] *** Unloading...");
		}

		public void RaiseFinish(params object[] list) {
			PluginEvent messageSent = Event;
			if (messageSent != null)
			{
				messageSent(this, list);
			}
		}

		public void Execute(params object[] list) {
			if (list == null)
				return;

			// Check if provided fax is valid!
			// II* TIFF
			// PNG PNG
			// JFIF JPG
			// %PDF- PDF

			string[] Pages = (string[])list [1];
			string CompleteOCROutput = string.Empty;
			FeuerwehrCloud.Helper.Logger.WriteLine ("|  + [OCR] *** Do OCR for: ");
			try {
				foreach (string item in Pages) {
					FeuerwehrCloud.Helper.Logger.WriteLine (item);
					System.Diagnostics.Process P = new System.Diagnostics.Process();
					P.StartInfo.FileName = "/usr/bin/tesseract";
					P.StartInfo.Arguments = "/tmp/"+item+" /tmp/"+(item)+" -l " + tesseractconfig["lang"] + (tesseractconfig["psm"]!=""?" -psm " + tesseractconfig["psm"]:"");
					P.StartInfo.UseShellExecute = false;
					P.StartInfo.RedirectStandardOutput = true;
					P.StartInfo.RedirectStandardError = true;
					P.Start(); P.WaitForExit();
					String XContent = string.Empty;
					try {
						XContent = System.IO.File.ReadAllText("/tmp/"+(item)+".txt", System.Text.Encoding.UTF8);
					} catch (Exception ex) {
						FeuerwehrCloud.Helper.Logger.WriteLine (FeuerwehrCloud.Helper.Helper.GetExceptionDescription (ex));
					}
					CompleteOCROutput = XContent;
				}
			} catch (Exception ex2) {
				CompleteOCROutput =  System.IO.File.ReadAllText("/tmp/probealarm.png.txt");
			}


	
            RaiseFinish ("text", new[]{ CompleteOCROutput, Pages[0] } );
		}
	}
}