using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MagicLeap;

public class PlayerCont : MonoBehaviour
{
    [SerializeField] NetworkManager_Client networkManager_Client;
    //[SerializeField] COBaseBe cOBaseBe;
    [SerializeField] int PlayerNumber;
    [SerializeField] GameObject Player1, Player2;
    GameObject Product2;

    // Start is called before the first frame update
    void Start()
    {
       

    }

    // Update is called once per frame
    void Update()
    {


    }

    void OnNextScene()
    {
        
        switch (PlayerNumber)
        {
            case 1:

                Product2 = Player1;
                break;

            case 2:

                Product2 = Player2;
                break;

            default:

                Debug.Log("PlayerNumberが間違えている");

                break;

        }
        //ここにプロジェクトを入れ込む
        //よっしに確認
        networkManager_Client.RequestCreatingNewAutonomousObject(Product2.GetComponent<ReplicatiorBase>(), Product2.name, gameObject.transform.position,gameObject.transform.position,"d");
       
    }

    private void OnDestroy()
    {
        
    }



}
