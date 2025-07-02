using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HN.Framework
{
    public class ProcedureState : IReference
    {
        public string Name => name;

        public Action OnEnter;
        public Action OnUpdate;
        public Action OnExit;
        
        protected string name;


        public virtual void Initialize(string name)
        {
            this.name = name;
        }

        public virtual void Clear()
        {
            name = null;
            OnEnter = null;
            OnUpdate = null;
            OnExit = null;
        }
    }
}
