using System;
using FeuerwehrCloud.Plugin;
using MySql.Data.MySqlClient;
using System.Collections;
using System.Collections.Generic;

namespace FeuerwehrCloud.Output.MySQL
{
	public class MySQL : FeuerwehrCloud.Plugin.IPlugin
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
			MySqlConnection M = new MySqlConnection ();
			M.ConnectionString = System.IO.File.ReadAllText (list[0]+".csf");
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
			de.SYStemiya.Helper.Logger.WriteLine ("| ["+System.DateTime.Now.ToString("T") +"] |-> [MySQL] *** Initializing...");
			return true;
		}

		public MySQL ()
		{
		}
	}
}

