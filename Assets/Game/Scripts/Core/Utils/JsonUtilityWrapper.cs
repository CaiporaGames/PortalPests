using System;
using System.Collections.Generic;
using UnityEngine;

public static class JsonUtilityWrapper
{    public static List<T> FromJsonList<T>(string jsonArray)
    {
        string wrapped = $"{{\"items\":{jsonArray}}}";
        Wrapper<T> w = JsonUtility.FromJson<Wrapper<T>>(wrapped);
        return new List<T>(w.items);
    }

    public static string ToJsonArray<T>(T[] arr, bool pretty = false)
    {
        var w = new Wrapper<T> { items = arr };
        string json = JsonUtility.ToJson(w, pretty);

        int start = json.IndexOf('[');
        int end = json.LastIndexOf(']');

        if (start < 0 || end < 0 || end <= start)
        {
            Debug.LogError("❌ JSON format error. Serialized output: " + json);
            return "[]"; // fallback to empty array
        }

        return json.Substring(start, end - start + 1);
    }


    [Serializable]
    private class Wrapper<T>
    {
        public T[] items;
    }
}
