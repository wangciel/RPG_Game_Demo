using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ButtonScript : InteractiveObjectScript
{
	[Header("Button Settings")]
	[SerializeField]
	private doorScript _associatedDoor;
	[SerializeField]
	private Powerups _correctPowerup;
	
	[Header("Animation Settings")]
	[SerializeField]
	private Sprite[] _buttonUpFrames;
	[Tooltip("How long in seconds it takes for the frames to be played.")] [SerializeField]
	[Range(0.01f, 3f)]
	private float _buttonUpDuration = 3;
	[Tooltip("Whether the animation loops back to the start when it's finished or not.")] [SerializeField]
	private bool _buttonUpLoops = true;
	[Space]
	[SerializeField]
	private Sprite[] _buttonActivatingFrames;
	[Tooltip("How long in seconds it takes for the frames to be played.")] [SerializeField]
	[Range(0.01f, 3f)]
	private float _buttonActivatingDuration = 3;
	[Space]
	[SerializeField]
	private Sprite _buttonDownFrame;


	
	private AudioManagerScript _soundManager;

	private SpriteRenderer _spriteRenderer;
	private bool _pauseAnimation;
	private float _animationTimer;
	private State _currentState;

	private enum Powerups
	{
		dash,
		doubleJump,
		grapple,
		wallJump,
		noneRequired
	}

	private enum State
	{
		up,
		activating,
		down
	}
	

	private void Start()
	{
		if (_associatedDoor == null)
		{
			Debug.LogError("JumpButton '"+gameObject.name
			                             +"' at position x: ["+transform.position.x
			                             +"] y: ["+transform.position.y
			                             +"] has no door assigned to it!");
//			UnityEditor.EditorApplication.isPlaying = false;
//			return;
		}
		_associatedDoor.SetSealed();
		var player = GameObject.FindGameObjectWithTag("Player").GetComponent<playerController>();
		_soundManager = player.GetComponent<AudioManagerScript>();
		_spriteRenderer = gameObject.GetComponent<SpriteRenderer>();
	}

	private void Update()
	{
		var animationDuration = _currentState == State.up ? _buttonUpDuration : _buttonActivatingDuration;
		bool looping = _currentState == State.up && _buttonUpLoops;
		Sprite[] animationFrames = _currentState == State.up ? _buttonUpFrames : _buttonActivatingFrames;
		
		
		var dT = _pauseAnimation ? 0 : Time.deltaTime;

		if (_pauseAnimation)
		{
			return;
		}

		if (_currentState == State.activating && _animationTimer >= animationDuration)
		{
			_currentState = State.down;
			_soundManager.PlayHurtSFX();
			_spriteRenderer.sprite = _buttonDownFrame;
			UnlockDoor();
			enabled = false;
			return;
		}
		_animationTimer = _animationTimer >= animationDuration
			? looping ? 0 : animationDuration
			: _animationTimer + dT;

		_spriteRenderer.sprite =
			animationFrames[
				Math.Min((int) (_animationTimer / animationDuration * animationFrames.Length),
					animationFrames.Length - 1)];

	}

	public override void OnInteraction(playerController playerScript)
	{
		//no behaviour
	}

	public override void OnCollisionBegin(playerController playerScript)
	{
		if (_currentState == State.up
		    && playerScript.transform.position.y > transform.position.y - 0.5f
		    && playerScript.GetComponent<Rigidbody2D>().velocity.y < -0.25f)
		{
			if (PlayerHasCorrectPowerup(playerScript))
			{
				var playerRb = playerScript.gameObject.GetComponent<Rigidbody2D>();
				playerRb.velocity = new Vector2(playerRb.velocity.x, 3f);
				_soundManager.PlayHurtSFX();
				_currentState = State.activating;
				_animationTimer = 0;
			}
			else
			{
				//a sound maybe too
				//playerScript.gameObject.GetComponent<Rigidbody2D>().velocity =
			}
		}
	}

	public override void OnCollisionEnd(playerController playerScript)
	{
		//no behaviour
	}

	private bool PlayerHasCorrectPowerup(playerController playerScript)
	{
		switch (_correctPowerup)
		{
			case Powerups.noneRequired:
				return true;
			case Powerups.dash:
				return playerScript.canDash;
			case Powerups.doubleJump:
				return playerScript.canDoubleJump;
			case Powerups.grapple:
				return playerScript.canGrapple;
			case Powerups.wallJump:
				return playerScript._canWallJump;
		}

		return false;
	}

	private void UnlockDoor()
	{
		_currentState = State.activating;
		_associatedDoor.OpenWithButton();
	}
	
	void OnDrawGizmos()
	{
		bool unassigned = _associatedDoor == null;
		Gizmos.color = unassigned ? Color.red : Color.grey;
		if (unassigned)
		{
			Gizmos.DrawWireSphere(new Vector3(transform.position.x, transform.position.y - 0.05f, transform.position.z), 0.36f);
		}
		else
		{
			Gizmos.DrawLine(transform.position, _associatedDoor.transform.position);
		}
	}
}
