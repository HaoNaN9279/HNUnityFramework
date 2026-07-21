using UnityEngine;

namespace HN.Framework.Unity.EditorUI.Attributes
{
    /// <summary>
    /// 控制字段在 Inspector 中的绘制顺序。数值越小越靠前。
    /// 仅在 HNSmartInspector 中生效，默认 Inspector 不受影响。
    /// </summary>
    public class PropertyOrderAttribute : PropertyAttribute
    {
        public int Order { get; }

        public PropertyOrderAttribute(int order)
        {
            Order = order;
        }
    }
}
