using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
public class VXMReader : MonoBehaviour
{
    bool SaveTextures = false;
    static byte NULL_VOXEL;

    int index = 0;

    void Start()
    {
        data = new VXMData();
        data.voxel = new VoxelVolume(1, 1, 1);
    }

    protected VXMData data;
    void ReadVXMC(byte[] b)
    {
        data.volumeW = GetInt(b);
        data.volumeH = GetInt(b);
        data.volumeD = GetInt(b);

        data.voxel = new VoxelVolume(data.volumeW, data.volumeH, data.volumeD);
        data.voxel.voxels = new Voxel[data.volumeW * data.volumeH * data.volumeD];
        data.voxel.voxelAmount = data.voxel.voxels.Length;

        data.pivot.x = GetFloat(b);
        data.pivot.y = GetFloat(b);
        data.pivot.z = GetFloat(b);

        //skip gamescreendata
        if (GetBool(b))
        {
            GetFloat(b);
            GetFloat(b);
            GetFloat(b);
            GetFloat(b);
            GetFloat(b);
            GetFloat(b);
            GetInt(b);

            int j = GetInt(b);
            int j0 = GetInt(b);
            Skip(j * j0);
        }

        float f = GetFloat(b);
        float f0 = GetFloat(b);
        float f1 = GetFloat(b);
        float f2 = GetFloat(b);
        data.voxelScale = f;
        data.lodPivot = new Vector3(f0, f1, f2);
        data.voxelSize = data.voxel.voxW / f;

        int LODGroup = GetInt(b);
        int i0 = 0;

        for (; i0 < LODGroup; i0 = i0 + 1)
        {
            GetInt(b);
            GetInt(b);
            Skip(GetInt(b));

            int i1 = 0;

            for (; i1 < 6; i1 = i1 + 1)
            {
                int i2 = GetInt(b);
                int i3 = 0;
                for (; i3 < i2; i3 = i3 + 1)
                {
                    Vertex v1 = new Vertex(new Vector3(GetFloat(b), GetFloat(b), GetFloat(b)), new Vector2(GetInt(b) * 0.01f, GetInt(b) * 0.01f), 0);
                    Vertex v2 = new Vertex(new Vector3(GetFloat(b), GetFloat(b), GetFloat(b)), new Vector2(GetInt(b) * 0.01f, GetInt(b) * 0.01f), 1);
                    Vertex v3 = new Vertex(new Vector3(GetFloat(b), GetFloat(b), GetFloat(b)), new Vector2(GetInt(b) * 0.01f, GetInt(b) * 0.01f), 2);
                    Vertex v4 = new Vertex(new Vector3(GetFloat(b), GetFloat(b), GetFloat(b)), new Vector2(GetInt(b) * 0.01f, GetInt(b) * 0.01f), 3);
                    if (i0 == 0)
                    {
                        data.quads.Add(new Quad(v1, v2, v3, v4, data.voxelScale));
                    }
                }
            }
        }

        Skip(1024);
        Skip(1024);

        int i4 = GetByteAsInt(b);
        int i5 = 0;

        for (; i5 < i4; i5 = i5 + 1)
        {
            String s = GetString(b);
            GetByteAsInt(b);
            int i6 = GetByteAsInt(b);
        }

        int colorAmount = GetByteAsInt(b);
        int i8 = 0;
        for (; i8 < colorAmount; i8 = i8 + 1)
        {
            int color = GetInt(b);
            bool e = GetBool(b);

            data.materials.Add(new VEMaterial(color, e));
        }

        if (data.materials.Count == 0)
        {
            data.materials.Add(new VEMaterial(16777215, false));
        }

        int i10 = GetByteAsInt(b);

        int i11 = 0;
        while (i11 < i10)
        {
            GetString(b);
            GetBool(b);

            int i12 = 0;

            while (true)
            {
                int i13 = GetByteAsInt(b);
                if (i13 != 0)
                {
                    int voxel = GetByteAsInt(b);
                    if (voxel != (int)NULL_VOXEL)
                    {
                        int voxelColorId = ParseByte(voxel);
                        int i16 = i12;
                        for (; i16 < i12 + i13; i16 = i16 + 1)
                        {
                            int x = i16 / (data.volumeH * data.volumeD);
                            int y = i16 / data.volumeD % data.volumeH;
                            int z = i16 % data.volumeD;
                            int index = x + y * data.volumeW + z * data.volumeW * data.volumeH;
                            data.voxel.voxels[index] = new Voxel((byte)voxelColorId, new Vector3(x, y, z));
                        }
                        i12 = i12 + i13;
                    }
                    else
                    {
                        i12 = i12 + i13;
                    }
                }
                else
                {
                    i11 = i11 + 1;
                    break;
                }
            }
        }
    }
    void ReadVXMA(byte[] b)
    {
        data.volumeW = GetInt(b);
        data.volumeH = GetInt(b);
        data.volumeD = GetInt(b);

        data.voxel = new VoxelVolume(data.volumeW, data.volumeH, data.volumeD);
        data.voxel.voxels = new Voxel[data.volumeW * data.volumeH * data.volumeD];
        data.voxel.voxelAmount = data.voxel.voxels.Length;

        data.pivot.x = GetFloat(b);
        data.pivot.y = GetFloat(b);
        data.pivot.z = GetFloat(b);

        //skip gamescreendata
        if (GetBool(b))
        {
            GetFloat(b);
            GetFloat(b);
            GetFloat(b);
            GetFloat(b);
            GetFloat(b);
            GetFloat(b);
            GetInt(b);

            int j = GetInt(b);
            int j0 = GetInt(b);
            Skip(j * j0);
        }

        float f = GetFloat(b);
        float f0 = GetFloat(b);
        float f1 = GetFloat(b);
        float f2 = GetFloat(b);

        data.voxelScale = f;
        data.voxelSize = data.voxel.voxW / f;
        data.lodPivot = new Vector3(f0, f1, f2);

        int LODGroup = GetInt(b);
        int i0 = 0;
        for (; i0 < LODGroup; i0 = i0 + 1)
        {

            GetInt(b);
            GetInt(b);

            int i1 = GetInt(b);
            int i2 = 0;

            for (; i2 < i1; i2 = i2 + 1)
            {
                GetString(b);
                Skip(GetInt(b));
            }

            int i3 = 0;
            for (; i3 < 6; i3 = i3 + 1)
            {
                int i4 = GetInt(b);
                int i5 = 0;
                for (; i5 < i4; i5 = i5 + 1)
                {
                    Vertex v1 = new Vertex(new Vector3(GetFloat(b), GetFloat(b), GetFloat(b)), new Vector2(GetInt(b) * 0.01f, GetInt(b) * 0.01f), 0);
                    Vertex v2 = new Vertex(new Vector3(GetFloat(b), GetFloat(b), GetFloat(b)), new Vector2(GetInt(b) * 0.01f, GetInt(b) * 0.01f), 1);
                    Vertex v3 = new Vertex(new Vector3(GetFloat(b), GetFloat(b), GetFloat(b)), new Vector2(GetInt(b) * 0.01f, GetInt(b) * 0.01f), 2);
                    Vertex v4 = new Vertex(new Vector3(GetFloat(b), GetFloat(b), GetFloat(b)), new Vector2(GetInt(b) * 0.01f, GetInt(b) * 0.01f), 3);
                    if (i0 == 0)
                    {
                        data.quads.Add(new Quad(v1, v2, v3, v4, data.voxelScale));
                    }
                }
            }
        }

        int colorAmount = GetByteAsInt(b);
        int i7 = 0;

        for (; i7 < colorAmount; i7 = i7 + 1)
        {

            int color = GetInt(b);
            bool e = GetBool(b);
            VEColor c = new VEColor(color);
            data.materials.Add(new VEMaterial(color, e));
        }

        if (data.materials.Count == 0)
        {
            data.materials.Add(new VEMaterial(16777215, false));
        }

        int i9 = 0;
        bool run = true;
        while (run)
        {
            int i10 = GetByteAsInt(b);
            if (i10 == 0)
            {
                importScreenDataVXMA(data, b);
                run = false;
                continue;
            }

            int voxel = GetByteAsInt(b);
            if (voxel != -1)
            {
                int voxelColorId = ParseByte(voxel);
                int i13 = i9;

                for (; i13 < i9 + i10; i13 = i13 + 1)
                {
                    int x = i13 / (data.volumeH * data.volumeD);
                    int y = i13 / data.volumeD % data.volumeH;
                    int z = i13 % data.volumeD;
                    int index = x + y * data.volumeW + z * data.volumeW * data.volumeH;
                    data.voxel.voxels[index] = new Voxel((byte)voxelColorId, new Vector3(x, y, z));
                }
                i9 = i9 + i10;
            }
            else
            {
                i9 = i9 + i10;
            }
        }
    }
    public bool ReadVXMFile(string name, MeshRenderer mr, int hand = 0, bool hip = false)
{
    Debug.Log($"[VXMReader] Starting ReadVXMFile for: {name}, hand={hand}, hip={hip}");
    
    index = 4;
    data = new VXMData();
    data.quads = new List<Quad>();
    data.materials = new List<VEMaterial>();
    
    if (hand == 4 || hand == 3)
    {
        Debug.Log("no skin hand " + name);
        name += "_nofinger";
    }
    
    Debug.Log($"[VXMReader] Loading file: Models/{name}");
    var vxmFile = Resources.Load<TextAsset>("Models/" + name);
    
    if (vxmFile == null)
    {
        Debug.LogError($"[VXMReader] File not found: Models/{name}");
        Debug.LogError(name + " " + mr.transform.root.name);
        return false;
    }
    
    byte[] b = vxmFile.bytes;
    Debug.Log($"[VXMReader] File loaded successfully, size: {b.Length} bytes");
    
    if (b.Length < 4)
    {
        Debug.LogError($"[VXMReader] File too small: {b.Length} bytes");
        return false;
    }
    
    Debug.Log($"[VXMReader] File header bytes: {b[0]}, {b[1]}, {b[2]}, {b[3]} (type byte: {b[3]})");
    
    try
    {
        if (b[3] == 65)  // 'A'
        {
            Debug.Log("[VXMReader] Detected VXMA format, calling ReadVXMA");
            ReadVXMA(b);
            Debug.Log("[VXMReader] ReadVXMA completed successfully");
        }
        else if (b[3] == 67)  // 'C'
        {
            Debug.Log("[VXMReader] Detected VXMC format, calling ReadVXMC");
            ReadVXMC(b);
            Debug.Log("[VXMReader] ReadVXMC completed successfully");
        }
        else
        {
            Debug.LogError($"[VXMReader] VXM type not supported! Type byte: {b[3]}");
            return false;
        }
    }
    catch (Exception e)
    {
        Debug.LogError($"[VXMReader] Exception during VXM reading: {e.Message}");
        Debug.LogError($"[VXMReader] Stack trace: {e.StackTrace}");
        return false;
    }
    
    Debug.Log($"[VXMReader] VXM data loaded - Volume: {data.volumeW}x{data.volumeH}x{data.volumeD}");
    Debug.Log($"[VXMReader] Materials count: {data.materials?.Count ?? 0}");
    Debug.Log($"[VXMReader] Quads count: {data.quads?.Count ?? 0}");
    Debug.Log($"[VXMReader] Voxel array size: {data.voxel?.voxels?.Length ?? 0}");
    
    float ZValue = -0.0625f;
    
    if (hand == 4 || hand == 3)
    {
        ZValue = -0.0938f;
    }
    
    List<Vertex> cutOffs = new List<Vertex>();
    List<Quad> newQuads = new List<Quad>();
    
    try
    {
        if (hand > 0 && hand < 3)
        {
            Debug.Log($"[VXMReader] Processing hand mesh with ZValue: {ZValue}");
            
            for (int qi = 0; qi < data.quads.Count; qi++)
            {
                Quad q = data.quads[qi];
                if (((q.GetVertex(0).pos).z < ZValue || (q.GetVertex(1).pos).z < ZValue || 
                     (q.GetVertex(2).pos).z < ZValue || (q.GetVertex(3).pos).z < ZValue))
                {
                    if (!((q.GetVertex(0).pos).z < ZValue && (q.GetVertex(1).pos).z < ZValue && 
                          (q.GetVertex(2).pos).z < ZValue && (q.GetVertex(3).pos).z < ZValue))
                    {
                        for (int i = 0; i < 4; i++)
                        {
                            Vector3 tv = q.GetVertex(i).pos;
                            if (tv.z < ZValue)
                            {
                                Vector2 voxels = new Vector2(q.voxelsX, q.voxelsY);
                                
                                tv.z = ZValue;
                                q.SetVertex(i, tv);
                                if (voxels.x == 1)
                                {
                                    //continue;
                                }
                                bool found = false;
                                foreach (Vertex v in cutOffs)
                                {
                                    if (Vector3.Distance(v.pos, q.GetVertex(i).pos) < 0.001f)
                                    {
                                        found = true;
                                        break;
                                    }
                                }
                                if (!found)
                                    cutOffs.Add(q.GetVertex(i));
                            }
                        }
                        q.topLeft.uv.y += 0.4f;
                        q.botLeft.uv.y += 0.4f;
                        q.topRight.uv.y += 0.4f;
                        q.botRight.uv.y += 0.4f;
                        q.topLeft.uv.x += qi * 0.1f;
                        q.botLeft.uv.x += qi * 0.1f;
                        q.topRight.uv.x += qi * 0.1f;
                        q.botRight.uv.x += qi * 0.1f;
                        q = new Quad(q.GetVertex(0), q.GetVertex(1), q.GetVertex(2), q.GetVertex(3), data.voxelScale);
                        newQuads.Add(q);
                    }
                    else
                    {
                        continue;
                    }
                }
                else
                {
                    newQuads.Add(q);
                }
            }
            
            Debug.Log($"[VXMReader] Hand processing - cutOffs: {cutOffs.Count}, newQuads: {newQuads.Count}");
            
            if (cutOffs.Count > 0)
            {
                cutOffs.Sort((p1, p2) => p1.pos.y.CompareTo(p2.pos.y));
                List<Vertex> aligneVertices = new List<Vertex>();
                
                // Check bounds before accessing
                if (cutOffs.Count >= 6)
                {
                    aligneVertices.Add(cutOffs[2]);
                    aligneVertices.Add(cutOffs[3]);
                    aligneVertices.Add(cutOffs[4]);
                    aligneVertices.Add(cutOffs[5]);
                    
                    if (hand == 1 || hand == 3)
                    {
                        aligneVertices.Sort((p1, p2) => p2.pos.x.CompareTo(p1.pos.x));
                    }
                    else
                    {
                        aligneVertices.Sort((p1, p2) => p1.pos.x.CompareTo(p2.pos.x));
                    }
                    
                    for (int replacementIndex = 0; replacementIndex < 4; replacementIndex++)
                    {
                        cutOffs[replacementIndex + 2] = aligneVertices[replacementIndex];
                    }
                }
                else
                {
                    Debug.LogWarning($"[VXMReader] Not enough cutOffs vertices: {cutOffs.Count}");
                }
                
                for (int qi = 0; qi < cutOffs.Count; qi += 4)
                {
                    if (qi + 3 >= cutOffs.Count) break; // Bounds check
                    
                    List<Vertex> vt = new List<Vertex>();
                    vt.Add(cutOffs[qi]);
                    vt.Add(cutOffs[qi + 1]);
                    vt.Add(cutOffs[qi + 2]);
                    vt.Add(cutOffs[qi + 3]);
                    vt.Sort((p1, p2) => p1.pos.x.CompareTo(p2.pos.x));
                    float startX = .9f - ((qi / 4) * 0.1f);
                    Vector2[] uvs = new Vector2[4] { 
                        new Vector2(startX, 0.82f), 
                        new Vector2(startX, 0.8f), 
                        new Vector2(startX + 0.02f, 0.82f), 
                        new Vector2(startX + 0.02f, 0.80f) 
                    };
                    int[] index = new int[4] { 0, 1, 3, 2 };
                    for (int i = 0; i < 4; i++)
                    {
                        Vertex tv = vt[i];
                        tv.index = index[i];
                        tv.uv = uvs[i];
                        vt[i] = tv;
                    }
                    Quad newQuad = new Quad(vt[0], vt[1], vt[3], vt[2], data.voxelScale);
                    newQuads.Add(newQuad);
                }
            }
            data.quads = newQuads;
        }
        
        if (hip)
        {
            Debug.Log("[VXMReader] Processing hip mesh");
            ZValue = 0.2f;
            for (int qi = 0; qi < data.quads.Count; qi++)
            {
                Quad q = data.quads[qi];
                if (((q.GetVertex(0).pos).z > ZValue || (q.GetVertex(1).pos).z > ZValue || 
                     (q.GetVertex(2).pos).z > ZValue || (q.GetVertex(3).pos).z > ZValue))
                {
                    if (!((q.GetVertex(0).pos).z > ZValue && (q.GetVertex(1).pos).z > ZValue && 
                          (q.GetVertex(2).pos).z > ZValue && (q.GetVertex(3).pos).z > ZValue))
                    {
                        float cutOff = 0;
                        for (int i = 0; i < 4; i++)
                        {
                            Vector3 tv = q.GetVertex(i).pos;
                            if (tv.z > ZValue)
                            {
                                tv.z = ZValue;
                                q.SetVertex(i, tv);
                            }
                        }
                        q.topLeft.uv.y += 0.4f;
                        q.botLeft.uv.y += 0.4f;
                        q.topRight.uv.y += 0.4f;
                        q.botRight.uv.y += 0.4f;
                        q.topLeft.uv.x += qi * 0.1f;
                        q.botLeft.uv.x += qi * 0.1f;
                        q.topRight.uv.x += qi * 0.1f;
                        q.botRight.uv.x += qi * 0.1f;
                        q = new Quad(q.GetVertex(0), q.GetVertex(1), q.GetVertex(2), q.GetVertex(3), data.voxelScale);
                        newQuads.Add(q);
                    }
                    else
                    {
                        if (q.normal.z == 1)
                        {
                            for (int i = 0; i < 4; i++)
                            {
                                Vector3 tv = q.GetVertex(i).pos;
                                tv.z = ZValue;
                                q.SetVertex(i, tv);
                            }
                            newQuads.Add(q);
                        }
                        else
                        {
                            continue;
                        }
                    }
                }
                else
                {
                    newQuads.Add(q);
                }
            }
            
            data.quads = newQuads;
        }
        
        Debug.Log("[VXMReader] ReadVXMFile completed successfully");
        return true;
    }
    catch (Exception e)
    {
        Debug.LogError($"[VXMReader] Exception in mesh processing: {e.Message}");
        Debug.LogError($"[VXMReader] Stack trace: {e.StackTrace}");
        return false;
    }
}
    void Skip(int i)
    {
        index += i;
    }
    public string GetString(byte[] b)
    {
        List<byte> foundLetters = new List<byte>();
        int i = 0;
        while (true)
        {
            if (index + i < b.Length && (int)b[index + i] != 0)
            {
                foundLetters.Add(b[index + i]);
                i++;
                continue;
            }
            string s = Convert.ToBase64String(foundLetters.ToArray());
            index = index + (i + 1);
            return s;
        }
    }
    public void importScreenDataVXMA(VXMData data, byte[] b)
    {
        bool hasSurface = GetBool(b);
        if (!hasSurface)
        {
            return;
        }
        data.surfaceStart.x = GetInt(b);
        data.surfaceStart.y = GetInt(b);
        data.surfaceStart.z = GetInt(b);
        data.surfaceEnd.x = GetInt(b);
        data.surfaceEnd.y = GetInt(b);
        data.surfaceEnd.z = GetInt(b);
        data.surfaceNormal = GetInt(b);
    }
    private bool GetBool(byte[] b)
    {
        bool v = ((int)b[index] == 1);
        index += 1;
        return v;
    }
    private int GetInt(byte[] b)
    {
        int v = BitConverter.ToInt32(b, index);
        index += 4;
        return v;
    }
    private float GetFloat(byte[] b)
    {
        float v = BitConverter.ToSingle(b, index);
        index += 4;
        return v;
    }
    public int GetByteAsInt(Byte[] b)
    {
        int i = (int)b[index];
        index++;
        return i;
    }
    public int ParseByte(int i)
    {
        if (i > 127)
        {
            int t = i - 128;
            int tt = 128 - t;
            int b = (-tt);
            return b;

        }
        return i;
    }
    public void CreateMesh(MeshRenderer meshRenderer, string name, Dictionary<string, Vector2> existingQuads, Texture2D tex, Texture2D emis, ref float currentX, ref float currentY, ref float highestQuad)
    {
        MeshFilter meshFilter = meshRenderer.GetComponent<MeshFilter>();

        Mesh mesh = new Mesh();
        data.vertices = new List<Vector3>();
        data.normals = new List<Vector3>();
        data.uvs = new List<Vector2>();
        data.tris = new List<int>();
        data.triCount = 0;


        buildTextures(existingQuads, tex, emis, ref currentX, ref currentY, ref highestQuad);

        foreach (Quad q in data.quads)
        {
            data.vertices.Add(((q.GetVertex(0).pos * data.voxelScale - data.pivot) * 32.0f + data.lodPivot));
            data.vertices.Add(((q.GetVertex(1).pos * data.voxelScale - data.pivot) * 32.0f + data.lodPivot));
            data.vertices.Add(((q.GetVertex(2).pos * data.voxelScale - data.pivot) * 32.0f + data.lodPivot));
            data.vertices.Add(((q.GetVertex(3).pos * data.voxelScale - data.pivot) * 32.0f + data.lodPivot));
            data.uvs.Add(q.GetVertex(0).uv);
            data.uvs.Add(q.GetVertex(1).uv);
            data.uvs.Add(q.GetVertex(2).uv);
            data.uvs.Add(q.GetVertex(3).uv);
            data.normals.Add(-Vector3.forward);
            data.normals.Add(-Vector3.forward);
            data.normals.Add(-Vector3.forward);
            data.normals.Add(-Vector3.forward);
            data.tris.Add(data.triCount + 0);
            data.tris.Add(data.triCount + 1);
            data.tris.Add(data.triCount + 2);
            data.tris.Add(data.triCount + 0);
            data.tris.Add(data.triCount + 2);
            data.tris.Add(data.triCount + 3);
            data.triCount += 4;


        }

        mesh.vertices = data.vertices.ToArray();
        mesh.triangles = data.tris.ToArray();
        mesh.normals = data.normals.ToArray();
        mesh.uv = data.uvs.ToArray();
        mesh.RecalculateNormals();

        meshFilter.mesh = mesh;
    }

    void SetPixel(int x, int y, Color c, Texture2D tex, Texture2D emis, float beginPixelX, float beginPixelY, float endPixelX, float endPixelY, int voxelIndex, int width, int height, bool rotated)
    {
        int textureBleed = 1;
        if (rotated)
        {
            if (x == beginPixelY && beginPixelY - textureBleed >= 0)
            {
                if (tex.GetPixel(x - textureBleed, y) == Color.black)
                {
                    tex.SetPixel(x - textureBleed, y, c);
                }
            }

            if (x == endPixelY - 1 && endPixelY + textureBleed < width)
            {
                if (tex.GetPixel(x + textureBleed, y) == Color.black)
                {
                    tex.SetPixel(x + textureBleed, y, c);
                }
            }

            if (y == beginPixelX && beginPixelX - textureBleed >= 0)
            {
                if (tex.GetPixel(x, y - textureBleed) == Color.black)
                {
                    tex.SetPixel(x, y - textureBleed, c);
                }
            }
            if (y == endPixelX - 1 && endPixelX + textureBleed < height)
            {
                if (tex.GetPixel(x, y + textureBleed) == Color.black)
                {
                    tex.SetPixel(x, y + textureBleed, c);
                }
            }
        }
        else
        {
            if (x == beginPixelX && beginPixelX - textureBleed >= 0)
            {
                if (tex.GetPixel(x - textureBleed, y) == Color.black)
                {
                    tex.SetPixel(x - textureBleed, y, c);
                }
            }

            if (x == endPixelX - 1 && endPixelX + textureBleed < width)
            {
                if (tex.GetPixel(x + textureBleed, y) == Color.black)
                {
                    tex.SetPixel(x + textureBleed, y, c);
                }
            }


            if (y == beginPixelY && beginPixelY - textureBleed >= 0)
            {
                if (tex.GetPixel(x, y - textureBleed) == Color.black)
                {
                    tex.SetPixel(x, y - textureBleed, c);
                }
            }
            if (y == endPixelY - 1 && endPixelY + textureBleed < height)
            {
                if (tex.GetPixel(x, y + textureBleed) == Color.black)
                {
                    tex.SetPixel(x, y + textureBleed, c);
                }
            }
        }
        if (tex.GetPixel(x, y) != c && tex.GetPixel(x, y) != Color.black)
            Debug.Log("help " + c);
        tex.SetPixel(x, y, c);

        if (data.materials[data.voxel.voxels[voxelIndex].colorID].emissive)
        {
            data.hasEmission = true;
            emis.SetPixel(x, y, c);
        }
    }
    public VXMData buildTextures(Dictionary<string, Vector2> existingQuads, Texture2D tex, Texture2D emis, ref float currentX, ref float currentY, ref float highestQuad)
    {
        float pixelSize = data.quads[0].voxelUvSizeX;
        int pixelsPerVoxelX = Mathf.RoundToInt(pixelSize * 100.0f);
        int pixelsPerVoxelY = Mathf.RoundToInt(pixelSize * 100.0f);

        List<Quad> ql = new List<Quad>();
        float voxSize = 32 * data.voxelScale;

        for (int i = 0; i < data.quads.Count; i++)
        {
            Quad tQ = data.quads[i];
            Color c = Color.black;

            float beginPixelX = currentX;
            float beginPixelY = currentY;
            float endPixelX = ((tQ.topRight.NormalizedUV(tQ.rotatedUV).x * tex.width));
            float endPixelY = ((tQ.botLeft.NormalizedUV(tQ.rotatedUV).y * tex.height));

            string key = tQ.voxelsX + "," + tQ.voxelsY + ",";
            for (int x = 0; x < tQ.voxelsX; x++)
            {
                for (int y = 0; y < tQ.voxelsY; y++)
                {
                    int voxelIndexY = y;
                    int voxelIndexX = x;

                    Vector3 nextVox = ((tQ.XVoxelMultiplier * voxelIndexX) + (tQ.YVoxelMultiplier * voxelIndexY)) * voxSize;

                    Vector3 voxSize3 = new Vector3(data.volumeW * data.pivot.x, data.volumeH * data.pivot.y, data.volumeD * data.pivot.z);
                    Vector3 voxelOffset = voxSize3 + data.lodPivot - Vector3.one * 0.5f;
                    Vector3 voxelPos = (tQ.firstVoxel * voxSize) - (tQ.normal * ((voxSize * (1.0f / voxSize)) / 2.0f)) + voxelOffset;

                    voxelPos += nextVox;
                    int index = (int)(Mathf.RoundToInt(voxelPos.x) + Mathf.RoundToInt(voxelPos.y) * data.volumeW + Mathf.RoundToInt(voxelPos.z) * data.volumeW * data.volumeH);
                    key += data.materials[data.voxel.voxels[index].colorID].col.Color().GetHashCode() + ",";
                }
            }


            if (existingQuads.ContainsKey(key))
            {
                Vector2 pos = existingQuads[key];
                beginPixelX = pos.x;
                beginPixelY = pos.y;
                endPixelY = beginPixelY + tQ.voxelsY * pixelsPerVoxelY;
                endPixelX = beginPixelX + tQ.voxelsX * pixelsPerVoxelX;
            }
            else
            {
                if (beginPixelX + tQ.voxelsX * pixelsPerVoxelX > tex.width)
                {
                    currentX = 0;
                    beginPixelX = 0;
                    beginPixelY += highestQuad + 2;
                    currentY += highestQuad + 2;
                    highestQuad = 0;
                }

                if (highestQuad < Mathf.Ceil(tQ.voxelsY * pixelsPerVoxelY))
                {
                    highestQuad = Mathf.Ceil(tQ.voxelsY * pixelsPerVoxelY);
                }

                endPixelY = beginPixelY + tQ.voxelsY * pixelsPerVoxelY;
                endPixelX = beginPixelX + tQ.voxelsX * pixelsPerVoxelX;

                for (int x = (int)beginPixelX; x < endPixelX; x++)
                {
                    for (int y = (int)beginPixelY; y < endPixelY; y++)
                    {
                        int voxelIndexY = Mathf.FloorToInt(((y - beginPixelY) / (pixelsPerVoxelY)));
                        int voxelIndexX = Mathf.FloorToInt(((x - beginPixelX) / (pixelsPerVoxelX)));

                        Vector3 nextVox = ((tQ.XVoxelMultiplier * voxelIndexX) + (tQ.YVoxelMultiplier * voxelIndexY)) * voxSize;

                        Vector3 voxSize3 = new Vector3(data.volumeW * data.pivot.x, data.volumeH * data.pivot.y, data.volumeD * data.pivot.z);
                        Vector3 voxelOffset = voxSize3 + data.lodPivot - Vector3.one * 0.5f;
                        Vector3 voxelPos = (tQ.firstVoxel * voxSize) - (tQ.normal * ((voxSize * (1.0f / voxSize)) / 2.0f)) + voxelOffset;

                        voxelPos += nextVox;
                        int index = (int)(Mathf.RoundToInt(voxelPos.x) + Mathf.RoundToInt(voxelPos.y) * data.volumeW + Mathf.RoundToInt(voxelPos.z) * data.volumeW * data.volumeH);


                        int mat = data.voxel.voxels[index].colorID;
                        if (mat < data.materials.Count)
                        {
                            c = data.materials[data.voxel.voxels[index].colorID].col.Color();
                        }
                        else
                        {
                            c = Color.black;
                        }

                        SetPixel(x, y, c, tex, emis, beginPixelX, beginPixelY, endPixelX, endPixelY, index, tex.width, tex.height, false);
                    }
                }

                existingQuads.Add(key, new Vector2(beginPixelX, beginPixelY));
                currentX += tQ.voxelsX * (float)pixelsPerVoxelX + 2;
            }

            float lx = (beginPixelX) / tex.width;
            float rx = (endPixelX) / tex.width;
            float ty = (beginPixelY) / tex.height;
            float by = (endPixelY) / tex.height;

            tQ.topRight.SetAdjustedUV(rx, ty);
            tQ.botRight.SetAdjustedUV(rx, by);
            tQ.botLeft.SetAdjustedUV(lx, by);
            tQ.topLeft.SetAdjustedUV(lx, ty);

            ql.Add(tQ);
        }

        data.quads = ql;
        return data;
    }
    public Color GetColorOfVoxel(Vector3 pos)
    {
        int index = (int)(Mathf.RoundToInt(pos.x) + Mathf.RoundToInt(pos.y) * data.volumeW + Mathf.RoundToInt(pos.z) * data.volumeW * data.volumeH);


        int mat = data.voxel.voxels[index].colorID;
        if (mat < data.materials.Count)
        {
            return data.materials[data.voxel.voxels[index].colorID].col.Color();
        }
        return Color.black;
    }

    public bool GetColorEmis(Vector3 pos)
    {
        int index = (int)(Mathf.RoundToInt(pos.x) + Mathf.RoundToInt(pos.y) * data.volumeW + Mathf.RoundToInt(pos.z) * data.volumeW * data.volumeH);


        int mat = data.voxel.voxels[index].colorID;
        if (mat < data.materials.Count)
        {
            return data.materials[data.voxel.voxels[index].colorID].emissive;
        }
        return false;
    }
}

public class VEMaterial
{

    public VEColor col;
    public bool emissive;

    public VEMaterial(int i)
    {

        this.col = new VEColor(i);
        this.emissive = false;
    }

    public VEMaterial(int i, bool b)
    {

        this.col = new VEColor(i);
        this.emissive = b;
    }

    private VEMaterial(VEMaterial a0)
    {

        this.col = a0.col;
        this.emissive = a0.emissive;
    }

    public VEMaterial clone()
    {
        return new VEMaterial(this);
    }

    public void copyFrom(VEMaterial a)
    {
        this.col = a.col;
        this.emissive = a.emissive;
    }

    public bool isEqual(VEMaterial a)
    {
        if (this.emissive == a.emissive && this.col.rgb == a.col.rgb)
        {
            return true;
        }
        return false;
    }

    public bool equals(VEMaterial a)
    {
        if (this == a)
        {
            return true;
        }
        return this.isEqual(a);

    }
}
public struct ImageIndex
{
    public int width;
    public int height;
    public int[] pixels;
    public Color[] colors;

    public ImageIndex(int i, int i0)
    {
        this.width = i;
        this.height = i0;
        this.pixels = new int[this.width * this.height];
        this.colors = new Color[this.width * this.height];
        int i1 = 0;
        for (; i1 < this.pixels.Length; i1 = i1 + 1)
        {
            this.pixels[i1] = -1;// (byte)-1;
        }
    }
    public ImageIndex(ImageIndex a0)
    {
        this.width = a0.width;
        this.height = a0.height;
        this.pixels = new int[this.width * this.height];
        this.colors = new Color[this.width * this.height];
        int i = 0;
        for (; i < this.pixels.Length; i = i + 1)
        {
            this.pixels[i] = a0.pixels[i];
        }
    }
}

public struct VEColor
{
    public VEColor(uint rgb)
    {
        this.rgb = (int)rgb;
        b = (byte)((rgb) & 0xFF);
        g = (byte)((rgb >> 8) & 0xFF);
        r = (byte)((rgb >> 16) & 0xFF);
    }
    public VEColor(int rgb)
    {
        this.rgb = rgb;
        b = (byte)((rgb) & 0xFF);
        g = (byte)((rgb >> 8) & 0xFF);
        r = (byte)((rgb >> 16) & 0xFF);
    }
    public Color Color()
    {
        return new Color(r / 255.0f, g / 255.0f, b / 255.0f);
    }
    public int rgb;
    public int r;
    public int g;
    public int b;
}
public struct Vertex
{
    public Vertex(Vector3 pos, Vector2 uv, int index)
    {
        this.index = index;
        this.pos = pos;
        this.uv = uv;
    }
    public Vertex SetAdjustedUV(float x, float y)
    {
        this.uv = new Vector2(x, y);
        return this;
    }
    public void SetAdjustedUV(float x, float y, bool flipped)
    {
        if (!flipped)
            this.uv = new Vector2(x, y);
        if (flipped)
            this.uv = new Vector2(y, x);

    }
    public Vector2 NormalizedUV(bool flipped)
    {
        Vector2 uv = this.uv;
        if (flipped)
        {
            float x = uv.y;
            uv.y = uv.x;
            uv.x = x;
        }
        return uv;
    }
    public int index;
    public Vector3 pos;
    public Vector2 uv;
}
public struct Quad
{
    public Quad(Vertex v1, Vertex v2, Vertex v3, Vertex v4, float voxelSize)
    {
        List<Vertex> v = new List<Vertex> { v1, v2, v3, v4 };
        Vector3 t1 = (v2.pos - v1.pos);
        Vector3 t2 = (v3.pos - v1.pos);
        normal.x = t1.y * t2.z - t1.z * t2.y;
        normal.y = t1.z * t2.x - t1.x * t2.z;
        normal.z = t1.x * t2.y - t1.y * t2.x;
        normal.Normalize();

        topLeft = v[0];
        botLeft = v[1];
        topRight = v[2];
        botRight = v[3];

        voxelsX = 0;
        voxelUvSizeX = 0;
        voxelsY = 0;
        voxelUvSizeY = 0;

        if (normal.x != 0)
        {

            v.Sort((a, b) => a.pos.z.CompareTo(b.pos.z));
            if (normal.x > 0)
            {
                if (v[0].pos.y > v[1].pos.y)
                {
                    topLeft = v[0];
                    botLeft = v[1];
                }
                else
                {
                    topLeft = v[1];
                    botLeft = v[0];
                }
                if (v[2].pos.y > v[3].pos.y)
                {
                    topRight = v[2];
                    botRight = v[3];
                }
                else
                {
                    topRight = v[3];
                    botRight = v[2];
                }
            }
            else
            {
                if (v[0].pos.y > v[1].pos.y)
                {
                    topLeft = v[0];
                    botLeft = v[1];
                }
                else
                {
                    topLeft = v[1];
                    botLeft = v[0];
                }
                if (v[2].pos.y > v[3].pos.y)
                {
                    topRight = v[2];
                    botRight = v[3];
                }
                else
                {
                    topRight = v[3];
                    botRight = v[2];
                }
            }
            bool flipped = (topLeft.uv.x == topRight.uv.x);
            voxelsX = (int)Math.Round((Math.Abs(topLeft.pos.z - topRight.pos.z)) / (1.0f / (32 * voxelSize)));
            voxelUvSizeX = (Math.Abs(topLeft.NormalizedUV(flipped).x - topRight.NormalizedUV(flipped).x)) / voxelsX;
            voxelsY = (int)Math.Round((Math.Abs(topLeft.pos.y - botLeft.pos.y)) / (1.0f / (32 * voxelSize)));
            voxelUvSizeY = (Math.Abs(topLeft.NormalizedUV(flipped).y - botLeft.NormalizedUV(flipped).y)) / voxelsY;
        }
        else if (normal.y != 0)
        {

            v.Sort((a, b) => a.pos.x.CompareTo(b.pos.x));
            if (normal.y > 0)
            {
                if (v[0].pos.z > v[1].pos.z)
                {
                    topLeft = v[1];
                    botLeft = v[0];
                }
                else
                {
                    topLeft = v[0];
                    botLeft = v[1];
                }
                if (v[2].pos.z > v[3].pos.z)
                {
                    topRight = v[3];
                    botRight = v[2];
                }
                else
                {
                    topRight = v[2];
                    botRight = v[3];
                }
            }
            else
            {
                if (v[0].pos.z > v[1].pos.z)
                {
                    topLeft = v[1];
                    botLeft = v[0];
                }
                else
                {
                    topLeft = v[0];
                    botLeft = v[1];
                }
                if (v[2].pos.z > v[3].pos.z)
                {
                    topRight = v[3];
                    botRight = v[2];
                }
                else
                {
                    topRight = v[2];
                    botRight = v[3];
                }
            }
            bool flipped = (topLeft.uv.x == topRight.uv.x);
            voxelsX = (int)Math.Round((Math.Abs(topLeft.pos.x - topRight.pos.x)) / (1.0f / (32 * voxelSize)));
            voxelUvSizeX = (float)((Math.Abs(topLeft.NormalizedUV(flipped).x - topRight.NormalizedUV(flipped).x)) / (float)voxelsX);
            voxelsY = (int)Math.Round((Math.Abs(topLeft.pos.z - botLeft.pos.z)) / (1.0f / (32 * voxelSize)));
            voxelUvSizeY = (Math.Abs(topLeft.NormalizedUV(flipped).y - botLeft.NormalizedUV(flipped).y)) / voxelsY;
        }
        else if (normal.z != 0)
        {

            v.Sort((a, b) => a.pos.y.CompareTo(b.pos.y));
            if (normal.z > 0)
            {
                if (v[0].pos.x > v[1].pos.x)
                {
                    botRight = v[0];
                    botLeft = v[1];
                }
                else
                {
                    botRight = v[1];
                    botLeft = v[0];
                }
                if (v[2].pos.x > v[3].pos.x)
                {
                    topRight = v[2];
                    topLeft = v[3];
                }
                else
                {
                    topRight = v[3];
                    topLeft = v[2];
                }
            }
            else
            {
                if (v[0].pos.x > v[1].pos.x)
                {
                    botRight = v[0];
                    botLeft = v[1];
                }
                else
                {
                    botRight = v[1];
                    botLeft = v[0];
                }
                if (v[2].pos.x > v[3].pos.x)
                {
                    topRight = v[2];
                    topLeft = v[3];
                }
                else
                {
                    topRight = v[3];
                    topLeft = v[2];
                }
            }
            bool flipped = (topLeft.uv.x == topRight.uv.x);
            voxelsX = (int)Math.Round((Math.Abs(topLeft.pos.x - topRight.pos.x)) / (1.0f / (32 * voxelSize)));
            voxelUvSizeX = (Math.Abs(topLeft.NormalizedUV(flipped).x - topRight.NormalizedUV(flipped).x)) / voxelsX;
            voxelsY = (int)Math.Round((Math.Abs(topLeft.pos.y - botLeft.pos.y)) / (1.0f / (32 * voxelSize)));
            voxelUvSizeY = (Math.Abs(topLeft.NormalizedUV(flipped).y - botLeft.NormalizedUV(flipped).y)) / voxelsY;
        }
        if (botLeft.pos.x > botRight.pos.x)
        {
            topRight = topRight;
        }
        //calculate the first voxel here
        //int pixelsX = topLeft.pos, botRight.pos
        Vector3 voxPos = topLeft.pos;
        voxelScale = Vector3.Distance(topRight.pos, topLeft.pos) / (voxelsX);
        voxPos += (topRight.pos - topLeft.pos) / (voxelsX * 2.0f);
        voxPos += (botLeft.pos - topLeft.pos) / (voxelsY * 2.0f);
        firstVoxel = voxPos;
        XVoxelMultiplier = (topRight.pos - topLeft.pos) / (voxelsX);
        YVoxelMultiplier = (botLeft.pos - topLeft.pos) / (voxelsY);
    }
    public bool rotatedUV
    {
        get
        {
            return topLeft.uv.x == topRight.uv.x;
        }
    }
    public bool flippedUVX
    {
        get
        {
            return topLeft.NormalizedUV(rotatedUV).x > topRight.NormalizedUV(rotatedUV).x;
        }
    }
    public bool flippedUVY
    {
        get
        {
            return topLeft.NormalizedUV(rotatedUV).y > botLeft.NormalizedUV(rotatedUV).y;
        }
    }


    public float voxelsX;
    public float voxelUvSizeX;
    public float voxelsY;
    public float voxelUvSizeY;
    public float voxelScale;
    public Vector3 XVoxelMultiplier;
    public Vector3 YVoxelMultiplier;
    public Vector3 firstVoxel;
    public Vertex topLeft;
    public Vertex topRight;
    public Vertex botLeft;
    public Vertex botRight;
    public Vector3 normal;
    public Vertex GetVertex(int index)
    {
        if (topLeft.index == index)
            return topLeft;
        if (topRight.index == index)
            return topRight;
        if (botLeft.index == index)
            return botLeft;
        if (botRight.index == index)
            return botRight;
        return botLeft;
    }
    public void SetVertex(int index, Vector3 value)
    {
        if (topLeft.index == index)
            topLeft.pos = value;
        if (topRight.index == index)
            topRight.pos = value;
        if (botLeft.index == index)
            botLeft.pos = value;
        if (botRight.index == index)
            botRight.pos = value;
    }
    public void ShrinkUvPixels(Vector2 value)
    {
        //if (flippedUVX)
        //    value.x = -value.x;
        //if (flippedUVY)
        //    value.y = -value.y;
        Vector2 adjustment = value;
        if (rotatedUV)
        {
            adjustment.x *= voxelUvSizeY;
            adjustment.y *= voxelUvSizeX;
        }
        else
        {
            adjustment.x *= voxelUvSizeX;
            adjustment.y *= voxelUvSizeY;
        }
        botLeft.uv -= adjustment;
        topLeft.uv -= adjustment;


    }
}
public struct Voxel
{
    public Voxel(int color, Vector3 pos)
    {
        this.colorID = color;
        this.pos = pos;
    }
    public int colorID;
    public Vector3 pos;
}
public class VoxelVolume
{
    public Voxel[] voxels;
    public int w;
    public int h;
    public int d;
    public int wh;
    public int wd;
    public int hd;
    public int whd;
    public float voxW;
    public float voxH;
    public float voxD;
    public float voxHW;
    public float voxHH;
    public float voxHD;
    public float halfW;
    public float halfH;
    public float halfD;
    public int voxelAmount;

    public VoxelVolume(int i, int i0, int i1)
    {
        this.w = i;
        this.h = i0;
        this.d = i1;
        this.wh = this.w * this.h;
        this.wd = this.w * this.d;
        this.hd = this.h * this.d;
        this.whd = this.w * this.h * this.d;
        int i2 = this.w;
        int i3 = this.h;
        this.voxW = 1f / (float)(this.w);
        this.voxH = this.voxW;
        this.voxD = this.voxW;
        int i4 = this.h;
        int i5 = this.w;
        this.voxD = 1f / (float)(this.d);
        this.voxW = this.voxD;
        this.voxH = this.voxD;
        this.voxH = 1f / (float)(this.h);
        this.voxW = this.voxH;
        this.voxD = this.voxH;
        this.voxHW = this.voxW * 0.5f;
        this.voxHH = this.voxH * 0.5f;
        this.voxHD = this.voxD * 0.5f;
        this.halfW = this.voxHW * (float)this.w;
        this.halfH = this.voxHH * (float)this.h;
        this.halfD = this.voxHD * (float)this.d;
        this.voxels = new Voxel[this.whd];
    }
}
public struct VXMData
{
    public int volumeW;
    public int volumeH;
    public int volumeD;

    public float voxelScale;
    public float voxelSize;
    public VoxelVolume voxel;
    public Vector3 surfaceStart;
    public Vector3 surfaceEnd;
    public int surfaceNormal;
    public Vector3 pivot;
    public Vector3 lodPivot;
    public List<Quad> quads;
    public List<VEMaterial> materials;
    public List<Vector3> vertices;
    public List<Vector3> normals;
    public List<Vector2> uvs;
    public List<int> tris;
    public int triCount;
    public Texture2D tex;
    public bool hasEmission;
    public Texture2D emis;

}