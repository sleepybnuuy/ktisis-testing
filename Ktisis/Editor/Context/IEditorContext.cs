using System;

using Ktisis.Data.Config;
using Ktisis.Editor.Actions;
using Ktisis.Editor.Camera;
using Ktisis.Editor.Characters.Types;
using Ktisis.Editor.Posing.Types;
using Ktisis.Editor.Selection;
using Ktisis.Editor.Transforms;
using Ktisis.Interface;
using Ktisis.Interface.Editor;
using Ktisis.Interface.Editor.Types;
using Ktisis.Interop.Ipc;
using Ktisis.Localization;
using Ktisis.Scene;
using Ktisis.Scene.Types;

namespace Ktisis.Editor.Context;

public interface IEditorContext : IDisposable {
	public bool IsValid { get; }
	
	public Configuration Config { get; }
	public LocaleManager Locale { get; }
	public IpcManager Ipc { get; }
	
	public IActionManager Actions { get; }
	public ICharacterState Characters { get; }
	public ICameraManager Cameras { get; }
	public IEditorInterface Interface { get; }
	public IPosingManager Posing { get; }
	public ISceneManager Scene { get; }
	public ISelectManager Selection { get; }
	public ITransformHandler Transform { get; }

	public IEditorContext Initialize();
	
	public void Update();
}
