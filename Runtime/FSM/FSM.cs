using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HN.Framework
{
    public class FSM : IReference
    {
        public int StateCount => states.Count;
        public FSMState CurrentState => currentState;
        public string Name => name;

        protected Dictionary<string, FSMState> states = new Dictionary<string, FSMState>();
        protected FSMState currentState;
        protected string name;


        public FSM()
        {
        }

        public virtual void Initialize(string name)
        {
            this.name = name;
        }

        public virtual void Start(string stateName)
        {
            if (states.ContainsKey(stateName))
            {
                currentState = states[stateName];
                currentState.OnEnter?.Invoke();
            }
            else
            {
                Debug.LogError($"Current FSM {this} does not contain state {stateName}.");
            }
        }

        public virtual void Start(FSMState startState)
        {
            if (states.ContainsValue(startState))
            {
                currentState = startState;
                currentState.OnEnter?.Invoke();
            }
            else
            {
                Debug.LogError($"Current FSM {this} does not contain state {startState}.");
            }
        }

        public virtual void Shutdown()
        {
            if (currentState != null)
            {
                currentState.OnExit?.Invoke();
            }
            currentState = null;
        }

        public virtual T AddState<T>(string stateName) where T : FSMState, new()
        {
            if (states.ContainsKey(stateName))
            {
                Debug.LogError($"Already contains state name {stateName}.");
                return null;
            }

            T newState = ReferencePool.Acquire<T>();
            newState.Initialize(stateName);
            states.Add(stateName, newState);
            newState.OnCreate?.Invoke();
            return newState;
        }

        public virtual void AddTransition(FSMState fromState, FSMState targetState, Func<bool> condition)
        {
            if (!states.ContainsValue(fromState))
            {
                Debug.LogError($"FSM {this} does not exist state {fromState}.");
            }

            if (!states.ContainsValue(targetState))
            {
                Debug.LogError($"FSM {this} does not exist state {targetState}.");
            }

            fromState.AddTransition(targetState, condition);
        }

        public virtual void RemoveState(string stateName)
        {
            if (!states.ContainsKey(stateName))
            {
                Debug.LogError($"state {stateName} does not exist.");
                return;
            }

            FSMState state = states[stateName];
            state.OnDestroy?.Invoke();
            states.Remove(stateName);
            ReferencePool.Release(state);
        }

        public virtual void RemoveState(FSMState state)
        {
            if (!states.ContainsValue(state))
            {
                Debug.LogError($"state {state} not exist.");
                return;
            }

            state.OnDestroy?.Invoke();
            states.Remove(state.Name);
            ReferencePool.Release(state);
        }

        public virtual void RemoveTransition(FSMState fromState, FSMState targetState)
        {
            if (!states.ContainsValue(fromState))
            {
                Debug.LogError($"FSM {this} does not exist state {fromState}.");
            }

            if (!states.ContainsValue(targetState))
            {
                Debug.LogError($"FSM {this} does not exist state {targetState}.");
            }

            if (fromState.Contains(targetState))
            {
                Debug.LogError($"FSM {this} state {fromState} can not trans to state {targetState}.");
            }

            fromState.RemoveTransition(targetState);
        }

        public virtual void Update()
        {
            if (currentState != null)
            {
                foreach (var targetState in currentState.FSMTransitions.Keys)
                {
                    if (currentState.FSMTransitions[targetState].Invoke())
                    {
                        // Debug.Log($"Change State from {currentState.Name} to {targetState.Name}.");
                        ChangeState(targetState);
                        break;
                    }
                }

                currentState.OnUpdate?.Invoke();
            }
        }

        protected virtual void ChangeState(string targetStateName)
        {
            if (states.ContainsKey(targetStateName))
            {
                currentState.OnExit?.Invoke();
                currentState = states[targetStateName];
                currentState.OnEnter?.Invoke();
            }
            else
            {
                Debug.LogError($"Current FSM {this} does not contains state {targetStateName}.");
            }
        }

        protected virtual void ChangeState(FSMState targetState)
        {
            if (states.ContainsValue(targetState))
            {
                currentState.OnExit?.Invoke();
                currentState = targetState;
                currentState.OnEnter?.Invoke();
            }
            else
            {
                Debug.LogError($"Current FSM {this} does not contains state {targetState.Name}.");
            }
        }

        public virtual void Clear()
        {
            currentState = null;
            foreach (var state in states.Values)
            {
                ReferencePool.Release(state);
            }
            states.Clear();
            name = null;
        }
    }
}
