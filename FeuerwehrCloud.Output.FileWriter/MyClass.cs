using System;
using System.Text;
using FeuerwehrCloud.Plugin;
using System.ComponentModel;

namespace FeuerwehrCloud.Output.FileWriter
{
	public class FileWriter : FeuerwehrCloud.Plugin.IPlugin 
	{
	
		public event PluginEvent Event;
		private IHost My;

		public bool IsAsync
		{
			get { return true; }
		}

		public ServiceType ServiceType
		{
			get { return ServiceType.output; }
		}

		public void Dispose() {
		}

		public void Execute(params object[] list) {
			string[] f = Array.ConvertAll (list, p => (string)p);
			string OutPut = string.Join ("\n", f, 1, f.Length - 1);
			System.IO.File.WriteAllText ((string)(list [0]), OutPut);
			de.SYStemiya.Helper.Logger.WriteLine ("| ["+System.DateTime.Now.ToString("T") +"] |-> [FileWriter] *** File ("+((string)(list [0])).ToUpper()+") written...");
		}

		public bool Initialize(IHost hostApplication) {
			My = hostApplication;
			de.SYStemiya.Helper.Logger.WriteLine ("| ["+System.DateTime.Now.ToString("T") +"] |-> [FileWriter] *** Initializing...");
			return true;
		}
			
		public FileWriter()
		{
		}

	}

	#region Actitity
    public class FileWriterActivityValidator : System.Workflow.ComponentModel.Compiler.ActivityValidator
    {
    }

    [System.Workflow.ComponentModel.Compiler.ActivityValidator(typeof(FileWriterActivityValidator))]
    [System.Drawing.ToolboxBitmap(typeof(FileWriterActivity), "filewriter.ico")]
    public class FileWriterActivity : System.Workflow.ComponentModel.Activity
    {
        public FileWriterActivity ()  {
            this.Name = "FileWriterActivity";
        }

        public static System.Workflow.ComponentModel.DependencyProperty FileNameProperty = System.Workflow.ComponentModel.DependencyProperty.Register(
			"Dateiname", typeof(string), typeof(FileWriterActivity));
        [Description("Legt den Dateinamen fest, in den die Daten geschrieben werden sollen")]
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

