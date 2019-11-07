using System;
using System.Windows;
using System.Windows.Forms;
//using //SpreadsheetGear;
//using //SpreadsheetGear.Windows;
using FeuerwehrCloud.Plugin;
using System.ComponentModel;
using System.IO;
using System.Reflection;

namespace FeuerwehrCloud.Output
{
	public class Excel:  IPlugin
	{
		public event PluginEvent Event;
		private FeuerwehrCloud.Plugin.IHost My;


		public string Name {
			get {
				return "Excel";
			}
		}
		public string FriendlyName {
			get {
				return "Excel-Ausgabe";
			}
		}

		public Guid GUID {
			get {
				return new Guid ("5");
			}
		}

		public byte[] Icon {
			get {
				var assembly = typeof(FeuerwehrCloud.Output.Excel).GetTypeInfo().Assembly;
				string[] resources = assembly.GetManifestResourceNames();
				Stream stream = assembly.GetManifestResourceStream("icon.ico");
				return ((MemoryStream)stream).ToArray();
			}
		}

		public bool Initialize(IHost hostApplication) {
			My = hostApplication;
			FeuerwehrCloud.Helper.Logger.WriteLine ("|  *** ExcelPrinter loaded...");
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
			FeuerwehrCloud.Helper.Logger.WriteLine ("|   [ExcelPrinter] *** Unloading...");
		}

		public void RaiseFinish(params object[] list) {
			PluginEvent messageSent = Event;
			if (messageSent != null)
			{
				messageSent(this, list);
			}
		}
			
		public void Execute(params object[] list) {
			try {
				System.Threading.Thread P1 = new System.Threading.Thread(new System.Threading.ParameterizedThreadStart(delegate(object obj) {
					try {
						var l = ((System.Collections.Generic.Dictionary<string, string>)(((object[])obj)[1]));

						System.Xml.XmlWriterSettings settings = new System.Xml.XmlWriterSettings(); 
						settings.Indent = true; 
						settings.Encoding = System.Text.Encoding.UTF8;
						string dataName = "/tmp/"+l.GetHashCode().ToString()+"-"+ (  System.Guid.NewGuid().ToString()  ) +".xml";

						using(System.Xml.XmlWriter writer = System.Xml.XmlWriter.Create(dataName, settings)) {
							System.Runtime.Serialization.DataContractSerializer serializer = new System.Runtime.Serialization.DataContractSerializer(typeof(System.Collections.Generic.Dictionary<string,string>)); 
							serializer.WriteObject(writer, ((System.Collections.Generic.Dictionary<string, string>)(object)list[1])); 
							writer.Close();
						}

						System.Diagnostics.Process PP = new System.Diagnostics.Process();
						PP.StartInfo.FileName="printxls.sh";
						PP.StartInfo.Arguments =  dataName+" "+list[0];
						PP.Start(); PP.WaitForExit();

					} catch (Exception ex) {

					}

				}));
				P1.Start(list);
			} catch (Exception ex) {
				Console.ForegroundColor = ConsoleColor.Red;
				FeuerwehrCloud.Helper.Logger.WriteLine(ex.ToString());
				Console.ForegroundColor = ConsoleColor.Gray;
			}
		}

		public Excel ()
		{
		}
	}


}

