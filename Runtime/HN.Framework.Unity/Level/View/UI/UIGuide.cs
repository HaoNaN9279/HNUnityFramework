#nullable enable

using System;
using System.Collections.Generic;
using HN.Framework.Core.Capability.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace HN.Framework.Unity.Level.View.UI
{
    /// <summary>
    /// 步骤驱动教程引导覆盖层面板。
    /// 管理一系列 <see cref="GuideStep"/>，支持步骤前进/后退导航和高亮区域指示。
    /// 遮罩阻挡下层 UI 交互，引导用户按步骤完成教程。
    /// </summary>
    public class UIGuide : UIPanel
    {
        /// <summary>
        /// 镂空遮罩 RectTransform，用于遮挡高亮区域外的 UI。
        /// 实际遮罩着色器/材质由外部实现，本类仅管理位置和尺寸。
        /// </summary>
        [SerializeField]
        private RectTransform? guideMask;

        /// <summary>
        /// 高亮区域边框 RectTransform，用于指示当前引导目标的矩形区域。
        /// </summary>
        [SerializeField]
        private RectTransform? highlightRect;

        /// <summary>
        /// 步骤提示文字组件。
        /// </summary>
        [SerializeField]
        private TextMeshProUGUI? descriptionText;

        /// <summary>
        /// 下一步按钮。
        /// </summary>
        [SerializeField]
        private Button? nextButton;

        private List<GuideStep>? _steps;
        private int _currentStep;

        /// <summary>
        /// 当前步骤索引。
        /// </summary>
        public int CurrentStepIndex => _currentStep;

        /// <summary>
        /// 总步骤数。
        /// </summary>
        public int TotalSteps => _steps?.Count ?? 0;

        /// <summary>
        /// 所有步骤完成时触发的回调。
        /// </summary>
        public Action? OnGuideCompleted { get; set; }

        /// <summary>
        /// 步骤变更时触发的回调，参数为新的步骤索引。
        /// </summary>
        public Action<int>? OnStepChanged { get; set; }

        /// <summary>
        /// 显示引导面板。
        /// </summary>
        /// <param name="parent">挂载到的父 Transform。</param>
        /// <param name="steps">引导步骤数组。</param>
        internal void Show(Transform parent, GuideStep[] steps)
        {
            _steps = new List<GuideStep>(steps);
            base.Open(parent);
            CanvasGroup.blocksRaycasts = true;
            GoToStep(0);
        }

        /// <summary>
        /// 跳转到指定步骤索引。
        /// 如果索引超出步骤范围，触发 <see cref="OnGuideCompleted"/> 并关闭面板。
        /// </summary>
        /// <param name="index">目标步骤索引。</param>
        internal void GoToStep(int index)
        {
            int previousStep = _currentStep;

            // 仅在从另一个有效步骤离开时调用 OnStepExit
            if (_steps != null && previousStep >= 0 && previousStep < _steps.Count && previousStep != index)
            {
                OnStepExit(previousStep);
            }

            _currentStep = index;

            if (_steps == null || index >= _steps.Count)
            {
                OnGuideCompleted?.Invoke();
                Close();
                return;
            }

            OnStepEnter(index);

            var step = _steps[index];
            if (highlightRect != null)
            {
                highlightRect.anchoredPosition = new Vector2(step.HighlightOffsetX, step.HighlightOffsetY);
                highlightRect.sizeDelta = new Vector2(step.HighlightSizeWidth, step.HighlightSizeHeight);
            }

            if (descriptionText != null)
            {
                descriptionText.text = step.Description;
            }

            OnStepChanged?.Invoke(index);
        }

        /// <summary>
        /// 进入下一步。
        /// </summary>
        internal void NextStep()
        {
            GoToStep(_currentStep + 1);
        }

        /// <summary>
        /// 返回上一步。如果是第一步则不操作。
        /// </summary>
        internal void PrevStep()
        {
            GoToStep(Mathf.Max(0, _currentStep - 1));
        }

        /// <summary>
        /// 进入指定步骤时调用。子类可重写以实现步骤入场逻辑。
        /// </summary>
        /// <param name="stepIndex">步骤索引。</param>
        protected virtual void OnStepEnter(int stepIndex) { }

        /// <summary>
        /// 离开指定步骤时调用。子类可重写以实现步骤退场逻辑。
        /// </summary>
        /// <param name="stepIndex">步骤索引。</param>
        protected virtual void OnStepExit(int stepIndex) { }

        /// <inheritdoc />
        protected override void OnOpen()
        {
            base.OnOpen();

            if (nextButton != null)
            {
                nextButton.onClick.AddListener(NextStep);
            }
        }

        /// <inheritdoc />
        protected override void OnClose()
        {
            if (nextButton != null)
            {
                nextButton.onClick.RemoveListener(NextStep);
            }

            base.OnClose();
        }
    }
}
