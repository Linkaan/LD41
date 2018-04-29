using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class WireframeTower : MonoBehaviour
{

    public Color lineColor;
    public Color backgroundColor;
    public bool ZWrite = true;
    public bool AWrite = true;
    public bool blend = true;

    public Color[] colors;

    public Shader backgroundShader;
    public Shader lineShader;

    private Vector3[] lines;
    private List<Color> colourArray;
    private ArrayList linesArray;
    private Material lineMaterial;
    private MeshRenderer meshRenderer;

    // Use this for initialization
    void Start()
    {
        GetComponent<Renderer>().enabled = false;
        meshRenderer = GetComponent<MeshRenderer>();
        meshRenderer.enabled = false;
        meshRenderer.material = new Material(backgroundShader);

        // Old Syntax without Bind :    
        //   lineMaterial = new Material("Shader \"Lines/Colored Blended\" { SubShader { Pass { Blend SrcAlpha OneMinusSrcAlpha ZWrite On Cull Front Fog { Mode Off } } } }"); 

        // New Syntax with Bind : 
        lineMaterial = new Material(lineShader);

        lineMaterial.hideFlags = HideFlags.HideAndDontSave;
        lineMaterial.shader.hideFlags = HideFlags.None;

        linesArray = new ArrayList();
        MeshFilter filter = GetComponent<MeshFilter>();
        Mesh mesh = filter.sharedMesh;
        Vector3[] vertices = mesh.vertices;

        colourArray = new List<Color>();
        List<int[]> inds = new List<int[]>();

        for (int i = 0; i < mesh.subMeshCount; i++) {
            inds.Add(mesh.GetIndices(i));
        }        
        
        int[] triangles = mesh.triangles;

        for (int i = 0; i < triangles.Length / 3; i++)
        {
            Color newColour = Color.black;
            for (int j = 0; j < mesh.subMeshCount; j++) {
                int[] ind = inds[j];
                int vertInd = triangles[i * 3];
                if (System.Array.IndexOf(ind, vertInd) >= 0) {
                    newColour = colors[j];
                    break;
                }
            }
            colourArray.Add(newColour);
            linesArray.Add(vertices[triangles[i * 3]]);
            linesArray.Add(vertices[triangles[i * 3 + 1]]);
            linesArray.Add(vertices[triangles[i * 3 + 2]]);
        }

        lines = new Vector3[triangles.Length];
        for (int i = 0; i < triangles.Length; i++)
        {
            lines[i] = (Vector3)linesArray[i];
        }
    }

    void OnRenderObject() {
        meshRenderer.sharedMaterial.color = backgroundColor;
        lineMaterial.SetPass(0);

        GL.PushMatrix();
        GL.MultMatrix(transform.localToWorldMatrix);
        GL.Begin(GL.LINES);
        //GL.Color(lineColor);

        for (int i = 0; i < lines.Length / 3; i++)
        {
            //int uv = (int) uvs[lines[i * 3].x].x;
            GL.Color(colourArray[i]);

            GL.Vertex(lines[i * 3]);
            GL.Vertex(lines[i * 3 + 1]);

            GL.Vertex(lines[i * 3 + 1]);
            GL.Vertex(lines[i * 3 + 2]);

            GL.Vertex(lines[i * 3 + 2]);
            GL.Vertex(lines[i * 3]);
        }

        GL.End();
        GL.PopMatrix();
    }
}