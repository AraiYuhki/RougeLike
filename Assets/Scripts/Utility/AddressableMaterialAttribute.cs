using System;
using UnityEngine;
using UnityEngine.AddressableAssets;
using Cysharp.Threading.Tasks;
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
    public override async void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.LabelField(position, label);
        position.width -= EditorGUIUtility.labelWidth;
        EditorGUI.BeginChangeCheck();
        position.width *= 0.5f;
        position.x += EditorGUIUtility.labelWidth;
        property.stringValue = EditorGUI.TextField(position, property.stringValue);
        position.x += position.width;
        var material = await LoadMaterial(property.stringValue);
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

    private async UniTask<Material> LoadMaterial(string materialName)
    {
        try
        {
            return await Addressables.LoadAssetAsync<Material>(materialName);
        }
        catch
        {
            return null;
        }
        
    }
}
#endif
