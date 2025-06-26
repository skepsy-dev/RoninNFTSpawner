using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public class MeshCombiner : MonoBehaviour
{
    public MeshFilter finalMesh;
    public MeshFilter[] meshesToAdd;

    Vector3 TTransformPoint(Vector3 transforPos, Quaternion transformRotation, Vector3 transformScale, Vector3 pos)
    {
        Matrix4x4 matrix = Matrix4x4.TRS(transforPos, transformRotation, transformScale);
        Matrix4x4 inverse = matrix;
        return inverse.MultiplyPoint3x4(pos);
    }

    public void CombineMeshes()
    {
        Mesh mesh = new Mesh();
        List<Vector3> vert = new List<Vector3>();
        List<int> tri = new List<int>();
        List<Vector3> normals = new List<Vector3>();
        List<Vector2> uvs = new List<Vector2>();
        foreach (Vector3 v in finalMesh.mesh.vertices)
        {
            Vector3 offset = finalMesh.transform.InverseTransformPoint(finalMesh.transform.TransformPoint(v));
            vert.Add(offset);
        }
        tri.AddRange(finalMesh.mesh.triangles);
        normals.AddRange(finalMesh.mesh.normals);
        uvs.AddRange(finalMesh.mesh.uv);

        foreach (MeshFilter m in meshesToAdd)
        {
            int triCount = vert.Count;
            foreach(Vector3 v in m.mesh.vertices)
            {
                Vector3 offset = finalMesh.transform.InverseTransformPoint(m.transform.TransformPoint(v));
                vert.Add(offset);
            }
            foreach (int tr in m.mesh.triangles)
            {
                tri.Add(triCount+tr);
            }
            normals.AddRange(m.mesh.normals);
            uvs.AddRange(m.mesh.uv);
            Destroy(m.gameObject);
        }
        mesh.vertices = vert.ToArray();
        mesh.triangles = tri.ToArray();
        mesh.normals = normals.ToArray();
        mesh.uv =  uvs.ToArray();
        mesh.RecalculateNormals();
        //var dirPath = "Assets/Resources";
        //string finalPath = dirPath + "/" + finalMesh.transform.name+"_"+ transform.root.name + ".asset";

        //if (!System.IO.Directory.Exists(dirPath))
        //{
        //    System.IO.Directory.CreateDirectory(dirPath);
        //}
        //AssetDatabase.CreateAsset(mesh, finalPath);
        //AssetDatabase.SaveAssets();
        //AssetDatabase.Refresh();

        //string loadPath = finalPath.Replace("Assets/Resources/", "").Replace(".asset", "");
        //Object ob = Resources.Load(loadPath);
        //Mesh newMesh = AssetDatabase.LoadAssetAtPath<Mesh>(finalPath);
        finalMesh.mesh = mesh;

    }
}
