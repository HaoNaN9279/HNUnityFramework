#nullable enable

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.OnScreen;

namespace HN.Framework.Unity.Capability.Input
{
    /// <summary>
    /// 触摸输入适配器，负责创建和管理虚拟屏幕控件（摇杆和按钮）。
    /// 基于 Unity InputSystem 的 OnScreenStick 和 OnScreenButton 组件。
    /// </summary>
    public sealed class TouchInputAdapter
    {
        private readonly List<GameObject> _createdObjects = new List<GameObject>();

        /// <summary>
        /// 创建一个虚拟摇杆 GameObject，挂载 OnScreenStick 组件，
        /// 并将其关联到指定的 InputAction 引用。
        /// </summary>
        /// <param name="actionReference">InputAction 的引用路径（例如 "Gameplay/Move"）。</param>
        /// <param name="parent">虚拟摇杆的父 RectTransform。</param>
        /// <returns>创建的虚拟摇杆 GameObject。</returns>
        /// <exception cref="ArgumentNullException">当 <paramref name="parent"/> 为 null 时抛出。</exception>
        public GameObject CreateVirtualStick(string actionReference, RectTransform parent)
        {
            if (parent == null)
                throw new ArgumentNullException(nameof(parent));

            var go = new GameObject("VirtualStick", typeof(RectTransform), typeof(OnScreenStick));
            go.transform.SetParent(parent, false);

            var stick = go.GetComponent<OnScreenStick>();
            stick.controlPath = actionReference;

            _createdObjects.Add(go);
            return go;
        }

        /// <summary>
        /// 创建一个虚拟按钮 GameObject，挂载 OnScreenButton 组件，
        /// 并将其关联到指定的 InputAction 引用。
        /// </summary>
        /// <param name="actionReference">InputAction 的引用路径（例如 "Gameplay/Jump"）。</param>
        /// <param name="parent">虚拟按钮的父 RectTransform。</param>
        /// <returns>创建的虚拟按钮 GameObject。</returns>
        /// <exception cref="ArgumentNullException">当 <paramref name="parent"/> 为 null 时抛出。</exception>
        public GameObject CreateVirtualButton(string actionReference, RectTransform parent)
        {
            if (parent == null)
                throw new ArgumentNullException(nameof(parent));

            var go = new GameObject("VirtualButton", typeof(RectTransform), typeof(OnScreenButton));
            go.transform.SetParent(parent, false);

            var button = go.GetComponent<OnScreenButton>();
            button.controlPath = actionReference;

            _createdObjects.Add(go);
            return go;
        }

        /// <summary>
        /// 销毁所有由此适配器创建的虚拟控件 GameObject。
        /// 在 Editor 模式下使用 DestroyImmediate，运行时使用 Destroy。
        /// </summary>
        public void DestroyAll()
        {
            foreach (var go in _createdObjects)
            {
                if (go == null) continue;

#if UNITY_EDITOR
                UnityEngine.Object.DestroyImmediate(go);
#else
                UnityEngine.Object.Destroy(go);
#endif
            }

            _createdObjects.Clear();
        }
    }
}
