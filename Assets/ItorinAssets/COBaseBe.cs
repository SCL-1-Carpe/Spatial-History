using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;


namespace MagicLeap{

    public class COBaseBe : MonoBehaviour
    {
        [SerializeField] string MainHistoryName;

        [System.NonSerialized] public bool HinannKanryou=true;
        public delegate void OnSceneChaned();
        public OnSceneChaned onSceneChaned;

        // Start is called before the first frame update
        void Start()
        {
            // Debug.Log("テストフラグ１３１");
            SceneManager.sceneLoaded += SceneLoaded;
        }

        // Update is called once per frame
        void Update()
        {
           
        }

        void SceneLoaded(Scene thisScene,LoadSceneMode loadSceneMode1)
        {
            Debug.Log(thisScene.name);

            if (MainHistoryName == thisScene.name)
            {
                GameObject imageCube = GameObject.FindGameObjectWithTag("imageStandardCube");

                if (imageCube!=null)
                {
                    gameObject.transform.parent = imageCube.transform;

                    onSceneChaned.Invoke();
                }
                else
                {
                    Debug.Log("We can not find imageTag");

                }

            }
            else
            {
                Debug.Log("シーンに置けるcobaseの変換が失敗している");

            }


        }



    }
}
