using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class moneyChestScript : InteractiveObjectScript
{
	public Sprite closedChest;
	public Sprite openChest;

	public int coinsWorth;

	public bool opened;
	SpriteRenderer sr;
	private ParticleSystem ps;


	// Use this for initialization
	void Start()
	{
		opened = false;
		sr = GetComponent<SpriteRenderer>();
		sr.sprite = closedChest;
		ps = gameObject.GetComponent<ParticleSystem>();
		ps.Pause();
	}

	private void open(GameObject player)
	{
		if (!opened)
		{
			ps.Play();
			opened = true;
			sr.sprite = openChest;

			player.GetComponent<playerController>().coins += coinsWorth;
		}
	}

	public override void OnInteraction(playerController playerScript)
	{
		if (playerScript.CurrentChestKeys == 0)
		{
			return;
		}
		open(playerScript.gameObject);
		playerScript.CurrentChestKeys--;
		playerScript.setChestKeyText();
		playerScript.setCoinText();
		playerScript.PickedUpItem();
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