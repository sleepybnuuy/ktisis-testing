using System;

using Dalamud.Bindings.ImGui;

using Ktisis.Editor.Context.Types;
using Ktisis.Interface.Editor.Properties.Types;
using Ktisis.Localization;
using Ktisis.Scene.Entities;
using Ktisis.Scene.Entities.World;
using Ktisis.Editor.Camera.Types;
using Ktisis.Interface.Windows.Editors;

namespace Ktisis.Interface.Editor.Properties;

public class CameraPropertyList : ObjectPropertyList {
	private readonly IEditorContext _ctx;
	private readonly GuiManager _gui;
	private readonly LocaleManager _locale;

	public CameraPropertyList (
		IEditorContext ctx,
		GuiManager gui,
		LocaleManager locale
	) {
        this._ctx = ctx;
        this._gui = gui;
        this._locale = locale;
    }
	
	public override void Invoke(IPropertyListBuilder builder, SceneEntity entity) {
		if (entity is not CameraEntity camera)
			return;
		
		builder.AddHeader("Camera", () => this.DrawCameraTab(camera));
        // todo: filters, chromatic aberration, etc
		// builder.AddHeader("Camera Settings", () => this.DrawSettingsTab(camera));
	}

    private unsafe void DrawCameraTab(CameraEntity camera) {
		var embedWindow = this._gui.GetOrCreate<CameraWindow>(this._ctx);
		embedWindow.PreDraw();
		embedWindow.DrawEmbed(camera);
    }
}