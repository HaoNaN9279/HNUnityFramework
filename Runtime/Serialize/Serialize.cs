using System;
using System.IO;
using System.Text;
using UnityEngine;

namespace HN.Serialize
{
    public class Json
    {
        /// <summary>
        /// 将字符串内容写入指定路径的文件。如果文件存在则覆盖，不存在则创建。
        /// </summary>
        /// <param name="path">文件路径。</param>
        /// <param name="text">要写入的文本内容。</param>
        /// <returns>写入成功返回 true，路径为空返回 false。</returns>
        public static bool WriteToDisk(string path, string text)
        {
            if(string.IsNullOrEmpty(path))
                return false;

            FileStream file = File.Open(path, FileMode.OpenOrCreate, FileAccess.Write);
            file.Seek(0, SeekOrigin.Begin);
            file.SetLength(0);
            file.Close();
            StreamWriter sw;
            sw = File.CreateText(path);
            sw.Write(text);
            sw.Close();
            sw.Dispose();
            return true;
        }

        /// <summary>
        /// 从指定路径的文件中读取全部文本内容。
        /// </summary>
        /// <param name="path">文件路径。</param>
        /// <returns>文件的文本内容。</returns>
        public static string ReadFromDisk(string path)
        {
            return File.ReadAllText(path, Encoding.UTF8);
        }

        /// <summary>
        /// 将对象序列化为 JSON 并写入指定文件。
        /// </summary>
        /// <param name="obj">要序列化的对象。</param>
        /// <param name="path">输出文件路径。</param>
        /// <returns>序列化并写入成功返回 true，否则返回 false。</returns>
        public static bool Serialize(System.Object obj, string path)
        {
            if(obj == null)
                return false;

            string jsonString = JsonUtility.ToJson(obj, true);
            return WriteToDisk(path, jsonString);
        }

        /// <summary>
        /// 将对象序列化为 JSON 字符串。
        /// </summary>
        /// <param name="obj">要序列化的对象。</param>
        /// <returns>JSON 字符串，对象为 null 时返回空字符串。</returns>
        public static string Serialize(System.Object obj)
        {
            if(obj == null)
            {
                return string.Empty;
            }
            return JsonUtility.ToJson(obj, true);
        }

        /// <summary>
        /// 从文件读取 JSON 并反序列化覆盖到已有对象上。
        /// </summary>
        /// <param name="obj">要覆盖写入的目标对象。</param>
        /// <param name="path">JSON 文件路径。</param>
        /// <typeparam name="T">对象类型。</typeparam>
        /// <returns>反序列化成功返回 true，否则返回 false。</returns>
        public static bool Deserialize<T>(T obj, string path)
        {
            if(string.IsNullOrEmpty(path) || obj == null)
            {
                return false;
            }
            string jsonString = ReadFromDisk(path);
            JsonUtility.FromJsonOverwrite(jsonString, obj);
            return true;
        }

        /// <summary>
        /// 从 JSON 字符串反序列化覆盖到已有对象上。
        /// </summary>
        /// <param name="obj">要覆盖写入的目标对象。</param>
        /// <param name="jsonString">JSON 字符串，为空时使用 "{}"。</param>
        /// <typeparam name="T">对象类型。</typeparam>
        public static void DeserializeFromString<T>(T obj, string jsonString)
        {
            if(obj == null)
                return;

            if(string.IsNullOrEmpty(jsonString))
            {
                jsonString = "{}";
            }
            JsonUtility.FromJsonOverwrite(jsonString, obj);
        }

        /// <summary>
        /// 从 JSON 字符串反序列化创建指定类型的新对象。
        /// </summary>
        /// <param name="typeName">类型的程序集限定名称。</param>
        /// <param name="jsonString">JSON 字符串。</param>
        /// <returns>反序列化后的对象，失败时返回 null。</returns>
        public static System.Object DeserializeFromString(string typeName, string jsonString)
        {
            if(string.IsNullOrEmpty(jsonString))
            {
                return null;
            }
            var type = Type.GetType(typeName);
            var obj = Activator.CreateInstance(type);
            if(obj == null)
            {
                return null;
            }
            DeserializeFromString(obj, jsonString);
            return obj;
        }

        /// <summary>
        /// 从文件读取 JSON 并反序列化为指定类型的新对象。
        /// </summary>
        /// <param name="path">JSON 文件路径。</param>
        /// <typeparam name="T">对象类型。</typeparam>
        /// <returns>反序列化后的对象，失败时返回 default(T)。</returns>
        public static T Deserialize<T>(string path)
        {
            var obj = Activator.CreateInstance<T>();
            if(Deserialize(obj, path))
            {
                return obj;
            }
            return default;
        }

        /// <summary>
        /// 从 JSON 字符串反序列化为指定类型的新对象。
        /// </summary>
        /// <param name="jsonString">JSON 字符串。</param>
        /// <typeparam name="T">对象类型。</typeparam>
        /// <returns>反序列化后的对象。</returns>
        public static T DeserializeFromString<T>(string jsonString)
        {
            var obj = Activator.CreateInstance<T>();
            DeserializeFromString(obj, jsonString);

            return obj;
        }
    }
}
