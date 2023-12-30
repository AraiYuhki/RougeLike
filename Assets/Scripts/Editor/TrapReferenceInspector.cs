using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(TrapReferenceAttribute))]
public class TrapReferenceInspector : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        if (property.propertyType != SerializedPropertyType.Integer)
            throw new Exception("This attribute can't attach for this type isn't int type property or field.");
        var traps = DB.Instance.MTrap.All;
        var selectedIndex = traps.IndexOf(trap => trap.Id == property.intValue);
        EditorGUI.BeginChangeCheck();
        selectedIndex = EditorGUI.Popup(position, selectedIndex, traps.Select(trap => $"ID{trap.Id}:{trap.Name}").ToArray());
        if (EditorGUI.EndChangeCheck())
        {
            var newTrap = traps[selectedIndex];
            if (newTrap != null) property.intValue = newTrap.Id;
        }

    }
}
