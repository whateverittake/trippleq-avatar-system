#if UNITY_EDITOR
using TrippleQ.AvatarSystem;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(AvatarDatabaseSO))]
public class AvatarDatabaseSOEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        GUILayout.Space(8);
        if (GUILayout.Button("Auto-Assign Icons"))
        {
            AutoAssignIcons((AvatarDatabaseSO)target);
        }
    }

    private static void AutoAssignIcons(AvatarDatabaseSO db)
    {
        // Find resolver in project (you can also store a direct reference in db if you want)
        var resolverGuids = AssetDatabase.FindAssets("t:AvatarIconResolverSO");
        AvatarIconResolverSO resolver = null;
        if (resolverGuids.Length > 0)
        {
            var path = AssetDatabase.GUIDToAssetPath(resolverGuids[0]);
            resolver = AssetDatabase.LoadAssetAtPath<AvatarIconResolverSO>(path);
        }

        if (resolver == null)
        {
            Debug.LogError("AvatarIconResolverSO not found. Create one via Create/Studio/Avatar System/Icon Resolver");
            return;
        }

        var folder = resolver.iconsFolder.TrimEnd('/');
        if (string.IsNullOrWhiteSpace(folder))
        {
            Debug.LogError("Resolver iconsFolder is empty.");
            return;
        }

        int assigned = 0;

        foreach (var a in db.avatars)
        {
            if (a == null) continue;
            if (string.IsNullOrWhiteSpace(a.iconKey)) continue;

            // If already assigned, skip (optional)
            if (a.icon != null) continue;

            Sprite found = null;

            // Try direct file match by name within folder
            // Option 1: find assets by name (more flexible)
            var spriteGuids = AssetDatabase.FindAssets($"{a.iconKey} t:Sprite", new[] { folder });
            if (spriteGuids.Length > 0)
            {
                var spPath = AssetDatabase.GUIDToAssetPath(spriteGuids[0]);
                found = AssetDatabase.LoadAssetAtPath<Sprite>(spPath);
            }

            if (found != null)
            {
                a.icon = found;
                assigned++;
            }
            else
            {
                Debug.LogWarning($"Icon not found for iconKey='{a.iconKey}' in '{folder}'");
            }
        }

        EditorUtility.SetDirty(db);
        AssetDatabase.SaveAssets();
        Debug.Log($"Auto-Assign Icons done. Assigned {assigned} icons.");
    }
}
#endif
