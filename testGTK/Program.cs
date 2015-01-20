using System;
using System.IO;
using System.Drawing;
using Gtk;

namespace FeuerwehrCloud.PrintImage
{
	class MainClass
	{
		public static void Main (string[] Pages)
		{
			Application.Init ();
			var print = new Gtk.PrintOperation();
			print.PrintSettings = new Gtk.PrintSettings();
			print.PrintSettings.Scale=33.75;
			print.PrintSettings.Resolution=1200;
			print.JobName = "FeuerwehrCloud Ausdruck";
			print.BeginPrint += (obj2, args) => { print.NPages = Pages.Length; };
			print.DrawPage += (obj2, args) => {
				try {
					de.SYStemiya.Helper.Logger.WriteLine ("| ["+System.DateTime.Now.ToString("T") +"] |-> [ImagePrinter] *** Page " + args.PageNr.ToString() + " of " + Pages.Length.ToString());
					var imageBit = default(byte[]);
					var image = System.Drawing.Image.FromFile("/tmp/"+Pages[args.PageNr]);
					using (var memoryStream = new MemoryStream()) {
						image.Save(memoryStream, System.Drawing.Imaging.ImageFormat.Png);
						imageBit = memoryStream.ToArray();
					}
					Gtk.PrintContext context = args.Context;
					try {
						var pixBuf = new Gdk.Pixbuf(imageBit, image.Width, image.Height);
						try {
							Cairo.Context cr = context.CairoContext;
							cr.MoveTo(0, 0);
							Gdk.Cairo.SetSourcePixbuf(cr, pixBuf,0,10);
							cr.Paint();
							((IDisposable) cr).Dispose();

						} catch (Exception ex4) {
							de.SYStemiya.Helper.Logger.WriteLine("| ["+System.DateTime.Now.ToString("T") +"] |-> [ImagePrinter] *** ERROR: " +ex4.ToString());
						}							
					} catch (Exception ex5) {
						de.SYStemiya.Helper.Logger.WriteLine("| ["+System.DateTime.Now.ToString("T") +"] |-> [ImagePrinter] *** ERROR: " +ex5.ToString());
					}
				} catch (Exception ex3) {
					de.SYStemiya.Helper.Logger.WriteLine("| ["+System.DateTime.Now.ToString("T") +"] |-> [ImagePrinter] *** ERROR: " +ex3.ToString());
				}
			};
			print.EndPrint += (obj2, args) => { de.SYStemiya.Helper.Logger.WriteLine ("| ["+System.DateTime.Now.ToString("T") +"] |-> [ImagePrinter] *** Printing finished "); Application.Quit(); };
			print.Run(Gtk.PrintOperationAction.Print, null);
		}
	}
}
