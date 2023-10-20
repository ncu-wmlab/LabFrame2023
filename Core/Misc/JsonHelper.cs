using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LabFrame2023 {
    public static class JsonHelper
    {
        public static T[] FromJsonArray<T>(string json)
        {
            Wrapper<T> wrapper = JsonUtility.FromJson<Wrapper<T>>(json);
            return wrapper.data;
        }

        public static string ToJsonArray<T>(T[] array)
        {
            Wrapper<T> wrapper = new Wrapper<T>();
            wrapper.data = array;
            return JsonUtility.ToJson(wrapper);
        }

        public static string ToJsonArray<T>(T[] array, bool prettyPrint)
        {
            Wrapper<T> wrapper = new Wrapper<T>();
            wrapper.data = array;
            return JsonUtility.ToJson(wrapper, prettyPrint);
        }

        public static string FixJson(string value)
        {
            value = "{\"data\":" + value + "}";
            return value;
        }

        [Serializable]
        private class Wrapper<T>
        {
            public T[] data;
        }
    }
}



