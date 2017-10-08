// Copyright (c) 2014-2015 StagPoint Consulting

using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.AI;

/*
* This component will render all NavMesh border edges in yellow in the SceneView.
* It does this in order to demonstrate a fast and efficient way to determine all
* outside border edges of a navmesh. This information may be useful for custom
* navigation or terrain analysis, etc.
*/

public class LineSegment
{
    public LineSegment(Vector3 _start, Vector3 _end)
    {
        start = _start;
        end = _end;
    }
    public Vector3 start;
    public Vector3 end;
}

[System.Serializable]
public class CoverSpot
{
    public CoverSpot(Vector3 _pos, float _cost = 0.0f)
    {
        pos = _pos;
        cost = _cost;
    }
    public Vector3 pos;
    public float cost = 0;
}

[ExecuteInEditMode]
public class FindCoverSpots : MonoBehaviour
{
    protected FindCoverSpots() { }

    private List<LineSegment> borderEdges = null;

    public List<CoverSpot> coverSpots = null; 

    protected void OnEnable()
    {
        this.borderEdges = FindNavMeshBorders(NavMesh.CalculateTriangulation());

        this.coverSpots = TransformToCovers(CleanCoverSpots(CreateCoverSpots(borderEdges)));
    }

    void Start()
    {
        coverSpots = new List<CoverSpot>();
        this.coverSpots = TransformToCovers(CleanCoverSpots(CreateCoverSpots(borderEdges)));
    }

    public void OnDrawGizmosSelected()
    {

        if (!this.enabled)
            return;

        Gizmos.color = Color.yellow;

        for (int i = 0; i < this.borderEdges.Count; i++)
        {
            var edge = this.borderEdges[i];
            Gizmos.DrawLine(edge.start, edge.end);
        }

        for (int i = 0; i < this.coverSpots.Count; i++)
        {
            var coverPoint = this.coverSpots[i];
            Gizmos.DrawWireSphere(coverPoint.pos, 0.4f);
        }

    }

    public void ClearData()
    {
        this.borderEdges = new List<LineSegment>();

        this.coverSpots = new List<CoverSpot>();
    }

    public void FindBorderEdges()
    {
        this.borderEdges = FindNavMeshBorders(NavMesh.CalculateTriangulation());
    }

    public void CreateCoverSpotsEditor()
    {
        this.coverSpots = this.coverSpots = TransformToCovers(CreateCoverSpots(borderEdges));
    }

    public void CleanCoverSpotsEditor()
    {
        this.coverSpots = TransformToCovers(CleanCoverSpots(CreateCoverSpots(borderEdges)));
    }

    private static List<LineSegment> FindNavMeshBorders(NavMeshTriangulation mesh)
    {

        Vector3[] verts = null;
        int[] triangles = null;

        weldVertices(mesh, 0.01f, 2f, out verts, out triangles);

        var map = new Dictionary<uint, int>();

        Action<ushort, ushort> processEdge = (a, b) =>
        {

            if (a > b)
            {
                var temp = b;
                b = a;
                a = temp;
            }

            uint key = ((uint)a << 16) | (uint)b;

            if (!map.ContainsKey(key))
                map[key] = 1;
            else
                map[key] += 1;

        };

        for (int i = 0; i < triangles.Length; i += 3)
        {

            var a = (ushort)triangles[i + 0];
            var b = (ushort)triangles[i + 1];
            var c = (ushort)triangles[i + 2];

            processEdge(a, b);
            processEdge(b, c);
            processEdge(c, a);

        }

        var borderEdges = new List<LineSegment>();

        foreach (var key in map.Keys)
        {

            var count = map[key];
            if (count != 1)
                continue;

            var a = (key >> 16);
            var b = (key & 0xFFFF);
            var line = new LineSegment(verts[a], verts[b]);

            borderEdges.Add(line);

        }

        return borderEdges;

    }

    private static void weldVertices(NavMeshTriangulation mesh, float threshold, float bucketStep, out Vector3[] vertices, out int[] indices)
    {

        // This code was adapted from http://answers.unity3d.com/questions/228841/dynamically-combine-verticies-that-share-the-same.html

        Vector3[] oldVertices = mesh.vertices;
        Vector3[] newVertices = new Vector3[oldVertices.Length];
        int[] old2new = new int[oldVertices.Length];
        int newSize = 0;

        // Find AABB
        Vector3 min = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
        Vector3 max = new Vector3(float.MinValue, float.MinValue, float.MinValue);
        for (int i = 0; i < oldVertices.Length; i++)
        {
            if (oldVertices[i].x < min.x)
                min.x = oldVertices[i].x;
            if (oldVertices[i].y < min.y)
                min.y = oldVertices[i].y;
            if (oldVertices[i].z < min.z)
                min.z = oldVertices[i].z;
            if (oldVertices[i].x > max.x)
                max.x = oldVertices[i].x;
            if (oldVertices[i].y > max.y)
                max.y = oldVertices[i].y;
            if (oldVertices[i].z > max.z)
                max.z = oldVertices[i].z;
        }

        // Make cubic buckets, each with dimensions "bucketStep"
        int bucketSizeX = Mathf.FloorToInt((max.x - min.x) / bucketStep) + 1;
        int bucketSizeY = Mathf.FloorToInt((max.y - min.y) / bucketStep) + 1;
        int bucketSizeZ = Mathf.FloorToInt((max.z - min.z) / bucketStep) + 1;
        List<int>[,,] buckets = new List<int>[bucketSizeX, bucketSizeY, bucketSizeZ];

        // Make new vertices
        for (int i = 0; i < oldVertices.Length; i++)
        {

            // Determine which bucket it belongs to
            int x = Mathf.FloorToInt((oldVertices[i].x - min.x) / bucketStep);
            int y = Mathf.FloorToInt((oldVertices[i].y - min.y) / bucketStep);
            int z = Mathf.FloorToInt((oldVertices[i].z - min.z) / bucketStep);

            // Check to see if it's already been added
            if (buckets[x, y, z] == null)
                buckets[x, y, z] = new List<int>(); // Make buckets lazily

            for (int j = 0; j < buckets[x, y, z].Count; j++)
            {
                Vector3 to = newVertices[buckets[x, y, z][j]] - oldVertices[i];
                if (Vector3.SqrMagnitude(to) < threshold)
                {
                    old2new[i] = buckets[x, y, z][j];
                    goto skip; // Skip to next old vertex if this one is already there
                }
            }

            // Add new vertex
            newVertices[newSize] = oldVertices[i];
            buckets[x, y, z].Add(newSize);
            old2new[i] = newSize;
            newSize++;

        skip:
            ;

        }

        // Make new triangles
        int[] oldTris = mesh.indices;
        indices = new int[oldTris.Length];
        for (int i = 0; i < oldTris.Length; i++)
        {
            indices[i] = old2new[oldTris[i]];
        }

        vertices = new Vector3[newSize];
        for (int i = 0; i < newSize; i++)
        {
            vertices[i] = newVertices[i];
        }

    }

    private static List<Vector3> CreateCoverSpots(List<LineSegment> borderEdges)
    {
        List<Vector3> covers = new List<Vector3>();

        float minDistBetweenCovers = 1.0f;

        foreach (LineSegment segment in borderEdges)
        {
            covers = addPointsAlongLine(covers, segment, minDistBetweenCovers);
        }

        return covers;
    }
    private static List<Vector3> CleanCoverSpots(List<Vector3> covers)
    {
        
        float minSpaceBetweenCovers = 0.4f;
        for (int i = covers.Count - 1; i >= 0; i--)
        {
            for (int j = covers.Count - 1; j >= 0; j--)
            {
                if (i != j)
                {
                    if (Vector3.Distance(covers[i], covers[j]) < minSpaceBetweenCovers)
                    {
                        covers.RemoveAt(i);
                        //i--;
                        j = 0;
                    }
                }
            }
        }
        for (int j = covers.Count - 1; j >= 0; j--)
        {
            var found = false;
            for (int i = 0; i < 20; i++)
            {
                //cast a ray and see if there is an object close enough
                RaycastHit hitInfo;
                if (Physics.Raycast(new Vector3(covers[j].x, covers[j].y + 0.5f, covers[j].z), new Vector3(Mathf.Sin(i * (360 / 20)), 0, Mathf.Cos(i * (360 / 20))), out hitInfo, 1.0f))
                {
                    found = true;
                    break;
                }

            }
            //if there isn't one remove this point
            if (!found)
            {
                covers.RemoveAt(j);
            }
        }

        return covers;
    }

    private static List<Vector3> addPointsAlongLine(List<Vector3> coversList, LineSegment segment, float minDistBetweenCovers)
    {
        List<Vector3> covers = coversList;
        Vector3 center = (segment.start + segment.end) / 2;
        covers.Add(center);
        if (Vector3.Distance(center, segment.start) > minDistBetweenCovers)
        {
            covers = addPointsAlongLine(covers, new LineSegment(center, segment.start), minDistBetweenCovers);
        }
        if (Vector3.Distance(center, segment.end) > minDistBetweenCovers)
        {
            covers = addPointsAlongLine(covers, new LineSegment(center, segment.end), minDistBetweenCovers);
        }

        return covers;
    }

    private static List<CoverSpot> TransformToCovers(List<Vector3> coverList)
    {
        List<CoverSpot> covers = new List<CoverSpot>();
        foreach(var cover in coverList)
        {
            covers.Add(new CoverSpot(cover));
        }
        return covers;
    }
}