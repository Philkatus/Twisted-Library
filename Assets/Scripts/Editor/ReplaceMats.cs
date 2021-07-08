using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;

public class ReplaceMats : EditorWindow
{
    [SerializeField] private Material fromMat;
    [SerializeField] private Material toMat;
    //[SerializeField] private List<GameObject> prefabsToChange = new List<GameObject>();

    [MenuItem("Tools/Replace Mats")]
    static void CreateReplaceWithMats()
    {
        EditorWindow.GetWindow<ReplaceMats>();
    }

    private void OnGUI()
    {
        /*
        ScriptableObject target = this;
        SerializedObject so = new SerializedObject(target);
        SerializedProperty stringsProperty = so.FindProperty("prefabsToChange");
        */
        fromMat = (Material)EditorGUILayout.ObjectField("From Material", fromMat, typeof(Material), false);
        toMat = (Material)EditorGUILayout.ObjectField("To Material", toMat, typeof(Material), false);
        //EditorGUILayout.PropertyField(stringsProperty, true);
        //so.ApplyModifiedProperties();

        if (GUILayout.Button("DO NOT CLICK THIS BUTTON, UNDER ANY CIRCUMSTANCES"))
        {
            Debug.Log("DO NOT CLICK THIS BUTTON, UNDER ANY CIRCUMSTANCES");
            int meshIndex = 0;
            MeshRenderer[] allMeshRenderers = FindObjectsOfType<MeshRenderer>();
            List<int> meshIndices = new List<int>();
            List<int> matIndices = new List<int>();

            foreach (MeshRenderer mr in allMeshRenderers)
            {
                List<int> allChangedIndices = new List<int>();
                int index = 0;
                foreach (Material m in mr.sharedMaterials)
                {
                    if (m.name == "Yellow")
                    {
                        meshIndices.Add(meshIndex);
                        matIndices.Add(index);
                    }
                    index++;
                }
                meshIndex++;
            }
            for (int i = 0; i < matIndices.Count; i++)
            {
                int j = meshIndices[i];
                int k = matIndices[i];
                allMeshRenderers[j].material = toMat;
            }
            GUI.enabled = false;
        }
        /*if (GUILayout.Button("Replace All Selected Prefabs"))
        {
            foreach (GameObject prefab in prefabsToChange)
            {
                MeshRenderer[] allMeshRenderers = prefab.GetComponentsInChildren<MeshRenderer>();
                foreach (MeshRenderer mr in allMeshRenderers)
                {
                    List<int> allChangedIndices = new List<int>();
                    int index = 0;
                    foreach (Material m in mr.sharedMaterials)
                    {
                        if (m == fromMat)
                        {
                            allChangedIndices.Add(index);
                        }
                        index++;
                    }
                    foreach (int i in allChangedIndices)
                    {
                        mr.sharedMaterials[i] = toMat;

                    }

                }
            }

        }
        GUI.enabled = false;
    }*/
    }
}
