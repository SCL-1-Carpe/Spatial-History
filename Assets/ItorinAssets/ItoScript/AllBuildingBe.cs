using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AllBuildingBe : MonoBehaviour
{
    [SerializeField] GameObject ImageGame;
    [SerializeField] GameObject Pregame;


    // Start is called before the first frame update
    void Start()
    {

        
    }

    // Update is called once per frame
    [System.Obsolete]
    void Update()
    {
        if (Pregame.active==true)
        {
            gameObject.transform.position = ImageGame.transform.position;
        }
       
        
    }



}
