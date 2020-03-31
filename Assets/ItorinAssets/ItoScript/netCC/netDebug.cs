using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class netDebug : MonoBehaviour
{
    [SerializeField] NetworkManager_Client client;
    bool Connected =false;

        // Start is called before the first frame update
    void Start()
    {
        Application.logMessageReceived += ApplicatonDid;
        client.OnAssignedNetwork += (s) =>
        {
            Connected = true;
        };
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void ApplicatonDid(string A,string B,LogType VV)
    {
        if (Connected) client.SendTcpPacket(A+"/"+B);
    }
}
