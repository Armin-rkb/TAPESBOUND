using UnityEditor;
using UnityEngine;

[InitializeOnLoad]
class ObjectIdIcon
{
    static Texture2D texture;

    static ObjectIdIcon() 
    {
        texture = AssetDatabase.LoadAssetAtPath("Assets/Scripts/Utils/ObjectId/Editor/objectIdIcon.png", typeof(Texture2D)) as Texture2D;
        EditorApplication.hierarchyWindowItemOnGUI += HierarchyItemCB;
    }

    static void HierarchyItemCB(int instanceID, Rect selectionRect)
    {
        if (texture == null) { return; }

        GameObject go = EditorUtility.InstanceIDToObject(instanceID) as GameObject;
        if (go != null && go.GetComponent<ObjectId>())
        {
            Rect r = new Rect(selectionRect);
            r.x = r.x - 30;
            r.width = 16;

            GUI.DrawTexture(r, texture);
        }
    }
}