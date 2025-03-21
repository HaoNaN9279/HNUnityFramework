using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace HN.Graph.Editor
{
    public class HNGraphEdgeView : Edge
    {
        public HNGraphEdge EdgeData => edgeData;

        public HNGraphPortView OutputPortView => output as HNGraphPortView;

        public HNGraphPortView InputPortView => input as HNGraphPortView;


        private HNGraphEdge edgeData;
        private HNGraphView graphView;
        

        public HNGraphEdgeView(HNGraphView graphView)
        {
            this.graphView = graphView;

        }

        public void Initialize(HNGraphEdge edgeData, HNGraphPortView output, HNGraphPortView input)
        {
            this.edgeData = edgeData;
            ConnectOutput(output);
            ConnectInput(input);
        }

        public void DisconnectOutput()
        {
            if(OutputPortView != null)
            {
                OutputPortView.DisconnectFromEdge(this);
                this.output = null;
            }
        }

        public void DisconnectInput()
        {
            if(InputPortView != null)
            {
                InputPortView.DisconnectFromEdge(this);
                this.input = null;
            }
        }

        public void DisconnectAll()
        {
            DisconnectOutput();
            DisconnectInput();
        }

        public HNGraphPortView GetAnotherPort(HNGraphPortView port)
        {
            if(OutputPortView == port)
            {
                return InputPortView;
            }
            else if(InputPortView == port)
            {
                return OutputPortView;
            }
            else
            {
                return null;
            }
        }
        

        protected void ConnectOutput(HNGraphPortView outputPortView)
        {
            this.output = outputPortView;
            OutputPortView.ConnectToEdge(this);
        }

        protected void ConnectInput(HNGraphPortView inputPortView)
        {
            this.input = inputPortView;
            inputPortView.ConnectToEdge(this);
        }    


    }
}
