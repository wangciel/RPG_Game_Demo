using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class doorScript : InteractiveObjectScript
{
	public Sprite closedDoor;
	public Sprite openDoor;

	SpriteRenderer sr;

	public bool opened;
	
	private bool _sealedByButton;

	// Use this for initialization
	void Start()
	{
		sr = GetComponent<SpriteRenderer>();
		opened = false;
		sr.sprite = closedDoor;
	}

	private void open()
	{
		opened = true;
		sr.sprite = openDoor;

		foreach (Collider2D col in GetComponents<Collider2D>())
		{
			Destroy(col);
		}

		transform.Translate(new Vector3(0.15f, 0, 0));
	}

	public override void OnInteraction(playerController playerScript)
	{
		if (playerScript.CurrentDoorKeys == 0 || _sealedByButton)
		{
			return;
		}
		open();
		playerScript.gameObject.GetComponent<AudioManagerScript>().PlayHurtSFX();
		playerScript.CurrentDoorKeys--;
		playerScript.setDoorKeyText();
	}

	public override void OnCollisionBegin(playerController playerScript)
	{
		//no behaviour
	}

	public override void OnCollisionEnd(playerController playerScript)
	{
		//no behaviour
	}

	public void SetSealed()
	{
		_sealedByButton = true;
	}

	public void OpenWithButton()
	{
		if (!opened)
		{
			open();
		}
	}
}