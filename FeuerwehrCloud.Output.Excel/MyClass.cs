using System;
using System.Windows;
using System.Windows.Forms;
//using //SpreadsheetGear;
//using //SpreadsheetGear.Windows;
using FeuerwehrCloud.Plugin;
using System.ComponentModel;

namespace FeuerwehrCloud.Output.Excel
{
	public class XLS:  IPlugin
	{
		public event PluginEvent Event;
		private IHost My;

		public bool Initialize(IHost hostApplication) {
			My = hostApplication;
			de.SYStemiya.Helper.Logger.WriteLine ("| ["+System.DateTime.Now.ToString("T") +"] |-> [ExcelPrinter] *** Initializing...");
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
			de.SYStemiya.Helper.Logger.WriteLine ("| ["+System.DateTime.Now.ToString("T") +"] |-> [ExcelPrinter] *** Unloading...");
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
				Console.Write(ex);
				Console.ForegroundColor = ConsoleColor.Gray;
			}
		}

		public XLS ()
		{
		}
	}

	#region Actitity
	public class ExcelWriterActivityValidator : System.Workflow.ComponentModel.Compiler.ActivityValidator
	{
	}

	[System.Workflow.ComponentModel.Compiler.ActivityValidator(typeof(ExcelWriterActivityValidator))]
	[System.Drawing.ToolboxBitmap(typeof(ExcelWriterActivity), "excel.ico")]
	public class ExcelWriterActivity : System.Workflow.ComponentModel.Activity
	{
		public ExcelWriterActivity ()  {
			this.Name = "ExcelWriterActivity";
		}

		public static System.Workflow.ComponentModel.DependencyProperty FileNameProperty = System.Workflow.ComponentModel.DependencyProperty.Register(
			"Dateiname", typeof(string), typeof(ExcelWriterActivity));
		[Description("Legt den Dateinamen der Excel-Datei fest die verarbeitet werden sollen")]
		[Category("Activity")]
		[Browsable(true)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		public string Dateiname
		{
			get
			{
				return ((string)base.GetValue(FileNameProperty));
			}
			set
			{
				base.SetValue(FileNameProperty, value);
			}
		}
	}
	#endregion

}

