using System;
using FeuerwehrCloud.Plugin;
using System.Reflection;
using System.IO;
using System.Net;

namespace FeuerwehrCloud.Output
{
    public class Cokpit : Plugin.IPlugin
    {
        #region IPlugin implementation

        public event PluginEvent Event;
        IHost My;
        private System.Collections.Generic.Dictionary<string, string> Config = new System.Collections.Generic.Dictionary<string, string> ();
        FileSystemWatcher watcher;

        public bool Initialize(IHost hostApplication)
        {
            My = hostApplication;
            if(!System.IO.File.Exists("cokpit.cfg")) {
                Config.Add ("automatic", "1");
                FeuerwehrCloud.Helper.AppSettings.Save(Config,"cokpit.cfg");
            } 
            Config = FeuerwehrCloud.Helper.AppSettings.Load ("cokpit.cfg");
            FeuerwehrCloud.Helper.Logger.WriteLine ("|  *** FeuerwehrCloud Cokpit loaded...");
            watcher = new FileSystemWatcher ();
            watcher.Path = System.Environment.CurrentDirectory;
            watcher.IncludeSubdirectories = false;
            watcher.Filter = "cokpit.cfg";
            watcher.NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.LastAccess | NotifyFilters.DirectoryName | NotifyFilters.FileName;
            watcher.Changed += new FileSystemEventHandler(delegate(object sender, FileSystemEventArgs e) {
                Config = FeuerwehrCloud.Helper.AppSettings.Load ("cokpit.cfg");
            });
            watcher.EnableRaisingEvents = true;
            return true;
        }

        public void Execute(params object[] list)
        {
            FeuerwehrCloud.Helper.Logger.WriteLine ("|  > [Cokpit] *** Preparing output...");
            System.Collections.Generic.Dictionary<string, string> Daten = list[0] as System.Collections.Generic.Dictionary<string, string>;


            CookieContainer cookieJar = new CookieContainer();
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create("https://www.feuerwehrcloud.de/deiva/einsatz.php");
            request.CookieContainer = cookieJar;
            request.UserAgent ="Mozilla/5.0 (U; Linux armv7; de_DE) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/36 Safari/537.36 Gecko/20140101 FeuerwehrCloud/1.0";
            request.CookieContainer = cookieJar;

            string XML = System.IO.File.ReadAllText("web/"+Daten["EinsatzNr"].Replace(" ","_")+".xml");
            string B64File = System.Convert.ToBase64String(System.IO.File.ReadAllBytes("/tmp/" + Daten["FILENAME"]));
            string PostData = "fwid="+System.Environment.MachineName+"&enum="+Daten["EinsatzNr"]+"&txt="+Daten["RAW"]+"&stw="+Daten["EinsatzStichwort"]+"&schw="+ Daten["EinsatzSchlagwort"] + "&xml="+XML+"&fax="+Daten["FILENAME"]+"&fFile="+B64File;
            request.ContentLength = PostData.Length;
            request.ContentType = "application/x-www-form-urlencoded";
            request.Method = "POST";
            Stream dataStream = request.GetRequestStream ();
            dataStream.Write (System.Text.Encoding.Default.GetBytes(PostData), 0, PostData.Length);
            dataStream.Close ();
            var response = (HttpWebResponse)request.GetResponse();
            var GRS = response.GetResponseStream ();
            var SR = new StreamReader (GRS);
            //Result = SR.ReadToEnd ();

        }

        public bool IsAsync {
            get {
                return true;
            }
        }

        public ServiceType ServiceType {
            get {
                return ServiceType.output;
            }
        }

        public string Name {
            get {
                return "Cokpit";
            }
        }

        public string FriendlyName {
            get {
                return "Cokpit Ausgabemodul";
            }
        }

        public Guid GUID {
            get {
                return new Guid(Name);
            }
        }

        public byte[] Icon {
            get {
                var assembly = typeof(FeuerwehrCloud.Output.Cokpit).GetTypeInfo().Assembly;
                string[] resources = assembly.GetManifestResourceNames();
                Stream stream = assembly.GetManifestResourceStream("icon.ico");
                return ((MemoryStream)stream).ToArray();
            }
        }

        #endregion

        #region IDisposable implementation

        public void Dispose()
        {
            FeuerwehrCloud.Helper.Logger.WriteLine ("|  > [Cokpit] *** Unloading...");
        }

        #endregion
        public Cokpit()
        {
        }
    }
}

