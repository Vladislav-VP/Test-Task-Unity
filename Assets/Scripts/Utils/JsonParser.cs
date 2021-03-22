using System;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

public class JsonParser
{
    public JToken RootObject { get; set; }

    public string[] ValueTypes => new string[]
    {
        "Boolean", "Integer", "Double", "String", "JObject", "JArray"
    };
    
    public JToken DeserializeJson(string json)
    {
        try
        {
            JToken root = (JToken)JsonConvert.DeserializeObject(json);
            return root;
        }
        catch (JsonReaderException e)
        {
            Debug.LogError("Invalid JSON Data!");
            return null;
        }
    }

    public JToken ParseValue(int selectedValue)
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
}
