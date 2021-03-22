using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEngine;

public class EditorJson : EditorWindow
{
    private static FileHelper fileHelper = new FileHelper();
    private static JsonSerializer serializer = new JsonSerializer();
    
    private JToken rootObject = null;
    private Dictionary<JToken, bool> foldState = new Dictionary<JToken, bool>();
    private Dictionary<JToken, string> addObjName = new Dictionary<JToken, string>();
    private Dictionary<JToken, int> addObjType = new Dictionary<JToken, int>();

    private bool showFileSettings = true;
    private bool showCredits = true;
    private bool showFileContents;
    private bool itemRemoved;
    private bool loadClicked;

    private int createType = 0;

    private string dataPath = null;

    private GUIStyle removeButtonStyle;
    private GUIStyle keyFieldStyle;
    private GUIStyle boldFoldoutStyle;
    private GUIStyle typeTextStyle;
    private GUIStyle textFieldStyle;
    private GUIStyle dropDownStyle;
    private GUIStyle btnAddStyle;

    private string[] valueTypes = new string[]
    {
        "Boolean", "Integer", "Double", "String", "JObject", "JArray"
    };

    private void LoadJSON(string path)
    {
        if (string.IsNullOrEmpty(path))
        {
            path = EditorUtility.OpenFilePanel("Open JSON File", "", "json");
        }

        if (string.IsNullOrEmpty(path))
        {
            return;
        }

        foldState.Clear();
        addObjName.Clear();
        addObjType.Clear();
        string text = fileHelper.Load(path);
        dataPath = path;

        try
        {
            rootObject = (JToken)JsonConvert.DeserializeObject(text);
            showCredits = false;
            showFileContents = true;
        }
        catch (JsonReaderException e)
        {
            dataPath = null;
            EditorUtility.DisplayDialog("Error!", "Invalid JSON Data!", "OK");
        }
    }

    private void SaveJSON()
    {
        fileHelper.Save(dataPath, rootObject.ToString());
    }

    [MenuItem("Window/JSON Editor")]
    public static void ShowWindow()
    {
        GetWindow(typeof(EditorJson));
    }
    
    private void Awake()
    {
        boldFoldoutStyle = new GUIStyle(EditorStyles.foldout)
        {
            fontStyle = FontStyle.Bold
        };
        removeButtonStyle = new GUIStyle(EditorStyles.miniButton)
        {
            stretchWidth = false,
            fontStyle = FontStyle.Bold
        };
        keyFieldStyle = new GUIStyle(EditorStyles.textField)
        {
            stretchWidth = false
        };
        typeTextStyle = new GUIStyle(EditorStyles.label)
        {
            alignment = TextAnchor.LowerRight
        };
        textFieldStyle = new GUIStyle(EditorStyles.textField)
        {
            stretchWidth = false
        };
        dropDownStyle = new GUIStyle(EditorStyles.popup)
        {
            stretchWidth = false
        };
        btnAddStyle = new GUIStyle(EditorStyles.miniButton)
        {
            alignment = TextAnchor.LowerLeft
        };
    }

    private void OnGUI()
    {
        SetVariableSettings();
        GUILayout.BeginVertical();

        showFileSettings = EditorGUILayout.Foldout(showFileSettings, "File Settings", boldFoldoutStyle);
        if (showFileSettings)
        {
            DisplayFileSettings();
        }            
        TryLoadTextAsset();

        showFileContents = EditorGUILayout.Foldout(showFileContents, "File Contents", boldFoldoutStyle);

        if (!showFileContents)
        {
            return;
        }

        if (rootObject == null)
        {
            GUILayout.Label("No JSON file opened", EditorStyles.largeLabel);
            return;
        }

        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Save"))
        {
            SaveJSON();
            return;
        }

        GUILayout.EndHorizontal();
        GUILayout.BeginVertical();
        DisplayRootObject();
        GUILayout.EndVertical();
    }

    private void SetVariableSettings()
    {
        keyFieldStyle.fixedWidth = Screen.width / 5f;
        textFieldStyle.fixedWidth = Screen.width * .6f;
        dropDownStyle.fixedWidth = Screen.width * .2f;
        itemRemoved = false;
    }

    private void DisplayRootObject()
    {
        GetAddRow(rootObject);

        if (rootObject.Type == JTokenType.Object)
        {
            JObject obj = (JObject)rootObject;
            foreach (JProperty token in obj.Properties())
            {
                DisplayItem(token.Value);
                if (itemRemoved)
                {
                    return;
                }
            }
        }

        if (rootObject.Type == JTokenType.Array)
        {
            JArray arr = (JArray)rootObject;
            foreach (JToken token in arr.Children())
            {
                DisplayItem(token);
                if (itemRemoved)
                {
                    return;
                }
            }
        }

        GUILayout.EndScrollView();
    }

    private void DisplayRowJObject(JToken objectToken)
    {
        BeginDisplay();
        GetRemoveButton(objectToken);
        if (itemRemoved)
        {
            return;
        }

        GetKey(objectToken);
        bool foldOut = GetObjectFoldLabel(objectToken);
        GUILayout.EndHorizontal();

        if (!foldOut)
        {
            return;
        }

        EditorGUI.indentLevel++;
        GetAddRow(objectToken);
        GUILayout.BeginVertical();

        foreach (JProperty token in ((JObject)(objectToken)).Properties())
        {
            DisplayItem(token.Value);
            if (itemRemoved)
            {
                return;
            }
        }

        EditorGUI.indentLevel--;
        GUILayout.EndVertical();
    }

    private void DisplayRowJValue(JToken objectToken)
    {
        BeginDisplay();
        GetRemoveButton(objectToken);
        if (itemRemoved)
        {
            return;
        }

        GetKey(objectToken);
        GetField(objectToken);
        GUILayout.EndHorizontal();
    }

    private void DisplayRowJArray(JToken objectToken)
    {
        BeginDisplay();
        GetRemoveButton(objectToken);
        if (itemRemoved)
        {
            return;
        }

        GetKey(objectToken);
        bool foldOut = GetObjectFoldLabel(objectToken);
        GUILayout.EndHorizontal();

        if (!foldOut)
        {
            return;
        }

        EditorGUI.indentLevel++;
        GetAddRow(objectToken);
        GUILayout.BeginVertical();
        foreach (JToken kid in ((JArray)(objectToken)).Children())
        {
            DisplayItem(kid);
            if (itemRemoved)
            {
                return;
            }
        }

        EditorGUI.indentLevel--;
        GUILayout.EndVertical();
    }

    private void BeginDisplay()
    {
        GUILayout.BeginHorizontal();
        GUILayout.Space(5 + EditorGUI.indentLevel * 15f);
    }

    private void DisplayItem(JToken token)
    {
        switch (token.Type)
        {
            case JTokenType.Object:
                DisplayRowJObject(token);
                break;
            case JTokenType.Array:
                DisplayRowJArray(token);
                break;
            default:
                DisplayRowJValue(token);
                break;
        }
    }

    private void GetAddRow(JToken parent)
    {
        BeginDisplay();
        string name = string.Empty;
        if (parent.Type == JTokenType.Object)
        {
            if (addObjName.ContainsKey(parent))
            {
                name = addObjName[parent];
            }
            else
            {
                addObjName.Add(parent, name);
            }

            name = GUILayout.TextField(name, keyFieldStyle);
            addObjName[parent] = name;
        }

        int selected = 0;
        if (addObjType.ContainsKey(parent))
        {
            selected = addObjType[parent];
        }
        else
        {
            addObjType.Add(parent, selected);
        }
        selected = EditorGUILayout.Popup(string.Empty, selected, valueTypes, dropDownStyle);
        addObjType[parent] = selected;
        if (GUILayout.Button("Add Object", btnAddStyle))
        {
            JToken val = new JArray();
            switch (selected)
            {
                case 0:
                    val = new bool();
                    break;
                case 1:
                    val = new int();
                    break;
                case 2:
                    val = new double();
                    break;
                case 3:
                    val = string.Empty;
                    break;
                case 4:
                    val = new JObject();
                    break;
            }
            if (parent.Type == JTokenType.Array)
            {
                JArray array = (JArray)parent;
                array.Add(val);
            }
            if (parent.Type == JTokenType.Object)
            {
                JObject obj = (JObject)parent;
                if (name.Equals(string.Empty) || name == null || obj[name] != null)
                {
                    return;
                }

                obj.Add(name, val);
                addObjName[parent] = string.Empty;
            }
        }

        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();
    }

    private void GetRemoveButton(JToken toRemove)
    {
        if (!GUILayout.Button("-", removeButtonStyle))
        {
            return;
        }

        if (toRemove.Parent.Type == JTokenType.Property)
        {
            ((JProperty)toRemove.Parent).Remove();
        }

        if (toRemove.Parent.Type == JTokenType.Array)
        {
            ((JArray)toRemove.Parent).Remove(toRemove);
        }

        itemRemoved = true;
    }

    private void GetKey(JToken token)
    {
        if (token.Parent.Type != JTokenType.Property)
        {
            return;
        }

        JProperty property = (JProperty)token.Parent;
        string key = GUILayout.TextField(property.Name, keyFieldStyle);

        if (key.Equals(property.Name))
        {
            return;
        }

        JProperty newToken = new JProperty(key, property.Value);
        try
        {
            property.Replace(newToken);
        }
        catch (ArgumentException)
        {
            Debug.LogError("Error when replacing token");
        }
    }

    private bool GetObjectFoldLabel(JToken objectToken)
    {
        bool foldOut = false;
        if (foldState.ContainsKey(objectToken))
        {
            foldOut = foldState[objectToken];
        }
        else
        {
            foldState.Add(objectToken, false);
        }

        int indentLevel = EditorGUI.indentLevel;
        EditorGUI.indentLevel = 0;
        string label = "JObject";
        if (objectToken.Type == JTokenType.Array)
        {
            label = "JArray";
        }
        if (objectToken.Parent.Type == JTokenType.Property)
        {
            label += " - " + ((JProperty)objectToken.Parent).Name;
        }

        foldOut = EditorGUILayout.Foldout(foldOut, label);
        EditorGUI.indentLevel = indentLevel;
        foldState[objectToken] = foldOut;
        return foldOut;
    }

    private void GetField(JToken obj)
    {
        int indentLevel = EditorGUI.indentLevel;
        EditorGUI.indentLevel = 0;
        var val = obj;
        switch (obj.Type)
        {
            case JTokenType.Integer:
                val = EditorGUILayout.IntField((int)obj, textFieldStyle);
                break;            
            case JTokenType.String:
                val = GUILayout.TextField((string)obj, textFieldStyle);
                break;
            case JTokenType.Boolean:
                val = GUILayout.Toggle((bool)obj, ((bool)obj ? "true" : "false"));
                break;
            case JTokenType.Float:
                val = EditorGUILayout.DoubleField((double)obj, textFieldStyle);
                break;
            default:
                break;
        }   

        if (obj.Type != JTokenType.Boolean)
        {
            GUILayout.Label(obj.Type.ToString(), typeTextStyle);
        }

        EditorGUI.indentLevel = indentLevel;

        if (val.Equals(obj))
        {
            return;
        }

        if (obj.Parent.Type == JTokenType.Property)
        {
            ((JProperty)obj.Parent).Value = val;
        }
        if (obj.Parent.Type == JTokenType.Array)
        {
            obj.Replace(val);
        }
    }

    private void ShowFileData(string name, string path)
    {
        GUILayout.BeginHorizontal();
        GUILayout.BeginVertical();
        GUILayout.Label("FileName: ");
        GUILayout.Label(name, EditorStyles.miniLabel);
        GUILayout.EndVertical();
        GUILayout.BeginVertical();
        GUILayout.Label("FilePath: ");
        GUILayout.Label(path, EditorStyles.miniLabel);
        GUILayout.EndVertical();
        GUILayout.EndHorizontal();
    }

    private void TryLoadTextAsset()
    {
        TextAsset asset = Selection.activeObject as TextAsset;

        if (asset == null)
        {
            return;
        }

        dataPath = AssetDatabase.GetAssetPath(asset);

        if (dataPath.Contains(".metadata") || dataPath.Contains(".txt"))
        {
            loadClicked = GUILayout.Button("Load");
        }

        if (loadClicked)
        {
            LoadJSON(dataPath);
            loadClicked = false;
            asset = null;
        }
    }

    private void DisplayFileSettings()
    {
        if (GUILayout.Button("Browse"))
        {
            LoadJSON(null);
        }
    }
}
