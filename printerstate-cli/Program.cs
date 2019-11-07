using System;

namespace printerstatecli
{
	class MainClass
	{

		static string WritePJL(string Type, string Address, string Command) {
			System.IO.Stream T = null;
			try {
				System.Net.Sockets.TcpClient TC;
				if(Type == "usb") {
					T = System.IO.File.Open (Address, System.IO.FileMode.Open, System.IO.FileAccess.ReadWrite,System.IO.FileShare.ReadWrite);
				} else if(Type == "serial") {
					T = System.IO.File.Open (Address, System.IO.FileMode.Open, System.IO.FileAccess.ReadWrite,System.IO.FileShare.ReadWrite);
				} else if(Type == "parallel") {
					T = System.IO.File.Open (Address, System.IO.FileMode.Open, System.IO.FileAccess.ReadWrite,System.IO.FileShare.ReadWrite);
				} else if(Type == "ethernet") {
					TC = new System.Net.Sockets.TcpClient ();
					TC.Connect(Address, 9100);
					T = TC.GetStream();
				}
				byte[] b = System.Text.Encoding.Default.GetBytes (Command);
				T.Write (b, 0, b.Length);
				byte[] b2 = new byte[1024]; int xLen = T.Read(b2, 0, 1024);
				T.Close();
				return System.Text.Encoding.Default.GetString(b2).Substring(0,xLen);

			} catch (Exception ex) {
				if (T != null)
					T.Close (); 
				return "";
			}
		}


		public static void Main (string[] args)
		{
			string StatusText = WritePJL("usb","/dev/usb/lp0",String.Format ("\x1B%-12345X@PJL INFO STATUS \r\n\x1B%-12345X\r\n"));
			Console.WriteLine (StatusText);
		}
	}
}
