using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MagicLeap;


namespace itorin
{
    public class ItorinManager : MonoBehaviour
    {
        [SerializeField] GameObject Player,Players;
        
         NetworkManager_Client networkManager_Client;

        //[SerializeField] NetworkManager_Client networkManager_Client;
        //[SerializeField] COBaseBe cOBaseBe;

        [SerializeField] int PlayerNumber;
        [SerializeField] GameObject Player1, Player2;

        [SerializeField] GameObject Pal, Pal2;


        GameObject Product2;


        [System.Obsolete]
        void Start()
        {
            networkManager_Client = GameObject.FindGameObjectWithTag("itorinNetwork").GetComponent<NetworkManager_Client>();

            OnNextScene();

            MakingPlayer();

        }

        void Update()
        {

        }

        void OnNextScene()
        {

            switch (PlayerNumber)
            {
                case 1:

                    Product2 = Player1;
                    Pal2.SetActive(false);

                    break;

                case 2:

                    Product2 = Player2;
                    Pal.SetActive(false);
                    break;

                default:

                    Debug.Log("PlayerNumberが間違えている");
                    break;

            }
        }

        [System.Obsolete]
        void MakingPlayer()
        {
            networkManager_Client.RequestCreatingNewAutonomousObject(Product2.GetComponent<ReplicatiorBase>(), Product2.name, gameObject.transform.position, Quaternion.ToEulerAngles(gameObject.transform.rotation), Players.name);
        }



    }
}

