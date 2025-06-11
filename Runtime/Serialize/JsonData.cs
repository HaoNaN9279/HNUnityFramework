using System;
using System.ComponentModel;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Unity.VisualScripting;
using UnityEngine;
using System.Reflection;

namespace HN.Serialize
{
    [Serializable]
    public class JsonData : ISerializationCallbackReceiver
    {
        public string JsonText => jsonText;
        public JsonObject Obj
        {
            get
            {
                DeserializeFromString(jsonText);
                return obj;
            }
        }


        [SerializeField]
        private string jsonText = "";

        [SerializeField]
        private string objTypeName = "";

        [SerializeField]
        private string objAssemblyName = "";

        private JsonObject obj;


        public JsonData(Type nodeDataType)
        {
            obj = Activator.CreateInstance(nodeDataType) as JsonObject;
            if(obj == null)
                return;

            Type objType = obj.GetType();
            objTypeName = objType.FullName;
            objAssemblyName = objType.Assembly.FullName;
        }

        public void Serialize()
        {
            jsonText = SerializeToJson();
        }
        
        public void Deserialize()
        {
            DeserializeFromString(jsonText);
        }


        private string SerializeToJson()
        {
            if (obj == null)
                return "";

            if (string.IsNullOrEmpty(objTypeName))
                return "";

            Assembly assembly = Assembly.Load(objAssemblyName);
            Type type = assembly.GetType(objTypeName);
            // var o = Convert.ChangeType(obj, type);
            return Json.Serialize(obj);
        }

        private void DeserializeFromString(string jsonString)
        {
            if(string.IsNullOrEmpty(jsonString))
                return;

            if(string.IsNullOrEmpty(objTypeName))
                return;
            
            Assembly assembly = Assembly.Load(objAssemblyName);
            Type type = assembly.GetType(objTypeName);
            if (obj == null)
            {
                obj = Activator.CreateInstance(type) as JsonObject;
            }
            Json.DeserializeFromString(obj, jsonString);
        }


        public void OnBeforeSerialize()
        {
            jsonText = "";
            Serialize();
        }

        public void OnAfterDeserialize()
        {
            DeserializeFromString(jsonText);
        }
    }
}
