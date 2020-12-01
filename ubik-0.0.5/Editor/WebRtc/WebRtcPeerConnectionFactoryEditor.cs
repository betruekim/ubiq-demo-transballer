using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;

namespace Ubik.WebRtc
{
    [CustomEditor(typeof(WebRtcPeerConnectionFactory))]
    public class WebRtcPeerConnectionFactoryEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            EditorGUILayout.HelpBox("The WebRtcPeerConnectionFactory will be created on demand, but creating one ahead of time means initialisation will be performed at start-up, instead of run-time.", MessageType.Info);

            var factory = target as WebRtcPeerConnectionFactory;
            if (factory.ready)
            {
                factory.playoutHelper.Select(EditorGUILayout.Popup("Playout Device", factory.playoutHelper.selected, factory.playoutHelper.names.ToArray()));
                factory.recordingHelper.Select(EditorGUILayout.Popup("Recording Device", factory.recordingHelper.selected, factory.recordingHelper.names.ToArray()));
            }
        }
    }
}