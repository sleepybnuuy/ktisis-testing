using System.Numerics;

using Dalamud.Interface;
using Dalamud.Interface.Utility.Raii;

using ImGuiNET;

using Ktisis.Editor.Context;
using Ktisis.Interface.Types;
using Ktisis.Interface.Components.Workspace;

namespace Ktisis.Interface.Windows; 

public class WorkspaceWindow : KtisisWindow {
	private readonly ContextManager _editor;

	private readonly ContextButtons _buttons;

	private readonly CameraSelector _camera;
	private readonly WorkspaceState _state;
	private readonly SceneTree _sceneTree;
	private readonly SceneTreeButtons _sceneButtons;
	
	public WorkspaceWindow(
		ContextManager editor,
		ContextButtons buttons,
		CameraSelector camera,
		WorkspaceState state,
		SceneTree sceneTree,
		SceneTreeButtons sceneTreeButtons
	) : base("Ktisis Workspace") {
		this._editor = editor;
		this._buttons = buttons;
		this._camera = camera;
		this._state = state;
		this._sceneTree = sceneTree;
		this._sceneButtons = sceneTreeButtons;
	}
	
	// Constants
	
	private static readonly Vector2 MinimumSize = new(280, 300);
	
	// Draw handler

	public override void Draw() {
		this.SizeConstraints = new WindowSizeConstraints {
			MinimumSize = MinimumSize,
			MaximumSize = ImGui.GetIO().DisplaySize * 0.9f
		};
		
		this.Draw(this._editor.Context);
	}

	private void Draw(IEditorContext? context) {
		var scene = context?.Scene;
		using var _disable = ImRaii.Disabled(context == null);

		var style = ImGui.GetStyle();
		
		// Context buttons
		
		if (context != null)
			this._buttons.Draw(context);
		
		// Cameras

		ImGui.Spacing();
		if (context != null)
			this._camera.Draw(context);
		
		// Scene
		
		this._state.Draw(context);

		var botHeight = UiBuilder.IconFont.FontSize + (style.ItemSpacing.Y + style.ItemInnerSpacing.Y) * 2;
		var treeHeight = ImGui.GetContentRegionAvail().Y - botHeight;
		this._sceneTree.Draw(scene, treeHeight);

		ImGui.Spacing();
		
		if (scene != null)
			this._sceneButtons.Draw(scene);
	}
}
 