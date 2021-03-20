using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public class UnityJsonEditor : EditorWindow
{
    #region Variables
    private JToken rootObject = null;
    private Dictionary<JToken, bool> foldState = new Dictionary<JToken, bool>();
    private Dictionary<JToken, string> addObjName = new Dictionary<JToken, string>();
    private Dictionary<JToken, int> addObjType = new Dictionary<JToken, int>();
    private bool showFileSettings = true, showCredits = true, showFileContents, itemRemoved;
    private int createType = 0;
    private TextAsset asset = null;
    private string dataPath = null;
    private GUIStyle remButtonStyle, keyFieldStyle, boldFoldoutStyle, typeTextStyle, textFieldStyle;
    private GUIStyle dropDownStyle, btnAddStyle, dropDownCreateStyle;
    private GUIStyle largeLabelStyle, boldLabelStyle;
    private Vector2 scrollPosContent = Vector2.zero;
    private string[] valueTypes = new string[]
    {
        "Boolean", "Integer", "Double", "String", "JObject", "JArray"
    };
    #endregion

    #region JSONFile
    private void OpenJSON(string path)
    {
        Debug.Log($"Asset path: {path}");

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
        scrollPosContent = Vector2.zero;
        string text = File.ReadAllText(path);
        dataPath = path;
        asset = null;
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

    private void SaveJSON(bool overwrite)
    {
        File.WriteAllText(dataPath, rootObject.ToString());
    }
    #endregion

    #region UnityMethods
    [MenuItem("Tools/JSON Editor")]
    public static void ShowWindow()
    {
        GetWindow(typeof(UnityJsonEditor));
    }
    
    private void Awake()
    {
        boldFoldoutStyle = new GUIStyle(EditorStyles.foldout)
        {
            fontStyle = FontStyle.Bold
        };
        remButtonStyle = new GUIStyle(EditorStyles.miniButton)
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
        dropDownCreateStyle = new GUIStyle(EditorStyles.popup)
        {
            alignment = TextAnchor.LowerLeft,
            stretchWidth = false
        };
        largeLabelStyle = new GUIStyle(EditorStyles.largeLabel)
        {
            fontStyle = FontStyle.Bold,
            fontSize = 14
        };
        boldLabelStyle = new GUIStyle(EditorStyles.boldLabel);
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
            SaveJSON(true);
            return;
        }

        GUILayout.EndHorizontal();
        GUILayout.BeginVertical();
        DisplayRootObject();
        GUILayout.EndVertical();
    }

    #region Other
    private void SetVariableSettings()
    {
        keyFieldStyle.fixedWidth = Screen.width / 5f;
        textFieldStyle.fixedWidth = Screen.width * .6f;
        dropDownStyle.fixedWidth = Screen.width * .2f;
        itemRemoved = false;
    }
    #endregion
    #endregion

    #region DisplayObject
    private void DisplayRootObject()
    {
        GetAddRow(rootObject);
        scrollPosContent = GUILayout.BeginScrollView(scrollPosContent, new GUIStyle());

        if (rootObject.Type == JTokenType.Object)
        {
            JObject obj = (JObject)rootObject;
            foreach (JProperty token in obj.Properties())
            {
                DisplayItem(token.Value);
                if (itemRemoved)
                    return;
            }
        }

        if (rootObject.Type == JTokenType.Array)
        {
            JArray arr = (JArray)rootObject;
            foreach (JToken token in arr.Children())
            {
                DisplayItem(token);
                if (itemRemoved)
                    return;
            }
        }

        GUILayout.EndScrollView();
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

    private void DisplayRowJObject(JToken objectToken)
    {
        BeginDisplay();
        GetRemoveButton(objectToken);
        if (itemRemoved)
            return;
        GetKey(objectToken);
        bool foldOut = GetObjectFoldLabel(objectToken);
        GUILayout.EndHorizontal();
        if (foldOut)
        {
            EditorGUI.indentLevel++;
            GetAddRow(objectToken);
            GUILayout.BeginVertical();
            foreach (JProperty token in ((JObject)(objectToken)).Properties())
            {
                DisplayItem(token.Value);
                if (itemRemoved)
                    return;
            }
            EditorGUI.indentLevel--;
            GUILayout.EndVertical();
        }
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
            return;
        GetKey(objectToken);
        bool foldOut = GetObjectFoldLabel(objectToken);
        GUILayout.EndHorizontal();
        if (foldOut)
        {
            EditorGUI.indentLevel++;
            GetAddRow(objectToken);
            GUILayout.BeginVertical();
            foreach (JToken kid in ((JArray)(objectToken)).Children())
            {
                DisplayItem(kid);
                if (itemRemoved)
                    return;
            }
            EditorGUI.indentLevel--;
            GUILayout.EndVertical();
        }
    }

    private void BeginDisplay()
    {
        GUILayout.BeginHorizontal();
        GUILayout.Space(5 + EditorGUI.indentLevel * 15f);
    }

    #region RowParts
    private void GetAddRow(JToken parent)
    {
        BeginDisplay();
        string name = string.Empty;
        if (parent.Type == JTokenType.Object)
        {
            if (addObjName.ContainsKey(parent))
                name = addObjName[parent];
            else addObjName.Add(parent, name);
            name = GUILayout.TextField(name, keyFieldStyle);
            addObjName[parent] = name;
        }
        int selected = 0;
        if (addObjType.ContainsKey(parent))
            selected = addObjType[parent];
        else addObjType.Add(parent, selected);
        selected = EditorGUILayout.Popup("", selected, valueTypes, dropDownStyle);
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
                default:
                    throw new ArgumentException("Invalid Object Type");
            }
            if (parent.Type == JTokenType.Array)
            {
                JArray array = (JArray)parent;
                array.Add(val);
            }
            else if (parent.Type == JTokenType.Object)
            {
                JObject obj = (JObject)parent;
                if (name.Equals(string.Empty) || name == null || obj[name] != null)
                    return;
                obj.Add(name, val);
                addObjName[parent] = string.Empty;
            }
        }
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();
    }

    private void GetRemoveButton(JToken toRemove)
    {
        if (GUILayout.Button("-", remButtonStyle))
        {
            if (toRemove.Parent.Type == JTokenType.Property)
                ((JProperty)toRemove.Parent).Remove();
            else if (toRemove.Parent.Type == JTokenType.Array)
                ((JArray)toRemove.Parent).Remove(toRemove);
            itemRemoved = true;
        }
    }

    private void GetKey(JToken token)
    {
        if (token.Parent.Type != JTokenType.Property)
            return;
        JProperty property = (JProperty)(token.Parent);
        string key = GUILayout.TextField(property.Name, keyFieldStyle);
        if (!key.Equals(property.Name))
        {
            JProperty newToken = new JProperty(key, property.Value);
            try
            {
                property.Replace(newToken);
            }
            catch (ArgumentException) { }
        }
    }

    private bool GetObjectFoldLabel(JToken objectToken)
    {
        bool foldOut = false;
        if (foldState.ContainsKey(objectToken))
            foldOut = foldState[objectToken];
        else
            foldState.Add(objectToken, false);
        int i = EditorGUI.indentLevel;
        EditorGUI.indentLevel = 0;
        string label = "JObject";
        if (objectToken.Type == JTokenType.Array)
            label = "JArray";
        if (objectToken.Parent.Type == JTokenType.Property)
            label += " - " + ((JProperty)objectToken.Parent).Name;
        foldOut = EditorGUILayout.Foldout(foldOut, label);
        EditorGUI.indentLevel = i;
        foldState[objectToken] = foldOut;
        return foldOut;
    }

    private void GetField(JToken obj)
    {
        int i = EditorGUI.indentLevel;
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
            GUILayout.Label(obj.Type.ToString(), typeTextStyle);

        EditorGUI.indentLevel = i;

        if (val.Equals(obj))
        {
            return;
        }

        if (obj.Parent.Type == JTokenType.Property)
        {
            ((JProperty)obj.Parent).Value = val;
        }
        else if (obj.Parent.Type == JTokenType.Array)
        {
            obj.Replace(val);
        }
    }
    #endregion
    #endregion

    #region DisplayFile
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

    private void DisplayFileSettings()
    {
        if (GUILayout.Button("Browse"))
        {
            OpenJSON(null);
        }
            
        /*if (dataPath == null || dataPath.Length == 0)
        {
            if (asset != null)
                ShowFileData(asset.name, AssetDatabase.GetAssetPath(asset));
            asset = (TextAsset)EditorGUILayout.ObjectField(asset, typeof(TextAsset), false);
            if (asset == null)
                rootObject = null;
            else OpenJSON(AssetDatabase.GetAssetPath(asset));
            if (GUILayout.Button("Browse"))
                OpenJSON(null);
            GUILayout.BeginHorizontal();
            createType = EditorGUILayout.Popup(createType, new string[] { valueTypes[4], valueTypes[5] }, dropDownCreateStyle);
            GUILayout.EndHorizontal();
        }
        else
        {
            ShowFileData(Path.GetFileNameWithoutExtension(dataPath), dataPath);
            if (GUILayout.Button("Close JSON File"))
                dataPath = null;
        }*/
    }
    #endregion
}
