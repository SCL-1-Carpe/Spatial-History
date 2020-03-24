using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace itorin
{
    public class ItorinManager : MonoBehaviour
    {
        [SerializeField] GameObject Player,Players;
        
         NetworkManager_Client networkManager_Client;


        // Start is called before the first frame update
        [System.Obsolete]
        void Start()
        {
            networkManager_Client = GameObject.FindGameObjectWithTag("itorinNetwork").GetComponent<NetworkManager_Client>();
        }

        // Update is called once per frame
        void Update()
        {

        }

        [System.Obsolete]
        void MaikingPlayer()
        {
            networkManager_Client.RequestCreatingNewAutonomousObject(Player.GetComponent<ReplicatiorBase>(), Player.name, gameObject.transform.position, Quaternion.ToEulerAngles(gameObject.transform.rotation), Players.name);
        }



    }
}

