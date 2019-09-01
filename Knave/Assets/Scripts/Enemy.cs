using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class Enemy : MonoBehaviour
{
	public int DamageDealt = 1; //amount of damage a player takes from enemy attack
	private int health; //the amount of health this enemy has
	private float speed = -1f;
	float timer = 1.0f;
	public int maxTurnDelaySeconds = 5; //how long an enemy waits before turning and walking in the opposite direction

	private playerController player;
	private System.Random rand = new System.Random();
	public LayerMask enemyMask;
	public LayerMask attackMask;
	public LayerMask avoidWalkingOnMask;
	private Rigidbody2D m_rb;
	private Animator animator;
	private Transform enemyTrans;
	float enemyWidth, enemyHeight;

	private bool walking = true;
	private bool attacking;
	public float attackTime;
	private float remainingAttackTime;

	// Use this for initialization
	private void Start()
	{
		player = GameObject.FindGameObjectWithTag("Player").GetComponent<playerController>();

		animator = this.GetComponent<Animator>();
		enemyTrans = this.transform;
		m_rb = this.GetComponent<Rigidbody2D>();
		SpriteRenderer enemySprite = this.GetComponent<SpriteRenderer>();
		enemyWidth = enemySprite.bounds.extents.x;
		enemyHeight = enemySprite.bounds.extents.y;
		//flip sprite
		Vector3 Scaler = transform.localScale;
		Scaler.x *= -1;
		transform.localScale = Scaler;
	}

	private void Update()
	{
		/** https://youtu.be/LPNSh9mwT4w?t=1284 */
		//code for setting attack animation
		//animator.SetBool("attacking",true);

		//check for ground in front of enemy


		Vector2 lineCastPos = GetLineCastPosition();
		Vector2 playerCastPos = GetPlayerCastPosition();

		Vector2 groundCheckPos = GetGroundCheckPosition(lineCastPos);
		Vector2 wallCheckPos = GetWallCheckPosition(lineCastPos);

		bool isGrounded = Physics2D.Linecast(lineCastPos + toVec2(-transform.right) * 0.3f + Vector2.up * 0.2f, groundCheckPos + Vector2.up * 0.1f, enemyMask);
		Debug.DrawLine(lineCastPos + toVec2(-transform.right) * 0.3f + Vector2.up * 0.2f, groundCheckPos + Vector2.up * 0.1f, isGrounded ? Color.red : Color.white);
		
		bool iAmAboutToHurtMyFeetsies = Physics2D.Linecast(lineCastPos, groundCheckPos + Vector2.up * 0.1f + toVec2(-transform.right) * 0.1f, avoidWalkingOnMask);
		Debug.DrawLine(lineCastPos, groundCheckPos + Vector2.up * 0.1f + toVec2(-transform.right) * 0.1f, iAmAboutToHurtMyFeetsies ? Color.red : Color.grey);
		
		bool isBlocked = Physics2D.Linecast(lineCastPos, wallCheckPos , enemyMask) || iAmAboutToHurtMyFeetsies;
		Debug.DrawLine(lineCastPos, wallCheckPos, isBlocked ? Color.red : Color.white);
		
		attacking = Physics2D.Linecast(playerCastPos, playerCastPos + toVec2(-transform.right) * 0.1f, attackMask);
		Debug.DrawLine(				   playerCastPos, playerCastPos + toVec2(-transform.right) * 0.1f, attacking ? Color.red : Color.blue);
		
//		if (!isBlocked)
//		{
//			isBlocked = Physics2D.Linecast(playerCastPos, playerCastPos - toVec2(enemyTrans.right) / 8, enemyMask);
//		}

		//if attacking, lock him into an attack where he doesn't do anything else

		if (remainingAttackTime <= 0)
		{
			remainingAttackTime = attackTime;
			animator.SetBool("attacking", false);
		}
		else
		{
			remainingAttackTime -= Time.deltaTime;
		}


		//check to see if player is in front
		if (attacking)
		{
			walking = false;
			animator.SetBool("attacking", true);
		}
		else
		{
			walking = true;
			animator.SetBool("attacking", false);
		}

		if (walking)
		{
			//if no ground ahead, turn around
			if (!isGrounded || isBlocked)
			{
				//pause before changing direction
				if (timer > 0)
				{
					animator.SetFloat("speed", 0); //play idle animation

					timer -= Time.deltaTime;
					return;
				}

				timer = rand.Next(1, maxTurnDelaySeconds);

				//flip Y 180 degrees
				Vector3 currentRotation = enemyTrans.eulerAngles;
				currentRotation.y += 180;
				enemyTrans.eulerAngles = currentRotation;
			}

			//move forwards
			Vector2 enemyVelocity = m_rb.velocity;
			enemyVelocity.x = enemyTrans.right.x * speed;
			m_rb.velocity = enemyVelocity;
			animator.SetFloat("speed", m_rb.velocity.x * m_rb.velocity.x); //play walking animation
		}
		//else attacking
		else
		{
			player.AttemptTakeDamage(DamageDealt);
		}
	}

	private Vector2 GetLineCastPosition()
	{
		return enemyTrans.position - enemyTrans.right * enemyWidth * 0.8f + Vector3.up * 0.1f;
	}

	private Vector2 GetPlayerCastPosition()
	{
		return enemyTrans.position - enemyTrans.right * 0.11f;
	}
	
	private Vector2 GetGroundCheckPosition(Vector2 origin)
	{
		return origin + Vector2.down * 0.75f;
	}

	private Vector2 GetWallCheckPosition(Vector2 origin)
	{
		return origin - toVec2(enemyTrans.right) / 8;
	}
	
	//private Vector2 Get

	//converts a Vector3 into a Vector2
	private static Vector2 toVec2(Vector3 vector3)
	{
		return new Vector2(vector3.x, vector3.y);
	}

}