using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Ubik.Matchmaking;

[CustomEditor(typeof(DiscoveryService))]
public class LocalDiscoveryEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        var component = target as DiscoveryService;

        switch (component.state)
        {
            case DiscoveryService.State.Searching:
                if (GUILayout.Button("Stop"))
                {
                    component.StopSearch();
                }
                break;

            case DiscoveryService.State.Waiting:
                if (GUILayout.Button("Start"))
                {
                    component.StartSearch();
                }
                break;
        }

        if (component.Responses != null)
        {
            foreach (var item in component.Responses)
            {
                EditorGUILayout.LabelField(item.Key);

                foreach (var uri in item.Value.uris)
                {
                    EditorGUILayout.LabelField(uri.ToString());
                }

                if (item.Value.uris.Count > 0)
                {
                    if (GUILayout.Button("Connect"))
                    {
                        component.Connect(item.Key);
                    }
                }
            }
        }
    }
}
