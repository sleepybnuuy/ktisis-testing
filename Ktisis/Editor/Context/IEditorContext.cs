using System;

using Ktisis.Data.Config;
using Ktisis.Editor.Actions;
using Ktisis.Editor.Camera;
using Ktisis.Editor.Characters;
using Ktisis.Editor.Selection;
using Ktisis.Editor.Transforms;
using Ktisis.Localization;
using Ktisis.Scene;

namespace Ktisis.Editor.Context;

public interface IEditorContext : IDisposable {
	public bool IsValid { get; }
	
	public Configuration Config { get; }
	public LocaleManager Locale { get; }
	
	public IActionManager Actions { get; }
	public IAppearanceManager Appearance { get; }
	public ICameraManager Cameras { get; }
	public ISceneManager Scene { get; }
	public ISelectManager Selection { get; }
	public ITransformHandler Transform { get; }

	public IEditorContext Initialize();
	
	public void Update();
}
