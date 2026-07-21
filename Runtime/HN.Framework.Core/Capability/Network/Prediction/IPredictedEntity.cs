namespace HN.Framework.Core.Capability.Network.Prediction
{
    /// <summary>
    /// 预测实体接口。定义支持客户端预测的实体的行为契约。
    /// 由 Core 层 Model 或 Controller 实现，Unity 层的 <see cref="PredictedNetworkEntityView"/>
    /// 通过此接口桥接预测逻辑。
    /// </summary>
    public interface IPredictedEntity
    {
        /// <summary>
        /// 执行预测模拟。在本地立即执行输入（乐观预测），
        /// 或在调和（Reconciliation）期间重放历史输入。
        /// </summary>
        /// <param name="input">预测输入数据</param>
        void Simulate(PredictionInputBase input);

        /// <summary>
        /// 应用服务端权威状态进行调和。
        /// 将当前状态回滚到服务端快照，然后重放未确认的输入。
        /// </summary>
        /// <typeparam name="T">权威状态类型（unmanaged 约束）</typeparam>
        /// <param name="data">调和数据，包含服务端 Tick 和权威状态</param>
        void Reconcile<T>(PredictionReconcileData<T> data) where T : unmanaged;

        /// <summary>
        /// 获取此实体最后处理的 Tick 号。
        /// 用于在调和时计算需要重放哪些未确认输入。
        /// </summary>
        /// <returns>最后一次模拟的 Tick</returns>
        uint GetLastProcessedTick();
    }
}
