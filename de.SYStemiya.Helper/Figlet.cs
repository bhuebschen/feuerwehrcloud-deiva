using System;

namespace de.SYStemiya.Helper
{
	
	public class Figlet
	{

		string strSignature;
		string strHardBlank;
		int intHeight;
		int intBaseline;
		int intMaxLen;
		int intOldLayout;
		int intCommentLines;
		int intPrintDirection;
		int intFullLayout;
		int intCodeTagCount;
		string strEndMark;

		string[] fontData;
		public string GetCharacter(string xchar, int intLine)
		{
			int intAscii = 0;
			int intStart = 0;
			string strTmp = null;

			intAscii = System.Text.Encoding.ASCII.GetBytes(xchar)[0];
			intStart = (intCommentLines + ((intAscii - 32) * intHeight));
			strTmp = fontData[(intStart + intLine)];
			strTmp = strTmp.Replace (strHardBlank, " ").Replace (strEndMark, " ");
			return strTmp;
		}

		public void LoadFont(string fontFile)
		{
			string[] arrTmp = null;
			string strTmp = null;
			int uBnd = 0;

			fontData = System.IO.File.ReadAllLines(fontFile);

			arrTmp = fontData[0].Split(new [] {" "},StringSplitOptions.None);
			uBnd = arrTmp.Length-1;
			strTmp = arrTmp[0];
			strSignature = strTmp.Left(strTmp.Length-1);
			strHardBlank = strTmp.Right (1);
			intHeight = Convert.ToInt32(arrTmp[1]);
			intBaseline = Convert.ToInt32(arrTmp[2]);
			intMaxLen = Convert.ToInt32(arrTmp[3]);
			intOldLayout = Convert.ToInt32(arrTmp[4]);
			intCommentLines = Convert.ToInt32(arrTmp[5]);
			if ((uBnd > 5))
				intPrintDirection = Convert.ToInt32(arrTmp[6]);
			if ((uBnd > 6))
				intFullLayout = Convert.ToInt32(arrTmp[7]);
			if ((uBnd > 7))
				intCodeTagCount = Convert.ToInt32(arrTmp[8]);
			if ((strSignature != "flf2a")) {
				throw new Exception ("Font " + fontFile + " is not a Figley font!");
			}
			strEndMark =  fontData[intCommentLines+intHeight].Substring(fontData[intCommentLines+intHeight].Length-1,1);
		}

		public void Display(string strText, string strFont)
		{
			int idx = 0;
			int cntr = 0;
			string xchar = null;

			LoadFont(strFont);
			for (cntr = 1; cntr <= intHeight; cntr++) {
				for (idx = 1; idx <= strText.Len(); idx++) {
					xchar = strText.Mid(idx, 1);
					Logger.Write(GetCharacter(xchar, cntr));
				}
				Logger.Write ("\r\n");
			}
		}

	}
}
