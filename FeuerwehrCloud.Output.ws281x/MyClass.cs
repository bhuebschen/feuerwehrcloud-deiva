using System;
using System.Runtime.InteropServices;
using System.Drawing;
using System.IO;
using System.Reflection;

namespace FeuerwehrCloud.Output
{
	public class ws281x : Plugin.IPlugin
	{

		[DllImport("libws2812")]
		static extern void Init(int LEDcount);

		[DllImport("libws2812")]
		static extern void setPixel(int pixel, byte r, byte g, byte b);

		[DllImport("libws2812")]
		static extern void setPixelA(int pixel, byte r, byte g, byte b, byte a);

		[DllImport("libws2812")]
		static extern void updatePixels();

		[DllImport("libws2812")]
		static extern void Dispose2();


		#region IPlugin implementation

		public event FeuerwehrCloud.Plugin.PluginEvent Event;

		public string Name {
			get {
				return "ws281x";
			}
		}
		public string FriendlyName {
			get {
				return "WS281x LED Controller";
			}
		}

		public Guid GUID {
			get {
				return new Guid ("ws281x");
			}
		}

		public byte[] Icon {
			get {
				var assembly = typeof(FeuerwehrCloud.Output.ws281x).GetTypeInfo().Assembly;
				string[] resources = assembly.GetManifestResourceNames();
				Stream stream = assembly.GetManifestResourceStream("icon.ico");
				return ((MemoryStream)stream).ToArray();
			}
		}


		public bool Initialize (FeuerwehrCloud.Plugin.IHost hostApplication)
		{
			return true;
		}

		public void Execute (params object[] list)
		{
			Init((int)list[0]);

			string MType = ((string)list [1]);
			switch (MType) {
				case "ON":
					new System.Threading.Thread (delegate() {
						for(int i=0;i<((int)list[0]); i++) {
							setPixel(i, GetXColor((string)list [2]) );
						}
						updatePixels();
					}).Start ();
					break;
				case "OFF":
					new System.Threading.Thread (delegate() {
						for(int i=0;i<((int)list[0]); i++) {
							setPixel(i, System.Drawing.Color.Black );
						}
						updatePixels();
					}).Start ();
					break;
				case "BLINK_SLOW":
					new System.Threading.Thread (delegate() {
						for (int i2 = 0; i2 < (int.Parse((string)list [3])); i2++) {
							for(int i=0;i<((int)list[0]); i++) {
								setPixel(i, GetXColor((string)list [2]) );
							}
							updatePixels();
							System.Threading.Thread.Sleep (500);
							for(int i=0;i<((int)list[0]); i++) {
								setPixel(i, System.Drawing.Color.Black );
							}
							updatePixels();
							System.Threading.Thread.Sleep (500);
						}
					}).Start ();
					break;
				case "FAST_DOUBLE_FLASH":
					new System.Threading.Thread(delegate() {
						for (int i2 = 0; i2 < (int.Parse((string)list [3])); i2++) {
							System.Threading.Thread.Sleep(500);
							for(int i=0;i<((int)list[0]); i++) {
								setPixel(i, GetXColor((string)list [2]) );
							}
							updatePixels();
							System.Threading.Thread.Sleep(50);
							for(int i=0;i<((int)list[0]); i++) {
								setPixel(i, System.Drawing.Color.Black );
							}
							updatePixels();
							System.Threading.Thread.Sleep(30);
							for(int i=0;i<((int)list[0]); i++) {
								setPixel(i, GetXColor((string)list [2]) );
							}
							updatePixels();
							System.Threading.Thread.Sleep(80);
							for(int i=0;i<((int)list[0]); i++) {
								setPixel(i, System.Drawing.Color.Black );
							}
							updatePixels();
						}
					}).Start();
					break;
				case "DOUBLE_FLASH":
					new System.Threading.Thread(delegate() {
						for (int i2 = 0; i2 < (int.Parse((string)list [3])); i2++) {
							System.Threading.Thread.Sleep(1000);
							for(int i=0;i<((int)list[0]); i++) {
								setPixel(i, GetXColor((string)list [2]) );
							}
							updatePixels();
							System.Threading.Thread.Sleep(50);
							for(int i=0;i<((int)list[0]); i++) {
								setPixel(i, System.Drawing.Color.Black );
							}
							updatePixels();
							System.Threading.Thread.Sleep(30);
							for(int i=0;i<((int)list[0]); i++) {
								setPixel(i, GetXColor((string)list [2]) );
							}
							updatePixels();
							System.Threading.Thread.Sleep(80);
							for(int i=0;i<((int)list[0]); i++) {
								setPixel(i, System.Drawing.Color.Black );
							}
							updatePixels();
						}
					}).Start();
					break;
				case "SINGLE_FLASH":
					new System.Threading.Thread(delegate() {
						for (int i2 = 0; i2 < (int.Parse((string)list [3])); i2++) {
							System.Threading.Thread.Sleep(1000);
							for(int i=0;i<((int)list[0]); i++) {
								setPixel(i, GetXColor((string)list [2]) );
							}
							updatePixels();
							System.Threading.Thread.Sleep(50);
							for(int i=0;i<((int)list[0]); i++) {
								setPixel(i, System.Drawing.Color.Black );
							}
							updatePixels();
						}
					}).Start();
					break;
				case "BLINK_FAST":
					new System.Threading.Thread (delegate() {
						for (int i2 = 0; i2 < (int.Parse((string)list [3])); i2++) {
							for(int i=0;i<((int)list[0]); i++) {
								setPixel(i, GetXColor((string)list [2]) );
							}
							updatePixels();
							System.Threading.Thread.Sleep (250);
							for(int i=0;i<((int)list[0]); i++) {
								setPixel(i, System.Drawing.Color.Black );
							}
							updatePixels();
							System.Threading.Thread.Sleep (250);
							for(int i=0;i<((int)list[0]); i++) {
								setPixel(i, GetXColor((string)list [2]) );
							}
							updatePixels();
							System.Threading.Thread.Sleep (250);
							for(int i=0;i<((int)list[0]); i++) {
								setPixel(i, System.Drawing.Color.Black );
							}
							updatePixels();
							System.Threading.Thread.Sleep (250);
						}
					}).Start ();
					break;			
				case "BLINK_UFAST":
					new System.Threading.Thread (delegate() {
						for (int i2 = 0; i2 < (int.Parse((string)list [3])); i2++) {
							for(int i=0;i<((int)list[0]); i++) {
								setPixel(i, GetXColor((string)list [2]) );
							}
							updatePixels();
							System.Threading.Thread.Sleep (125);
							for(int i=0;i<((int)list[0]); i++) {
								setPixel(i, System.Drawing.Color.Black );
							}
							updatePixels();
							System.Threading.Thread.Sleep (125);
							for(int i=0;i<((int)list[0]); i++) {
								setPixel(i, GetXColor((string)list [2]) );
							}
							updatePixels();
							System.Threading.Thread.Sleep (125);
							for(int i=0;i<((int)list[0]); i++) {
								setPixel(i, System.Drawing.Color.Black );
							}
							updatePixels();
							System.Threading.Thread.Sleep (125);
							for(int i=0;i<((int)list[0]); i++) {
								setPixel(i, GetXColor((string)list [2]) );
							}
							updatePixels();
							System.Threading.Thread.Sleep (125);
							for(int i=0;i<((int)list[0]); i++) {
								setPixel(i, System.Drawing.Color.Black );
							}
							updatePixels();
							System.Threading.Thread.Sleep (125);
							for(int i=0;i<((int)list[0]); i++) {
								setPixel(i, GetXColor((string)list [2]) );
							}
							updatePixels();
							System.Threading.Thread.Sleep (125);
							for(int i=0;i<((int)list[0]); i++) {
								setPixel(i, System.Drawing.Color.Black );
							}
							updatePixels();
							System.Threading.Thread.Sleep (125);
						}
					}).Start ();
					break;
			}
			//FeuerwehrCloud.Helper.Helper.exec ("/bin/send", (string)(list [1]) + " " + (string)(list [2]) + " " + (string)(list [3]));
		}

		public bool IsAsync {
			get {
				return true;
			}
		}

		public FeuerwehrCloud.Plugin.ServiceType ServiceType {
			get {
				return FeuerwehrCloud.Plugin.ServiceType.output;
			}
		}

		#endregion

		private void setPixel(int pixel, System.Drawing.Color Color) {
			if(Color.A != 255) {
				setPixelA(pixel, Color.R, Color.G, Color.B, Color.A);
			} else {
				setPixel(pixel, Color.R, Color.G, Color.B);
			}

		}

		private System.Drawing.Color GetXColor(string ColorName)
		{
			if (ColorName.Contains(",")) {
				string[] C = ColorName.Replace(" ", "").Split(new [] { "," }, StringSplitOptions.RemoveEmptyEntries);
				return System.Drawing.Color.FromArgb(255, Convert.ToByte(C[0]), Convert.ToByte(C[1]), Convert.ToByte(C[2]));
			} else if (ColorName.StartsWith("#")) {
				return System.Drawing.ColorTranslator.FromHtml(ColorName);
			} else {
				System.Drawing.Color C2 = default(System.Drawing.Color);
				C2 = System.Drawing.Color.FromName(ColorName);
				if (C2.IsEmpty) {
					System.Diagnostics.Debugger.Break();
				}
				return C2;
			}
		}

		#region IDisposable implementation

		public void Dispose ()
		{
		}

		#endregion

		public ws281x ()
		{
		}
	}
}

