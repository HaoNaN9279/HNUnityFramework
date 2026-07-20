using System;
using UnityEngine;

namespace HN.Framework.Unity.EditorUI.Attributes
{
    /// <summary>
    /// 在 Inspector 中将标记的方法渲染为一个可点击的按钮。
    /// 方法必须为无参方法（public void MethodName()）。
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public class ButtonAttribute : Attribute
    {
        /// <summary>按钮上显示的文本，为空时使用方法名</summary>
        public string ButtonName { get; set; }
        /// <summary>按钮高度（像素），默认 22</summary>
        public float ButtonHeight { get; set; } = 22f;
        /// <summary>点击后是否标记场景为脏（需要保存）</summary>
        public bool DirtyOnClick { get; set; } = true;

        /// <param name="buttonName">按钮显示文本，为空则使用方法名</param>
        public ButtonAttribute(string buttonName = null)
        {
            ButtonName = buttonName;
        }
    }
}