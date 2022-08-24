using System.Linq;
using UnityEditor;
using UnityEngine;

public class Test : MonoBehaviour
{
    public Vector3 center = new Vector3(0f, 0f, 0f);
    public Vector3 size = new Vector3(1f, 1f, 1f);

    [Space]
    public bool oldSolution = false;
    [Range(0, 5)] public int layer = 0;
    [Range(1, 32)] public int resolution = 2;
    [Range(0f, 1f)] public float roundness = 0f;
    
    [Header("Debug options")]
    public bool showVertexLabel = false;
    [Range(0.01f, 0.1f)] public float vertexSize = 0.05f;

    private void OnDrawGizmos()
    {
        if (oldSolution)
        {
            BuildSelfRoundVertices();
        }
        else
        {
            BuildCatlikeRoundVertices();
        }
    }

    private void BuildCatlikeRoundVertices()
    {
        int edgeLength = resolution + 1;
        int verticesCount = (int) Mathf.Pow(edgeLength, 3);
        var vertices = BuildVertices(verticesCount);
        
        var roundedVertices = new Vector3[vertices.Length];
        for (int i = 0; i < roundedVertices.Length; i++)
        {
            var v = vertices[i] - (-0.5f * size + center);
            var r = size * (roundness / 2f);

            var inner = new Vector3
            (
                x: Mathf.Clamp(v.x, r.x, size.x - r.x),
                y: Mathf.Clamp(v.y, r.y, size.y - r.y),
                z: Mathf.Clamp(v.z, r.z, size.z - r.z)
            );

            inner += (-0.5f * size + center);

            Gizmos.color = Color.red;
            Gizmos.DrawSphere(inner, vertexSize);
            
            var n = (vertices[i] - inner).normalized;
            roundedVertices[i] = inner + Vector3.Scale(n, r);
            
            Gizmos.color = new Color(1f, 0f, 0f, 0.3f);
            Gizmos.DrawLine(inner, vertices[i]);
        }
        
        DrawVertices(roundedVertices, Color.yellow, false);
    }
    
    private void BuildSelfRoundVertices()
    {
        int edgeLength = resolution + 1;
        var vertices = BuildVertices(edgeLength * edgeLength);
        
        int[] levels = vertices.Select((_, i) => GetLevelByVertexIndex(i)).ToArray();
        var s = new Vector3(size.x, size.y);
        var r = s * (roundness / 2f);
        var roundedVertices = new Vector3[vertices.Length];
        
        for (int i = 0; i < roundedVertices.Length; i++)
        {
            Vector3 O = (Vector3) center;
            Vector3 V = vertices[i] - O;
            
            float Xs = Mathf.Sign(Vector3.Dot(V, Vector3.right));
            float Ys = Mathf.Sign(Vector3.Dot(V, Vector3.up));

            Vector3 Vc = (s.x / 2f - r.x) * Xs * Vector3.right + (s.y / 2f - r.y) * Ys * Vector3.up;
            
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(O + Vc, vertexSize);
            
            Vector3 dt = V - Vc;
            int maxLevelWithFromCorner = vertices
                .Select((v, index) => new { level = levels[index], position = v })
                .Where(v =>
                {
                    var dist = (v.position - O) - Vc;
                    float xs = Mathf.Sign(Vc.x);
                    float ys = Mathf.Sign(Vc.y);

                    return xs * Vector3.Dot(dist, Vector3.right) > 0f && ys * Vector3.Dot(dist, Vector3.up) > 0f;
                })
                .Select(v => v.level)
                .DefaultIfEmpty()
                .Max();
            
            if (Xs * dt.x > 0f && Ys * dt.y > 0f)
            {
                int level = levels[i];
                var rFactor = 1f - (float) level / maxLevelWithFromCorner;
                var rByLevel = rFactor * r;

                Vector3 n = (V - Vc).normalized;
                roundedVertices[i] = O + Vc + Vector3.Scale(rByLevel, n);
                
                Gizmos.color = new Color(1f, 0f, 0f, 0.3f);
                Gizmos.DrawLine(O + Vc, roundedVertices[i]);
            }
            else
            {
                roundedVertices[i] = O + V;
            }
        }

        DrawVertices(roundedVertices, Color.yellow, false);
    }
    
    private Vector3[] BuildVertices(int length)
    {
        int edgeLength = resolution + 1;
        
        var vertices = new Vector3[length];
        for (int i = 0, vIndex = 0; i < vertices.Length; i++)
        {
            vertices[vIndex++] =  new Vector3(
                x: size.x / resolution * Mathf.Repeat(i / edgeLength, edgeLength),
                y: size.y / resolution * (i % edgeLength),
                z: size.z / resolution * (i / (edgeLength * edgeLength))
            ) - size / 2f + center;
        }
        DrawVertices(vertices, new Color(0f, 0f, 0f, 0.3f), showVertexLabel);

        return vertices;
    }

    private int GetLevelByVertexIndex(int i)
    {
        int currentLevel = -1;
        while (IsVertexOnLevel(i, ++currentLevel) == false) {}
        return currentLevel;
    }

    private bool IsVertexOnLevel(int i, int level = 0)
    {
        int edgeLength = resolution + 1;

        int minIndexOnLayer = edgeLength * level + level;
        int maxIndexOnLayer = (edgeLength * edgeLength - 1) - minIndexOnLayer;
        bool isLeftExcludedIndexOnLayer = (i % edgeLength) >= level;
        bool isRightExcludedIndexOnLayer = (i % edgeLength) + level < edgeLength;

        bool isOnLayer = i >= minIndexOnLayer && i <= maxIndexOnLayer && isLeftExcludedIndexOnLayer && isRightExcludedIndexOnLayer;
        if (isOnLayer)
        {
            bool isVertical = i % edgeLength == level || i % edgeLength == resolution - level;
            bool isHorizontal = i / edgeLength == level || i / edgeLength == resolution - level;
            
            return isVertical || isHorizontal;
        }

        return false;
    }
    
    private void DrawVertices(Vector3[] vertices, Color color, bool showLabels)
    {
        Gizmos.color = color;
        for (int i = 0; i < vertices.Length; i++)
        {
            Gizmos.DrawSphere(vertices[i], vertexSize);
            if (showLabels)
            {
                Handles.Label(vertices[i], $"V[{i}]");
            }
        }
    }
}
