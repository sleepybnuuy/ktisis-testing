using System;

using Ktisis.Scene.Entities.World;
using Ktisis.Scene.Factory.Types;
using Ktisis.Scene.Types;
using Ktisis.Editor.Camera.Types;

namespace Ktisis.Scene.Factory.Builders;

public interface ICameraBuilder : IEntityBuilder<CameraEntity, ICameraBuilder> { }

public sealed class CameraBuilder : EntityBuilder<CameraEntity, ICameraBuilder>, ICameraBuilder {

	public CameraBuilder(
		ISceneManager scene
	) : base(scene) { }
	
	protected override CameraBuilder Builder => this;

	protected override CameraEntity Build() {
		return new CameraEntity(this.Scene) {};
	}
}
