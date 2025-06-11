using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using HN.Serialize;
using UnityEngine;
using UnityEngine.UIElements;

namespace HN.Graph
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
    public abstract class HNGraphPortInfo : Attribute
    {
        public string PortName => name;
        // public string Identity => identity;
        public Orientation orientation;
        public Direction PortDirection => direction;
        public Capacity PortCapacity => capacity;

        protected string name;
        // protected string identity;
        protected Direction direction;
        protected Capacity capacity;

        public HNGraphPortInfo(string slotName, /* string identity,  */Orientation orientation, Direction direction, Capacity capacity)
        {
            this.name = slotName;
            // this.identity = identity;
            this.orientation = orientation;
            this.direction = direction;
            this.capacity = capacity;
        }


        public enum Orientation
        {
            Horizontal,
            Vertical
        }

        public enum Direction
        {
            Input,
            Output
        }

        public enum Capacity
        {
            Single,
            Multi
        }
    }


    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public abstract class HNGraphNodeInfo : Attribute
    {
        public string NodeTitle => nodeTitle;
        // public string Identity => identity;
        public string MenuItem => menuItem;

        private string nodeTitle;
        // protected string identity;
        private string menuItem;

        public HNGraphNodeInfo(string nodeTitle, /* string identity,  */string menuItem = "")
        {
            this.nodeTitle = nodeTitle;
            // this.identity = identity;
            this.menuItem = menuItem;
        }
    }


    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public abstract class HNGraphInspectableInfo : Attribute
    {
        public abstract VisualElement Inspect(JsonData jsonData, PropertyInfo propertyInfo);
    }
}
