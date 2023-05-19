using UnityEditor;
using UnityEngine;

namespace Core.Network {

    [CustomEditor(typeof(BurstConnectionOrchestrator))]
    public class BurstConnectionOrchestratorEditor : Editor {

        string command = "";

        public override void OnInspectorGUI() {
            command = EditorGUILayout.TextField("Map command", command);
            if(GUILayout.Button("Send")) {
                //var pkt = new CZ.REQUEST_CHAT(command);
                //pkt.Send();
            }
        }
    }
}
