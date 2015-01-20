using System;

namespace FeuerwehrCloud.Plugin
{

	public enum ServiceType {
		input     = 0,
		processor = 1,
		output    = 2,
		general   = 3
	}

	[Serializable]
	public delegate void PluginEvent (object sender, params object[] list);

	public interface IHost : IDisposable {
		void Execute(string library, params object[] list);
	}

	public interface IPlugin : IDisposable
	{
		event PluginEvent Event;
		bool IsAsync { get; }
		ServiceType ServiceType { get; }
		bool Initialize(IHost hostApplication);
		void Execute(params object[] list);
	}
}

