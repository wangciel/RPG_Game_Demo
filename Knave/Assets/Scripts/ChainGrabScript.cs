using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChainGrabScript : InteractiveObjectScript
{
	private Rigidbody2D _rb;
	private float _interactionTimer;
	
	// Use this for initialization
	void Start ()
	{
		_interactionTimer = 0;
		_rb = GetComponent<Rigidbody2D>();
	}
	
	// Update is called once per frame
	void Update ()
	{
		if (_interactionTimer > 0.0f)
		{
			_interactionTimer -= Time.deltaTime;
		}
	}

	public override void OnInteraction(playerController playerScript)
	{
	}

	public override void OnCollisionBegin(playerController playerScript)
	{
		if (GetInteractionPermitted())
		{
			TriggerInteractionTimer(0.2f);
			if (!playerScript.IsSwinging())
			{
				playerScript.SetSwinging(true, _rb);
				GetComponent<Rigidbody2D>().AddForce(playerScript.GetComponent<Rigidbody2D>().velocity * 3000f);
			}
		}
	}

	public override void OnCollisionEnd(playerController playerScript)
	{
		
	}

	public bool GetInteractionPermitted()
	{
		return _interactionTimer < 0.0001f;
	}

	public void TriggerInteractionTimer(float multiplier)
	{
		_interactionTimer = 1.0f * multiplier;
	}

	public void TriggerInteractionTimer()
	{
		TriggerInteractionTimer(1);
	}
}
