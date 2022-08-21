using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class Test : MonoBehaviour
{
    [Range(0, 3)] public int layer = 1;
    
    [Range(0f, 1f)] public float roundness = 0f;
    public Vector2 center = new Vector2(0f, 0f);
    public Vector2 size = new Vector2(1f, 1f);
    [Range(1, 32)] public int resolution = 2;
    [Range(0.01f, 0.1f)] public float vertexSize = 0.05f;

    private void OnDrawGizmos()
    {
        int edgeLength = resolution + 1;
        
        var vertices = new Vector3[edgeLength * edgeLength];
        for (int i = 0, vIndex = 0; i < vertices.Length; i++)
        {
            vertices[vIndex++] =  size / resolution * new Vector3(x: i % edgeLength, y: i / edgeLength) - size / 2f + center;
        }
        DrawVertices(vertices, Color.black);

        var r = size * (roundness / 2f);
        var roundedVertices = new Vector3[vertices.Length];
        for (int i = 0; i < roundedVertices.Length; i++)
        {
            Vector3 O = (Vector3) center;
            Vector3 V = vertices[i] - O;
            
            float Xs = Mathf.Sign(Vector3.Dot(V, Vector3.right));
            float Ys = Mathf.Sign(Vector3.Dot(V, Vector3.up));
            
            Vector3 Vc = (size.x / 2f - r.x) * Xs * Vector3.right + (size.y / 2f - r.y) * Ys * Vector3.up;
            
            // Gizmos.color = Color.green;
            // Gizmos.DrawWireSphere(Vc, 0.5f * roundness);

            Gizmos.color = Color.red;
            Gizmos.DrawSphere(O + Vc, vertexSize);

            Vector3 dt = V - Vc;
            if (Xs * dt.x > 0 && Ys * dt.y > 0)
            {
                // Vector3 n = (V - Vc).normalized;
                // roundedVertices[i] = O + Vc + Vector3.Scale(r, n);
                Vector3 n = (Vc - V).normalized;
                float d = (Vc - V).magnitude;

                roundedVertices[i] = O + V + Vector3.Scale(d * Vector2.one - r, n);
                
                // if (isHorizontal || isVertical)
                // {
                //     Gizmos.color = Color.green;
                //     Gizmos.DrawSphere(V, vertexSize);
                // }
                // else
                // {
                //     Gizmos.color = Color.yellow;
                //     Gizmos.DrawSphere(V, vertexSize);
                //     
                //     Gizmos.color = Color.green;
                //     Gizmos.DrawWireSphere(Vc, 0.5f * 0.5f * roundness);
                // }
            }
            else
            {
                roundedVertices[i] = O + V;
            }
            
            if (IsInnerVertex(i, layer))
            {
                Gizmos.color = Color.green;
                Gizmos.DrawSphere(V, vertexSize);
            }
        }
        // DrawVertices(roundedVertices, Color.yellow);
    }

    private bool IsInnerVertex(int i, int layer = 1)
    {
        int edgeLength = resolution + 1;

        int minIndexOnLayer = edgeLength * layer + layer;
        int maxIndexOnLayer = (edgeLength * edgeLength - 1) - minIndexOnLayer;
        bool isRightSide = (i % edgeLength) + layer < edgeLength;
        bool isLeftSide = (i % edgeLength) >= layer;

        bool isOnLayer = i >= minIndexOnLayer && i <= maxIndexOnLayer && isLeftSide && isRightSide;
        if (isOnLayer)
        {
            bool isVertical = (i % edgeLength == layer || i % edgeLength == resolution - layer);
            bool isHorizontal = (i / edgeLength == layer || i / edgeLength == resolution - layer);

            return isVertical || isHorizontal;
        }

        return false;
    }
    
    private void DrawVertices(Vector3[] vertices, Color color)
    {
        int edgeLength = resolution + 1;
        
        Gizmos.color = color;
        for (int i = 0; i < vertices.Length; i++)
        {
            bool isVertical = i % edgeLength == 0 || i % edgeLength == resolution;
            bool isHorizontal = i / edgeLength == 0 || i / edgeLength == resolution;
            
            if (isVertical || isHorizontal)
            {
                // Gizmos.DrawSphere(vertices[i], vertexSize);
            }
            
            Gizmos.DrawSphere(vertices[i], vertexSize);
            Handles.Label(vertices[i], $"V[{i}]");
        }
    }

    // private void OnDrawGizmos()
    // {
    //     int edgeLength = resolution + 1;
    //     var outerVertices = new Vector3[edgeLength * edgeLength];
    //     
    //     for (int i = 0, vIndex = 0; i < outerVertices.Length; i++)
    //     {
    //         outerVertices[vIndex++] =  size / resolution * new Vector3(x: i % edgeLength, y: i / edgeLength);
    //     }
    //
    //     Gizmos.color = Color.black;
    //     for (int i = 0; i < outerVertices.Length; i++)
    //     {
    //         bool isVertical = i % edgeLength == 0 || i % edgeLength == resolution;
    //         bool isHorizontal = i / edgeLength == 0 || i / edgeLength == resolution;
    //         
    //         if (isVertical || isHorizontal)
    //         {
    //             Gizmos.DrawSphere(outerVertices[i], vertexSize);
    //         }
    //     }
    //
    //     var innerVertices = new Vector3[outerVertices.Length];
    //
    //     for (int i = 0; i < innerVertices.Length; i++)
    //     {
    //         innerVertices[i].x = outerVertices[i].x * (1f - roundness / size.x) + roundness / 2;
    //         innerVertices[i].y = outerVertices[i].y * (1f - roundness / size.y) + roundness / 2;
    //     }
    //     
    //     Gizmos.color = Color.white;
    //     for (int i = 0; i < innerVertices.Length; i++)
    //     {
    //         bool isVertical = i % edgeLength == 0 || i % edgeLength == resolution;
    //         bool isHorizontal = i / edgeLength == 0 || i / edgeLength == resolution;
    //         
    //         if (isVertical || isHorizontal)
    //         {
    //             Gizmos.DrawSphere(innerVertices[i], vertexSize);
    //         }
    //     }
    //
    //     Gizmos.color = Color.red;
    //     Gizmos.DrawWireSphere(innerVertices[0], roundness/2f);
    //
    //     var roundedVertices = new Vector3[outerVertices.Length];
    //
    //     for (int i = 0; i < roundedVertices.Length; i++)
    //     {
    //         var delta = outerVertices[i] - innerVertices[i];
    //         var delta2 = outerVertices[i] - innerVertices[0]; 
    //         var normal = delta.normalized;
    //
    //         bool isVertical = i % edgeLength == 0 || i % edgeLength == resolution;
    //         bool isHorizontal = i / edgeLength == 0 || i / edgeLength == resolution;
    //         
    //         if (isVertical || isHorizontal)
    //         {
    //             Gizmos.color = Color.green;
    //             Gizmos.DrawRay(innerVertices[i], normal);
    //         }
    //
    //         var r = (roundness / 2);
    //         
    //         if (delta2.sqrMagnitude > r * r && delta2.x < 0 && delta2.y < 0)
    //         {
    //             roundedVertices[i] = innerVertices[0] + (r) * delta2.normalized;
    //         }
    //         else
    //         {
    //             roundedVertices[i] = outerVertices[i];
    //         }
    //     }
    //     
    //     Gizmos.color = Color.yellow;
    //     for (int i = 0; i < roundedVertices.Length; i++)
    //     {
    //         bool isVertical = i % edgeLength == 0 || i % edgeLength == resolution;
    //         bool isHorizontal = i / edgeLength == 0 || i / edgeLength == resolution;
    //         
    //         if (isVertical || isHorizontal)
    //         {
    //             Gizmos.DrawSphere(roundedVertices[i], vertexSize);
    //         }
    //     }
    // }
}
