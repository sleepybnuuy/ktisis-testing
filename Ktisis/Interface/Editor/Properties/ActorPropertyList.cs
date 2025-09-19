using System.Collections.Generic;
using System.Numerics;
using System.Linq;

using Dalamud.Interface;
using Dalamud.Interface.Utility.Raii;
using Dalamud.Bindings.ImGui;

using GLib.Widgets;

using Ktisis.Data.Config;
using Ktisis.Editor.Context.Types;
using Ktisis.Interface.Editor.Properties.Types;
using Ktisis.Interface.Components.Transforms;
using Ktisis.Editor.Transforms.Types;
using Ktisis.Localization;
using Ktisis.Scene.Entities;
using Ktisis.Scene.Entities.Game;
using Ktisis.Scene.Entities.Skeleton;
using Ktisis.Structs.Actors;
using Ktisis.Interface.Editor.Popup;

namespace Ktisis.Interface.Editor.Properties;

public class ActorPropertyList : ObjectPropertyList {
	private readonly IEditorContext _ctx;
	private readonly ConfigManager _cfg;
	private readonly LocaleManager _locale;
	private readonly GuiManager _gui;
	private static Dictionary<GazeControl, TransformTable>? GazeTables;

	private bool IsLinked {
		get => this._ctx.Config.Editor.LinkedGaze;
		set => this._ctx.Config.Editor.LinkedGaze = value;
	}
	
	public ActorPropertyList(
		IEditorContext ctx,
		ConfigManager cfg,
		LocaleManager locale,
		GuiManager gui
	) {
		this._ctx = ctx;
		this._cfg = cfg;
		this._locale = locale;
		this._gui = gui;
	}
	
	public override void Invoke(IPropertyListBuilder builder, SceneEntity entity) {
		if (
			entity switch {
				BoneNode node => node.Pose.Parent,
				EntityPose pose => pose.Parent,
				_ => entity
			} is not ActorEntity actor
		) return;

		builder.AddHeader("Actor", () => this.DrawActorTab(actor), priority: 0);
		builder.AddHeader("Gaze Control", () => this.DrawGazeTab(actor), priority: 1);
	}
	
	// Actor tab

	private const string ImportOptsPopupId = "##KtisisCharaImportOptions";

	private void DrawActorTab(ActorEntity actor) {
		var spacing = ImGui.GetStyle().ItemInnerSpacing.X;
		
		// Position lock
		
		var posLock = this._ctx.Animation.PositionLockEnabled;
		if (ImGui.Checkbox(this._locale.Translate("actors.pos_lock"), ref posLock))
			this._ctx.Animation.PositionLockEnabled = posLock;
		
		ImGui.Spacing();
		
		// Open appearance editor

		if (Buttons.IconButton(FontAwesomeIcon.Edit))
			this._ctx.Interface.OpenActorEditor(actor);
		ImGui.SameLine(0, spacing);
		ImGui.Text("Edit actor appearance");
		
		ImGui.Spacing();
		
		// Import/export

		if (ImGui.Button("Import"))
			this._ctx.Interface.OpenCharaImport(actor);
		ImGui.SameLine(0, spacing);
		if (ImGui.Button("Export"))
			this._ctx.Interface.OpenCharaExport(actor);
	}
	
	// Gaze tab

	private unsafe void DrawGazeTab(ActorEntity actor) {
		if (GazeTables == null)
			GazeTables = new();

		// work from existing gaze on ActorEntity or make a new one if its our first touch w them
		var gaze = actor.Gaze != null ? (ActorGaze)actor.Gaze : new ActorGaze();
		// if human actor, enable link/unlinked controls
		bool isHuman = actor.GetHuman() != null;

		var spacing = ImGui.GetStyle().ItemInnerSpacing.X;
		var result = false;

		using (ImRaii.Disabled(this._ctx.Posing.IsEnabled)) {
			result |= DrawActorTargeting(actor);
			if (isHuman) {
				var icon = IsLinked ? FontAwesomeIcon.Link : FontAwesomeIcon.Unlink;
				if (Buttons.IconButton(icon)) {
					if (IsLinked) {
						var move = gaze.Other;
						if (move.Gaze.Mode != 0) {
							result = true;
							gaze.Head = move;
							gaze.Eyes = move;
							gaze.Torso = move;
							gaze.Other.Gaze.Mode = GazeMode.Disabled;
						}
					}
					IsLinked = !IsLinked;
				}
				ImGui.SameLine(0, spacing);
				ImGui.Text(IsLinked ? "Linked" : "Unlinked");
				ImGui.Spacing();
			}

			if (IsLinked || !isHuman)
				result |= DrawGaze(actor, ref gaze.Other.Gaze, GazeControl.All);
			else {
				result |= DrawGaze(actor, ref gaze.Eyes.Gaze, GazeControl.Eyes);
				ImGui.Spacing();
				result |= DrawGaze(actor, ref gaze.Head.Gaze, GazeControl.Head);
				ImGui.Spacing();
				result |= DrawGaze(actor, ref gaze.Torso.Gaze, GazeControl.Torso);
			}

			if (result) {
				Ktisis.Log.Info($"resetting gaze: {gaze}");
				actor.Gaze = gaze;
			}
		}

		return;
	}

	private unsafe bool DrawGaze(ActorEntity actor, ref Gaze gaze, GazeControl type) {
		if (!GazeTables.ContainsKey(type))
			GazeTables.Add(type, new TransformTable(this._cfg));

		using var _ = ImRaii.PushId($"Gaze_{type}");
		var spacing = ImGui.GetStyle().ItemInnerSpacing.X;

		var result = false;
		var enabled = gaze.Mode != 0;
		var actorCharacter = (CharacterEx*)actor.Character;
		var isTracking = gaze.Mode == GazeMode._KtisisFollowCam_;

		if (type != GazeControl.All || !enabled) {
			// if we're changing individual gazes (or viewing All w/o activation), set each to the basegaze for that type
			// for all when not enabled, this will zero it out
			var baseGaze = actorCharacter->Gaze[type];
			gaze.Pos = baseGaze.Pos;
		} else {
			// if we're changing ALL gazes and enabled, load in any as theyre all equal to gaze.Pos for the transformtable
			var baseGaze = actorCharacter->Gaze[GazeControl.Torso];
			gaze.Pos = baseGaze.Pos;
		}

		if (ImGui.Checkbox($"{type}", ref enabled)) {
			result = true;
			gaze.Mode = enabled ? GazeMode.Target : GazeMode.Disabled;
		}

		// calc button space for camera and gizmo buttons
		var btnSpace = Icons.CalcIconSize(FontAwesomeIcon.Eye).X
			+ Icons.CalcIconSize(FontAwesomeIcon.LocationArrow).X
			+ spacing * 3;
		ImGui.SameLine(0, spacing);
		ImGui.SameLine(0, ImGui.GetContentRegionAvail().X - btnSpace);

		// camera tracking - when pressed, toggle enabled and change gaze mode to KtisisFollowCam (or revert to Target mode)
		using (ImRaii.PushColor(ImGuiCol.Button, ImGui.GetColorU32(ImGuiCol.ButtonActive), isTracking)) {
			if (Buttons.IconButtonTooltip(FontAwesomeIcon.Eye, "Camera Tracking", Vector2.Zero)) {
				result = true;
				enabled = true;
				gaze.Mode = isTracking ? GazeMode.Target : GazeMode._KtisisFollowCam_;
			}
		}
		ImGui.SameLine(0, spacing);

		// TODO: gizmo, needs work to generate a new one w/o using ObjectWindow/OverlayWindow
		// 	or, possible to create a dummy scene object (GazeTarget?) to hijack overlay gizmo?
		using (ImRaii.Disabled()) {
			using var __ = ImRaii.PushColor(ImGuiCol.Button, ImGui.GetColorU32(ImGuiCol.ButtonActive), isTracking);
			if (Buttons.IconButtonTooltip(FontAwesomeIcon.LocationArrow, "Gizmo Tracking [TODO]", Vector2.Zero)) {}
		}

		result |= GazeTables[type].DrawPosition(ref gaze.Pos, TransformTableFlags.UseAvailable);

		return result;
	}

	private unsafe bool DrawActorTargeting(ActorEntity actor) {
		// 1. select button to choose entities in scene to set to target (or null target)
		// 2. show current target if one is set

		var spacing = ImGui.GetStyle().ItemInnerSpacing.X;
		var result = false;

		// grab an arbitrary basegaze from the 3 defaults, since all will be targeting the same obj if any
		var actorCharacter = (CharacterEx*)actor.Character;
		var baseGaze = actorCharacter->Gaze[GazeControl.Torso];
		var hasTarget = baseGaze.Mode == GazeMode.Object
			&& baseGaze.TargetId.Type > 0
			&& baseGaze.TargetId.ObjectId >= 201
			&& baseGaze.TargetId.ObjectId <= 243;

		// todo: enable target when none existed pre-gpose?
		using (ImRaii.Disabled(!hasTarget)) {
			// try to derive an entity in the scene that we're targeting
			ActorEntity? targetEntity = null;
			if (hasTarget) {
				var targetId = baseGaze.TargetId.ObjectId;
				var currentActors = this._ctx.Scene.Children
					.OfType<ActorEntity>()
					.ToList();
				foreach (ActorEntity ent in currentActors) {
					if (ent.Actor.ObjectIndex == targetId)
						targetEntity = ent;
				}
			}

			// button
			if (Buttons.IconButtonTooltip(FontAwesomeIcon.Users, "Select target actor"))
				this._gui.CreatePopup<ActorGazeTargetPopup>(this._ctx, actor).Open();

			// label
			ImGui.AlignTextToFramePadding();
			var label = "No Actor Target";
			if (hasTarget)
				label = $"Targeting: {(targetEntity != null ? targetEntity.Name : "Unknown")}";
			ImGui.SameLine(0, spacing);
			ImGui.Text(label);

			return result;
		}
	}
}
