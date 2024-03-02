using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

[CustomEditor(typeof(ObjectId))]
public class ObjectIdEditor : Editor 
{
    private ObjectId objectId;

    private void Awake() 
    {
        objectId = (ObjectId)target;
    }

    public override void OnInspectorGUI() 
    {
        DrawDefaultInspector();

        if (GUILayout.Button("Copy Id"))
        {
            EditorGUIUtility.systemCopyBuffer = objectId.Id;
        }

        if (GUILayout.Button("Generate Id")) 
        {
            objectId.GenerateId();

            EditorUtility.SetDirty(objectId);
            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        }
    }
}
