using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class DropThroughEnabler : MonoBehaviour
{
	private TilemapCollider2D _collider;
	private float _interactionTimer;
	
	// Use this for initialization
	void Start ()
	{
		_interactionTimer = 0;
		_collider = gameObject.GetComponent<TilemapCollider2D>();
	}
	
	// Update is called once per frame
	void Update ()
	{
		if (_interactionTimer > 0.0f)
		{
			_interactionTimer -= Time.deltaTime;
		}

		if (Input.GetAxisRaw("Vertical") < 0)
		{
			_collider.enabled = false;
			_interactionTimer = 0.25f;
		}
		else if (!_collider.enabled && _interactionTimer < 0.001)
		{
			_collider.enabled = true;
		}
	}
}
