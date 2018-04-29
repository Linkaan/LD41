using UnityEngine;
using System.Collections;

public class Wireframe : MonoBehaviour
{

    public Color lineColor;
    public Color backgroundColor;
    public bool ZWrite = true;
    public bool AWrite = true;
    public bool blend = true;

    public Transform terrainTransform;
    public MeshRenderer meshRenderer;
    public MeshFilter filter;

    public Shader backgroundShader;
    public Shader lineShader;

    private Vector3[] lines;
    private ArrayList linesArray;
    private Material lineMaterial;

    // Use this for initialization
    void Start()
    {
        //GetComponent<Renderer>().enabled = false;
        //meshRenderer = GetComponent<MeshRenderer>();
        meshRenderer.enabled = false;
        meshRenderer.material = new Material(backgroundShader);

        // Old Syntax without Bind :    
        //   lineMaterial = new Material("Shader \"Lines/Colored Blended\" { SubShader { Pass { Blend SrcAlpha OneMinusSrcAlpha ZWrite On Cull Front Fog { Mode Off } } } }"); 

        // New Syntax with Bind : 
        lineMaterial = new Material(lineShader);

        lineMaterial.hideFlags = HideFlags.HideAndDontSave;
        lineMaterial.shader.hideFlags = HideFlags.None;

        linesArray = new ArrayList();
        //MeshFilter filter = GetComponent<MeshFilter>();
        Mesh mesh = filter.sharedMesh;
        Vector3[] vertices = mesh.vertices;
        int[] triangles = mesh.triangles;

        for (int i = 0; i < triangles.Length / 3; i++)
        {
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

    void OnPostRender()
    {
        meshRenderer.sharedMaterial.color = backgroundColor;
        lineMaterial.SetPass(0);

        GL.PushMatrix();
        GL.MultMatrix(terrainTransform.localToWorldMatrix);
        GL.Begin(GL.LINES);
        GL.Color(lineColor);

        for (int i = 0; i < lines.Length / 3; i++)
        {
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