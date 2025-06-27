using System.Collections;
using System.Collections.Generic;
using HN.Framework;
using UnityEngine;

namespace HN
{
    public class HNUnityFramework : MonoBehaviour
    {
        public static HNUnityFramework Instance = null;


        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        internal static void RuntimeInit()
        {
            Instance = FindObjectOfType<HNUnityFramework>();
            GameObject go;
            if (Instance == null)
            {
                go = new GameObject { name = "[HNUnityFramework]" };
                Instance = go.AddComponent<HNUnityFramework>();
            }
            go = Instance.gameObject;

            Debug.Log("HNUnityFramework Runtime Init.");
        }


        [SerializeField]
        public HNGameManager gameManager;


        public AssetManager assetManager;


        void Awake()
        {
            assetManager = AssetManager.Instance;

            if (gameManager == null)
            {
                Debug.LogWarning("Game Manager is Null.");
            }
            else
            {
                gameManager.Initialize(this);
            }
        }

        void Start()
        {

        }

        void Update()
        {

        }
    }
}
