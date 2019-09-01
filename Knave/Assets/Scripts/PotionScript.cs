using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PotionScript : InteractiveObjectScript {

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	public override void OnInteraction(playerController playerScript)
	{
		//no behaviour
	}

	public override void OnCollisionBegin(playerController playerScript)
	{
		playerScript.CurrentPotions++;
		playerScript.setPotionText(); //unsure if this needs to be a thing we need to call at all, maybe can be automatic?
		playerScript.PickedUpItem();
		gameObject.SetActive(false);
	}

	public override void OnCollisionEnd(playerController playerScript)
	{
		//no behaviour
	}
}
