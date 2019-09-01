using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoorKeyScript : InteractiveObjectScript {

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
		playerScript.CurrentDoorKeys++;
		playerScript.setDoorKeyText();
		playerScript.PickedUpItem();
		gameObject.SetActive(false);
	}

	public override void OnCollisionEnd(playerController playerScript)
	{
		//no behaviour
	}
}
