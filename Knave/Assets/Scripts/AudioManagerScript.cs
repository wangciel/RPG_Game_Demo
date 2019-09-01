using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class AudioManagerScript : MonoBehaviour
{
	public AudioClip DungeonMusic;
	public AudioClip[] PickUpSFX;
	public AudioClip[] JumpSFX;
	public AudioClip HealSFX;
	public AudioClip DashSFX;
	public AudioClip[] HurtSFX;
	public AudioClip[] UseSFX;
	
	private AudioSource _sound;
	private Camera _gameCamera;
	
	
	// Use this for initialization
	void Start () {
		_gameCamera = transform.GetComponentInChildren<Camera>();
		_sound = _gameCamera.gameObject.GetComponent<AudioSource>();
		_sound.clip = DungeonMusic;
		_sound.Play();
	}

	public void PlayPickUpSFX()
	{
		_sound.PlayOneShot(PickUpSFX[Random.Range(0, PickUpSFX.Length)]);
	}
	
	public void PlayJumpSFX()
	{
		_sound.PlayOneShot(JumpSFX[Random.Range(0, JumpSFX.Length)]);
	}
	
	public void PlayHealSFX()
	{
		_sound.PlayOneShot(HealSFX);
	}

	public void PlayDashSFX()
	{
		//_sound.PlayOneShot(DashSFX);
		PlayJumpSFX();
	}

	public void PlayHurtSFX()
	{
		_sound.PlayOneShot(HurtSFX[Random.Range(0, HurtSFX.Length)]);
	}

	public void PlayUseSFX()
	{
		_sound.PlayOneShot(UseSFX[Random.Range(0, UseSFX.Length)]);
	}
}
