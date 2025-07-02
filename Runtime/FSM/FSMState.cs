using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HN.Framework
{
    public class FSMState : IReference
    {
        public IReadOnlyDictionary<FSMState, Func<bool>> FSMTransitions => fsmTransitions;
        public int TransitionCount => fsmTransitions.Count;
        public string Name => name;

        public Action OnCreate;
        public Action OnEnter;
        public Action OnUpdate;
        public Action OnExit;
        public Action OnDestroy;

        protected Dictionary<FSMState, Func<bool>> fsmTransitions = new Dictionary<FSMState, Func<bool>>();
        protected string name;


        public FSMState()
        {
        }

        public bool Contains(FSMState targetState)
        {
            return fsmTransitions.ContainsKey(targetState);
        }

        public void AddTransition(FSMState targetState, Func<bool> condition)
        {
            if (fsmTransitions.ContainsKey(targetState))
            {
                Debug.LogError($"State {this} already contains transition target {targetState}.");
            }

            fsmTransitions.Add(targetState, condition);
        }

        public void SetTransition(FSMState targetState, Func<bool> condition)
        {
            if (!fsmTransitions.ContainsKey(targetState))
            {
                Debug.LogError($"State {this} does not contains transition target {targetState}.");
            }

            fsmTransitions[targetState] = condition;
        }

        public void AddOrSetTransition(FSMState targetState, Func<bool> condition)
        {
            if (fsmTransitions.ContainsKey(targetState))
            {
                fsmTransitions[targetState] = condition;
            }
            else
            {
                fsmTransitions.Add(targetState, condition);
            }
        }

        public void RemoveTransition(FSMState targetState)
        {
            if (!fsmTransitions.ContainsKey(targetState))
            {
                Debug.LogError($"State {this} does not contains transition target {targetState}.");
            }

            fsmTransitions.Remove(targetState);
        }

        public virtual void Initialize(string name)
        {
            this.name = name;
        }

        public virtual void Clear()
        {
            fsmTransitions.Clear();
            name = null;
            OnCreate = null;
            OnEnter = null;
            OnUpdate = null;
            OnExit = null;
            OnDestroy = null;
        }
    }
}
