using System;

namespace BosAlarm
{
	[Serializable]
	internal class Alarm
	{
		private string _Name;
		private string _Trigger;
		private string _Type;

		public Alarm()
		{
		}

		public Alarm(string name, string trigger, string type)
		{
			this._Name = name;
			this._Trigger = trigger;
			this._Type = type;
		}

		public string Name
		{
			get
			{
				return this._Name;
			}
			set
			{
				this._Name = value;
			}
		}

		public string Trigger
		{
			get
			{
				return this._Trigger;
			}
			set
			{
				this._Trigger = value;
			}
		}

		public string Type
		{
			get
			{
				return this._Type;
			}
			set
			{
				this._Type = value;
			}
		}
	}
}

