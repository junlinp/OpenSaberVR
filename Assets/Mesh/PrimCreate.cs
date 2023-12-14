using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Text;

public static class ObjExporter
{
    public static void SaveObj(string path, Mesh mesh)
    {
        StringBuilder sb = new StringBuilder();

        sb.AppendLine("g " + mesh.name);

        foreach (Vector3 v in mesh.vertices)
            sb.AppendLine("v " + v.x + " " + v.y + " " + v.z);

        foreach (Vector3 n in mesh.normals)
            sb.AppendLine("vn " + n.x + " " + n.y + " " + n.z);

        foreach (Vector2 uv in mesh.uv)
            sb.AppendLine("vt " + uv.x + " " + uv.y);

        for (int i = 0; i < mesh.subMeshCount; i++)
        {
            sb.AppendLine("usemtl submesh" + i);
            sb.AppendLine("usemap submesh" + i);

            int[] triangles = mesh.GetTriangles(i);
            for (int j = 0; j < triangles.Length; j += 3)
            {
                sb.AppendLine(string.Format("f {0}/{0}/{0} {1}/{1}/{1} {2}/{2}/{2}",
                    triangles[j] + 1, triangles[j + 1] + 1, triangles[j + 2] + 1));
            }
        }

        File.WriteAllText(path, sb.ToString());
    }
}
public class PrimCreate : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }
    void OnEnable() {
        var mesh = new Mesh {
            name = "Prim",
        };
        mesh.vertices = new Vector3[] {
            new Vector3(1.0f, 1.0f, 1.0f),
            new Vector3(1.0f, -1.0f, 1.0f),
            new Vector3(-1.0f, 0.0f, 1.0f),

            new Vector3(1.0f, 1.0f, -1.0f),
            new Vector3(1.0f, -1.0f, -1.0f),
            new Vector3(-1.0f, 0.0f, -1.0f),
        };

        mesh.triangles = new int[] {
            0, 1, 2,
            3, 4, 5,

            0, 1, 3,
            3, 1, 4,

            1, 2, 4,
            4, 2, 5,

            0, 2, 3,
            3, 2, 5,
        };
        ObjExporter.SaveObj("Assets/Mesh/Prim.obj", mesh);
        GetComponent<MeshFilter>().mesh = mesh;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
