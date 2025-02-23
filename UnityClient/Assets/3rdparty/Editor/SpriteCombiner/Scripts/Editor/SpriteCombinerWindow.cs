using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace SpriteCombiner.EditorTools
{
    public sealed class SpriteCombinerWindow : EditorWindow
    {
        private const float
            MAX_ZOOM = 32f,
            MIN_ZOOM = 1f,
            PROP_PANEL_WIDTH = 220f,
            TOOLBAR_HEIGHT = 24;
        private const int
            BORDER_X = 15,
            BORDER_Y = 35;

        private readonly List<SpriteTextureData> spriteTextureData = new List<SpriteTextureData>();
        private SpriteTextureData selectedSpriteTextureData;
        private readonly TextureCombinerSettings textureCombinerSettings = new TextureCombinerSettings();
        private float zoomScale = 1f;
        private bool
            isDragging,
            showGrid = true,
            snapPositions = true;

        [MenuItem("Window/Sprite Combiner")]
        public static void OpenWindow()
        {
            _ = GetWindow<SpriteCombinerWindow>("Sprite Combiner");
        }

        private static SpriteTextureData GetSpriteTextureData(string path, Vector2 centerPos)
        {
            if (!string.IsNullOrEmpty(path))
            {
                string relativePath = path.StartsWith(Application.dataPath) ? "Assets" + path[Application.dataPath.Length..] : path;
                Texture2D newTex = AssetDatabase.LoadAssetAtPath<Texture2D>(relativePath);
                if (newTex != null)
                {
                    return new SpriteTextureData
                    (
                        newTex,
                        new Vector2(centerPos.x - (newTex.width / 2f), centerPos.y - (newTex.height / 2f)),
                        Vector2.one,
                        0
                    );
                }
            }

            return null;
        }

        private void OnGUI()
        {
            float
                previewRectWidth = position.width - (BORDER_X * 2f) - PROP_PANEL_WIDTH,
                previewRectHeight = position.height - BORDER_Y;

            Rect previewRect = new Rect
            (
                BORDER_X / 2f,
                (BORDER_Y / 2f) + (TOOLBAR_HEIGHT / 2f),
                previewRectWidth,
                previewRectHeight
            );

            List<string> toolbarOptions = new List<string>()
            {
                "Add Sprite",
                "Reset Sprites",
                $"{(snapPositions ? "Disable" : "Enable")} Position Snapping",
                $"{(showGrid ? "Hide" : "Show")} Pixel Grid"
            };

            int selectedToolbarIndex = GUI.Toolbar(new Rect(0, 0, position.width, TOOLBAR_HEIGHT), -1, toolbarOptions.ToArray());
            if (selectedToolbarIndex != -1) { ProcessToolbarAction(selectedToolbarIndex); }

            GUI.Box(previewRect, "", EditorStyles.helpBox);

            Matrix4x4 scaleMatrix = Matrix4x4.Scale(new Vector2(zoomScale, zoomScale));

            Event e = Event.current;
            if (previewRect.Contains(e.mousePosition))
            {
                switch (e.type)
                {
                    case EventType.MouseDown:
                        foreach (SpriteTextureData std in spriteTextureData)
                        {
                            Rect texRect = std.TextureRect;
                            if (GetScaledRect(texRect, scaleMatrix).Contains(e.mousePosition))
                            {
                                selectedSpriteTextureData = std;

                                break;
                            }
                        }

                        if (selectedSpriteTextureData != null)
                        {
                            isDragging = false;

                            Rect texRect = selectedSpriteTextureData.TextureRect;
                            Rect scaledTexRect = GetScaledRect(texRect, scaleMatrix);
                            if (scaledTexRect.Contains(e.mousePosition))
                            {
                                isDragging = true;
                            }
                            else
                            {
                                selectedSpriteTextureData = null;
                            }
                        }
                        e.Use();

                        break;
                    case EventType.MouseUp:
                        isDragging = false;
                        e.Use();

                        break;
                    case EventType.MouseDrag:
                        if (isDragging)
                        {
                            selectedSpriteTextureData.textureData.texturePosition = e.mousePosition / zoomScale;
                        }
                        e.Use();

                        break;

                // Drag and Drop sprites into window
                case EventType.DragUpdated:
                case EventType.DragPerform:
                    DragAndDrop.visualMode = DragAndDropVisualMode.Copy;

                    if (e.type == EventType.DragPerform)
                    {
                        DragAndDrop.AcceptDrag();

                        foreach (string path in DragAndDrop.paths)
                        {
                            SpriteTextureData std = GetSpriteTextureData
                            (
                                path,
                                new Vector2(previewRect.width / zoomScale / 2f, previewRectHeight / zoomScale / 2f)
                            );

                            if (std != null)
                            {
                                spriteTextureData.Add(std);
                            }
                        }

                        EditorUtility.SetDirty(this);
                    }
                    e.Use();

                    break;

                // Zoom scale of window
                case EventType.ScrollWheel:
                    zoomScale -= e.delta.y * 0.5f;
                    zoomScale = Mathf.Clamp(zoomScale, MIN_ZOOM, MAX_ZOOM);

                    EditorUtility.SetDirty(this);
                    e.Use();

                    break;
                }
            }

            if (showGrid && zoomScale >= MAX_ZOOM / 4f)
                DrawPreviewGrid();

            DrawSpritePreviews();

            DrawPropertyPanel();

            static Rect GetScaledRect(Rect rect, Matrix4x4 scaleMatrix)
            {
                Vector2
                    min = scaleMatrix.MultiplyPoint3x4(rect.min),
                    max = scaleMatrix.MultiplyPoint3x4(rect.max);

                return Rect.MinMaxRect
                (
                    min.x,
                    min.y,
                    max.x,
                    max.y
                );
            }

            void DrawPreviewGrid()
            {
                Handles.color = new Color(0.5f, 0.5f, 0.5f, 0.5f);

                for (int x = 0; x < Mathf.CeilToInt(previewRect.width / zoomScale); x++)
                {
                    Vector2
                        startPos = new Vector2(x * zoomScale, previewRect.y),
                        endPos = new Vector2(x * zoomScale, previewRect.yMax);

                    Handles.DrawLine(startPos, endPos);
                }

                for (int y = 0; y < Mathf.CeilToInt(previewRect.height / zoomScale); y++)
                {
                    Vector2
                        startPos = new Vector2(previewRect.x, y * zoomScale),
                        endPos = new Vector2(previewRect.xMax, y * zoomScale);

                    Handles.DrawLine(startPos, endPos);
                }
            }

            void DrawPropertyPanel()
            {
                GUILayout.BeginArea
                (
                    new Rect
                    (
                        position.width - PROP_PANEL_WIDTH - BORDER_X,
                        BORDER_Y,
                        PROP_PANEL_WIDTH,
                        position.height
                    )
                );
                EditorGUILayout.BeginVertical();

                if (spriteTextureData.Count > 1)
                {
                    GUILayout.Label("Combined Texture Settings", EditorStyles.whiteLargeLabel);
                    EditorGUI.BeginChangeCheck();
                    FilterMode filterMode = (FilterMode)EditorGUILayout.EnumPopup("Filter Mode:", textureCombinerSettings.filterMode);
                    TextureFormat textureFormat = (TextureFormat)EditorGUILayout.EnumPopup("Texture Format:", textureCombinerSettings.textureFormat);
                    TextureWrapMode wrapMode = (TextureWrapMode)EditorGUILayout.EnumPopup("Texture Wrap Mode:", textureCombinerSettings.wrapMode);
                    float alphaClipThreshold = EditorGUILayout.Slider("Alpha Clip Threshold:", textureCombinerSettings.alphaClipThreshold, 0, 1);
                    bool alphaBlend = EditorGUILayout.Toggle("Alpha Blend:", textureCombinerSettings.alphaBlend);
                    bool alphaIsTransparency = EditorGUILayout.Toggle("Alpha is Transparency:", textureCombinerSettings.alphaIsTransparency);
                    if (EditorGUI.EndChangeCheck())
                    {
                        textureCombinerSettings.filterMode = filterMode;
                        textureCombinerSettings.textureFormat = textureFormat;
                        textureCombinerSettings.wrapMode = wrapMode;
                        textureCombinerSettings.alphaClipThreshold = alphaClipThreshold;
                        textureCombinerSettings.alphaBlend = alphaBlend;
                        textureCombinerSettings.alphaIsTransparency = alphaIsTransparency;
                    }

                    EditorGUILayout.Space(15);

                    if (GUILayout.Button("Combine Sprites"))
                    {
                        CombineSprites();
                    }
                }

                if (selectedSpriteTextureData != null)
                {
                    EditorGUILayout.Space(32);
                    GUILayout.Label($"Selected Sprite: {selectedSpriteTextureData.textureData.texture.name}", new GUIStyle(EditorStyles.whiteLargeLabel) { wordWrap = true });

                    EditorGUILayout.Space(10f);

                    EditorGUI.BeginChangeCheck();
                    selectedSpriteTextureData.textureData.texture =
                        (Texture2D)EditorGUILayout.ObjectField("Texture:", selectedSpriteTextureData.textureData.texture, typeof(Texture2D), false);

                    selectedSpriteTextureData.textureData.texturePosition =
                        EditorGUILayout.Vector2Field("Position:", selectedSpriteTextureData.textureData.texturePosition);

                    selectedSpriteTextureData.textureData.textureScale =
                        EditorGUILayout.Vector2Field("Scale:", selectedSpriteTextureData.textureData.textureScale);

                    selectedSpriteTextureData.textureData.textureRotation =
                        EditorGUILayout.Slider("Rotation:", selectedSpriteTextureData.textureData.textureRotation, -180, 180);

                    selectedSpriteTextureData.sortingOrder =
                        EditorGUILayout.IntField("Sorting Order:", selectedSpriteTextureData.sortingOrder);

                    EditorGUILayout.Space(25);
                    if (GUILayout.Button("Remove Sprite"))
                    {
                        spriteTextureData.Remove(selectedSpriteTextureData);
                        selectedSpriteTextureData = null;
                    }
                    else if (EditorGUI.EndChangeCheck())
                    {
                        spriteTextureData.Sort((SpriteTextureData a, SpriteTextureData b) => a.sortingOrder - b.sortingOrder);
                        EditorUtility.SetDirty(this);
                    }
                }
                EditorGUILayout.EndVertical();
                GUILayout.EndArea();
            }

            void DrawSpritePreviews()
            {
                foreach (SpriteTextureData std in spriteTextureData)
                {
                    if (snapPositions)
                        std.textureData.texturePosition = Vector2Int.RoundToInt(std.textureData.texturePosition);

                    Rect texRect = std.TextureRect;

                    Rect scaledTexRect = GetScaledRect(texRect, scaleMatrix);

                    GUIUtility.RotateAroundPivot(std.textureData.textureRotation, scaledTexRect.center);

                    GUI.DrawTexture(scaledTexRect, std.textureData.texture, ScaleMode.StretchToFill);

                    if (std == selectedSpriteTextureData)
                    {
                        GUI.Box(scaledTexRect, "", EditorStyles.selectionRect);
                    }

                    if (!isDragging)
                        EditorGUIUtility.AddCursorRect(scaledTexRect, MouseCursor.Pan);

                    GUI.matrix = Matrix4x4.identity;
                }
            }

            void ProcessToolbarAction(int toolbarIndex)
            {
                switch (toolbarIndex)
                {
                    case 0:
                        AddSpriteTexture();

                        break;

                    case 1:
                        ResetSprites();

                        break;
                    case 2:
                        snapPositions = !snapPositions;

                        break;
                    case 3:
                        showGrid = !showGrid;

                        break;
                }

                void AddSpriteTexture()
                {
                    SpriteTextureData std = GetSpriteTextureData
                    (
                        EditorUtility.OpenFilePanel("Select Sprite", "", "png"),
                        new Vector2(previewRect.width / zoomScale / 2f, previewRectHeight / zoomScale / 2f)
                    );
                    if (std != null) { spriteTextureData.Add(std); }
                }

                void ResetSprites()
                {
                    selectedSpriteTextureData = null;

                    foreach (SpriteTextureData std in spriteTextureData)
                    {
                        TextureData td = std.textureData;
                        td.texturePosition = new Vector2(previewRect.width / zoomScale / 2f, previewRectHeight / zoomScale / 2f);
                        td.textureScale = Vector2.one;
                        td.textureRotation = 0;
                        std.sortingOrder = 0;
                    }
                }
            }
        }

        private void CombineSprites()
        {
            Rect combinedRect = default;
            for (int i = 0; i < spriteTextureData.Count; i++)
            {
                if (i == 0)
                    combinedRect = spriteTextureData[i].TextureRect;
                else
                    combinedRect = GetCombinedRect(combinedRect, spriteTextureData[i].TextureRect);
            }

            TextureData[] textureData = new TextureData[spriteTextureData.Count];
            for (int i = 0; i < spriteTextureData.Count; i++)
            {
                SpriteTextureData std = spriteTextureData[i];
                Rect texRect = std.TextureRect;

                Vector2 texPos = new Vector2(combinedRect.max.x - texRect.max.x, combinedRect.max.y - texRect.max.y);

                textureData[i] = new TextureData
                (
                    std.textureData.texture,
                    texPos,
                    std.textureData.textureScale,
                    std.textureData.textureRotation
                );
            }

            Texture2D combinedTexture = TextureCombiner.CombineTextures
            (
                textureData,
                (int)combinedRect.width,
                (int)combinedRect.height,
                textureCombinerSettings
            );

            string path = EditorUtility.SaveFilePanel("Save Texture", "", "CombinedSprite", "png");

            if (!string.IsNullOrEmpty(path))
            {
                SpriteCombinerSaveUtility.SaveTextureAsset(combinedTexture, path);
                AssetDatabase.Refresh();

                string relativePath = path.StartsWith(Application.dataPath) ? "Assets" + path[Application.dataPath.Length..] : path;
                TextureImporter ti = (TextureImporter)AssetImporter.GetAtPath(relativePath);
                if (ti != null)
                {
                    ti.isReadable = true;
                    ti.filterMode = textureCombinerSettings.filterMode;
                    ti.wrapMode = textureCombinerSettings.wrapMode;
                    ti.alphaIsTransparency = textureCombinerSettings.alphaIsTransparency;

                    EditorUtility.SetDirty(ti);
                    ti.SaveAndReimport();
                }

                Debug.Log($"SpriteCombiner: Saved combined sprite at {relativePath}!");
            }

            static Rect GetCombinedRect(Rect a, Rect b)
            {
                return Rect.MinMaxRect
                (
                    Mathf.Min(a.xMin, b.xMin),
                    Mathf.Min(a.yMin, b.yMin),
                    Mathf.Max(a.xMax, b.xMax),
                    Mathf.Max(a.yMax, b.yMax)
                );
            }
        }
    }
}
