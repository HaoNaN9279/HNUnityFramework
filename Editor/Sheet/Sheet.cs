using System;
using System.Linq;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace HN.Framework
{
    public class Sheet : ScriptableObject
    {
        public void Initialize(string path)
        {
            this.assetPath = Path.GetFullPath(path);
            List<string[]> data = ReadCSV(assetPath);
            if (data.Count == 0)
                return;

            lineCount = 0;
            for (int dataLineId = 0; dataLineId < data.Count; dataLineId++)
            {
                if (dataLineId == 0)
                {
                    ReadType(data[dataLineId]);
                }
                else if (dataLineId == 1)
                {
                    ReadHeaders(data[dataLineId]);
                }
                else if (dataLineId == 2)
                {
                    ReadDescriptions(data[dataLineId]);
                }
                else
                {
                    ReadSheetLine(data[dataLineId]);
                }

            }
        }

        public void SaveAsset()
        {
            //将SO数据写入到csv文件中
        }


        private List<string[]> ReadCSV(string filePath)
        {
            List<string[]> data = new List<string[]>();

            using (StreamReader reader = new StreamReader(filePath))
            {
                while (!reader.EndOfStream)
                {
                    string line = reader.ReadLine();
                    string[] values = line.Split(',');
                    data.Add(values);
                }
            }

            return data;
        }

        private void ReadType(string[] typeNames)
        {
            for (int i = 0; i < typeNames.Length; i++)
            {
                types.Add(typeNames[i]);
            }
            typeCount = typeNames.Length;
        }

        private void ReadHeaders(string[] headers)
        {
            if (typeCount == 0)
                return;
            
            for (int i = 0; i < typeCount; i++)
            {
                if (headers.Length > i)
                {
                    this.headers.Add(headers[i]);
                }
            }
        }

        private void ReadDescriptions(string[] descriptions)
        {
            if (typeCount == 0)
                return;
            
            for(int i = 0; i < typeCount; i++)
            {
                if(descriptions.Length > i)
                {
                    this.descriptions.Add(descriptions[i]);
                }
            }
        }

        private void ReadSheetLine(string[] sheetLine)
        {
            if (typeCount == 0)
                return;

            for (int i = 0; i < sheetLine.Length; i++)
            {
                if (sheetLine.Length > i)
                {
                    elements.Add(sheetLine[i]);
                }
            }

            lineCount++;
        }

        // private Type ReflectionFindType(string typeName)
        // {
        //     var assemblies = AppDomain.CurrentDomain.GetAssemblies();
        //     foreach (var assembly in assemblies)
        //     {
        //         Type[] types;
        //         try
        //         {
        //             types = assembly.GetTypes();
        //         }
        //         catch (ReflectionTypeLoadException e)
        //         {
        //             types = e.Types.Where(t => t != null).ToArray();
        //         }
        //         catch
        //         {
        //             continue;
        //         }

        //         foreach (var type in types)
        //         {
        //             var attrs = type.GetCustomAttributes(typeof(SheetElementTypeAttribute), false);
        //             if(attrs.Length > 0)
        //             {
        //                 foreach(SheetElementTypeAttribute attr in attrs)
        //                 {
        //                     if(attr.typeName == typeName)
        //                     {
        //                         return type;
        //                     }
        //                 }
        //             }
        //         }
        //     }

        //     return null;
        // }


        [SerializeField]
        public string assetPath;

        [SerializeField]
        public List<string> types = new List<string>();

        [SerializeField]
        public int typeCount;

        [SerializeField]
        public List<string> headers = new List<string>();

        [SerializeField]
        public List<string> descriptions = new List<string>();

        [SerializeField]
        public List<string> elements = new List<string>();

        [SerializeField]
        public int lineCount;

        public const string defaultHeaderName = "[Undefined]";
    }


}
