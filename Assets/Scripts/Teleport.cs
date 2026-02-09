using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Teleport : MonoBehaviour {
	
	public bool bLogStateChanges;

	public enum State
	{
		None,
		ReadyBlink,
		DoBlink
	}
	public State state;

	#region VARIABLES
	// PARTICLES
	public GameObject blinkParticle;
	public GameObject blinkFloorParticle;
	public Material blinkLineMat;
	public float blinkLineWidth = 10f;

	// BLINK INFO
	public Vector3 playerHeightOffset = new Vector3(0f, 0.1f, 0f); // HALF PLAYER HEIGHT + 0.1f
	public float blinkSpeed = 1f;
	public float blinkDistanceMax = 10f;
	public float blinkDistanceMin = 2f;
	public float blinkStaminaCost = 20f;
	private float blinkDistanceCurrent = 10f;
	public float scrollWheelSensitivity = 3f;

	// PARTICLES
	private GameObject blinkParticleInstance;
	private GameObject blinkParticleFloorInstance;
	private Vector3 blinkToPosition;
	public float blinkParticleSmoothSpeed = 10f;

	// RAYS
	private Ray rayToMousePos;
	private Ray rayToFloor;

	// PLAYER
	private Transform player;
	public Camera playerCamera;
	public Animation playerHandsModelAnim;

	// ANIMS
	public AnimationClip animHandsReady, animHandsReadyWait, animHandsDoBlink;
	private AnimationClip animPrevious;
	#endregion

	//============================================

	#region Events
	public delegate void StateHandler(GameObject thisGO, State state);
	public static event StateHandler StateChanged;

	public delegate void EventHandler();
	public static event EventHandler BlinkReady, BlinkDo;
	#endregion

	#region Event Subscriptions
	void OnEnable()
	{

	}
	void OnDisable()
	{

	}
	#endregion

	//============================================

	void Start()
	{
		NextState();
	}

	//============================================
	// STATES
	//============================================

	#region StateFunctions
	void NextState()
	{
		string methodName = state.ToString() + "State";
		System.Reflection.MethodInfo info = GetType().GetMethod(methodName, System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
		StartCoroutine((IEnumerator)info.Invoke(this, null));
	}
	void StateEnter()
	{
		if (bLogStateChanges)
		    Debug.Log(this.GetType().Name + " / " + state + " / " + "ENTER");

		// SEND EVENT
		if (StateChanged != null)
			StateChanged(gameObject, state);
	}
	void StateExit()
	{
		// DOESN'T WORK - NEED TO SAVE PreviousState SOMEWHERE
		//if (bLogStateChanges)
		//    Debug.Log(this.GetType().Name + " / " + previousState + " / " + "EXIT");
	}
	#endregion

	IEnumerator NoneState()
	{
		StateEnter();

		while (state == State.None)
		{
			// WHEN RMB IS PRESSED
			if (Input.GetMouseButtonDown(1))
			{
				state = State.ReadyBlink;
			}

			yield return null;
		}

		StateExit();

		NextState();
	}

	IEnumerator ReadyBlinkState()
	{
		StateEnter();

		// SEND EVENT
		if (BlinkReady != null) BlinkReady();

		// INTANTIATE PARTICLES
		blinkParticleInstance = Instantiate(blinkParticle, Vector3.zero, Quaternion.identity) as GameObject;
		blinkParticleFloorInstance = Instantiate(blinkFloorParticle, Vector3.zero, Quaternion.identity) as GameObject;

		// SET blinkDistanceCurrent
		blinkDistanceCurrent = blinkDistanceMax;

		// CREATE LINE
		Vector3[] linePoints = new Vector3[] { blinkParticleInstance.transform.position, blinkParticleFloorInstance.transform.position };
		//VectorLine line = new VectorLine("MyLine", linePoints, blinkLineMat, blinkLineWidth);

		while (state == State.ReadyBlink)
		{
			// WHILE RMB IS HELD DOWN
			if (Input.GetMouseButton(1))
			{
				// POSITION PARTICLE TOP
				// IF RAYCAST HITS, POSITION AT HIT POINT, ELSE, POSITION AT END OF RAY
				if (RaycastHitPosition() == Vector3.zero)
				{
					blinkParticleInstance.transform.position = Vector3.Lerp(blinkParticleInstance.transform.position, rayToMousePos.GetPoint(blinkDistanceCurrent), blinkParticleSmoothSpeed * Time.deltaTime);
				}
				else
				{
					blinkParticleInstance.transform.position = Vector3.Lerp(blinkParticleInstance.transform.position, RaycastHitPosition() + RaycastHitInfo().normal * 0.6f, blinkParticleSmoothSpeed * Time.deltaTime);
				}

				// POSITION PARTICLE FLOOR
				if (RaycastFloorPosition() == Vector3.zero)
				{
					blinkParticleFloorInstance.transform.position = Vector3.Lerp(blinkParticleFloorInstance.transform.position, rayToFloor.GetPoint(100f), blinkParticleSmoothSpeed * Time.deltaTime);
				}
				else
				{
					blinkParticleFloorInstance.transform.position = Vector3.Lerp(blinkParticleFloorInstance.transform.position, RaycastFloorPosition() + Vector3.up * 0.5f, blinkParticleSmoothSpeed * Time.deltaTime);
				}

				// STORE TARGET POSITION
				blinkToPosition = blinkParticleInstance.transform.position;

				// CANCEL
				if (Input.GetKeyDown(KeyCode.F))
				{
					state = State.None;
				}

				// MOUSE SCROLL DISTANCE
				blinkDistanceCurrent += Input.GetAxis("Mouse ScrollWheel") * scrollWheelSensitivity;
				// MOUSE SCROLL DISTANCE CLAMP
				blinkDistanceCurrent = Mathf.Clamp(blinkDistanceCurrent, blinkDistanceMin, blinkDistanceMax);

				// DRAW LINE
				linePoints[0] = blinkParticleInstance.transform.position;
				linePoints[1] = blinkParticleFloorInstance.transform.position;
				//line.SetTextureScale(1.0f);
				//line.Draw3D();
			}
			else
			{
				if (blinkToPosition != Vector3.zero)
					state = State.DoBlink;
				else
					state = State.None;
			}

			yield return null;
		}

		// DESTROY PARTICLES/LINE
		Destroy(blinkParticleInstance);
		Destroy(blinkParticleFloorInstance);
		//VectorLine.Destroy(ref line);

		StateExit();

		NextState();
	}

	IEnumerator DoBlinkState()
	{
		StateEnter();

		// HAS STAMINA SCRIPT?
		/*if (GetComponent<StaminaScript>() != null)
		{
			StaminaScript staminaScript = GetComponent<StaminaScript>();

			// IF STAMINA IS LESS THAN REQUIRED
			if (staminaScript.staminaCurrent < blinkStaminaCost)
			{
				// EXIT COROUTINE, WITHOUT BLINKING
				state = State.None;
				NextState();
				yield break;
			}
		}*/

		// SEND EVENT
		if (BlinkDo != null) BlinkDo();

		// ANIM BLINK
		playerHandsModelAnim.CrossFade(animHandsDoBlink.name);

		// GET PLAYER
		player = GameObject.FindGameObjectWithTag("Player").transform;

		// MOVE PLAYER
		StopAllCoroutines();
		StartCoroutine(MovePlayer());

		// RESET STATE
		state = State.None;

		while (state == State.DoBlink)
		{
			yield return null;
		}

		StateExit();

		NextState();
	}

	//============================================
	// FUNCTIONS
	//============================================

	Vector3 RaycastHitPosition()
	{
		rayToMousePos = Camera.main.ScreenPointToRay(Input.mousePosition);

		RaycastHit hitInfo;

		if (Physics.Raycast(rayToMousePos, out hitInfo, blinkDistanceCurrent))
		{
			return hitInfo.point;
		}
		else
		{
			return Vector3.zero;
		}
	}
	RaycastHit RaycastHitInfo()
	{
		rayToMousePos = Camera.main.ScreenPointToRay(Input.mousePosition);

		RaycastHit hitInfo;

		if (Physics.Raycast(rayToMousePos, out hitInfo, blinkDistanceCurrent))
		{
			return hitInfo;
		}
		else
		{
			return hitInfo;
		}
	}

	Vector3 RaycastFloorPosition()
	{
		rayToFloor = new Ray(blinkParticleInstance.transform.position, -Vector3.up);

		RaycastHit hitInfo;

		if (Physics.Raycast(rayToFloor, out hitInfo, 100f))
		{
			return hitInfo.point;
		}
		else
		{
			return Vector3.zero;
		}

	}

	IEnumerator MovePlayer()
	{
		// DISABLE PLAYER'S CHARACTER CONTROLLER
		player.GetComponent<CharacterController>().enabled = false;
		playerCamera.enabled = false;
		//playerCamera.MouseAcceleration = false;

		// GET PLAYERS START POSITION
		Vector3 startPos = player.transform.position;

		// CREATE NEW BLINK TARGET POSITION
		Vector3 newBlinkToPosition = blinkToPosition;

		float animatedValue = 0f;

		while (animatedValue < 1f)
		{
			animatedValue += blinkSpeed * Time.deltaTime;

			// MOVE PLAYER
			player.transform.position = Vector3.Lerp(startPos, newBlinkToPosition + playerHeightOffset, animatedValue);

			yield return null;
		}

		// ENABLE PLAYER'S CHARACTER CONTROLLER
		player.GetComponent<CharacterController>().enabled = true;
		playerCamera.enabled = true;
		//playerCamera.MouseAcceleration = false;

		yield return null;
	}

	//============================================
	// RECEIVERS
	//============================================



}