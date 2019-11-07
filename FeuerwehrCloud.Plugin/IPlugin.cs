using System;

namespace FeuerwehrCloud.Plugin
{

	/// <remarks>
	/// input = Servicetyp für Dateneingang
	/// processor = Servicetyp für Datenverarbeitung
	/// output = Servicetype für Datenausgabe
	/// general = Servicetyp für eigenständige Plugins
	/// </remarks>
	public enum ServiceType {
		input     = 0,
		processor = 1,
		output    = 2,
		general   = 3
	}

	[Serializable]
	public delegate void PluginEvent (object sender, params object[] list);

	/// <summary>
	/// Interfaceklasse der FeuerwehrCloud DEIVA Hostanwendung
	/// </summary>
	public interface IHost : IDisposable {
		/// <summary>
		/// Ruft ein vom der Hostanwendung geladenes Plugin auf.
		/// </summary>
		/// <param name="library">Pluginname des aufzurufenden Plugins</param>
		/// <param name="list">Parameter die an das Plugin übergeben werden sollen</param>
		void Execute(string library, params object[] list);

        DateTime LastAlert { get; set; }
        string LastENumber { get; set; }

	}

	/// <summary>
	/// Interfaceklasse der FeuerwehrCloud DEIVA Plugins
	/// </summary>
	public interface IPlugin : IDisposable
	{
		/// <summary>
		/// Ruft das Ereignis "Event" in der Hostanwendung auf.
		/// </summary>
		/// <remarks>Durch den Aufruf dieses Events wird die zum Plugin gehörige .LUA-Datei ausgeführt (sofern vorhanden)</remarks>
		event PluginEvent Event;

		/// <summary>
		/// Gibt an, ob das Plugin seine Operationen asynchron ausführt
		/// </summary>
		bool IsAsync { get; }

		/// <summary>
		/// Gibt den Servicetyp des Plugins zurück
		/// </summary>
		ServiceType ServiceType { get; }

		/// <summary>
		/// Führt die Initialisierungsfunktion aus und übergibt eine Referenz zur Hostanwendung an das Plugin
		/// </summary>
		/// <param name="hostApplication">Die Hostanwendung</param>
		bool Initialize(IHost hostApplication);

		/// <summary>
		/// Gibt den internen Namen des Plugins zurück
		/// </summary>
		void Execute(params object[] list);

		/// <summary>
		/// Gibt den internen Namen des Plugins zurück
		/// </summary>
		string Name { get;}

		/// <summary>
		/// Gibt den Anzeigename des Plugins zurück
		/// </summary>
		string FriendlyName { get;}

		/// <summary>
		/// Gibt die GUID des Plugins zurück
		/// </summary>
		Guid GUID { get;}

		/// <summary>
		/// Gibt das Symbol des Plugins zurück
		/// </summary>
		byte[] Icon { get;}
	}
}

