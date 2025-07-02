using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HN.Framework
{
    public class ProcedureManager
    {
        public static ProcedureManager Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new ProcedureManager();
                    return instance;
                }

                return instance;
            }
        }
        private static ProcedureManager instance;

        public static int ProcedureStateCount => Instance.states.Count;

        public static ProcedureState CurrentState => Instance.currentState;


        private Dictionary<string, ProcedureState> states = new Dictionary<string, ProcedureState>();
        private ProcedureState currentState;


        public static void StartProcedure(string stateName)
        {
            if (Instance.states.ContainsKey(stateName))
            {
                Instance.currentState = Instance.states[stateName];
                Instance.currentState.OnEnter?.Invoke();
            }
            else
            {
                Debug.LogError($"Current procedure does not contain state {stateName}.");
            }
        }

        public static void StartProcedure(ProcedureState startState)
        {
            if (Instance.states.ContainsValue(startState))
            {
                Instance.currentState = startState;
                Instance.currentState.OnEnter?.Invoke();
            }
            else
            {
                Debug.LogError($"Current procedure does not contain state {startState}.");
            }
        }

        public static void ShutdownProcedure()
        {
            if (Instance.currentState != null)
            {
                Instance.currentState.OnExit?.Invoke();
            }
            Instance.currentState = null;
        }

        public static T AddProcedureState<T>(string stateName) where T : ProcedureState, new()
        {
            if (Instance.states.ContainsKey(stateName))
            {
                Debug.LogError($"Already contains procedure state name {stateName}.");
                return null;
            }
            T newState = ReferencePool.Acquire<T>();
            newState.Initialize(stateName);
            Instance.states[stateName] = newState;
            return newState;
        }

        public static void AddProcedureState(ProcedureState state)
        {
            if (Instance.states.ContainsValue(state))
            {
                Debug.LogError($"Already contains procedure state {state}.");
                return;
            }

            Instance.states[state.Name] = state;
        }

        public static void RemoveProcedureState(string stateName)
        {
            if (!Instance.states.ContainsKey(stateName))
            {
                Debug.LogError($"state {stateName} does not exist.");
                return;
            }

            ProcedureState state = Instance.states[stateName];
            Instance.states.Remove(stateName);
            ReferencePool.Release(state);
        }
        public static void RemoveProcedureState(ProcedureState state)
        {
            if (!Instance.states.ContainsValue(state))
            {
                Debug.LogError($"state {state} not exist.");
                return;
            }

            Instance.states.Remove(state.Name);
            ReferencePool.Release(state);
        }
        public static void UpdateProcedure()
        {
            if (Instance.currentState != null)
            {
                Instance.currentState.OnUpdate?.Invoke();
            }
        }

        public static void ChangeProcedureState(string targetStateName)
        {
            if (Instance.currentState == null)
            {
                Debug.LogError($"Procedure has not start.");
            }

            if (!Instance.states.ContainsKey(targetStateName))
            {
                Debug.LogError($"Procedure does not contain state {targetStateName}.");
            }

            Instance.currentState.OnExit?.Invoke();
            Instance.currentState = Instance.states[targetStateName];
            Instance.currentState.OnEnter?.Invoke();
        }

        public static void ChangeProcedureState(ProcedureState targetState)
        {
            if (Instance.currentState == null)
            {
                Debug.LogError($"Procedure has not start.");
            }

            if (!Instance.states.ContainsValue(targetState))
            {
                Debug.LogError($"Procedure does not contain state {targetState}.");
            }

            Instance.currentState.OnExit?.Invoke();
            Instance.currentState = targetState;
            Instance.currentState.OnEnter?.Invoke();
        }
    }
}
