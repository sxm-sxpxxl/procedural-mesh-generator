using System;
using UnityEngine;
using UnityEngine.Assertions;

[Serializable]
public abstract class VertexModifier
{
    protected Plane _targetPlane;
    protected Vector3 _offset;

    public bool IsInit { get; private set; } = false;
    
    public VertexModifier Init(Plane plane, Vector2 offset)
    {
        _targetPlane = plane;
        _offset = offset.AsFor(plane);
        IsInit = true;

        return this;
    }

    public virtual void OnDrawGizmosSelected(Transform relativeTransform)
    {
        Assert.IsTrue(IsInit);
    }

    public virtual Vector3[] Modify(Vector3[] vertices)
    {
        Assert.IsTrue(IsInit);
        return null;
    }
}
