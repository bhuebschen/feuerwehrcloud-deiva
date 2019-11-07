using System;
using SQLite;

namespace HttpServer.Addons
{
	[Table ("users")]
	public class users  
	{
		[PrimaryKey, AutoIncrement, Column("id")]
		public int id { get; set; }

		[MaxLength (250), Column("Vorname")]
		public string Vorname { get; set; }

		[MaxLength (250), Column("Nachname")]
		public string Nachname { get; set; }

		[MaxLength (250), Column("mobile-id")]
		public string mobile_id { get; set; }

		[MaxLength (250), Column("rights")]
		public string rights { get; set; }


	}

}

