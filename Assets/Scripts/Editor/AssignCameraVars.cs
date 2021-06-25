using UnityEditor;
using UnityEngine;

//[CustomEditor(typeof(PlayerFollowTarget))]
//[CanEditMultipleObjects]
public class AssignCameraVars : Editor
{
    // SerializedProperty camera;
    // SerializedProperty playerTarget;
    // SerializedProperty ladderTarget;
    // SerializedProperty damping;
    // SerializedProperty playerSM;
    // SerializedProperty environmentLayer;
//     SerializedObject obj;
//     PlayerFollowTarget follow;
//     void OnEnable()
//     {
//         follow = (PlayerFollowTarget)target;
//         obj = new SerializedObject(follow);
//     }
//     public override void OnInspectorGUI()
//     {
//         DrawDefaultInspector();

//         if (GUILayout.Button("Assign"))
//         {
//             follow.AssignAllVars();
//             // camera = serializedObject.FindProperty("Camera");
//             // playerTarget = serializedObject.FindProperty("PlayerTarget");
//             // ladderTarget = serializedObject.FindProperty("LadderTarget");
//             // damping = serializedObject.FindProperty("Damping");
//             // playerSM = serializedObject.FindProperty("PlayerSM");
//             // environmentLayer = serializedObject.FindProperty("EnvironmentLayer");

//             obj.Update();
//         }
//         serializedObject.ApplyModifiedProperties();

//     }
// }

// [CustomEditor(typeof(CameraController))]
// [CanEditMultipleObjects]
// public class CameraAssign : Editor
// {
//     public override void OnInspectorGUI()
//     {
//         DrawDefaultInspector();

//         CameraController follow = (CameraController)target;
//         if (GUILayout.Button("Assign"))
//         {
//             follow.AssignAllVars();
//         }
//     }
}
