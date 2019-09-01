using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class powerChestScript : InteractiveObjectScript
{
    public PowerUp GrantedPowerUp;
    public Sprite closedChest;
    public Sprite openChest;

    public bool opened;
    SpriteRenderer sr;


    public enum PowerUp
    {
        Dash,
        Grapple,
        DoubleJump,
        WallJump,
        Push
    }

    // Use this for initialization
    void Start()
    {
        opened = false;
        sr = GetComponent<SpriteRenderer>();
        sr.sprite = closedChest;
    }

    // Update is called once per frame
    void Update()
    {
    }

    public override void OnInteraction(playerController playerScript)
    {
        if (opened)
        {
            return;
        }

        opened = true;
        sr.sprite = openChest;
        playerScript.PickedUpItem();
        //add power here
        switch (GrantedPowerUp)
        {
            case PowerUp.Dash:
                {
                    playerScript.canDash = true;
                    break;
                }
          
            case PowerUp.Grapple:
                {
                    playerScript.canGrapple = true;
                    break;
                }

            case PowerUp.DoubleJump:
                {
                    playerScript.canDoubleJump = true;
                    break;
                }
            case PowerUp.WallJump:
                {
                    playerScript._canWallJump = true;
                    break;
                }
            case PowerUp.Push:
                {
                    playerScript.canPushPull = true;
                    break;
                }
        }
    }

    public override void OnCollisionBegin(playerController playerScript)
    {
        //no behaviour
    }

    public override void OnCollisionEnd(playerController playerScript)
    {
        //no behaviour
    }
}
