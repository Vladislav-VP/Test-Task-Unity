
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using UnityEngine;

public class JsonSerializer
{
    public ParsedData Root { get; private set; } = new ParsedData();

    public string GetFromFile(string path)
    {
        string json;
        using (var stream = new FileStream(path, FileMode.OpenOrCreate, FileAccess.ReadWrite))
        {
            using (var reader = new StreamReader(stream))
            {
                json = reader.ReadToEnd();
            }
        }

        return json;
    }
    
    public JToken Deserialize(string json)
    {
        try
        {
            JToken jsonObject = (JToken)JsonConvert.DeserializeObject(json);
            return jsonObject;
        }
        catch (JsonReaderException e)
        {
            Debug.LogError("Invalid JSON data");
            return null;
        }
    }

    public void ParseData(JToken token)
    {
        Root.DataDictionary.Clear();
        Root.FillData(token);
    }
}
