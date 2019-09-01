using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class candleScript : MonoBehaviour
{


    [SerializeField]
    private Sprite[] _litCandleFrames;
    private float _animTimer;
    
    public Sprite unlitCandle;
    private SpriteRenderer spriteRenderer;
    

    // Use this for initialization
    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        spriteRenderer.sprite = _litCandleFrames[0];
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            spriteRenderer.sprite = unlitCandle;
            enabled = false;
        }
    }

    void Update()
    {
        _animTimer = _animTimer < 0.666f ? _animTimer + Time.deltaTime : 0;
        spriteRenderer.sprite =
            _litCandleFrames[(int) Math.Min(_animTimer / 0.666f * _litCandleFrames.Length, _litCandleFrames.Length-1)];
            //_litCandleFrames[Random.Range(0, _litCandleFrames.Length-1)];
    }

}
