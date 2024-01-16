using Dalamud.Game;
using Dalamud.IoC;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;

using Microsoft.Extensions.DependencyInjection;

namespace Ktisis.Services; 

// ReSharper disable once ClassNeverInstantiated.Global
public sealed class DalamudServices {
	[PluginService] private IClientState _clientState { get; set; } = null!;
	[PluginService] private ICommandManager _cmd { get; set; } = null!;
	[PluginService] private IFramework _framework { get; set; } = null!;
	[PluginService] private IGameGui _gui { get; set; } = null!;
	[PluginService] private IGameInteropProvider _interop { get; set; } = null!;
	[PluginService] private IObjectTable _objectTable { get; set; } = null!;
	[PluginService] private IKeyState _keyState { get; set; } = null!;
	[PluginService] private IDataManager _data { get; set; } = null!;
	[PluginService] private ITextureProvider _tex { get; set; } = null!;
	[PluginService] private ISigScanner _sigScanner { get; set; } = null!;

	public void Add(DalamudPluginInterface dpi, IServiceCollection services) {
		dpi.Inject(this);
		services.AddSingleton(dpi)
			.AddSingleton(dpi.UiBuilder)
			.AddSingleton(this._clientState)
			.AddSingleton(this._cmd)
			.AddSingleton(this._framework)
			.AddSingleton(this._gui)
			.AddSingleton(this._interop)
			.AddSingleton(this._objectTable)
			.AddSingleton(this._keyState)
			.AddSingleton(this._data)
			.AddSingleton(this._tex)
			.AddSingleton(this._sigScanner);
	}
}