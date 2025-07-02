using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HN.Framework
{
    public class FSMManager
    {
        private static bool Active = true;
        private static Dictionary<string, FSM> fsmDict = new Dictionary<string, FSM>();


        public static FSM CreateFSM(string name)
        {
            if (fsmDict.ContainsKey(name))
            {
                Debug.LogError($"fsm name {name} already contained.");
                return null;
            }

            FSM fsm = ReferencePool.Acquire<FSM>();
            fsm.Initialize(name);
            fsmDict.Add(name, fsm);
            return fsm;
        }

        public static void RemoveFSM(string name)
        {
            if (!fsmDict.ContainsKey(name))
            {
                Debug.LogError($"fsm {name} does not exist.");
                return;
            }

            FSM fsm = fsmDict[name];
            fsmDict.Remove(name);
            ReferencePool.Release(fsm);
        }

        public static void RemoveFSM(FSM fsm)
        {
            if (!fsmDict.ContainsValue(fsm))
            {
                Debug.LogError($"fsm {fsm.Name} does not exist.");
                return;
            }
            
            fsmDict.Remove(fsm.Name);
            ReferencePool.Release(fsm);
        }

        public static void UpdateFSM()
        {
            if (!Active)
            {
                return;
            }
            
            if (fsmDict.Count == 0)
            {
                return;
            }

            foreach (var fsm in fsmDict.Values)
            {
                fsm.Update();
            }
        }

        public static void ClearAll()
        {
            if (fsmDict.Count == 0)
            {
                return;
            }

            foreach (var fsm in fsmDict.Values)
            {
                ReferencePool.Release(fsm);
            }
            fsmDict.Clear();
        }
    }
}
