using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(PlayerFollowTarget))]
[CanEditMultipleObjects]
public class AssignCameraVars : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        PlayerFollowTarget follow = (PlayerFollowTarget)target;
        if (GUILayout.Button("Assign"))
        {
            follow.AssignAllVars();
        }
    }
}

[CustomEditor(typeof(CameraController))]
[CanEditMultipleObjects]
public class CameraAssign : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        CameraController follow = (CameraController)target;
        if (GUILayout.Button("Assign"))
        {
            follow.AssignAllVars();
        }
    }
}
