using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using UnityEngine;

[Serializable]
public class ParsedData
{
    public Dictionary<string, object> DataDictionary = new Dictionary<string, object>();

    public object Value;
    public JTokenType Type;
    
    public ParsedData Parent;
    public List<ParsedData> Children = new List<ParsedData>();

    public void FillData(JToken token)
    {

       /* Value = token.Value<object>();*/
        var type = token.Type;
        string tokenString = token.ToString();
        ParseValue(token);
        
       
        var tokenChildren = token.Children();

        foreach (var tokenChild in tokenChildren)
        {
            DataDictionary.Add(tokenChild.Path, tokenChild.Values());
            var child = new ParsedData
            {
                Parent = this
            };
            Children.Add(child);
            child.FillData(tokenChild);
        }
    }

    private void ParseValue(JToken token)
    {
        Type = token.Type;

        switch (Type)
        {
            case JTokenType.Object:
                Value = token.Value<object>();
                break;
            case JTokenType.String:
                Value = token.Value<string>();
                break;
            case JTokenType.Integer:
                Value = token.Value<int>();
                break;
            case JTokenType.Uri:
                Value = token.Value<string>();
                break;
            case JTokenType.Property:
                Value = token.Path;
                break;
            case JTokenType.Array:
                JArray array = token.Value<JArray>();
                Value = array.ToList();
                break;
            default:
                break;
        }
    }
}
