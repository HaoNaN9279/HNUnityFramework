using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace HN.Framework
{
    [CreateAssetMenu(fileName = "NewGameManager", menuName = "HN Unity Framework/Game Manager")]
    public class HNGameManager : ScriptableObject
    {
        private AssetManager assetManager;

        public void Initialize(HNUnityFramework framework)
        {
            this.assetManager = framework.assetManager;

            assetManager.LoadScene("Assets/TestScene01.unity", LoadSceneMode.Additive, true).Completed += (handle) =>
            {
                Debug.Log("Scene TestScene01 has loaded.");
            };
        }
    }
}
