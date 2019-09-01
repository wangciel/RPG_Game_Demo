using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LadderScript : MonoBehaviour
{

    private playerController player;
    private Transform topOfLadder;

    // Use this for initialization
    void Start()
    {
        player = FindObjectOfType<playerController>();

    }






    void OnTriggerEnter2D(Collider2D playerScript)
    {
        //no behaviour
        if (playerScript.name == "Player")
        {
            if (!player.dashing)
            {
                

                    player.onLadder = true;
            }
        }

    }


    void OnTriggerExit2D(Collider2D playerScript)
    {
        //no behaviour
        if (playerScript.name == "Player")
        {
            player.onLadder = false;
        }


    }
}