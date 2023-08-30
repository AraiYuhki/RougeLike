using System;
using UnityEngine;
using UnityEngine.AddressableAssets;
#if UNITY_EDITOR
using UnityEditor.AddressableAssets;
using UnityEditor;
#endif
[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = true, Inherited = false)]
public class AddressableMaterialAttribute : PropertyAttribute
{
}

#if UNITY_EDITOR
[CustomPropertyDrawer(typeof(AddressableMaterialAttribute))]
public class AddressableMaterialAttributeDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.LabelField(position, label);
        position.width -= EditorGUIUtility.labelWidth;
        position.width *= 0.5f;
        position.x += EditorGUIUtility.labelWidth;
        property.stringValue = EditorGUI.TextField(position, property.stringValue);
        position.x += position.width;
        var material = LoadMaterial(property.stringValue);
        EditorGUI.BeginChangeCheck();
        var newMaterial = EditorGUI.ObjectField(position, material, typeof(Material), false) as Material;
        if (newMaterial != null && newMaterial != material)
        {
            if (material != null) Addressables.Release(material);
            material = newMaterial;
            var assetPath = AssetDatabase.GetAssetPath(material);
            var guid = AssetDatabase.AssetPathToGUID(assetPath);
            var entry = AddressableAssetSettingsDefaultObject.Settings.FindAssetEntry(guid);
            property.stringValue = entry.address;
        }
        if (material != null)
        {
            Addressables.Release(material);
        }
    }

    private Material LoadMaterial(string materialName)
    {
        try
        {
            return Addressables.LoadAssetAsync<Material>(materialName).WaitForCompletion();
        }
        catch
        {
            return null;
        }
        
    }
}
#endif
