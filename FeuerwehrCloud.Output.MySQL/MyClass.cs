  using System;
using FeuerwehrCloud.Plugin;
using MySql.Data.MySqlClient;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace FeuerwehrCloud.Output
{
	public class MySQL : FeuerwehrCloud.Plugin.IPlugin
	{
		public event PluginEvent Event;
		private FeuerwehrCloud.Plugin.IHost My;

		public string Name {
			get {
				return "MySQL";
			}
		}

		public string FriendlyName {
			get {
				return "MySQL";
			}
		}

		public Guid GUID {
			get {
				return new Guid ("A");
			}
		}

		public byte[] Icon {
			get {
				var assembly = typeof(FeuerwehrCloud.Output.MySQL).GetTypeInfo().Assembly;
				string[] resources = assembly.GetManifestResourceNames();
				Stream stream = assembly.GetManifestResourceStream("icon.ico");
				return ((MemoryStream)stream).ToArray();
			}
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
		
		}

		public void Execute(params object[] list) {
			MySqlConnection M = new MySqlConnection ();
			M.ConnectionString = System.IO.File.ReadAllText (list[0]+".constrf");
			M.Open ();
			var C = M.CreateCommand ();
			C.CommandText = (string)(list [1]);
			MySql.Data.MySqlClient.MySqlDataReader Reader;
			if((int)(list [2]) == 1) {
				System.Collections.Generic.Dictionary<string, object>[] ResultCollections = new Dictionary<string, object>[0];
				Reader = C.ExecuteReader();
				try {
					if (Reader.HasRows) {
						int Count = 0;
						while (Reader.Read()) {
							Array.Resize(ref ResultCollections, Count + 1);
							ResultCollections[Count] = new System.Collections.Generic.Dictionary<string, object>();
							for (int Fid = 0; Fid <= Reader.FieldCount - 1; Fid++) {
								if (ResultCollections[Count].ContainsKey(Reader.GetName(Fid))) {
									ResultCollections[Count].Add(Fid + Reader.GetName(Fid),Reader.GetValue(Fid));
								} else {
									ResultCollections[Count].Add(Reader.GetName(Fid),Reader.GetValue(Fid));
								}
							}
							Count += 1;
						}
					}
				} catch (Exception ex) {
				} finally {
					if ((Reader != null))
						Reader.Close();
					Reader = null;
				}
				list [3] = ResultCollections;
			} else {
				C.ExecuteNonQuery ();
			}
		}
			
		public bool Initialize(IHost hostApplication) {
			My = hostApplication;
			FeuerwehrCloud.Helper.Logger.WriteLine ("|  *** MySQL loaded...");
			return true;
		}

		public MySQL ()
		{
		}
	}
}

