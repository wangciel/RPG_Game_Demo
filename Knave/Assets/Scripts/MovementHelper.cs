using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;

/// <summary>
/// Autonomous movement enhancing script for attachment to the player or anything you want to be a bit floatier and stickier.
/// </summary>
/// Author: Matthew Taylor
public class MovementHelper : MonoBehaviour
{
	private Rigidbody2D _entityRB;
	private bool _attachedToPlayer;
	
	
	private float _fallFloatiness = 0.33f;

	private void Start()
	{
		_entityRB = gameObject.GetComponent<Rigidbody2D>();
		_attachedToPlayer = gameObject.GetComponent<playerController>() != null;
		//Debug.Log(gameObject.name+": "+_attachedToPlayer);
	}
	
	/*
	 * If we're lucky this won't need any special consideration to run at the end 
	 */
	void FixedUpdate()
	{
		FloatFallingEntity();
	}

	void Update()
	{
		if (_attachedToPlayer)
		{
			//_fallFloatiness = (float)(gameObject.GetComponent<playerController>().jumpForce * 0.1);
			_fallFloatiness = gameObject.GetComponent<playerController>().jumpForce * 1.5f;
		}
	}

	private void FloatFallingEntity()
	{
		if (Mathf.Abs(_entityRB.velocity.y) > 0.001f)
		{
			if (_entityRB.velocity.y < 0 || !_attachedToPlayer)
			{
				_entityRB.AddForce(new Vector2(0, _fallFloatiness));
			}
			else if (_attachedToPlayer && gameObject.GetComponent<playerController>().AttemptingAscent())
			{
				_entityRB.AddForce(new Vector2(0, _fallFloatiness*1.333f));
			}
		}
	}
}