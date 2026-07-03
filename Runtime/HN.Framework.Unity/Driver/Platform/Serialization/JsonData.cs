using System;
using System.ComponentModel;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
using System.Reflection;
using HN.Framework.Core.Driver.Common.Serialization;

namespace HN.Framework.Unity.Driver.Platform.Serialization
{
    [Serializable]
    public class JsonData : ISerializationCallbackReceiver
    {
        /// <summary>
        /// 获取序列化后的 JSON 字符串。
        /// </summary>
        public string JsonText => jsonText;
        /// <summary>
        /// 获取反序列化后的 JsonObject 对象。
        /// </summary>
        public JsonObject Obj
        {
            get
            {
                if (jsonText != cachedJsonText)
                {
                    DeserializeFromString(jsonText);
                    cachedJsonText = jsonText ?? string.Empty;
                }
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

        private string cachedJsonText;

        /// <summary>
        /// 创建 JsonData 实例，内部创建指定类型的 JsonObject。
        /// </summary>
        /// <param name="nodeDataType">继承 JsonObject 的类型。</param>
        public JsonData(Type nodeDataType)
        {
            obj = Activator.CreateInstance(nodeDataType) as JsonObject;
            if(obj == null)
                return;

            Type objType = obj.GetType();
            objTypeName = objType.FullName;
            objAssemblyName = objType.Assembly.FullName;
        }

        /// <summary>
        /// 将 <see cref="Obj"/> 序列化为 JSON 字符串并存入 <see cref="JsonText"/>。
        /// </summary>
        public void Serialize()
        {
            jsonText = SerializeToJson();
            cachedJsonText = jsonText ?? string.Empty;
        }
        
        /// <summary>
        /// 从 <see cref="JsonText"/> 反序列化并更新 <see cref="Obj"/>。
        /// </summary>
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
            Serialize();
        }

        public void OnAfterDeserialize()
        {
            cachedJsonText = null; // Force re-parse on next access
            DeserializeFromString(jsonText);
        }
    }
}
