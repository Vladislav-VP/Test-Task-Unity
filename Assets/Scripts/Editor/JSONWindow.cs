using System;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEditor.UI;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Windows;

public class JSONWindow : EditorWindow
{
    private static JsonSerializer serializer = new JsonSerializer();

    [SerializeField] 
    private List<object> data;

    [SerializeField] 
    private ParsedData parsedData;
    
    private Editor editor;
    
    private string selectedFileName = String.Empty;

    private bool loadSelected;
    
    [MenuItem("Window/JSON")]
    private static void Init()
    {
        JSONWindow window = GetWindow<JSONWindow>();
        window.Show();
    }

    private void OnGUI()
    {
        if (!editor)
        {
            editor = Editor.CreateEditor(this);
        }

        if (GUILayout.Button("Browse"))
        {
            LoadJsonFile();
        }

        var ids = Selection.assetGUIDs;
        
        if (!string.IsNullOrEmpty(selectedFileName) &&
            (selectedFileName.Contains(".metadata") || selectedFileName.Contains(".txt")))
        {
            loadSelected = GUILayout.Button("Load");
        }

        if (loadSelected)
        {
            LoadJsonFile();
        }
    }
    
    private void LoadJsonFile()
    {
        string path = EditorUtility.OpenFilePanel("Select Json File", String.Empty, String.Empty);
        if (string.IsNullOrEmpty(path))
        {
            return;
        }

        string json = serializer.GetFromFile(path);
        var deserializedObject = serializer.Deserialize(json);

        if (deserializedObject == null)
        {
            EditorUtility.DisplayDialog("Error!", "Invalid JSON Data!", "OK");
            return;
        }

        serializer.ParseData(deserializedObject);
        parsedData = serializer.Root;
        editor.OnInspectorGUI();
    }
}

[CustomEditor(typeof(JSONWindow), true)]
public class JsonDrawer : Editor
{
    public override void OnInspectorGUI()
    {
        var data = serializedObject.FindProperty("parsedData");
        EditorGUILayout.PropertyField(data, new GUIContent("Json data"), true);
    }
}