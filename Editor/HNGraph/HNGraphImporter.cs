using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using HN.Serialize;
using UnityEngine;
using UnityEditor;
using UnityEditor.AssetImporters;
using UnityEditor.VersionControl;

namespace HN.Graph.Editor
{
    public abstract class HNGraphImporter<T, U> : ScriptedImporter 
    where T : HNGraphData
    where U : HNGraphObject
    {
        protected string iconPath = "";
        protected HNGraphData graphData;
        protected HNGraphObject graphObject;
        
        private string graphDataPath;


        public virtual void LoadGraphData(string path)
        {
            graphDataPath = path;
            graphData = Activator.CreateInstance<T>();
            graphData.Initialize(path);
            graphObject = ScriptableObject.CreateInstance<U>();
            graphData.GraphObject = graphObject;
        }

        public virtual void SetObject(AssetImportContext ctx)
        {
            if (graphObject == null)
                return;

            if (string.IsNullOrEmpty(iconPath))
                ctx.AddObjectToAsset("MainAsset", graphObject);
            else
            {
                Texture2D texture = Resources.Load<Texture2D>(iconPath);
                if (texture == null)
                    ctx.AddObjectToAsset("MainAsset", graphObject);
                else
                    ctx.AddObjectToAsset("MainAsset", graphObject, texture);
            }
            ctx.SetMainObject(graphObject);
        }
    }
}
