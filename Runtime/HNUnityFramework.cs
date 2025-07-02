using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HN.Framework
{
    public abstract class HNUnityFramework : MonoBehaviour
    {
        public float logicRate = 30f;


        private float logicDeltaTime = 0f;


        void Awake()
        {
        }

        void Start()
        {
            StartLogicTick();

            logicDeltaTime = 0f;
        }

        void Update()
        {
            logicDeltaTime += Time.deltaTime;
            float expectDeltaTime = 1.0f / logicRate;
            if (logicDeltaTime < expectDeltaTime)
            {
                return;
            }
            else
            {
                FSMManager.UpdateFSM();
                ProcedureManager.UpdateProcedure();

                LogicTick(logicDeltaTime);

                logicDeltaTime -= expectDeltaTime;
            }
        }

        public abstract void StartLogicTick();

        public abstract void LogicTick(float deltaTime);
    }
}
