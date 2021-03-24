using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEngine;

public class EditorJson : EditorWindow
{
    private FileHelper fileHelper = new FileHelper();
    private JsonParser parser = new JsonParser();

    private bool showFileSettings = true;
    private bool showFileContents;
    private bool itemRemoved;
    private bool loadClicked;

    private string dataPath = null;

    private GUIStyle removeButtonStyle;
    private GUIStyle keyFieldStyle;
    private GUIStyle boldFoldoutStyle;
    private GUIStyle typeTextStyle;
    private GUIStyle textFieldStyle;
    private GUIStyle dropDownStyle;
    private GUIStyle btnAddStyle;

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

        if (parser.RootObject == null)
        {
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

    private void LoadJSON(string path)
    {
        if (string.IsNullOrEmpty(path))
        {
            path = EditorUtility.OpenFilePanel("Open JSON File", string.Empty, "json");
        }

        if (string.IsNullOrEmpty(path))
        {
            return;
        }
        
        string json = fileHelper.Load(path);
        dataPath = path;

        parser.DeserializeJson(json);
        if (parser.RootObject == null)
        {
            EditorUtility.DisplayDialog("Error!", "Invalid JSON Data!", "OK");
            return;
        }
        
        showFileContents = true;
    }

    private void SaveJSON()
    {
        fileHelper.Save(dataPath, parser.RootObject.ToString());
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
        GetAddRow(parser.RootObject);

        if (parser.RootObject.Type == JTokenType.Object)
        {
            JObject obj = (JObject)parser.RootObject;
            foreach (JProperty token in obj.Properties())
            {
                DisplayItem(token.Value);
                if (itemRemoved)
                {
                    return;
                }
            }
        }

        if (parser.RootObject.Type == JTokenType.Array)
        {
            JArray arr = (JArray)parser.RootObject;
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

        foreach (JProperty token in ((JObject)objectToken).Properties())
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
        foreach (JToken kid in ((JArray)objectToken).Children())
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
            name = parser.GetObjectName(parent);
            name = GUILayout.TextField(name, keyFieldStyle);
            parser.SetObjectName(parent, name);
        }

        int selected = parser.GetObjectType(parent);
        selected = EditorGUILayout.Popup(string.Empty, selected, parser.ValueTypes, dropDownStyle);
        parser.SetObjectType(parent, selected);
        
        if (GUILayout.Button("Add Object", btnAddStyle))
        {
            parser.AddNewObject(parent, selected, name);
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

        parser.RemoveObject(toRemove);

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

        parser.ReplaceProperty(key, property);
    }

    private bool GetObjectFoldLabel(JToken objectToken)
    {
        bool foldOut = parser.GetFoldOut(objectToken);

        int indentLevel = EditorGUI.indentLevel;
        EditorGUI.indentLevel = 0;
        string label = "JObject";
        if (objectToken.Type == JTokenType.Array)
        {
            label = "JArray";
        }
        if (objectToken.Parent.Type == JTokenType.Property)
        {
            label = $"{label} - {((JProperty) objectToken.Parent).Name}";
        }

        foldOut = EditorGUILayout.Foldout(foldOut, label);
        EditorGUI.indentLevel = indentLevel;
        parser.SetFoldOut(objectToken, foldOut);
        return foldOut;
    }

    private void GetField(JToken obj)
    {
        int indentLevel = EditorGUI.indentLevel;
        EditorGUI.indentLevel = 0;
        JToken token = obj;
        switch (obj.Type)
        {
            case JTokenType.Integer:
                token = EditorGUILayout.IntField((int)obj, textFieldStyle);
                break;            
            case JTokenType.String:
                token = GUILayout.TextField((string)obj, textFieldStyle);
                break;
            case JTokenType.Boolean:
                token = GUILayout.Toggle((bool)obj, (bool)obj ? "true" : "false");
                break;
            case JTokenType.Float:
                token = EditorGUILayout.DoubleField((double)obj, textFieldStyle);
                break;
        }   

        if (obj.Type != JTokenType.Boolean)
        {
            GUILayout.Label(obj.Type.ToString(), typeTextStyle);
        }

        EditorGUI.indentLevel = indentLevel;

        if (token.Equals(obj))
        {
            return;
        }

        parser.ReplaceObject(obj, token);
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