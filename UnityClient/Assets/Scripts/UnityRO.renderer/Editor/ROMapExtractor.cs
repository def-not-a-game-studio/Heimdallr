using ROIO;
using ROIO.Loaders;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

public class ROMapExtractor : EditorWindow {

    private string mapName = "prontera";

    private string grfRootPath = "C:/foo";
    private List<string> grfPaths = new List<string>();
    private ReorderableList GrfReordableList;

    [MenuItem("Window/ROMapExtractor")]
    public static void ShowWindow() {
        EditorWindow.GetWindow(typeof(ROMapExtractor));
    }

    async void ExtractMap() {
        AsyncMapLoader.GameMapData gameMapData = await new AsyncMapLoader().Load($"{mapName}.rsw");

        var gameMap = new GameObject(mapName).AddComponent<GameMap>();
        gameMap.tag = "Map";
        gameMap.SetMapSize((int) gameMapData.Ground.width, (int) gameMapData.Ground.height);
        gameMap.SetMapAltitude(new Altitude(gameMapData.Altitude));

        var ground = new Ground(gameMapData.CompiledGround, gameMapData.World.water);

        for(int i = 0; i < gameMapData.World.sounds.Count; i++) {
            gameMapData.World.sounds[i].pos[0] += gameMap.Size.x;
            gameMapData.World.sounds[i].pos[1] *= -1;
            gameMapData.World.sounds[i].pos[2] += gameMap.Size.y;
            //world.sounds[i].pos[2] = tmp;
            gameMapData.World.sounds[i].range *= 0.3f;
            gameMapData.World.sounds[i].tick = 0;
        }

        await new Models(gameMapData.CompiledModels.ToList()).BuildMeshesAsync(null, true, gameMap.Size);
    }

    private void OnEnable() {
        GrfReordableList = new ReorderableList(grfPaths, typeof(string));
        GrfReordableList.drawHeaderCallback = (rect) => EditorGUI.LabelField(rect, "GRF List");
        GrfReordableList.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) => {
            rect.y += 2f;
            rect.height = EditorGUIUtility.singleLineHeight;

            GrfReordableList.list[index] = EditorGUI.TextField(rect, (string) GrfReordableList.list[index]);
        };
    }

    private void OnGUI() {
        GUILayout.Space(8);
        GUILayout.Label("GRF Settings", EditorStyles.boldLabel);
        grfRootPath = EditorGUILayout.TextField("GRF Root Path", grfRootPath);
        GUILayout.Space(8);
        GrfReordableList.DoLayoutList();

        if(GUILayout.Button("Load GRF")) {
            FileManager.LoadGRF(grfRootPath, grfPaths.Where(it => it.Length > 0).ToList());
        }
        GUILayout.Space(16);
        GUILayout.Label("Base Settings", EditorStyles.boldLabel);
        mapName = EditorGUILayout.TextField("Map name", mapName);
        if(GUILayout.Button("Load Map")) {
            ExtractMap();
        }
    }

    private void OnInspectorUpdate() {
        Repaint();
    }
}
