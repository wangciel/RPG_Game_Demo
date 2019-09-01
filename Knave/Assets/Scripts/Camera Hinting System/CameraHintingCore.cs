using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;


public class CameraHintingCore : MonoBehaviour {
	
	/************************
	 *	Editor Variables	*
	 ************************/
	[Header("Camera Position Settings")]
		[Tooltip("The default vertical offset that the camera returns to when not influenced by a Camera Hint")]
		[SerializeField]
		private float DefaultYOffset = 0.27f; //0.27 felt like a pretty good default balance but it interacts weird.
	
	[Header("Damage Effect Application")]
		[Tooltip("How strongly the damage effect will be blended in.\nExpects 0.0 ~ 1.0.")]
		[Range(0f, 1f)]
		[SerializeField]
		private float FilterBlendStrength = 1;
	
	[Header("Node Behaviour Settings")]
	[Tooltip("Which layers will dull the node behaviour?")]
	[SerializeField]
	private LayerMask NodeBlockersMask;

	[FormerlySerializedAs("BlockedNodeMultiplier")] [Tooltip("Base multiplier for blocked nodes.")] [SerializeField] [Range(0f, 1f)]
	private float BaseBlockedNodeStrengthMultiplier = 0.5f;

	
	/************************
	*	Private Variables	*
	************************/
	
	//graphics guts stuff
	private Material _cameraMaterial;
	
	//position / hint stuff
	private Transform _cameraTransform;

	private List<CameraHintNode> _hintNodes;
	private GameObject _playerGameObject;

	//palette definitions
	private Color[] PaletteShades =
	{
		new Color(0, 0, 0)/255,
		new Color(34, 32, 52)/255,
		new Color(69, 40, 60)/255,
		new Color(102, 57, 49)/255,
		new Color(143, 86, 59)/255,
		new Color(223, 113, 38)/255,
		new Color(217, 160, 102)/255,
		new Color(238, 195, 154)/255,
		new Color(251, 242, 54)/255,
		new Color(153, 229, 80)/255,
		new Color(106, 190, 48)/255,
		new Color(55, 148, 110)/255,
		new Color(75, 105, 47)/255,
		new Color(82, 75, 36)/255,
		new Color(50, 60, 57)/255,
		new Color(63, 63, 116)/255,
		new Color(48, 96, 130)/255,
		new Color(91, 110, 225)/255,
		new Color(99, 155, 255)/255,
		new Color(95, 205, 228)/255,
		new Color(203, 219, 252)/255,
		new Color(255, 255, 255)/255,
		new Color(155, 173, 183)/255,
		new Color(132, 126, 135)/255,
		new Color(105, 106, 106)/255,
		new Color(89, 86, 82)/255,
		new Color(118, 66, 138)/255,
		new Color(172, 50, 50)/255,
		new Color(217, 87, 99)/255,
		new Color(215, 123, 186)/255,
		new Color(143, 151, 74)/255,
		new Color(138, 111, 48)/255
	};
	
	private void Awake()
	{
		_cameraMaterial = new Material(Shader.Find("Hidden/CameraPaletteFilter"));
		_cameraMaterial.SetColorArray("_Colours", PaletteShades);
	}

	private void Start()
	{

		//get the player's transform.
		_playerGameObject = transform.parent.gameObject;
		
		//get the player's camera transform. This is for code clarity.
		_cameraTransform = transform;
		
		//reset the camera position in case it's been messed with.
		_cameraTransform.localPosition = new Vector3(0, DefaultYOffset, _cameraTransform.localPosition.z);
		
		//find and store all the nodes
		_hintNodes = new List<CameraHintNode>(FindObjectsOfType<CameraHintNode>());
		
	}
	
	//Camera position hinting stuff happens here
	void Update ()
	{
		var playerPos = _playerGameObject.transform.position;

		//do the camera position magic
		int nodesCount = 1;
		Vector3 targetPosition = new Vector3(playerPos.x, playerPos.y + DefaultYOffset, 0);
		
		foreach (CameraHintNode n in _hintNodes) 
		{
			Vector3 nodePos = n.transform.position;
			var distance = Vector2.Distance(playerPos, nodePos);
			if (n.gameObject.activeInHierarchy && n.enabled && distance < n.InfluenceRadius && distance > n.DeadzoneRadius)
			{
				float strength = n.InfluenceStrength;
				nodesCount++;
				if (n.Repel)
				{
					nodePos -= (nodePos - playerPos) * 2;
				}

				if (Physics2D.Linecast(n.transform.position, playerPos, NodeBlockersMask))
				{
					strength *= BaseBlockedNodeStrengthMultiplier * n.BlockedNodeMultiplier;
				}
				
				targetPosition +=
					Vector3.Lerp(new Vector3(playerPos.x, playerPos.y + DefaultYOffset, 0),
							new Vector3(nodePos.x, nodePos.y + DefaultYOffset, 0),
						strength * (n.SmoothEnterAndExit ? 1 - Math.Abs((distance - n.DeadzoneRadius) / (n.InfluenceRadius - n.DeadzoneRadius) - 0.5f ) / 0.5f : 1f));
			}

			if (n.DeactivateWhenReached && (distance < n.DeadzoneRadius || distance < 0.15))
			{
				n.enabled = false;
			}
		}
		targetPosition /= nodesCount;

		_cameraTransform.position 
			= Vector3.Lerp(new Vector3 (playerPos.x, playerPos.y + DefaultYOffset, _cameraTransform.position.z), Vector3.Lerp(_cameraTransform.position,
						   new Vector3(targetPosition.x, targetPosition.y, _cameraTransform.position.z),
						   0.02f), 0.975f);

	}

	//Atmosphere System stuff happens here
	private void OnRenderImage(RenderTexture source, RenderTexture destination)
	{
		if (FilterBlendStrength <= 0.0f) {
			Graphics.Blit(source, destination);
			return;
		}
		//update the shader's values here
		_cameraMaterial.SetFloat("_FilterBlendStrength", FilterBlendStrength);
		//end shader values update
		
		//push it to the screen!
		Graphics.Blit(source, destination, _cameraMaterial);

		if (FilterBlendStrength > 0.05f)
		{
			FilterBlendStrength *= 0.99f;
		}
		else
		{
			FilterBlendStrength = 0f;
		}
	}

	public void SetHurtEffect(float f)
	{
		FilterBlendStrength = f;
		f *= 0.2f;
		f *= f;
		transform.position += new Vector3(Random.Range(-f, f), Random.Range(-f*0.4f, f*0.4f), 0);
		
	}
}



/****************************************************************************************************
 * Matthew's notes to self, but you can read them too:												*
 ****************************************************************************************************
 * 
 *	A good default camera Y offset is '0.27' I think.
 *		This is felt like a sweet-spot to me between the needs to show both
 *		platforms the player could feasibly jump to, and areas they could jump down to.
 *	
 *	For the time being this is just a rudimentary 'gravity'-based system.
 * 		It's not very elegant in terms of engineering, but it should be intuitive to the other designers.
 * 		Essentially, a 'camera hint' can either attract or repel the camera offset.
 * 			The player acts as a very strong attractor.
 * 			Enemies could also be weak attractors? That'd be neat!
 *
 *
 *
 ****************************************************************************************************
 * 	Unrelated to camera stuff that I need to write down somewhere and should put in a blog later:	*
 ****************************************************************************************************
 *
 *	Unionise gamedev.
 *
 *
 *	Some enemy design thoughts:
 * 		Enemies will have two "patrol nodes" that they patrol between.
 * 			These would be set in the editor and hopefully locked in the y-axis to make enemy placement much easier.
 * 		Enemies could have two trigger boxes they check: their attack trigger, and their awareness trigger.
 * 			The attack trigger is a small region directly in front of them.
 * 				If you're within this box, they'll stop and attempt to swing their weapon at you.
 * 			The awareness trigger is a larger area that starts *a little* behind them and ends far in front of them.
 * 				If you are within this box and the enemy is in their 'unaware' state,
 * 					they'll stop, express alarm, then start chasing you to try to get you in their attack trigger.
 * 		The broader enemy AI states are thus as follows:
 * 			"Chasing" - Which they will enter immediately after Alarmed. If you leave awareness they will enter Reassessing.
 * 			"Attacking" - where they're swinging their weapon, and will return to their chasing state. Pause before and after the attack though!
 * 			"Unaware" - where they will simply patrol to their next destination before entering Reassessing
 *			"Alarmed" - which they will enter if you enter their awareness during Unaware (and a raycast is successful)
 * 			"Reassessing" - where they will stop for a moment before deciding what to do next.
 * 		This sort of rework will allow us to let enemies hit much harder without being too brutal (two hearts?)
 * 
 */
