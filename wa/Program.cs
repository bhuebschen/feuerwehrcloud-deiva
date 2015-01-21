using System;
using de;
using de.SYStemiya;

namespace wa
{
	class MainClass
	{
		static WhatsAppApi.WhatsApp wa;
		public static void Main (string[] args)
		{
			System.Collections.Generic.Dictionary<string, string> WAConfig = new System.Collections.Generic.Dictionary<string, string> ();
			WAConfig = de.SYStemiya.Helper.AppSettings.Load ("WhatsApp.cfg");

			wa = new WhatsAppApi.WhatsApp(WAConfig["Sender"], WAConfig["Password"], WAConfig["NickName"], true);
			wa.OnLoginSuccess += wa_OnLoginSuccess;
			wa.OnConnectSuccess += wa_OnConnectSuccess;
			wa.OnError += wa_OnError;
			wa.OnConnectFailed += wa_ConnectFailed;
			wa.OnGetContactName += delegate(string from, string contactName) {
				Console.WriteLine("*** " + contactName + "@" + from);
			};
			wa.OnGetGroupParticipants += delegate(string gjid, string[] jids) {
				
			};
			wa.OnGetGroups += delegate(WhatsAppApi.Response.WaGroupInfo[] groups) {
				Console.WriteLine("================================================");
				foreach (var item in groups) {
					Console.WriteLine(item.subject + "@" + item.id);
				}
				Console.WriteLine("================================================");
			};
			wa.OnGetGroupSubject += delegate(string gjid, string jid, string username, string subject, DateTime time) {
				Console.WriteLine("*** Topic on " + gjid + " is '"+subject+"' set by "+username+" on "+time.ToString());
			};
			wa.OnGetLastSeen += delegate(string from, DateTime lastSeen) {
				Console.WriteLine("*** " + from + " " + lastSeen.ToString());
			};
			wa.OnGetMessage += delegate(WhatsAppApi.de.SYStemiya.de.SYStemiya.Helper.ProtocolTreeNode messageNode, string from, string id, string name, string message, bool receipt_sent) {
				Console.WriteLine("<" + from +"["+id+"]> " + message);
			};
			wa.OnGetParticipantAdded += delegate(string gjid, string jid, DateTime time) {
				
			};
			wa.OnGetParticipantRemoved += delegate(string gjid, string jid, string author, DateTime time) {
				
			};
			wa.OnGetParticipantRenamed += delegate(string gjid, string oldJid, string newJid, DateTime time) {
				
			};
			//wa.On
			wa.OnLoginFailed += wa_LoginFailed;
			wa.Connect();
			wa.Login();
			wa.SendMessage ("4980353749-1391848501", "*** TESTALARM *** TESTALARM *** TESTALARM *** TESTALARM *** TESTALARM *** TESTALARM *** ");

			do {
				System.Threading.Thread.Sleep(1);
			} while (true);
			//wa.SendCreateGroupChat("ALARMIERUNG FFW-Neubeuern");

			//wa.SendAddParticipants ("4980353749-1391848501", new System.Collections.Generic.List<string>() { "491635583524" });
			//wa.SendAddParticipants ("4980353749-1391848501@g.us", new System.Collections.Generic.List<string>() { "491635583524" });
		}

		private static void wa_OnLoginSuccess(string phonenum, byte[] data)
		{
			de.SYStemiya.Helper.Logger.WriteLine ("| ["+System.DateTime.Now.ToString("T") +"] |-> [WhatsApp] *** Login success");
			wa.SendGetGroups ();
			wa.SendGetServerProperties ();
		}

		static void  wa_OnConnectSuccess ()
		{
			de.SYStemiya.Helper.Logger.WriteLine ("| ["+System.DateTime.Now.ToString("T") +"] |-> [WhatsApp] *** Connection success");
		}

		static void  wa_LoginFailed (string data)
		{
			de.SYStemiya.Helper.Logger.WriteLine ("| ["+System.DateTime.Now.ToString("T") +"] |-> [WhatsApp] *** Login failed! " + data);
		}

		static void  wa_ConnectFailed (Exception ex)
		{
			de.SYStemiya.Helper.Logger.WriteLine ("| ["+System.DateTime.Now.ToString("T") +"] |-> [WhatsApp] *** Connection failed!");
			de.SYStemiya.Helper.Logger.WriteLine (ex.ToString());
		}

		static void  wa_OnError (string id, string from, int code, string text)
		{
			de.SYStemiya.Helper.Logger.WriteLine ("| ["+System.DateTime.Now.ToString("T") +"] |-> [WhatsApp] *** ERROR! " + text);
		}
	}
}
