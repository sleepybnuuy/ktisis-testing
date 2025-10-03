using System;

using CSObject = FFXIVClientStructs.FFXIV.Client.Graphics.Scene.Object;

using Ktisis.Common.Utility;
using Ktisis.Scene.Decor;
using Ktisis.Editor.Camera.Types;
using Ktisis.Scene.Types;

namespace Ktisis.Scene.Entities.World;
public class CameraEntity : WorldEntity, IDeletable {
    public readonly KtisisCamera Camera;
	public unsafe override CSObject* GetObject() {
        var gcPtr = Camera.GameCamera;
        Ktisis.Log.Info($"getting data for camera {this.Name}");
        Ktisis.Log.Info($"pos: {gcPtr->CameraBase.SceneCamera.Object.Position}");
        return &gcPtr->CameraBase.SceneCamera.Object;
    }

	public CameraEntity(
		ISceneManager scene
	) : base(scene) {
		this.Type = EntityType.Camera;
        this.Camera = scene.Context.Cameras.Create();
        this.Name = Camera.Name;
	}

	public unsafe override void SetTransform(Transform trans) {
		base.SetTransform(trans);
        Camera.FixedPosition = base.GetTransform().Position;
        Camera.WritePosition();
	}

	public bool Delete() {
        this.Camera.Dispose();
		return this.Address == nint.Zero;
	}
}
