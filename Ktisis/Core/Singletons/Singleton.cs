﻿namespace Ktisis.Core.Singletons;

public abstract class Singleton {
	public virtual void Init() { }
	public virtual void Dispose() { }
	public virtual void OnReady() { }
}