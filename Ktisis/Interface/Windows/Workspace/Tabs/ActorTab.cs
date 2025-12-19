using System.IO;

using Dalamud.Interface;
using Dalamud.Interface.Components;
using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Interface.Utility.Raii;
using Dalamud.Bindings.ImGui;

using GLib.Widgets;

using Ktisis.Data.Files;
using Ktisis.Data.Serialization;
using Ktisis.Interop.Hooks;
using Ktisis.Structs.Actor;
using Ktisis.Interface.Components;
using Ktisis.Interface.Windows.ActorEdit;
using Ktisis.Interop;
using Ktisis.Structs.Actor.State;

namespace Ktisis.Interface.Windows.Workspace.Tabs {
	public static class ActorTab {
		private static readonly NpcImport _npcImport = new();
		
		public unsafe static void Draw(IGameObject target) {
			var actor = (Actor*)target.Address;
			if (actor == null) return;

			// Actor details

			ImGui.Spacing();

			// Customize button
			ImGui.BeginDisabled(); // TODO: unfuck actor/equipment panels
			if (ImGuiComponents.IconButton(FontAwesomeIcon.UserEdit)) {
				if (EditActor.Visible)
					EditActor.Hide();
				else
					EditActor.Show();
			}
			ImGui.EndDisabled();
			ImGui.SameLine();
			ImGui.Text("Edit actor's appearance");
			ImGui.SameLine();
			Buttons.IconButtonTooltip(
				FontAwesomeIcon.ExclamationCircle,
				"Patch 7.4 has disrupted a number of Ktisis v0.2's functions, including Appearance/Equipment edits.\n" +
				"You may be able to successfully edit your actor's appearance using alternative tools, like Glamourer or Anamnesis.\n" +
				"Please try our testing version (v0.3) until this feature can be redeployed to the main branch!"
			);

			ImGui.Spacing();

			// Actor list
			ActorsList.Draw();

			// Animation control
			AnimationControls.Draw(target);

			// Gaze control
			if (ImGui.CollapsingHeader("Gaze Control")) {
				if (PoseHooks.PosingEnabled)
					ImGui.TextWrapped("Gaze controls are unavailable while posing.");
				else
					EditGaze.Draw(actor);
			}

			// Import & Export
			if (ImGui.CollapsingHeader("Import & Export"))
				ImportExportChara(actor);

			ImGui.EndTabItem();
		}
		
		public unsafe static void ImportExportChara(Actor* actor) {
			var mode = Ktisis.Configuration.CharaMode;

			// Equipment

			ImGui.BeginGroup();
			ImGui.Text("Equipment");

			var gear = mode.HasFlag(AnamCharaFile.SaveModes.EquipmentGear);
			if (ImGui.Checkbox("Gear##ImportExportChara", ref gear))
				mode ^= AnamCharaFile.SaveModes.EquipmentGear;

			var accs = mode.HasFlag(AnamCharaFile.SaveModes.EquipmentAccessories);
			if (ImGui.Checkbox("Accessories##ImportExportChara", ref accs))
				mode ^= AnamCharaFile.SaveModes.EquipmentAccessories;

			var weps = mode.HasFlag(AnamCharaFile.SaveModes.EquipmentWeapons);
			if (ImGui.Checkbox("Weapons##ImportExportChara", ref weps))
				mode ^= AnamCharaFile.SaveModes.EquipmentWeapons;

			ImGui.EndGroup();

			// Appearance

			ImGui.SameLine();
			ImGui.BeginGroup();
			ImGui.Text("Appearance");

			var body = mode.HasFlag(AnamCharaFile.SaveModes.AppearanceBody);
			if (ImGui.Checkbox("Body##ImportExportChara", ref body))
				mode ^= AnamCharaFile.SaveModes.AppearanceBody;

			var face = mode.HasFlag(AnamCharaFile.SaveModes.AppearanceFace);
			if (ImGui.Checkbox("Face##ImportExportChara", ref face))
				mode ^= AnamCharaFile.SaveModes.AppearanceFace;

			var hair = mode.HasFlag(AnamCharaFile.SaveModes.AppearanceHair);
			if (ImGui.Checkbox("Hair##ImportExportChara", ref hair))
				mode ^= AnamCharaFile.SaveModes.AppearanceHair;

			ImGui.EndGroup();

			// Import & Export buttons

			Ktisis.Configuration.CharaMode = mode;

			ImGui.Spacing();
			ImGui.Separator();
			ImGui.Spacing();

			var isUseless = mode == AnamCharaFile.SaveModes.None;
			if (isUseless) ImGui.BeginDisabled();

			ImGui.BeginDisabled();
			if (ImGui.Button("Import##ImportExportChara")) {
				KtisisGui.FileDialogManager.OpenFileDialog(
					"Importing Character",
					"Anamnesis Chara (.chara){.chara}",
					(success, path) => {
						if (!success) return;

						var content = File.ReadAllText(path[0]);
						var chara = JsonParser.Deserialize<AnamCharaFile>(content);
						if (chara == null) return;

						chara.Apply(actor, mode);
					},
					1,
					null
				);
			}
			ImGui.EndDisabled();

			ImGui.SameLine();

			if (ImGui.Button("Export##ImportExportChara")) {
				KtisisGui.FileDialogManager.SaveFileDialog(
					"Exporting Character",
					"Anamnesis Chara (.chara){.chara}",
					"Untitled.chara",
					".chara",
					(success, path) => {
						if (!success) return;

						var chara = new AnamCharaFile();
						chara.WriteToFile(*actor, mode);

						var json = JsonParser.Serialize(chara);
						using (var file = new StreamWriter(path))
							file.Write(json);
					}
				);
			}
			
			ImGui.Spacing();
			ImGui.BeginDisabled();
			if (ImGui.Button("Import NPC"))
				_npcImport.Open();

			if (isUseless) ImGui.EndDisabled();

			ImGui.Spacing();
			if (ImGui.Button("Revert Changes"))
				ActorStateWatcher.RevertToOriginal(actor);
			ImGui.EndDisabled();

			Buttons.IconButtonTooltip(
				FontAwesomeIcon.ExclamationCircle,
				"Patch 7.4 has disrupted a number of Ktisis v0.2's functions, including Chara/NPC importing.\n" +
				"You may be able to successfully edit your actor's appearance using alternative tools, like Glamourer or Anamnesis.\n" +
				"Please try our testing version (v0.3) until this feature can be redeployed to the main branch!"
			);

			_npcImport.Draw(mode);
		}
	}
}
