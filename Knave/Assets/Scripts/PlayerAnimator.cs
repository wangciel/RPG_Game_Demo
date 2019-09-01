using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAnimator : MonoBehaviour {
	[Header("Idle Animation Settings")]
		[Tooltip("Idle Sprites")] [SerializeField]
		private Sprite[] _idleSprites;
		[Tooltip("How long in seconds it takes for the frames to be played.")] [SerializeField]
		[Range(0.01f, 3f)]
		private float _idleAnimationDuration = 3;
		[Tooltip("Whether the animation loops back to the start when it's finished or not.")] [SerializeField]
		private bool _idleAnimationLoops = true;
	
	[Header("Running Animation Settings")]
		[Tooltip("Running Sprites")] [SerializeField]
		private Sprite[] _runningSprites;
		[Tooltip("How long in seconds it takes for the frames to be played.")] [SerializeField]
		[Range(0.01f, 2f)]
		private float _runningAnimationDuration = 0.55f;

    [Header("Jumping Animation Settings")]
		[Tooltip("Jumping Sprites")] [SerializeField]
		private Sprite[] _jumpingSprites;
		[Tooltip("How long in seconds it takes for the frames to be played.")] [SerializeField]
		[Range(0.01f, 2f)]
		private float _jumpingAnimationDuration = 0.175f;
		[Tooltip("Whether the animation loops back to the start when it's finished or not.")] [SerializeField]
		private bool _jumpingAnimationLoops;
	
	[Header("Dash Animation Settings")]
		[Tooltip("Dash Sprites")] [SerializeField]
		private Sprite[] _dashSprites;
		[Tooltip("How long in seconds it takes for the frames to be played.")] [SerializeField]
		[Range(0.01f, 2f)]
		private float _dashAnimationDuration = 0.175f;
		[Tooltip("Whether the animation loops back to the start when it's finished or not.")] [SerializeField]
		private bool _dashAnimationLoops;
	
	[Header("Falling Animation Settings")]
		[Tooltip("Falling Down Sprites")] [SerializeField]
		private Sprite[] _fallingDownSprites;
	[Space]
		[Tooltip("How long in seconds it takes for the frames to be played.")] [SerializeField]
		[Range(0.01f, 2f)]
		private float _fallingDownDuration = 0.2f;
		[Tooltip("Whether the animation loops back to the start when it's finished or not.")] [SerializeField]
		private bool _fallingDownLoops;
	[Space]
		[Tooltip("Falling Forwards Sprites")] [SerializeField]
		private Sprite[] _fallingForwardsSprites;
		[Tooltip("How long in seconds it takes for the frames to be played.")] [SerializeField]
		[Range(0.01f, 2f)]
		private float _fallingForwardsAnimationDuration = 0.2f;
		[Tooltip("Whether the animation loops back to the start when it's finished or not.")] [SerializeField]
		private bool _fallingForwardsAnimationLoops;
	[Space]
		[Range(0.01f, 5f)]
		[SerializeField] private float _fallingAnimationVelocityThreshold = 2.75f;
		[Range(0.01f, 1.5f)]
		[SerializeField] private float _fallingAnimationAirtimeThreshold = 0.5f;
		[SerializeField] private LayerMask _jumpProximityMask;

	[Header("Climbing Animation Settings")]
		[Tooltip("Climbing Sprites")]
		[SerializeField]
		private Sprite[] _climbingSprites;
		[Tooltip("How long in seconds it takes for the frames to be played.")] [SerializeField]
		[Range(0.01f, 2f)]
		private float _climbingAnimationDuration = 0.175f;
		[Tooltip("Whether the animation loops back to the start when it's finished or not.")]
		[SerializeField]
		private bool _climbingAnimationLoops;
	
	//[SerializeField]
	private float _idleAnimationPantingOffset;
	[SerializeField]
	private AnimationState _currentAnimationState;
	private float _animationTimer;
	private bool _pauseAnimation;

	private float _airTime;


	
	public enum AnimationState
	{
		Idle,
		Running,
        Climbing,
		Jumping,
		Dashing,
		Falling,
		FallingForwards
	}

	private playerController _playerScript;
	private SpriteRenderer _spriteRenderer;
	private Rigidbody2D _rb;
	


	private void RunAnimation(IList<Sprite> animationFrames, float animationDuration, bool looping)
	{
		var dT = _pauseAnimation ? 0 : Time.deltaTime;
		_animationTimer = _animationTimer >= animationDuration ? looping ? 0 : animationDuration : _animationTimer + dT;
		_spriteRenderer.sprite =
			animationFrames[
				Math.Min((int) (_animationTimer / animationDuration * animationFrames.Count),
					animationFrames.Count - 1)];
	}

	public void SetAnimationState(AnimationState newState)
	{
		if (_currentAnimationState == newState)
		{
			return;
		}
		_pauseAnimation = false;
		_currentAnimationState = newState;
		_animationTimer = 0;
	}

	public void UpdateAnimationState()
	{
		if (_playerScript.dashing)
		{
			SetAnimationState(AnimationState.Dashing);
			return;
		}
		if (_playerScript.GetOnGround())
		{
			if (Math.Abs(_rb.velocity.x) > 0.3)
			{
				SetAnimationState(AnimationState.Running);
			}
			else
			{
				SetAnimationState(AnimationState.Idle);
			}
			_airTime = 0;
		}
		else //the player is in the air.
		{
			if (_playerScript.IsClimbing())
			{
				_airTime = 0;
				SetAnimationState(AnimationState.Climbing);
				_pauseAnimation = _rb.velocity.magnitude < 0.1;
				return;
			}

			bool overGround = Physics2D.Linecast(transform.position, transform.position + new Vector3(_rb.velocity.x*0.1f, -0.90f, 0),
				_jumpProximityMask);
			_airTime += overGround ? 0 : Time.deltaTime;
			Debug.DrawLine(transform.position, transform.position + new Vector3(_rb.velocity.x*0.1f, -0.90f, 0), overGround ? Color.red : Color.green);
			//_airTime += Time.deltaTime * (_rb.velocity.y <= 0 ? 1.0f : 1.0f);
			//Debug.Log(_airTime + "\t \t" +_rb.velocity.y);
			if (_airTime >= _fallingAnimationAirtimeThreshold && _rb.velocity.y <= -_fallingAnimationVelocityThreshold)
			{
				SetAnimationState(Math.Abs(_rb.velocity.x) > 0.005 ? AnimationState.FallingForwards : AnimationState.Falling);
			}
			else
			{
				SetAnimationState(AnimationState.Jumping);
			}
		}
	}
	
	// Use this for initialization
	private void Start ()
	{
		_playerScript = gameObject.GetComponent<playerController>();
		_spriteRenderer = gameObject.GetComponent<SpriteRenderer>();
		_rb = gameObject.GetComponent<Rigidbody2D>();
		_animationTimer = 0;
		_currentAnimationState = AnimationState.Idle;
		_idleAnimationPantingOffset = 1f;
	}
	
	// Update is called once per frame
	private void Update ()
	{
		_idleAnimationPantingOffset = Math.Min(_idleAnimationPantingOffset, 3.0f);
		float dT = Time.deltaTime;
		switch (_currentAnimationState)
		{
			case AnimationState.Idle:
			{
				_idleAnimationPantingOffset *= 1 - 0.03f * dT;
				RunAnimation(_idleSprites, _idleAnimationDuration - Math.Min(2.5f, _idleAnimationPantingOffset),
					_idleAnimationLoops);
				break;
			}
			case AnimationState.Running:
			{
				_idleAnimationPantingOffset += dT * 0.05f;
				RunAnimation(_runningSprites, _runningAnimationDuration, true);
				break;
			}
			case AnimationState.Jumping:
			{
				_idleAnimationPantingOffset += dT * 0.06f;
				RunAnimation(_jumpingSprites, _jumpingAnimationDuration, _jumpingAnimationLoops);
				break;
			}
			case AnimationState.Dashing:
			{
				_idleAnimationPantingOffset += dT * 0.06f;
				RunAnimation(_dashSprites, _dashAnimationDuration, _dashAnimationLoops);
				break;
			}
			case AnimationState.Falling:
			{
				RunAnimation(_fallingDownSprites, _fallingDownDuration, _fallingDownLoops);
				break;
			}
			case AnimationState.FallingForwards:
			{
				RunAnimation(_fallingForwardsSprites, _fallingForwardsAnimationDuration, _fallingForwardsAnimationLoops);
				break;
			}
			case AnimationState.Climbing:
			{
				RunAnimation(_climbingSprites, _climbingAnimationDuration, _climbingAnimationLoops);
				break;
			}
		}
	}

	public void PlayerHealthAdjusted(bool decreased)
	{
		if (decreased)
		{
			_idleAnimationPantingOffset += 1.5f;
		}
		else
		{
			_idleAnimationPantingOffset *= 0.3f;
		}
	}
}
