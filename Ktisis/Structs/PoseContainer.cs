﻿using System;
using System.Collections.Generic;

using Dalamud.Logging;

using FFXIVClientStructs.FFXIV.Client.Graphics.Render;

using Ktisis.Interop.Hooks;

namespace Ktisis.Structs {
	[Serializable]
	public class PoseContainer : Dictionary<string, Transform> {
		// TODO: Make a helper function somewhere for skeleton iteration?

		public unsafe void Store(Skeleton* modelSkeleton) {
			Clear();

			var partialCt = modelSkeleton->PartialSkeletonCount;
			var partials = modelSkeleton->PartialSkeletons;
			for (var p = 0; p < partialCt; p++) {
				var partial = partials[p];

				var pose = partial.GetHavokPose(0);
				if (pose == null) continue;

				var skeleton = pose->Skeleton;
				for (var i = 0; i < skeleton->Bones.Length; i++) {
					if (i == partial.ConnectedBoneIndex)
						continue; // Unsupported by .pose files :(

					var bone = modelSkeleton->GetBone(p, i);
					var name = bone.HkaBone.Name.String;

					var model = bone.AccessModelSpace();
					this[name] = Transform.FromHavok(*model);
				}
			}
		}

		public unsafe void Apply(Skeleton* modelSkeleton, PoseLoadMode mode = PoseLoadMode.Rotation) {
			var partialCt = modelSkeleton->PartialSkeletonCount;
			ApplyToPartial(modelSkeleton, 0, mode);
		}

		public unsafe void ApplyToPartial(Skeleton* modelSkeleton, int p, PoseLoadMode mode = PoseLoadMode.Rotation) {
			var partial = modelSkeleton->PartialSkeletons[p];

			var pose = partial.GetHavokPose(0);
			if (pose == null) return;

			var skeleton = pose->Skeleton;
			for (var i = 0; i < skeleton->Bones.Length; i++) {
				var bone = modelSkeleton->GetBone(p, i);
				var name = bone.HkaBone.Name.String;

				if (TryGetValue(name, out var val)) {
					var model = bone.AccessModelSpace();

					var initial = *model;
					var initialPos = initial.Translation.ToVector3();
					var initialRot = initial.Rotation.ToQuat();

					var aaa_aaa = false;

					if (p == 0 && bone.ParentId < 1) {
						var pos = val.Position.ToHavok();
						model->Translation = pos;
						initialRot = val.Rotation; // idk why this hack works but it does
					} else if (i == partial.ConnectedBoneIndex) { // Is face, hair, etc
						var parent = modelSkeleton->GetBone(0, partial.ConnectedParentBoneIndex);
						var pModel = parent.AccessModelSpace();

						// This is extremely hacky due to the requirements of loading poses in LoadSkeletonHook.

						model->Translation = pModel->Translation;
						Overlay.Skeleton.PropagateChildren(bone, model, initialPos, initialRot);
						initialPos = model->Translation.ToVector3();
					}

					if (!aaa_aaa) {
						if (mode.HasFlag(PoseLoadMode.Rotation))
							model->Rotation = val.Rotation.ToHavok();
						if (mode.HasFlag(PoseLoadMode.Position))
							model->Translation = val.Position.ToHavok();
						if (mode.HasFlag(PoseLoadMode.Scale))
							model->Scale = val.Scale.ToHavok();

						Overlay.Skeleton.PropagateChildren(bone, model, initialPos, initialRot);
					}
				}
			}
		}
	}

	[Flags]
	public enum PoseLoadMode {
		None = 0,
		Rotation = 1,
		Position = 2,
		Scale = 4
	}
}