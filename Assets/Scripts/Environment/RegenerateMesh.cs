using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RegenerateMesh : MonoBehaviour
{
    Mesh mesh;
    void Start()
    {
        mesh = this.GetComponent<MeshCollider>().sharedMesh;
        mesh.RecalculateBounds();
    }
}
