using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

public class JsonParser
{
    public JToken RootObject { get; private set; }

    private Dictionary<JToken, bool> foldState  = new Dictionary<JToken, bool>();
    private Dictionary<JToken, string> addObjName = new Dictionary<JToken, string>();
    private Dictionary<JToken, int> addObjType = new Dictionary<JToken, int>();
    
    public string[] ValueTypes => new string[]
    {
        "Boolean", "Integer", "Double", "String", "JObject", "JArray"
    };
    
    public void DeserializeJson(string json)
    {
        ClearConfigDictionaries();
        
        try
        {
            RootObject = (JToken)JsonConvert.DeserializeObject(json);
        }
        catch (JsonReaderException e)
        {
            Debug.LogError("Invalid JSON Data!");
            RootObject = null;
        }
    }

    public void AddNewObject(JToken token, int selectedValue, string name)
    {
        JToken child = ParseValue(selectedValue);
        if (token.Type == JTokenType.Array)
        {
            JArray array = (JArray)token;
            array.Add(child);
        }
        if (token.Type == JTokenType.Object)
        {
            JObject obj = (JObject)token;
            if (string.IsNullOrEmpty(name) || obj[name] != null)
            {
                return;
            }

            obj.Add(name, child);
            SetObjectName(token,string.Empty);
        }
    }

    public void RemoveObject(JToken toRemove)
    {
        if (toRemove.Parent.Type == JTokenType.Property)
        {
            ((JProperty)toRemove.Parent).Remove();
        }

        if (toRemove.Parent.Type == JTokenType.Array)
        {
            ((JArray)toRemove.Parent).Remove(toRemove);
        }
    }

    public void ReplaceObject(JToken oldValue, JToken newValue)
    {
        if (oldValue.Parent.Type == JTokenType.Property)
        {
            ((JProperty)oldValue.Parent).Value = newValue;
        }
        if (oldValue.Parent.Type == JTokenType.Array)
        {
            oldValue.Replace(newValue);
        }
    }
    
    public void ReplaceProperty(string key, JProperty property)
    {
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
    
    public bool GetFoldOut(JToken token)
    {
        if (foldState.ContainsKey(token))
        {
            return foldState[token];
        }
        else
        {
            foldState.Add(token, false);
            return false;
        }
    }

    public void SetFoldOut(JToken token, bool foldOut)
    {
        foldState[token] = foldOut;
    }

    public string GetObjectName(JToken token)
    {
        if (addObjName.ContainsKey(token))
        {
            return addObjName[token];
        }
        else
        {
            addObjName.Add(token, string.Empty);
            return string.Empty;
        }
    }

    public void SetObjectName(JToken token, string name)
    {
        addObjName[token] = name;
    }

    public int GetObjectType(JToken token)
    {
        if (addObjType.ContainsKey(token))
        {
            return addObjType[token];
        }
        else
        {
            addObjType.Add(token, 0);
            return 0;
        }
    }

    public void SetObjectType(JToken token, int type)
    {
        addObjType[token] = type;
    }
    
    private JToken ParseValue(int selectedValue)
    {
        JToken token = new JArray();
        switch (selectedValue)
        {
            case 0:
                token = new bool();
                break;
            case 1:
                token = new int();
                break;
            case 2:
                token = new double();
                break;
            case 3:
                token = string.Empty;
                break;
            case 4:
                token = new JObject();
                break;
        }

        return token;
    }
    
    private void ClearConfigDictionaries()
    {
        foldState.Clear();
        addObjName.Clear();
        addObjType.Clear();
    }
}