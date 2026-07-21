using HN.Framework.Core.Level.Logic.AI.KnowledgePool;

namespace HN.Framework.Unity.Level.Logic.AI
{
    public interface IPerceptionSensor
    {
        void Sense(PerceptionState state);
    }
}
