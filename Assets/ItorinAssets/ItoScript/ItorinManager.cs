using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace itorin
{
    public class ItorinManager : MonoBehaviour
    {
        [SerializeField] GameObject Player,Players;
        
         NetworkManager_Client networkManager_Client;

        [System.Obsolete]
        void Start()
        {
            networkManager_Client = GameObject.FindGameObjectWithTag("itorinNetwork").GetComponent<NetworkManager_Client>();
        }

        void Update()
        {

        }

        [System.Obsolete]
        void MakingPlayer()
        {
            networkManager_Client.RequestCreatingNewAutonomousObject(Player.GetComponent<ReplicatiorBase>(), Player.name, gameObject.transform.position, Quaternion.ToEulerAngles(gameObject.transform.rotation), Players.name);
        }



    }
}

