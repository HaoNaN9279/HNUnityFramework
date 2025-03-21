using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace HN.Graph.Editor
{
    public class HNGraphGroupView : Group
    {
        public HNGraphGroup GroupData => groupData;
        
        private HNGraphGroup groupData;

        private HNGraphView graphView;


        public HNGraphGroupView(HNGraphView graphView, HNGraphGroup groupData)
        {
            this.graphView = graphView;
            this.groupData = groupData;

            title = groupData.Title;
            headerContainer.Q<TextField>().RegisterCallback<ChangeEvent<string>>((e) =>
            {
                groupData.SetTitle(e.newValue);
            });
        }

        public void Initialize()
        {
            SetPosition(groupData.GetLayout());
            AddElementsToGroup();
            AddSelectionsToGroup();
        }

        protected override void OnElementsAdded(IEnumerable<GraphElement> elements)
        {
            foreach(var element in elements)
            {
                var nodeView = element as HNGraphNodeView;
                if(nodeView == null)
                {
                    continue;
                }

                groupData.AddNode(nodeView.NodeData.Guid);
            }

            base.OnElementsAdded(elements);
        }

        protected override void OnElementsRemoved(IEnumerable<GraphElement> elements)
        {
            if(parent == null)
            {
                return;
            }

            foreach( var element in elements)
            {
                if(element is HNGraphNodeView nodeView)
                {
                    groupData.RemoveNode(nodeView.NodeData.Guid);
                }
            }

            base.OnElementsRemoved(elements);
        }

        private void AddElementsToGroup()
        {
            foreach(var nodeGuid in groupData.InnerNodeGuids)
            {
                if(string.IsNullOrEmpty(nodeGuid))
                {
                    continue;
                }

                foreach(var nodeView in graphView.NodeViews)
                {
                    if(nodeView.NodeData.Guid == nodeGuid)
                    {
                        AddElement(nodeView);
                    }
                }
            }
        }

        private void AddSelectionsToGroup()
        {
            foreach(var selectedNode in graphView.selection)
            {
                if(selectedNode is HNGraphNodeView)
                {
                    if(graphView.GroupViews.ToList().Exists(x => x.ContainsElement(selectedNode as HNGraphNodeView)))
                    {
                        continue;
                    }
                    HNGraphNodeView selectedNodeView = selectedNode as HNGraphNodeView;
                    AddElement(selectedNodeView);
                    GroupData.AddNode(selectedNodeView.NodeData.Guid);
                }
            }
        }

        public void SavePosition()
        {
            groupData.SetLayout(GetPosition());
        }

        public override void SetPosition(Rect newPos)
        {
            base.SetPosition(newPos);

            SavePosition();
        }
    }
}
