using System;
using System.Collections.Generic;
using System.Net;
using UnityEngine;
using UnityEngine.Serialization;

public class NewEnemy : MonoBehaviour
{
	private playerController _player;
	private System.Random _rng = new System.Random();
	private AudioManagerScript _soundManager;
	private ChestKeyScript _heldKey;

	[Tooltip("Damage dealt by the enemy's attack, should it hit.")] [SerializeField]
	private int _damageDealt = 1;
	
	[Tooltip("The BoxCollider2D trigger to use for checking if the enemy hits something with their attack.")] [SerializeField]
	private BoxCollider2D _attackHurtbox;
	
	[Tooltip("How quickly the enemy moves in space when walking."), Range(0.25f, 3.0f)] [SerializeField]
	private float _movementSpeed = 1f;

	[Tooltip("How quickly the enemy moves in space when pursuing."), Range(0.25f, 3.0f)] [SerializeField]
	private float _pursuitSpeed = 1.5f;
	
	[FormerlySerializedAs("_sightRange")] [Tooltip("How far away the enemy can see you from."), Range(0.01f, 6.0f)] [SerializeField]
	private float _horizontalSightRange;
	
	[FormerlySerializedAs("_upwardsSightRange")] [Tooltip("How far up the enemy can notice you."), Range(0.01f, 2.0f)] [SerializeField]
	private float _verticalSightRange;
	
	[Tooltip("How far away the enemy will hear you from, even behind them"), Range(0.01f, 1.0f)] [SerializeField]
	private float _hearingRange;

	[Header("Masks & Navigation Settings")]
		[Tooltip("Layers that the enemy will avoid walking into.")] [SerializeField]
		private LayerMask _avoidWalkingIntoMask;
	
		[Tooltip("Layers that the enemy will attempt to attack.")] [SerializeField]
		private LayerMask _hostileToMask;
	
		[Tooltip("Layers that the enemy will avoid walking on top of.")] [SerializeField]
		private LayerMask _avoidWalkingOnMask;
	
		private Rigidbody2D _rb;
		private SpriteRenderer _spriteRenderer;
		private int _facingDirection = 1;
	
		float _enemyWidth, _enemyHeight;

	[Header("State Timing Settings")]
		[Tooltip("How long a still enemy will wait for."), Range(0.05f, 10.0f)]
		[SerializeField]
		private float _idleDuration;
	
		[Tooltip("How long a patrolling enemy will move for (unless they are interrupted)."), Range(0.05f, 10.0f)]
		[SerializeField]
		private float _patrolDuration;
	
		[Tooltip("How long the enemy takes to start giving chase after becoming alerted."), Range(0.05f, 10.0f)]
		[SerializeField]
		private float _alertDuration;
	
		[Tooltip(
			 "How long the enemy will give chase for while they can't see the player. This is extended if they can still see you."),
		 Range(0.0f, 10.0f)]
		[SerializeField]
		private float _pursueDuration;
	
		[Tooltip("After deciding to swing, how long until the swing occurs in seconds."), Range(0.05f, 1.0f)]
		[SerializeField]
		private float _attackWindupDuration;
	
		[Tooltip("Damage dealt by the enemy's attack, should it hit."), Range(0.05f, 1.0f)] [SerializeField]
		private float _attackOutDuration = 1;
	
		[Tooltip("How long the enemy takes to recover from swinging, in seconds."), Range(0.05f, 1.0f)] [SerializeField]
		private float _attackRecoveryDuration;

	[Header("Animation Timing Settings")]
		[Tooltip("How long a loop of the enemy walk-cycle is."), Range(0.05f, 2.0f)]
		[SerializeField]
		private float _walkCycleDuration;
	
		[Tooltip("How long a loop of the enemy walk-cycle is while they're pursuing."), Range(0.05f, 2.0f)] [SerializeField]
		private float _chasingCycleDuration;
	
	[Header("Animation Sprite/Frame Settings")]
		[Tooltip("Frames when the enemy is still")] [SerializeField]
		private Sprite[] _idleSprites;
	
		[Tooltip("Frames when the enemy is moving")] [SerializeField]
		private Sprite[] _walkSprites;
	
		[Tooltip("Frames when the enemy is preparing to attack")] [SerializeField]
		private Sprite[] _windupSprites;
	
		[Tooltip("Frames when the enemy is attacking")] [SerializeField]
		private Sprite[] _attackSprites;
	
		[Tooltip("Frames when the enemy is recovering from an attack")] [SerializeField]
		private Sprite[] _recoverySprites;

	private EnemyState _currentState;

	private enum EnemyState
	{
		Idle,
		Patrolling,
		Alerted,
		Pursuing,
		Windup,
		Attacking,
		Recovery
	}

	private float _animationTimer;
	private float _stateTimer;

	/// <summary>
	/// Set the state and animation timers at the same time.
	/// </summary>
	/// <param name="stateTime">The duration the enemy will remain in this state unless otherwise changed.</param>
	/// <param name="animationTime">The timespan over which all animation frames for a given state will loop.</param>
	private void SetTimers(float stateTime, float animationTime)
	{
		_stateTimer = stateTime;
		_animationTimer = animationTime;
	}

	/// <summary>
	/// Updates the state and animation timers by the deltaTime and sets the enemy sprite according to the provided animation variables.
	/// </summary>
	/// <param name="animationFrames">Which Sprite array to use for the animation loop.</param>
	/// <param name="animationDuration">How long a single loop of the given animation is.</param>
	private void RunEnemyTimers(IList<Sprite> animationFrames, float animationDuration)
	{
		var dT = Time.deltaTime;
		_stateTimer += dT;
		_animationTimer = _animationTimer >= animationDuration ? 0 : _animationTimer + dT;
		_spriteRenderer.sprite =
			animationFrames[
				Math.Min((int) (_animationTimer / animationDuration * animationFrames.Count),
					animationFrames.Count - 1)];
	}

	/// <summary>
	/// Changes the current enemy state and resets the state timer.
	/// </summary>
	/// <param name="newState">The new AI state to be in.</param>
	private void ChangeStateTo(EnemyState newState)
	{
		_stateTimer = 0;
		_currentState = newState;
	}

	/// <summary>
	/// Returns whether the enemy's state timer has met the given duration.
	/// </summary>
	/// <param name="duration">Duration to check the timer against.</param>
	/// <returns></returns>
	private bool StateTimerExceeds(float duration)
	{
		return _stateTimer >= duration;
	}

	private bool PlayerDetected()
	{
		float distanceToPlayer = Vector2.Distance(transform.position, _player.transform.position);
		float verticalOffsetToPlayer = Math.Abs(_player.transform.position.y - transform.position.y);

		
		
		bool enemyHasLineOfSight = false;
		bool playerInRangeVertically = verticalOffsetToPlayer < 0.18 && verticalOffsetToPlayer < _verticalSightRange;
		bool playerHeard = distanceToPlayer < _hearingRange && playerInRangeVertically;
		if (playerInRangeVertically && (distanceToPlayer < _horizontalSightRange
		    && Math.Sign(_player.transform.position.x - transform.position.x) == _facingDirection
				|| _currentState == EnemyState.Alerted))
		{
			var castFrom = new Vector2(transform.position.x + _facingDirection*0.27f, transform.position.y + 0.36f);
			var castTo = new Vector2(_player.transform.position.x, transform.position.y + 0.09f);
			enemyHasLineOfSight = !Physics2D.Linecast(castFrom, castTo, _avoidWalkingIntoMask);
			Debug.DrawLine(castFrom, castTo, enemyHasLineOfSight ? Color.red : Color.white);
		}


		return enemyHasLineOfSight || playerHeard;
	}

	private void TurnAround()
	{
		_facingDirection *= -1;
		_spriteRenderer.flipX = !_spriteRenderer.flipX;
		if (_heldKey != null && _heldKey.gameObject.activeSelf)
		{
			var pos = _heldKey.transform.localPosition;
			var rot = _heldKey.transform.localRotation.eulerAngles;
			_heldKey.transform.localPosition = new Vector3(-pos.x, pos.y, pos.z);
			_heldKey.transform.localRotation = Quaternion.Euler(rot.x, rot.y, -rot.z);
			_heldKey.GetComponent<SpriteRenderer>().flipX = !_heldKey.GetComponent<SpriteRenderer>().flipX;
		}
	}

	private void FacePlayer()
	{
		int playerDirection = Math.Sign(_player.transform.position.x - transform.position.x);
		if (playerDirection != _facingDirection)
		{
			TurnAround();
		}
	}

	private void Start()
	{
		_currentState = EnemyState.Patrolling;
		SetTimers(_idleDuration, _idleDuration);

		_player = GameObject.FindGameObjectWithTag("Player").GetComponent<playerController>();
		_soundManager = _player.GetComponent<AudioManagerScript>();
		
		_rb = gameObject.GetComponent<Rigidbody2D>();
		_spriteRenderer = gameObject.GetComponent<SpriteRenderer>();

		_heldKey = gameObject.GetComponentInChildren<ChestKeyScript>();
		
		_enemyWidth = _spriteRenderer.bounds.extents.x;
		_enemyHeight = _spriteRenderer.bounds.extents.y;


		/* WHY was this a thing? I'm permanently encasing this snippet here in resin for future generations.
				A curse upon the bloodline of whoever inflicted this upon me.
		/-----------------------------------------------\
		|	//flip sprite								|
		|	Vector3 Scaler = transform.localScale;		|
		|	Scaler.x *= -1;								|
		|	transform.localScale = Scaler;				|
		\_______________________________________________/
		-----										-----
		*/
	}

	private void FixedUpdate()
	{
		//todo: put movement state code here
	}


	private void Update()
	{
		//todo: have to clean this whole fucker up.
		Vector2 lineCastPos = GetLineCastPosition();
		Vector2 playerCastPos = GetPlayerCastPosition();

		Vector2 groundCheckPos = GetGroundCheckPosition(lineCastPos);
		Vector2 wallCheckPos = GetWallCheckPosition(lineCastPos);

		//todo: make these use the facing and also clean it the hell up. Build the debug draw into a method.
		bool isGrounded = Physics2D.Linecast(lineCastPos + toVec2(transform.right * _facingDirection) * 0.3f + Vector2.up * 0.2f,
			groundCheckPos + Vector2.up * 0.1f, _avoidWalkingIntoMask);
		Debug.DrawLine(lineCastPos + toVec2(transform.right * _facingDirection) * 0.3f + Vector2.up * 0.2f,
			groundCheckPos + Vector2.up * 0.1f, isGrounded ? Color.red : Color.white);

		bool iAmAboutToHurtMyFeetsies = Physics2D.Linecast(lineCastPos,
			groundCheckPos + Vector2.up * 0.1f + toVec2(transform.right * _facingDirection) * 0.1f, _avoidWalkingOnMask);
		Debug.DrawLine(lineCastPos, groundCheckPos + Vector2.up * 0.1f + toVec2(transform.right * _facingDirection) * 0.1f,
			iAmAboutToHurtMyFeetsies ? Color.red : Color.grey);

		bool isBlocked = Physics2D.Linecast(lineCastPos + toVec2(transform.right * -0.1f * _facingDirection), wallCheckPos, _avoidWalkingIntoMask) ||
		                 iAmAboutToHurtMyFeetsies;
		Debug.DrawLine(lineCastPos + toVec2(transform.right * -0.1f * _facingDirection), wallCheckPos, isBlocked ? Color.red : Color.white);

		bool attackableInRange =
			Physics2D.Linecast(playerCastPos, playerCastPos + toVec2(transform.right * _facingDirection) * 0.25f, _hostileToMask);
		Debug.DrawLine(playerCastPos, playerCastPos + toVec2(transform.right * _facingDirection) * 0.25f,
			attackableInRange ? Color.red : Color.blue);


		//Debug.Log(_currentState);
		var dT = Time.deltaTime;
		switch (_currentState)
		{
			case EnemyState.Idle:
			{
				RunEnemyTimers(_idleSprites, _idleDuration);
				if (StateTimerExceeds(_idleDuration))
				{
					TurnAround();
					ChangeStateTo(EnemyState.Patrolling);
				}
				else if (attackableInRange || PlayerDetected())
				{
					ChangeStateTo(EnemyState.Alerted);
					_soundManager.PlayHurtSFX();
				}

				break;
			}
			case EnemyState.Patrolling:
			{
				RunEnemyTimers(_walkSprites, _walkCycleDuration);
				_rb.velocity = new Vector2(_movementSpeed * _facingDirection, _rb.velocity.y);

				if (attackableInRange || PlayerDetected())
				{
					ChangeStateTo(EnemyState.Alerted);
					FacePlayer();
					_soundManager.PlayHurtSFX();
				}

				bool decideToStop = StateTimerExceeds(_patrolDuration)
				                    || iAmAboutToHurtMyFeetsies 
				                    || isBlocked || !isGrounded
									|| Math.Abs(_rb.velocity.x) < 0.01f;
				if (decideToStop)
				{
					ChangeStateTo(EnemyState.Idle);
				}

				break;
			}
			case EnemyState.Alerted:
			{
				RunEnemyTimers(_idleSprites, _alertDuration);

				if (StateTimerExceeds(_alertDuration))
				{
					if (PlayerDetected() || attackableInRange)
					{
						if (isGrounded)
						{
							ChangeStateTo(EnemyState.Pursuing);
							FacePlayer();
						}
						else
						{
							FacePlayer();
							ChangeStateTo(EnemyState.Windup);
						}
					}
					else if (iAmAboutToHurtMyFeetsies || isBlocked || !isGrounded)
					{
						ChangeStateTo(EnemyState.Idle);
					}
					else
					{
						if (_rng.NextDouble() < 0.33)
						{
							TurnAround(); //this is to make them look a little confused sometimes.
						}

						ChangeStateTo(EnemyState.Idle);
					}
				}

				break;
			}
			case EnemyState.Pursuing:
			{
				RunEnemyTimers(_walkSprites, _chasingCycleDuration);
				_rb.velocity = new Vector2(!isBlocked ? _pursuitSpeed * _facingDirection : _rb.velocity.x * 0.99f, _rb.velocity.y);
				
				bool decideToStop = StateTimerExceeds(_pursueDuration)
				                    || iAmAboutToHurtMyFeetsies
				                    || isBlocked || !isGrounded
				                    || Math.Abs(_rb.velocity.x) < 0.01f;
				if (decideToStop)
				{
					if (!isGrounded)
					{
						_rb.velocity = new Vector2(_rb.velocity.x * 0.5f, _rb.velocity.y);
					}
					ChangeStateTo(EnemyState.Alerted);
				}
				
				if (attackableInRange)
				{
					ChangeStateTo(EnemyState.Windup);
				}

				if (PlayerDetected())
				{
					_stateTimer -= dT;
				}

				break;
			}
			case EnemyState.Windup:
			{
				RunEnemyTimers(_windupSprites, _attackWindupDuration);
				if (StateTimerExceeds(_attackWindupDuration))
				{
					ChangeStateTo(EnemyState.Attacking);
				}

				break;
			}
			case EnemyState.Attacking:
			{
				RunEnemyTimers(_attackSprites, _attackOutDuration);

				var filter = new ContactFilter2D();
				filter.layerMask = _hostileToMask;
				filter.useLayerMask = true;
				Collider2D[] contacts = new Collider2D[1];
				_attackHurtbox.OverlapCollider(filter, contacts);
				if (contacts[0] != null)
				{
					//Debug.Log(contacts[0]);
					_player.AttemptTakeDamage(_damageDealt);
					_player.GetComponent<Rigidbody2D>().velocity
						+= new Vector2(Math.Sign(_player.transform.position.x - transform.position.x) * 5f, 0);
				}
				
				if (StateTimerExceeds(_attackOutDuration))
				{
					ChangeStateTo(EnemyState.Recovery);
				}

				break;
			}
			case EnemyState.Recovery:
			{
				RunEnemyTimers(_recoverySprites, _attackRecoveryDuration);
				if (StateTimerExceeds(_attackRecoveryDuration))
				{
					FacePlayer();
					ChangeStateTo(EnemyState.Alerted);
				}

				break;
			}
		}
	}

	private Vector2 GetLineCastPosition()
	{
		return gameObject.transform.position + gameObject.transform.right * _facingDirection * _enemyWidth * 0.8f + Vector3.up * 0.1f;
	}

	private Vector2 GetPlayerCastPosition()
	{
		return gameObject.transform.position + gameObject.transform.right * _facingDirection * 0.11f;
	}

	private Vector2 GetGroundCheckPosition(Vector2 origin)
	{
		return origin + Vector2.down * 0.75f;
	}

	private Vector2 GetWallCheckPosition(Vector2 origin)
	{
		return origin + toVec2(gameObject.transform.right * _facingDirection) / 4;
	}

	//converts a Vector3 into a Vector2
	private static Vector2 toVec2(Vector3 vector3)
	{
		return new Vector2(vector3.x, vector3.y);
	}
}