using System;
using System.Windows.Forms;
using System.Drawing;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
namespace FeuerwehrCloud.PrintXLS
{
	class Program
	{
		[STAThread]
		public static void Main (string[] Pages)
		{
			XmlReader reader = XmlReader.Create(Pages[0]); 
			DataContractSerializer serializer = new DataContractSerializer(typeof(Dictionary<string,string>));

			Dictionary<string,string> list = (Dictionary<string,string>)serializer.ReadObject(reader); 
			reader.Close();

			SpreadsheetGear.Windows.Forms.WorkbookView WV = new SpreadsheetGear.Windows.Forms.WorkbookView();

			de.SYStemiya.Helper.Logger.WriteLine ("| ["+System.DateTime.Now.ToString("T") +"] |-> [ExcelPrinter] *** Opening " +  Pages[1]);
			WV.ActiveWorkbook =  SpreadsheetGear.Factory.GetWorkbookSet().Workbooks.Open(Pages[1]);
			WV.GetLock();
			SpreadsheetGear.IWorksheet WB = WV.ActiveWorkbook.Worksheets [0];
			for(int x = 0; x<25; x++) {
				for(int y = 0; y<25; y++) {
					SpreadsheetGear.IRange IR = WB.Cells [x, y];
					string CValue = IR.Text;
					try { 
						if(CValue == "#EINSATZORT#") { 
							IR.Value = list["EinsatzOrt"]; 
						} 
					} catch(Exception e1) {

					}
					try { 
						if(CValue == "#EINSATZNR#") { 
							IR.Value = list["EinsatzNr"]; 
						} 
					} catch(Exception e1) {

					}
					try { 
						if(CValue == "#EINSATZSTRASSE#") { 
							IR.Value = list["EinsatzStrasse"]; 
						} 
					} catch(Exception e1) {

					}
					try { 
						if(CValue == "#EINSATZABSCHNITT#") { 
							IR.Value = list["EinsatzAbschnitt"]; 
						}
					} catch(Exception e1) {

					}
					try { 
						if(IR.Value == "#EINSATZKREUZUNG#") { 
							IR.Value = list["EinsatzKreuzung"];
						} 
					} catch(Exception e1) {

					}
					try { 
						if(IR.Value == "#EINSATZOBJEKT#") { 
							IR.Value = list["EinsatzObjekt"];
						} 
					} catch(Exception e1) {

					}
					try { 
						if(IR.Value == "#EINSATZBMERKUNG#") {
							IR.Value = list["EinsatzBemerkung"];
						}
					} catch(Exception e1) {

					}
					try { 
						if(IR.Value == "#EINSATZPRIORITAET#") { 
							IR.Value = list["EinsatzPrioritaet"]; 
						} 
					} catch(Exception e1) {

					}
					try { 
						if(IR.Value == "#EINSATZSTICHWORT#") { 
							IR.Value = list["EinsatzStichwort"]; 
						} 
					} catch(Exception e1) {

					}
					try { 
						if(IR.Value == "#EINSATZSCHLAGWORT#") { 
							IR.Value = list["EinsatzSchlagwort"];
						} 
					} catch(Exception e1) {

					}
					try { 
						if(IR.Value == "#DATUM#") { 
							IR.Value = System.DateTime.Now.ToString("d");
						}
					} catch(Exception e1) {

					}
				}

			}
			WB.PageSetup.LeftMargin=0;
			WB.PageSetup.RightMargin=0;
			WB.PageSetup.TopMargin=0;
			WB.PageSetup.BottomMargin=0;
			WB.PageSetup.PaperSize = SpreadsheetGear.PaperSize.A4;
			WB.PageSetup.FitToPagesWide=1;
			WV.ReleaseLock();
			WV.Print(false);	
			WV.GetLock();
			WV.ActiveWorkbook.Close();
		}
	}
}

