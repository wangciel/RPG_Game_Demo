using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This abstract class should be inherited from for all scripts that govern the behaviour of objects the player can interact with.
/// This allows us to have less code in the playerController script.
/// </summary>
public abstract class InteractiveObjectScript : MonoBehaviour
{
	/// <summary>
	/// Method called when the player attempts to interact with the object.
	/// </summary>
	/// <param name="playerScript">The playerController script calling this method.</param>
	public abstract void OnInteraction(playerController playerScript);

	/// <summary>
	/// Method called when the player first touches the object.
	/// </summary>
	/// <param name="playerScript">The playerController script calling this method.</param>
	public abstract void OnCollisionBegin(playerController playerScript);

	/// <summary>
	/// Method called when the player stops touching the object.
	/// </summary>
	/// <param name="playerScript">The playerController script calling this method.</param>
	public abstract void OnCollisionEnd(playerController playerScript);

	/// <summary>
	/// Attempt to interact with the object by calling the script's OnInteraction().
	/// </summary>
	/// <param name="playerScript">The playerController script calling this method.</param>
	public void PlayerInteract(playerController playerScript)
	{
		OnInteraction(playerScript);
	}
}
