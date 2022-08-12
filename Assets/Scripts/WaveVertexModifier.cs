using System;
using Sirenix.OdinInspector;
using Unity.VisualScripting;
using UnityEngine;

[Serializable]
public sealed class WaveVertexModifier : VertexModifier
{
    [SerializeField] private Type type = Type.Sine;
    [SerializeField, Indent, LabelText("Around axis"), ShowIf(nameof(IsRippleSelected))] private AxisType rippleAroundAxis = AxisType.Local;
    [SerializeField, Indent, ShowIf(nameof(IsShowCustomAxis))] private Vector2 customAxis = Vector2.zero;
    
    [Title("Wave settings")]
    [SerializeField, EnumToggleButtons] private Axis axis = Axis.OY;
    [SerializeField, Indent, Min(0f)] private float amplitude = 0.1f;
    [SerializeField, Indent, Min(1), SuffixLabel("Hz", true)] private int frequency = 1;
    [SerializeField, Indent, LabelText("Phase"), Wrap(0, 360), SuffixLabel("°", true)] private float phaseInDeg = 0f;

    [Title("Motion settings")]
    [SerializeField, LabelText("Enabled")] private bool isMotionEnabled = false;
    [SerializeField, LabelText("Speed"), Range(0.1f, 5f), SuffixLabel("/s"), ShowIf(nameof(isMotionEnabled))] private float motionSpeed = 1f;

    private enum AxisType
    {
        Local,
        CustomLocal
    }
    
    private enum Axis
    {
        OX,
        OY,
        OZ
    }
    
    private enum Type
    {
        Sine,
        Ripple
    }

    private bool IsShowCustomAxis => IsRippleSelected && IsCustomAxisTypeSelected;

    private bool IsRippleSelected => type == Type.Ripple;

    private bool IsCustomAxisTypeSelected => rippleAroundAxis == AxisType.CustomLocal;

    private Vector3 Direction => axis switch
    {
        Axis.OX => Vector3.right,
        Axis.OY => Vector3.up,
        Axis.OZ => Vector3.forward,
        _ => Vector3.zero
    };

    public override void OnDrawGizmosSelected(Transform relativeTransform)
    {
        base.OnDrawGizmosSelected(relativeTransform);
        
        if (IsShowCustomAxis == false)
        {
            return;
        }
        
        Gizmos.color = Color.yellow;
        Gizmos.DrawRay(relativeTransform.TransformPoint(_offset + customAxis.AsFor(_targetPlane)), Direction);
    }

    public override Vector3[] Modify(Vector3[] vertices)
    {
        base.Modify(vertices);
        
        return type switch
        {
            Type.Sine => SimpleModify(vertices),
            Type.Ripple => RippleModify(vertices),
            _ => null
        };
    }

    private Vector3[] SimpleModify(Vector3[] vertices)
    {
        float time = isMotionEnabled ? Mathf.Repeat(Time.time, 1f / motionSpeed) : 0f;
        float currentPosition = 0f, phaseInRad = phaseInDeg * Mathf.Deg2Rad;
        
        for (int i = 0; i < vertices.Length; i++)
        {
            currentPosition = frequency * (vertices[i].x + vertices[i].y + vertices[i].z);
            vertices[i] += Direction * (amplitude * Mathf.Sin(2f * Mathf.PI * (currentPosition + motionSpeed * time) + phaseInRad));
        }
        
        return vertices;
    }

    private Vector3[] RippleModify(Vector3[] vertices)
    {
        float time = isMotionEnabled ? Mathf.Repeat(Time.time, 1f / motionSpeed) : 0f;
        float currentPosition = 0f, phaseInRad = phaseInDeg * Mathf.Deg2Rad;
        Vector3 targetAxis = rippleAroundAxis == AxisType.Local ? _offset : _offset + customAxis.AsFor(_targetPlane);

        for (int i = 0; i < vertices.Length; i++)
        {
            currentPosition = frequency * (vertices[i] - targetAxis).magnitude;
            vertices[i] += Direction * (amplitude * Mathf.Sin(2f * Mathf.PI * (currentPosition + motionSpeed * time) + phaseInRad));
        }

        return vertices;
    }
}
