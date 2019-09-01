using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraHintNode : MonoBehaviour
{
	[Space]
	[Header("Editor Settings")]
		[Tooltip("Visualise the position hint influence ranges in the editor.")]
		public bool ShowHintingRange = true;
		[Tooltip("Hide the preview sphere that shows the hint node in the editor.")]
		public bool HideNodePoint = false;

	[Header("Camera Position Hinting Settings")]
		[Tooltip("Distance from which the Camera Hint will begin to affect the camera.")]
		public float InfluenceRadius = 3f;
		
		[Tooltip("Distance from which the Camera Hint will stop affecting the player.")]
		public float DeadzoneRadius = 1f;
	
		[Tooltip("If enabled, this node will repel the camera instead.")]
		public bool Repel;
	
		[Tooltip("If enabled, the effect strength will scale smoothly at the start and end of the active area.")]
		public bool SmoothEnterAndExit;
	
		[Range(0f, 1f)]
		[Tooltip("How strongly the Camera Hint will push or pull the camera.")]
		public float InfluenceStrength = 1f;
	
		[Range(0f, 1f)]
		[Tooltip("Multiplier to strength when view is blocked. Use 0.0f to disable hinting through walls.")]
		public float BlockedNodeMultiplier = 0.4f;
	
		[Tooltip("If enabled, getting close to the node will disable it thereafter.")]
		public bool DeactivateWhenReached;

	void OnDrawGizmos()
	{
		if (!HideNodePoint)
		{
			Gizmos.color = Repel ? Color.red : Color.white;
			Gizmos.DrawSphere(transform.position, 0.125f);
		}
		
		if (ShowHintingRange)
		{
			Gizmos.color = Repel ? Color.red : Color.white;
			Gizmos.DrawWireSphere(transform.position, DeadzoneRadius);
			Gizmos.DrawWireSphere(transform.position, InfluenceRadius);
		}
	}
}
