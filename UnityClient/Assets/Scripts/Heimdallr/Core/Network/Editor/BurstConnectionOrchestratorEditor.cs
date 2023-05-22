using UnityEditor;
using UnityEngine;

namespace Core.Network {

    [CustomEditor(typeof(BurstConnectionOrchestrator))]
    public class BurstConnectionOrchestratorEditor : Editor {

        string command = "";

        public override void OnInspectorGUI() {
            var component = (BurstConnectionOrchestrator)target;
            base.OnInspectorGUI();
            
            command = EditorGUILayout.TextField("Map command", command);
            if(GUILayout.Button("Send")) {
                component.SendCommand(command);
            }
        }
    }
}
