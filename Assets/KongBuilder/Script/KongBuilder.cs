using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using SimpleJSON;
using System;
#if UNITY_EDITOR
using UnityEditor;
#endif


public class KongBuilder : VXMReader
{
    bool SaveTextures = false;
    protected static KongBuilder _instance;

    public static KongBuilder Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<KongBuilder>();
                if (_instance == null)
                {
                    GameObject obj = new GameObject();
                    _instance = obj.AddComponent<KongBuilder>();
                }
            }
            return _instance;
        }
    }

    public class Node
    {
        public Node(string name, string model)
        {
            this.name = name;
            this.model = model;
        }
        public string name;
        public string model;
    }
    public class Kong
    {
        public int assetId { get; set; }

        public Dictionary<string, string> models = new Dictionary<string, string>();
    }

    public List<Kong> kongz = new List<Kong>();
    GameObject baseKong;

    // Index cache for lazy loading
    private Dictionary<int, IndexEntry> _indexCache;
    private class IndexEntry
    {
        public int StartByte;
        public int Length;
    }

    // Start is called before the first frame update
    void Awake()
    {
        _instance = this as KongBuilder;
        baseKong = Resources.Load("BaseKong") as GameObject;

        // Load the index instead of all blueprints
        LoadIndex();
    }

    private void LoadIndex()
    {
        TextAsset indexAsset = Resources.Load<TextAsset>("blueprints_index");
        if (indexAsset == null)
        {
            Debug.LogError("[KongBuilder] Index file not found! Run 'Tools/Kong Builder/Build Blueprint Index' first!");
            return;
        }

        _indexCache = new Dictionary<int, IndexEntry>();

        string[] lines = indexAsset.text.Split('\n');
        foreach (string line in lines)
        {
            if (line.StartsWith("#") || string.IsNullOrWhiteSpace(line))
                continue;

            string[] parts = line.Split(',');
            if (parts.Length == 3)
            {
                int kongId = int.Parse(parts[0]);
                _indexCache[kongId] = new IndexEntry
                {
                    StartByte = int.Parse(parts[1]),
                    Length = int.Parse(parts[2])
                };
            }
        }

        Debug.Log($"[KongBuilder] Loaded index with {_indexCache.Count} entries");
    }

    /// <summary>
    /// Function to create kong model in T pose. If you specify a ID which is not correct this function will return null.
    /// </summary>
    /// <param name="id">The ID of the kong you want to load. ID gets checked inside the function when ID not valid will return null,</param>
    /// <param name="position">Where to spawn the kong</param>
    /// <param name="withFingers">When true the fingers will have seperate meshes and are moveable otherwise the hands and fingers will be one static mesh</param>
    /// <param name="combineMeshes">Kongz have multiple meshes for the head body and hands. When true these will be merged together loading might take a little bit longer.</param>
    /// <returns></returns>
    public GameObject LoadKong(int id, Vector3 position, bool withFingers = true, bool combineMeshes = true)
    {
        if (id == 101)
        {
            Debug.LogError("Kong 101 doesn't exist.");
            return null;
        }
        if (id < 1)
        {
            Debug.LogError("Id must be 1 or higher.");
            return null;
        }
        if (id > 15000)
        {
            Debug.LogError("Id must be lower then 15001.");
            return null;
        }

        if (_indexCache == null || !_indexCache.ContainsKey(id))
        {
            Debug.LogError($"[KongBuilder] Kong #{id} not found in index!");
            return null;
        }

        // Load just this Kong's data
        IndexEntry entry = _indexCache[id];
        TextAsset blueprintsAsset = Resources.Load<TextAsset>("blueprints");
        if (blueprintsAsset == null)
        {
            Debug.LogError("[KongBuilder] blueprints.json not found!");
            return null;
        }

        // Extract only the bytes we need
        byte[] allBytes = blueprintsAsset.bytes;
        byte[] kongBytes = new byte[entry.Length];
        System.Array.Copy(allBytes, entry.StartByte, kongBytes, 0, entry.Length);

        // Convert only these bytes to string
        string kongJsonStr = System.Text.Encoding.UTF8.GetString(kongBytes);
        Debug.Log($"[KongBuilder] Extracted Kong #{id} data: {entry.Length} bytes");
        Debug.Log($"[KongBuilder] First 200 chars of extracted JSON: {kongJsonStr.Substring(0, Math.Min(200, kongJsonStr.Length))}");
        Debug.Log($"[KongBuilder] Last 50 chars: {kongJsonStr.Substring(Math.Max(0, kongJsonStr.Length - 50))}");

        // Parse only this Kong's data
        JSONNode kongData = JSON.Parse(kongJsonStr);

        // Create a temporary Kong object for this specific Kong
        Kong kong = new Kong();
        kong.assetId = kongData["assetUName"].AsInt;
        foreach (JSONNode node in kongData["nodes"])
        {
            kong.models.Add(node["node"].Value, node["models"].ToString().Replace("[\"", "").Replace("\"]", ""));
        }

        int height = 512;
        int width = 512;

        Dictionary<string, Vector2> existingQuads = new Dictionary<string, Vector2>();
        Texture2D tex = new Texture2D(width, height);
        Texture2D emis = new Texture2D(width, height);
        // Load the Kong SDK's Standard material instead of creating a new one
        Material kongMaterial = Resources.Load<Material>("Materials/Standard");
        if (kongMaterial == null)
        {
            Debug.LogError("[KongBuilder] Could not load Kong Standard material!");
            kongMaterial = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        }
        float highestQuad = 4;
        float currentX = 2;
        float currentY = 0;

        for (int xe = 0; xe < width; xe++)
        {
            for (int ye = 0; ye < height; ye++)
            {
                tex.SetPixel(xe, ye, Color.black);
                emis.SetPixel(xe, ye, Color.black);
            }
        }

        GameObject kongObj = Instantiate(baseKong);
        kongObj.name = id.ToString();
        kongObj.transform.position = position;
        MeshRenderer[] models = kongObj.transform.GetComponentsInChildren<MeshRenderer>();
        bool hasEmission = false;
        foreach (MeshRenderer mr in models)
        {
            if (kong.models.ContainsKey(mr.name))
            {
                if (mr.name.Contains("hand") && withFingers)
                {
                    if (mr.name == "skin_right_hand")
                    {
                        ReadVXMFile(kong.models[mr.name], mr, 2);
                        tex.SetPixel(0, 1, GetColorOfVoxel(new Vector3(8, 2, 3)));
                        tex.SetPixel(0, 0, GetColorOfVoxel(new Vector3(9, 2, 3)));

                        if (GetColorEmis(new Vector3(8, 2, 3)))
                            emis.SetPixel(0, 1, GetColorOfVoxel(new Vector3(8, 2, 3)));
                        if (GetColorEmis(new Vector3(9, 2, 3)))
                            emis.SetPixel(0, 0, GetColorOfVoxel(new Vector3(9, 2, 3)));
                    }
                    else if (mr.name == "skin_left_hand")
                    {
                        ReadVXMFile(kong.models[mr.name], mr, 1);
                        tex.SetPixel(0, 3, GetColorOfVoxel(new Vector3(4, 2, 3)));
                        tex.SetPixel(0, 2, GetColorOfVoxel(new Vector3(3, 2, 3)));

                        if (GetColorEmis(new Vector3(4, 2, 3)))
                            emis.SetPixel(0, 3, GetColorOfVoxel(new Vector3(4, 2, 3)));
                        if (GetColorEmis(new Vector3(3, 2, 3)))
                            emis.SetPixel(0, 2, GetColorOfVoxel(new Vector3(3, 2, 3)));


                    }
                    else
                    {
                        if (mr.name.Contains("left"))
                        {
                            if (!kong.models.ContainsKey("skin_left_hand"))
                            {
                                if (ReadVXMFile(kong.models[mr.name], mr, 3))
                                {
                                    tex.SetPixel(0, 3, GetColorOfVoxel(new Vector3(4, 2, 3)));
                                    tex.SetPixel(0, 2, GetColorOfVoxel(new Vector3(3, 2, 3)));

                                    if (GetColorEmis(new Vector3(4, 2, 3)))
                                        emis.SetPixel(0, 3, GetColorOfVoxel(new Vector3(4, 2, 3)));
                                    if (GetColorEmis(new Vector3(3, 2, 3)))
                                        emis.SetPixel(0, 2, GetColorOfVoxel(new Vector3(3, 2, 3)));
                                }
                                else
                                {
                                    continue;
                                }
                            }
                            else
                            {
                                ReadVXMFile(kong.models[mr.name], mr);
                            }
                        }
                        else
                        {

                            if (!kong.models.ContainsKey("skin_right_hand"))
                            {
                                if (ReadVXMFile(kong.models[mr.name], mr, 4))
                                {
                                    tex.SetPixel(0, 1, GetColorOfVoxel(new Vector3(11, 2, 3)));
                                    tex.SetPixel(0, 0, GetColorOfVoxel(new Vector3(10, 2, 3)));

                                    if (GetColorEmis(new Vector3(11, 2, 3)))
                                        emis.SetPixel(0, 1, GetColorOfVoxel(new Vector3(11, 2, 3)));
                                    if (GetColorEmis(new Vector3(10, 2, 3)))
                                        emis.SetPixel(0, 0, GetColorOfVoxel(new Vector3(10, 2, 3)));
                                }
                                else
                                {
                                    continue;
                                }

                            }
                            else
                            {
                                ReadVXMFile(kong.models[mr.name], mr);
                            }
                        }
                    }
                }
                else
                {
                    if (mr.name.Contains("body_hip") && id != 10039 && id != 801)
                    {
                        ReadVXMFile(kong.models[mr.name], mr, 0, true);
                    }
                    else
                    {
                        ReadVXMFile(kong.models[mr.name], mr);
                    }
                }

                CreateMesh(mr, name, existingQuads, tex, emis, ref currentX, ref currentY, ref highestQuad);
            }
            else
            {
                if (mr.name.Contains("finger"))
                {
                    if (withFingers)
                    {
                        if (mr.name.Contains("right"))
                        {
                            Mesh m = mr.GetComponent<MeshFilter>().mesh;
                            Vector2[] uvs = m.uv;
                            m.uv = uvs;
                        }
                        else
                        {
                            Mesh m = mr.GetComponent<MeshFilter>().mesh;
                            Vector2[] uvs = m.uv;
                            m.uv = uvs;
                        }
                    }
                    else
                    {
                        Destroy(mr.gameObject);
                    }
                }
                else
                {
                    mr.enabled = false;
                }
            }
            mr.sharedMaterial = kongMaterial;
            if (data.hasEmission)
            {
                hasEmission = true;
            }

        }


        MeshCombiner[] meshCombiner = kongObj.transform.GetComponentsInChildren<MeshCombiner>();
        foreach (MeshCombiner mc in meshCombiner)
        {
            if (combineMeshes)
            {
                mc.CombineMeshes();
            }
            Destroy(mc);
        }


        emis.Apply();
        emis.filterMode = FilterMode.Point;

        tex.Apply();
        tex.filterMode = FilterMode.Point;
        kongMaterial.mainTexture = tex;
        if (hasEmission)
        {
            kongMaterial.EnableKeyword("_EMISSION");
            kongMaterial.SetColor("_EmissionColor", Color.white);
            kongMaterial.SetTexture("_EmissionMap", emis);
        }
        kongMaterial.SetColor("Specular", Color.clear);
        kongMaterial.SetFloat("_Glossiness", .0f);
        kongMaterial.SetFloat("_GlossMapScale", .0f);
        kongMaterial.SetFloat("_Smoothness", .0f);

        return kongObj;
    }
}