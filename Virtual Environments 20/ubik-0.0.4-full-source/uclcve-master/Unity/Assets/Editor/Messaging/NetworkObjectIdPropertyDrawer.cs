using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Ubik.Messaging;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using System;

[CustomPropertyDrawer(typeof(NetworkObjectId))]
public class NetworkObjectIdPropertyDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        var id = property.FindPropertyRelative("id");
        var type = property.FindPropertyRelative("type");

        var isAuto = ((NetworkObjectId.Type[])Enum.GetValues(typeof(NetworkObjectId.Type)))[type.enumValueIndex] == NetworkObjectId.Type.Auto;

        EditorGUI.BeginProperty(position, label, property);

        label = new GUIContent("Network Object Id");

        var labelRect = position;
        labelRect.size = GUI.skin.label.CalcSize(label);
        EditorGUI.LabelField(labelRect, label);

        var controlRect = labelRect;
        controlRect.x += EditorGUIUtility.labelWidth;
        controlRect.width = 100;

        EditorGUI.PropertyField(controlRect, type, GUIContent.none);

        controlRect.x += controlRect.width;
        controlRect.width = position.width - (controlRect.x);

        if(isAuto)
        {
            GUI.enabled = false;
        }

        EditorGUI.PropertyField(controlRect, id, GUIContent.none);

        GUI.enabled = true;

        EditorGUI.EndProperty();
    }
}
