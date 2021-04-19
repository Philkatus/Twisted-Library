using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PathCreation;
using UnityEditor;

public class PathCreatorTool : MonoBehaviour
{
    #region PUBLIC

    #endregion

    #region PRIVATE
    [Tooltip("add all Path Creators you want to combine into one path into this List and then click COMBINE")]
    [SerializeField] private List<PathCreator> currentPath;

    private List<Vector3> currentPoints;
    private PathCreator currentLongPath;
    private List<PathCreator> newPath; //for Disconnect only
    private Dictionary<PathCreator, List<PathCreator>> longPaths;
    #endregion

    void Start()
    {

    }

    private IEnumerator Connect()
    {
        // Go through the List of all Current Paths
        // Add All Points To Current Points
        // Deactivate The Slides That are Part of the Long Path
        // Go Through the List Again, Cut Out All Doubles
        // Create Path & Object it is located on
        // Add to Dictionary
        Clear();
        yield return null;
    }
    private IEnumerator Disconnect()
    {
        // Go through the List of all Current Paths: Find the Long Path
        // Find the Long Path in the Dictionary and Add the list entries into "newPath"
        // Delete all Entries that are in current Path for new Path
        // Add All Points From New Path To Current Points
        // Cut Old Entry From Dictionary, Delete Long Path, Activate ALL Small Slides in Hierarchy Again
        // Deactivate The Slides That are Part of the Long Path
        // Go Through the List Again, Cut Out All Doubles
        // Create Path & Object it is located on
        // Add to Dictionary
        Clear();
        yield return null;
    }
    public void BeginCoroutine(string couroutineName) //Editor cannot call Coroutines, because it doesnt derive from Monobehavior
    {
        StartCoroutine(couroutineName);
    }
    private void Clear()
    {
        currentPath.Clear();
        currentPoints.Clear();
    }
}

[CustomEditor(typeof(PathCreatorTool), true)]
public class PathConnectorEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        PathCreatorTool pathCreatorTool = (PathCreatorTool)target;

        GUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Shape Paths");
        if (GUILayout.Button("Connect"))
        {
            pathCreatorTool.BeginCoroutine("Connect");
        }
        if (GUILayout.Button("Disconnect"))
        {
            pathCreatorTool.BeginCoroutine("Disconnect");
        }
        GUILayout.EndHorizontal();

        EditorUtility.SetDirty(target);
    }

}
