using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using System;

namespace Ubik.Rooms
{
    [CustomEditor(typeof(RoomClient))]
    public class RoomClientEditor : Editor
    {
        private ReorderableList managerReorderableList;
        private SerializedProperty serversProperty;

        private void OnEnable()
        {
            serversProperty = serializedObject.FindProperty("servers");
            managerReorderableList = new ReorderableList(serializedObject, serversProperty, false, false, true, true);
            managerReorderableList.onCanAddCallback += CanAddCallbackDelegate;
            managerReorderableList.drawElementCallback += DrawElementCallback;
            managerReorderableList.drawHeaderCallback += HeaderCallbackDelegate;
            managerReorderableList.elementHeightCallback += ElementHeightCallbackDelegate;
        }

        bool CanAddCallbackDelegate(ReorderableList list)
        {
            return list.serializedProperty.arraySize < 1;
        }

        void HeaderCallbackDelegate(Rect rect)
        {
            EditorGUI.LabelField(rect, "Default Server");
        }

        float ElementHeightCallbackDelegate(int index)
        {
            float propertyHeight = EditorGUI.GetPropertyHeight(serversProperty.GetArrayElementAtIndex(index), true);
            float spacing = EditorGUIUtility.singleLineHeight / 2;
            return propertyHeight + spacing;
        }

        private void DrawElementCallback(Rect rect, int index, bool isactive, bool isfocused)
        {
            rect.y += EditorGUIUtility.singleLineHeight / 4;
            EditorGUI.PropertyField(rect, serversProperty.GetArrayElementAtIndex(index), new GUIContent(""), true);
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            var component = target as RoomClient;

            managerReorderableList.DoLayoutList();

            EditorGUILayout.PropertyField(serializedObject.FindProperty("roomGuid"));

            if(component.joinedRoom)
            {
                EditorGUILayout.HelpBox("Joined Room " + component.room.guid, MessageType.Info);
            }

            GUI.enabled = false;

            EditorGUILayout.PropertyField(serializedObject.FindProperty("me"));

            GUI.enabled = true;

            serializedObject.ApplyModifiedProperties();


            if(GUILayout.Button("Join"))
            {
                component.Join();
            }
        }
    }
}