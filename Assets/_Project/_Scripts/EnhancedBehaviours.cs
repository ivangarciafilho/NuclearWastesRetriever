/*===================================================================
Product:    ScriptingTest
Developer:  Ivan Garcia Filho - ivan.garcia.filho@gmail.com
Company:    The Spare Partshttps://www.facebook.com/IvanGarciaFilho
Date:       30/05/2017 09:45

Please, don't distribute this code without asking me beforehand and
MY AGREEMENT! You SHALL give me the credits for using EnhancedBehaviours.cs!
Ps : EVEN FOR NON-COMMERCIAL PURPOSES! Thanks for playing fair!
====================================================================*/

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Linq.Expressions;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

namespace EnhancedBehaviours
{
	public class MyMonoBehaviour : MonoBehaviour
	{
		[DisallowMultipleComponent]
		public class MyGameManager : MyMonoBehaviour
		{
			public static MyGameManager gameManagerInstance { get; private set; }
			[SerializeField] [Range( 0, 30 )] private int defaultGlobalFrameskipping = 0;
			private static int currentFrameSkipping;
			private static int framesToNextGlobalUpdate;

			public static event VoidDelegate BeforeUpdate;
			public static event VoidDelegate AfterUpdate;

			[SerializeField] [Range( 1, 90 )] private int slowUpdateFrameSkipping = 3;
			public static event VoidDelegate BeforeSlowUpdate;
			public static event VoidDelegate AfterSlowUpdate;
			private static int framesToSlowUpdate = 0;

			public static event VoidDelegate BeforeLateUpdate;
			public static event VoidDelegate AfterLateUpdate;

			[SerializeField] [Range(2, 180)] private int slowLateUpdateFrameSkipping = 9;
			public static event VoidDelegate BeforeSlowLateUpdate;
			public static event VoidDelegate AfterSlowLateUpdate;
			private static int framesToSlowLateUpdate = 0;

			public static event VoidDelegate BeforeFixedUpdate;
			public static event VoidDelegate AfterFixedUpdate;

			[SerializeField] [Range(1f, 30f)] private float slowFixedUpdateDelay = 2f;
			public static event VoidDelegate BeforeSlowFixedUpdate;
			public static event VoidDelegate AfterSlowFixedUpdate;
			private static float nextSlowFixedUpdate = 0f;
			private bool fixedUpdateLogicAlreadyExecutedThisFrame = false;

			[SerializeField] [Range(2f, 30f)] private float verySlowFixedUpdateDelay = 6f;
			public static event VoidDelegate BeforeVerySlowFixedUpdate;
			public delegate void AfterVerySlowFixedUpdateDelegate();
			public static event VoidDelegate AfterVerySlowFixedUpdate;
			private static float nextVerySlowFixedUpdate = 0f;

			public static event VoidDelegate Execute15TimesPerSecond;
			public const float delayBetweenFramesTo15ExecutionsPerSecond = 1 / 15;
			private static float nextExecutionOf15FramesPerSecond;

			public static event VoidDelegate Execute30TimesPerSecond;
			public const float delayBetweenFramesTo30ExecutionsPerSecond = 1 / 30;
			private static float nextExecutionOf30FramesPerSecond;

			public static event VoidDelegate Execute60TimesPerSecond;
			public const float delayBetweenFramesTo60ExecutionsPerSecond = 1 / 60;
			private static float nextExecutionOf60FramesPerSecond;

			public static event VoidDelegate OnAlternateUpdate0;
			public static event VoidDelegate OnAlternateUpdate1;
			private static bool currentAlternateUpdate = false;

			public static List<VoidDelegate> lazyUpdateDelegates = new List<VoidDelegate>();
			static int currentLazyUpdateDelegate = 0;


			[SerializeField] private float maximumLogicExecutionTime = 1 / 30f;
			public static float currentRenderRate { get; private set; }
			public static float currentLogicExecutionRate { get; private set; }
			private float previousLogicExecutionEndTime = 0f;
			private float currentEllapsedRealtime = 0f;

			private float currentLogicExecutionEllapsedTime
			{
				get
				{
					currentEllapsedRealtime = Time.realtimeSinceStartup;
					return currentEllapsedRealtime - previousLogicExecutionEndTime;
				}
			}

			private bool exceededMaximumLogicExecutionTime
			{
				get { return currentLogicExecutionEllapsedTime > maximumLogicExecutionTime; }
			}

			protected virtual void Awake()
			{
				if (gameManagerInstance != null)
					if (gameManagerInstance != this)
						DestroyImmediate(this);

				gameManager = gameManagerInstance = this;
				DontDestroyOnLoad(gameObject);

				slowUpdateFrameSkipping = Mathf.Clamp(slowUpdateFrameSkipping, 1, 90);
				slowLateUpdateFrameSkipping = Mathf.Clamp(slowLateUpdateFrameSkipping, 2, 180);
				framesToSlowUpdate = Mathf.Clamp(slowUpdateFrameSkipping, 1, framesToSlowLateUpdate);
				framesToSlowLateUpdate = Mathf.Clamp(slowLateUpdateFrameSkipping, framesToSlowUpdate, 60);

				slowFixedUpdateDelay = Mathf.Clamp(slowFixedUpdateDelay,1,30);
				verySlowFixedUpdateDelay = Mathf.Clamp(verySlowFixedUpdateDelay, 1, 90);
				nextSlowFixedUpdate = Time.time + Mathf.Clamp( slowFixedUpdateDelay, 1, verySlowFixedUpdateDelay);
				nextVerySlowFixedUpdate = Time.time + Mathf.Clamp(slowFixedUpdateDelay, slowFixedUpdateDelay, 90);

				currentFrameSkipping = defaultGlobalFrameskipping;

				AfterLateUpdate += ClearGarbage;
			}

			protected virtual void Update()
			{
				timeScale = Time.timeScale;
				if (timeScale == 0) return;

				time = Time.time;

				deltatime = Time.deltaTime;
				halfDeltatime = deltatime / 2f;
				twiceDeltatime = deltatime * 2f;
				thriceDeltatime = deltatime * 3f;

				smoothDeltatime = Time.smoothDeltaTime;
				halfSmoothDeltatime = smoothDeltatime / 2f;
				twiceSmoothDeltatime = smoothDeltatime * 2f;
				thriceSmoothDeltatime = smoothDeltatime * 3f;

				currentRenderRate = 1f/deltatime;

				verticalAxisInputKey = Input.GetKey(KeyCode.W) ? 1 : Input.GetKey(KeyCode.S) ? -1 : 0;
				horizontalAxisInputKey = Input.GetAxis("Horizontal");
				vector3InputKeyAxis.x = verticalAxisInputKey;
				vector3InputKeyAxis.z = horizontalAxisInputKey;
				verticalAxisInputMouse = Input.GetAxis("Mouse Y");
				horizontalAxisInputmouse = Input.GetAxis("Mouse X");
				vector2InputMouseAxis.x = horizontalAxisInputmouse;
				vector2InputMouseAxis.y = verticalAxisInputMouse;
				mouseWheelScrollAxis = Input.GetAxis("Mouse ScrollWheel");

				fixedUpdateLogicAlreadyExecutedThisFrame = false;

				if ( framesToNextGlobalUpdate > 0 ){framesToNextGlobalUpdate--; return;}
				framesToNextGlobalUpdate = currentFrameSkipping;

				if (BeforeUpdate != null) BeforeUpdate();
				if (AfterUpdate != null) AfterUpdate();

				if (exceededMaximumLogicExecutionTime) { return; }

				if (currentAlternateUpdate) { if (OnAlternateUpdate1 != null) OnAlternateUpdate1(); }
				else { if (OnAlternateUpdate0 != null) OnAlternateUpdate0(); }
				currentAlternateUpdate = !currentAlternateUpdate;

				if (exceededMaximumLogicExecutionTime) { return; }

				if ( framesToSlowUpdate > 0 ){ framesToSlowUpdate--; return;}
				framesToSlowUpdate = slowUpdateFrameSkipping;

				if ( BeforeSlowUpdate != null ) BeforeSlowUpdate();
				if ( AfterSlowUpdate != null) AfterSlowUpdate();
			}

			protected virtual void LateUpdate()
			{
				previousLogicExecutionEndTime = Time.realtimeSinceStartup;

				int delegatesToExecuteOnCurrentFrame = (int)Mathf.Ceil(currentRenderRate / (float)(lazyUpdateDelegates.Count));

				int i = 0;
				int totalAmountOfLazyDelegates = lazyUpdateDelegates.Count;
				for (i = currentLazyUpdateDelegate; i < delegatesToExecuteOnCurrentFrame; i++)
				{
					if (i < totalAmountOfLazyDelegates && !exceededMaximumLogicExecutionTime)
					{
						lazyUpdateDelegates[i]();
					}
					else
					{
						currentLazyUpdateDelegate = 0;
						break;
					}
				}
				currentLazyUpdateDelegate = i;

				if (exceededMaximumLogicExecutionTime) { return; }
				if (framesToNextGlobalUpdate > 0) return;

				if (BeforeLateUpdate != null) BeforeLateUpdate();
				if (AfterLateUpdate != null) AfterLateUpdate();

				if (exceededMaximumLogicExecutionTime) { return; }

				if (framesToSlowLateUpdate > 0){framesToSlowLateUpdate--; return;}
				framesToSlowLateUpdate = slowLateUpdateFrameSkipping;

				if (BeforeSlowLateUpdate != null) BeforeSlowLateUpdate();
				if (AfterSlowLateUpdate != null) AfterSlowLateUpdate();
			}

			protected virtual void FixedUpdate()
			{
				fixedDeltatime = Time.fixedDeltaTime;

				if (fixedUpdateLogicAlreadyExecutedThisFrame){ return; }
				fixedUpdateLogicAlreadyExecutedThisFrame = true;


				if (BeforeFixedUpdate != null) BeforeFixedUpdate();
				if (AfterFixedUpdate != null) AfterFixedUpdate();

				if (exceededMaximumLogicExecutionTime) { return; }

				if (time > nextExecutionOf15FramesPerSecond)
				{
					nextExecutionOf15FramesPerSecond = time + delayBetweenFramesTo15ExecutionsPerSecond;
					if (Execute15TimesPerSecond != null) Execute15TimesPerSecond();

					if (time > nextExecutionOf30FramesPerSecond)
					{
						nextExecutionOf30FramesPerSecond = time + delayBetweenFramesTo30ExecutionsPerSecond;
						if (Execute30TimesPerSecond != null) Execute30TimesPerSecond();

						if (time > nextExecutionOf60FramesPerSecond)
						{
							nextExecutionOf60FramesPerSecond = time + delayBetweenFramesTo60ExecutionsPerSecond;
							if (Execute60TimesPerSecond != null) Execute60TimesPerSecond();
						}
					}
				}

				if (exceededMaximumLogicExecutionTime) { return; }

				if (time < nextSlowFixedUpdate) return;
				nextSlowFixedUpdate = time + slowFixedUpdateDelay;

				if (BeforeSlowFixedUpdate != null) BeforeSlowFixedUpdate();
				if (AfterSlowFixedUpdate != null) AfterSlowFixedUpdate();


				if (time < nextVerySlowFixedUpdate) return;
				nextVerySlowFixedUpdate = time + verySlowFixedUpdateDelay;

				if (BeforeVerySlowFixedUpdate != null) BeforeVerySlowFixedUpdate();
				if (AfterVerySlowFixedUpdate != null) AfterVerySlowFixedUpdate();
			}

			protected virtual void SetFrameskipping(int frames = 1)
			{
				if (frames < 0) return;
				currentFrameSkipping = frames;
			}

			private void ClearGarbage()
			{
				System.GC.Collect();
				AfterLateUpdate -= ClearGarbage;
			}
		}

		[DisallowMultipleComponent]
		public class MyProtagonist : MyMonoBehaviour
		{
			public static MyProtagonist protagonistInstance { get; private set; }
			protected virtual void Awake()
			{
				if (protagonistInstance != null)
					if (protagonistInstance != this)
						DestroyImmediate(this);

				protagonist = protagonistInstance = this;
				DontDestroyOnLoad(gameObject);
			}
		}

		[DisallowMultipleComponent]
		public class MyMainViewport : MyMonoBehaviour
		{
			public static MyMainViewport viewportInstance { get; private set; }
			public static Camera itsCamera { get; private set; }

			protected virtual void Awake()
			{
				if (viewportInstance != null)
					if (viewportInstance != this)
						DestroyImmediate(this);

				mainViewport = viewportInstance = this;
				DontDestroyOnLoad(gameObject);

				itsCamera = GetComponentsInChildren<Camera>()[0];
			}
		}


		[HideInInspector]   public int itsInstanceID;

							[Header("Behaviour Properties : ")]
							//Shortcut to main Entities
							protected static MyGameManager    gameManager;
							protected static MyProtagonist    protagonist;
							protected static MyMainViewport   mainViewport;

		//Shortcuts to gameobject's statistics
		[HideInInspector]   public GameObject   itsGameobject;
		[HideInInspector]   public int          itsGameobjectID;
							protected static    Dictionary<GameObject, MyMonoBehaviour> behaviourByGameobject;

		[HideInInspector]   public Transform    itsTransform;
		[HideInInspector]   public int          itsChildCount;
		[HideInInspector]   public int          itsTransformID;
							protected static    Dictionary<Transform, MyMonoBehaviour>  behaviourByTransform;

		[HideInInspector]   public Vector3      itsPosition;
		[HideInInspector]   public Vector2      its2DPosition;
		[HideInInspector]   public Vector3      itsLocalPosition;
		[HideInInspector]   public Vector2      its2DLocalPosition;
		[HideInInspector]   public Quaternion   itsRotation;
		[HideInInspector]   public Quaternion   itsYaw;
		[HideInInspector]   public Quaternion   itsPitch;
		[HideInInspector]   public Quaternion   itsRoll;
		[HideInInspector]   public Quaternion   itsLocalRotation;
		[HideInInspector]   public Quaternion   itsLocalYaw;
		[HideInInspector]   public Quaternion   itsLocalPitch;
		[HideInInspector]   public Quaternion   itsLocalRoll;
		[HideInInspector]   public Vector3      itsEulerRotation;
		[HideInInspector]   public Vector3      itsYawEulerRotation;
		[HideInInspector]   public Vector3      itsPitchEulerRotation;
		[HideInInspector]   public Vector3      itsRollEulerRotation;
		[HideInInspector]   public Vector3      itsEulerLocalRotation;
		[HideInInspector]   public Vector3      itsYawEulerLocalRotation;
		[HideInInspector]   public Vector3      itsPitchEulerLocalRotation;
		[HideInInspector]   public Vector3      itsRollEulerLocalRotation;
		[HideInInspector]   public Vector3      itsForward;
		[HideInInspector]   public Vector3      itsRight;
		[HideInInspector]   public Vector3      itsUp;

		[HideInInspector]   public bool         hasRigidbody;
		[HideInInspector]   public Rigidbody    itsRigidbody;
		[HideInInspector]   public int          itsRigidbodyID;
							protected static    Dictionary<Rigidbody, MyMonoBehaviour>      behaviourByRigidbodies;
							protected static    Dictionary<Collider, Rigidbody>             rigidbodyByCollider;
							protected static    Dictionary<Collider, Rigidbody>             rigidbodyByTrigger;
							protected static    Dictionary<NavMeshAgent, Rigidbody>         rigidbodyByNavmeshAgent;
							protected static    Dictionary<Animator, Rigidbody>             rigidbodyByAnimator;
							protected static    Dictionary<GameObject, Rigidbody>           rigidbodyByGameobject;
							protected static    Dictionary<Transform, Rigidbody>            rigidbodyByTransform;

		[HideInInspector]   public Vector3      itsCenterOfMass;
		[HideInInspector]   public Vector3      itsVelocity;
		[HideInInspector]   public Vector3      itsAngularVelocity;

		[HideInInspector]   public bool         hasColliders;
		[HideInInspector]   public Collider[]   itsColliders;
		[HideInInspector]   public int[]        itsCollidersIDs;
		[HideInInspector]   public int          amountOfColliders;
							protected static    Dictionary<Collider, MyMonoBehaviour>       behaviourByColliders;
							protected static    Dictionary<Rigidbody, Collider[]>           collidersByRigidbody;
							protected static    Dictionary<NavMeshAgent, Collider[]>        collidersByNavmeshAgent;
							protected static    Dictionary<Animator, Collider[]>            collidersByAnimator;
							protected static    Dictionary<GameObject, Collider[]>          collidersByGameobject;
							protected static    Dictionary<Transform, Collider[]>           collidersByTransform;

		[HideInInspector]   public bool         hasTriggers;
		[HideInInspector]   public Collider[]   itsTriggers;
		[HideInInspector]   public int[]        itsTriggersIDs;
		[HideInInspector]   public int          amountOfTriggers;
							protected static    Dictionary<Collider, MyMonoBehaviour>       behaviourByTrigger;
							protected static    Dictionary<Rigidbody, Collider[]>           triggersByRigidbody;
							protected static    Dictionary<NavMeshAgent, Collider[]>        triggersByNavmeshAgent;
							protected static    Dictionary<Animator, Collider[]>            triggersByAnimator;
							protected static    Dictionary<GameObject, Collider[]>          triggersByGameobject;
							protected static    Dictionary<Transform, Collider[]>           triggersByTransform;

		[HideInInspector]   public bool         hasAnimator;
		[HideInInspector]   public Animator     itsAnimator;
		[HideInInspector]   public int          itsAnimatorID;
							protected static    Dictionary<Animator, MyMonoBehaviour>       behaviourByAnimator;
							protected static    Dictionary<Rigidbody, Animator>             animatorByRigidbody;
							protected static    Dictionary<Collider, Animator>              animatorByCollider;
							protected static    Dictionary<Collider, Animator>              animatorByTrigger;
							protected static    Dictionary<NavMeshAgent, Animator>          animatorByNavmeshAgent;
							protected static    Dictionary<GameObject,Animator>             animatorByGameobject;
							protected static    Dictionary<Transform, Animator>             animatorByTransform;

		[HideInInspector]   public bool         hasNavmeshAgent;
		[HideInInspector]   public NavMeshAgent itsNavmeshAgent;
		[HideInInspector]   public int          itsNavMeshAgentID;
							protected static    Dictionary<NavMeshAgent, MyMonoBehaviour>   behaviourByNavmeshAgent;
							protected static    Dictionary<Rigidbody, NavMeshAgent>         navmeshAgentByRigidbody;
							protected static    Dictionary<Collider, NavMeshAgent>          navmeshAgentByCollider;
							protected static    Dictionary<Collider, NavMeshAgent>          navmeshAgentByTrigger;
							protected static    Dictionary<Animator, NavMeshAgent>          navmeshAgentByAnimator;
							protected static    Dictionary<GameObject, NavMeshAgent>        navmeshAgentByGameobject;
							protected static    Dictionary<Transform, NavMeshAgent>         navmeshAgentByTransform;

							protected static readonly Vector3 vector3Zero = new Vector3(0f, 0f, 0f);
							protected static readonly Vector3 vector3One = new Vector3(1f, 1f, 1f);
							protected static readonly Vector3 vector3NegativeOne = new Vector3(-1f, -1f, -1f);
							protected static readonly Vector3 vector3Up = new Vector3(0f, 1f, 0f);
							protected static readonly Vector3 vector3UpForward = new Vector3(0f, 1f, 1f);
							protected static readonly Vector3 vector3UpBack = new Vector3(0f, 1f, -1f);
							protected static readonly Vector3 vector3UpRight = new Vector3(1f, 1f, 0f);
							protected static readonly Vector3 vector3UpLeft = new Vector3(-1f, 1f, 0f);
							protected static readonly Vector3 vector3Down = new Vector3(0f, -1f, 0f);
							protected static readonly Vector3 vector3DownForward = new Vector3(0f, -1f, 1f);
							protected static readonly Vector3 vector3DownBack = new Vector3(0f, -1f, -1f);
							protected static readonly Vector3 vector3DownLeft = new Vector3(-1f, -1f, 0f);
							protected static readonly Vector3 vector3DownRight = new Vector3(1f, -1f, 0f);
							protected static readonly Vector3 vector3Forward = new Vector3(0f, 0f, 1f);
							protected static readonly Vector3 vector3ForwardLeft = new Vector3(-1f, 0f, 1f);
							protected static readonly Vector3 vector3ForwardRight = new Vector3(1f, 0f, 1f);
							protected static readonly Vector3 vector3Back = new Vector3(0f, 0f, -1f);
							protected static readonly Vector3 vector3BackLeft = new Vector3(-1f, 0f, -1f);
							protected static readonly Vector3 vector3BackRight = new Vector3(1f, 0f, -1f);
							protected static readonly Vector3 vector3Right = new Vector3(1f, 0f, 0f);
							protected static readonly Vector3 vector3Left = new Vector3(-1f, 0f, 0f);
							protected static readonly Quaternion quaternionIdentity = new Quaternion(0f, 0f, 0f, 0f);

							protected static float timeScale;
							protected static float time;
							protected static float halfDeltatime;
							protected static float deltatime;
							protected static float twiceDeltatime;
							protected static float thriceDeltatime;
							protected static float halfSmoothDeltatime;
							protected static float smoothDeltatime;
							protected static float twiceSmoothDeltatime;
							protected static float thriceSmoothDeltatime;
							protected static float fixedDeltatime;
							protected static float verticalAxisInputKey;
							protected static float verticalAxisInputMouse;
							protected static float horizontalAxisInputKey;
							protected static float horizontalAxisInputmouse;
							protected static Vector3 vector3InputKeyAxis;
							protected static Vector3 vector2InputMouseAxis;
							protected static Vector3 gravity;
							protected static float mouseWheelScrollAxis;

							public delegate void VoidDelegate();

							protected event VoidDelegate OnEnableEvent;
							protected virtual void TriggerListenersOnEnable() { if (OnEnableEvent != null) OnEnableEvent(); }
							public virtual void AddListenerOnEnable(VoidDelegate callback) { OnEnableEvent += callback;}
							public virtual void RemoveListenerOnEnable(VoidDelegate callback) { OnEnableEvent -= callback; }

							protected event VoidDelegate OnDisableEvent;
							protected virtual void TriggerListenersOnDisable() { if (OnDisableEvent != null) OnDisableEvent(); }
							public virtual void AddListenerOnDisable(VoidDelegate callback) { OnDisableEvent += callback; }
							public virtual void RemoveListenerOnDisable(VoidDelegate callback) { OnDisableEvent -= callback; }

							protected event VoidDelegate OnDestroyEvent;
							protected virtual void TriggerListenersOnDestroy() { if (OnDestroyEvent != null) OnDestroyEvent(); }
							public virtual void AddListenerOnDestroy(VoidDelegate callback) { OnDestroyEvent += callback; }
							public virtual void RemoveListenerOnDestroy(VoidDelegate callback) { OnDestroyEvent -= callback; }

							public delegate void CollisionDelegate(Collision _collision);
							public delegate void ColliderDelegate(Collider _collision);

							protected event CollisionDelegate OnCollisionEnterEvent;
							protected virtual void CallOnCollisionEnterCallbacks(Collision _collision) { if (OnCollisionEnterEvent != null) OnCollisionEnterEvent(_collision); }
							public virtual void AddListenerOnCollisionEnter(CollisionDelegate callback) { OnCollisionEnterEvent += callback; }
							public virtual void RemoveListenerOnCollisionEnter(CollisionDelegate callback) { OnCollisionEnterEvent -= callback; }

							protected event CollisionDelegate OnCollisionExitEvent;
							protected virtual void CallOnCollisionExitCallbacks(Collision _collision) { if (OnCollisionExitEvent != null) OnCollisionExitEvent(_collision); }
							public virtual void AddListenerOnCollisionExit(CollisionDelegate callback) { OnCollisionExitEvent += callback; }
							public virtual void RemoveListenerOnCollisionExit(CollisionDelegate callback) { OnCollisionExitEvent -= callback; }

							protected event ColliderDelegate OnTriggerEnterEvent;
							protected virtual void CallOnTriggerEnterCallbacks(Collider _collider) { if (OnTriggerEnterEvent != null) OnTriggerEnterEvent(_collider); }
							public virtual void AddListenerOnTriggerEnter(ColliderDelegate callback) { OnTriggerEnterEvent += callback; }
							public virtual void RemoveListenerOnTriggerEnter(ColliderDelegate callback) { OnTriggerEnterEvent -= callback; }

							protected event ColliderDelegate OnTriggerExitEvent;
							protected virtual void CallOnTriggerExitCallbacks(Collider _collider) { if (OnTriggerExitEvent != null) OnTriggerExitEvent(_collider); }
							public virtual void AddListenerOnTriggerExit(ColliderDelegate callback) { OnTriggerExitEvent += callback; }
							public virtual void RemoveListenerOnTriggerExit(ColliderDelegate  callback) { OnTriggerExitEvent -= callback; }

		protected virtual void Initialize()
		{
			itsInstanceID = GetInstanceID();

			//Cache GameObject
			if (itsGameobject == null)
				itsGameobject = gameObject;

			if (behaviourByGameobject == null)
				behaviourByGameobject = new Dictionary<GameObject, MyMonoBehaviour>();

			itsGameobjectID = itsGameobject.GetInstanceID();

			if ((behaviourByGameobject.ContainsKey(itsGameobject) == false))
				behaviourByGameobject.Add(itsGameobject, this);


			//Cache Transform
			if (behaviourByTransform == null)
				behaviourByTransform = new Dictionary<Transform, MyMonoBehaviour>();

			itsTransform = itsGameobject.GetComponent<Transform>();
			itsTransformID = itsTransform.GetInstanceID();
			itsChildCount = itsTransform.childCount;

			if ((behaviourByTransform.ContainsKey(itsTransform) == false))
				behaviourByTransform.Add(itsTransform, this);


			//Cache Gravity
			gravity = Physics.gravity;


			//Cache Rigidbody
			if (behaviourByRigidbodies == null)
				behaviourByRigidbodies = new Dictionary<Rigidbody, MyMonoBehaviour>();

			if (hasRigidbody = itsRigidbody = itsGameobject.GetComponent<Rigidbody>())
			{
				itsRigidbodyID = itsRigidbody.GetInstanceID();
				if ((behaviourByRigidbodies.ContainsKey(itsRigidbody) == false))
					behaviourByRigidbodies.Add(itsRigidbody, this);
			}


			//Cache Colliders
			if (behaviourByColliders == null)
				behaviourByColliders = new Dictionary<Collider, MyMonoBehaviour>();

			itsColliders = itsGameobject.GetComponentsInChildren<Collider>().
				Where(_collider => _collider.isTrigger == false).ToArray();

			amountOfColliders = itsColliders.Length;
			if (hasColliders = (amountOfColliders > 0))
			{
				itsCollidersIDs = new int[amountOfColliders];
				for (int i = 0; i < amountOfColliders; i++)
				{
					itsCollidersIDs[i] = itsColliders[i].GetInstanceID();
					if ((behaviourByColliders.ContainsKey(itsColliders[i]) == false))
						behaviourByColliders.Add(itsColliders[i], this);
				}
			}


			//Cache Triggers
			if (behaviourByTrigger == null)
				behaviourByTrigger = new Dictionary<Collider, MyMonoBehaviour>();

			itsTriggers = itsGameobject.GetComponentsInChildren<Collider>().
				Where(_collider => _collider.isTrigger == true).ToArray();

			amountOfTriggers = itsTriggers.Length;
			if (hasTriggers = (amountOfTriggers > 0))
			{
				itsTriggersIDs = new int[amountOfTriggers];
				for (int i = 0; i < amountOfTriggers; i++)
				{
					itsTriggersIDs[i] = itsTriggers[i].GetInstanceID();
					if ((behaviourByTrigger.ContainsKey(itsTriggers[i]) == false))
						behaviourByTrigger.Add(itsTriggers[i], this);
				}
			}


			//Cache NavMeshAgents
			if (behaviourByNavmeshAgent == null)
				behaviourByNavmeshAgent = new Dictionary<NavMeshAgent, MyMonoBehaviour>();

			if (hasNavmeshAgent = itsNavmeshAgent = itsGameobject.GetComponent<NavMeshAgent>())
			{
				itsNavMeshAgentID = itsNavmeshAgent.GetInstanceID();
				if ((behaviourByNavmeshAgent.ContainsKey(itsNavmeshAgent) == false))
					behaviourByNavmeshAgent.Add(itsNavmeshAgent, this);
			}


			//Cache Animators
			if (behaviourByAnimator == null)
				behaviourByAnimator = new Dictionary<Animator, MyMonoBehaviour>();

			if (hasAnimator = itsAnimator = itsGameobject.GetComponent<Animator>())
			{
				itsAnimatorID = itsAnimator.GetInstanceID();
				if ((behaviourByAnimator.ContainsKey(itsAnimator) == false))
					behaviourByAnimator.Add(itsAnimator, this);
			}


			//Create cross refference dictionaries

			//rigidbody
			if (rigidbodyByCollider == null)
				rigidbodyByCollider = new Dictionary<Collider, Rigidbody>();
			if (rigidbodyByTrigger == null)
				rigidbodyByTrigger = new Dictionary<Collider, Rigidbody>();
			if (rigidbodyByNavmeshAgent == null)
				rigidbodyByNavmeshAgent = new Dictionary<NavMeshAgent, Rigidbody>();
			if (rigidbodyByAnimator == null)
				rigidbodyByAnimator = new Dictionary<Animator, Rigidbody>();
			if (rigidbodyByGameobject == null)
				rigidbodyByGameobject = new Dictionary<GameObject, Rigidbody>();
			if (rigidbodyByTransform == null)
				rigidbodyByTransform = new Dictionary<Transform, Rigidbody>();

			if (hasRigidbody)
			{
				if (hasColliders)
				{
					for (int i = 0; i < amountOfColliders; i++)
					{
						if (rigidbodyByCollider.ContainsKey(itsColliders[i]) == false)
							rigidbodyByCollider.Add(itsColliders[i], itsRigidbody);
					}
				}

				if (hasTriggers)
				{
					for (int i = 0; i < amountOfTriggers; i++)
					{
						if (rigidbodyByTrigger.ContainsKey(itsTriggers[i]) == false)
							rigidbodyByTrigger.Add(itsTriggers[i], itsRigidbody);
					}
				}

				if (hasNavmeshAgent)
				{
					if (rigidbodyByNavmeshAgent.ContainsKey(itsNavmeshAgent) == false)
						rigidbodyByNavmeshAgent.Add(itsNavmeshAgent, itsRigidbody);
				}

				if (hasAnimator)
				{
					if (rigidbodyByAnimator.ContainsKey(itsAnimator) == false)
						rigidbodyByAnimator.Add(itsAnimator, itsRigidbody);
				}

				if (rigidbodyByGameobject.ContainsKey(itsGameobject) == false)
					rigidbodyByGameobject.Add(itsGameobject, itsRigidbody);

				if (rigidbodyByTransform.ContainsKey(itsTransform) == false)
					rigidbodyByTransform.Add(itsTransform, itsRigidbody);
			}

			//Colliders
			if (collidersByRigidbody == null)
				collidersByRigidbody = new Dictionary<Rigidbody, Collider[]>();
			if (collidersByNavmeshAgent == null)
				collidersByNavmeshAgent = new Dictionary<NavMeshAgent, Collider[]>();
			if (collidersByAnimator == null)
				collidersByAnimator = new Dictionary<Animator, Collider[]>();
			if (collidersByGameobject == null)
				collidersByGameobject = new Dictionary<GameObject, Collider[]>();
			if (collidersByTransform == null)
				collidersByTransform = new Dictionary<Transform, Collider[]>();

			if (hasColliders)
			{
				if (hasRigidbody)
				{
					if (collidersByRigidbody.ContainsKey(itsRigidbody) == false)
						collidersByRigidbody.Add(itsRigidbody,itsColliders);
				}

				if (hasNavmeshAgent)
				{
					if (collidersByNavmeshAgent.ContainsKey(itsNavmeshAgent) == false)
						collidersByNavmeshAgent.Add(itsNavmeshAgent, itsColliders);
				}

				if (hasAnimator)
				{
					if (collidersByAnimator.ContainsKey(itsAnimator) == false)
						collidersByAnimator.Add(itsAnimator,itsColliders);
				}

				if (collidersByGameobject.ContainsKey(itsGameobject) == false)
					collidersByGameobject.Add(itsGameobject,itsColliders);

				if (collidersByTransform.ContainsKey(itsTransform) == false)
					collidersByTransform.Add(itsTransform, itsColliders);
			}

			//Triggers
			if (triggersByRigidbody == null)
				triggersByRigidbody = new Dictionary<Rigidbody, Collider[]>();
			if (triggersByNavmeshAgent == null)
				triggersByNavmeshAgent = new Dictionary<NavMeshAgent, Collider[]>();
			if (triggersByAnimator == null)
				triggersByAnimator = new Dictionary<Animator, Collider[]>();
			if (triggersByGameobject == null)
				triggersByGameobject = new Dictionary<GameObject, Collider[]>();
			if (triggersByTransform == null)
				triggersByTransform = new Dictionary<Transform, Collider[]>();

			if (hasTriggers)
			{
				if (hasRigidbody)
				{
					if (triggersByRigidbody.ContainsKey(itsRigidbody) == false)
						triggersByRigidbody.Add(itsRigidbody, itsTriggers);
				}

				if (hasNavmeshAgent)
				{
					if (triggersByNavmeshAgent.ContainsKey(itsNavmeshAgent) == false)
						triggersByNavmeshAgent.Add(itsNavmeshAgent, itsTriggers);
				}

				if (hasAnimator)
				{
					if (triggersByAnimator.ContainsKey(itsAnimator) == false)
						triggersByAnimator.Add(itsAnimator, itsTriggers);
				}

				if (triggersByGameobject.ContainsKey(itsGameobject) == false)
					triggersByGameobject.Add(itsGameobject, itsTriggers);

				if (triggersByTransform.ContainsKey(itsTransform) == false)
					triggersByTransform.Add(itsTransform, itsTriggers);
			}

			//Navmesh Agents
			if (navmeshAgentByRigidbody == null)
				navmeshAgentByRigidbody = new Dictionary<Rigidbody, NavMeshAgent>();
			if (navmeshAgentByCollider == null)
				navmeshAgentByCollider = new Dictionary<Collider, NavMeshAgent>();
			if (navmeshAgentByTrigger == null)
				navmeshAgentByTrigger = new Dictionary<Collider, NavMeshAgent>();
			if (navmeshAgentByAnimator == null)
				navmeshAgentByAnimator = new Dictionary<Animator, NavMeshAgent>();
			if (navmeshAgentByGameobject == null)
				navmeshAgentByGameobject = new Dictionary<GameObject, NavMeshAgent>();
			if (navmeshAgentByTransform == null)
				navmeshAgentByTransform = new Dictionary<Transform, NavMeshAgent>();

			if (hasNavmeshAgent)
			{
				if (hasRigidbody)
				{
					if (navmeshAgentByRigidbody.ContainsKey(itsRigidbody) == false)
						navmeshAgentByRigidbody.Add(itsRigidbody, itsNavmeshAgent);
				}

				if (hasColliders)
				{
					for (int i = 0; i < amountOfColliders; i++)
						if (navmeshAgentByCollider.ContainsKey(itsColliders[i]) == false)
							navmeshAgentByCollider.Add(itsColliders[i], itsNavmeshAgent);
				}

				if (hasTriggers)
				{
					for (int i = 0; i < amountOfTriggers; i++)
						if (navmeshAgentByTrigger.ContainsKey(itsTriggers[i]) == false)
							navmeshAgentByTrigger.Add(itsTriggers[i], itsNavmeshAgent);
				}

				if (hasAnimator)
				{
					if (navmeshAgentByAnimator.ContainsKey(itsAnimator) == false)
						navmeshAgentByAnimator.Add(itsAnimator, itsNavmeshAgent);
				}

				if (navmeshAgentByGameobject.ContainsKey(itsGameobject) == false)
					navmeshAgentByGameobject.Add(itsGameobject, itsNavmeshAgent);

				if (navmeshAgentByTransform.ContainsKey(itsTransform) == false)
					navmeshAgentByTransform.Add(itsTransform, itsNavmeshAgent);
			}

			//Animators
			if (animatorByRigidbody == null)
				animatorByRigidbody = new Dictionary<Rigidbody, Animator>();
			if (animatorByCollider == null)
				animatorByCollider = new Dictionary<Collider, Animator>();
			if (animatorByTrigger == null)
				animatorByTrigger = new Dictionary<Collider, Animator>();
			if (animatorByNavmeshAgent == null)
				animatorByNavmeshAgent = new Dictionary<NavMeshAgent, Animator>();
			if (animatorByGameobject == null)
				animatorByGameobject = new Dictionary<GameObject, Animator>();
			if (animatorByTransform == null)
				animatorByTransform = new Dictionary<Transform, Animator>();

			if (hasAnimator)
			{
				if (hasRigidbody)
				{
					if (animatorByRigidbody.ContainsKey(itsRigidbody) == false)
						animatorByRigidbody.Add(itsRigidbody, itsAnimator);
				}

				if (hasColliders)
				{
					for (int i = 0; i < amountOfColliders; i++)
						if (animatorByCollider.ContainsKey(itsColliders[i]) == false)
							animatorByCollider.Add(itsColliders[i], itsAnimator);
				}

				if (hasTriggers)
				{
					for (int i = 0; i < amountOfTriggers; i++)
						if (animatorByTrigger.ContainsKey(itsTriggers[i]) == false)
							animatorByTrigger.Add(itsTriggers[i], itsAnimator);
				}

				if (hasNavmeshAgent)
				{
					if (animatorByNavmeshAgent.ContainsKey(itsNavmeshAgent) == false)
						animatorByNavmeshAgent.Add(itsNavmeshAgent, itsAnimator);
				}

				if (animatorByGameobject.ContainsKey(itsGameobject) == false)
					animatorByGameobject.Add(itsGameobject, itsAnimator);

				if (animatorByTransform.ContainsKey(itsTransform) == false)
					animatorByTransform.Add(itsTransform, itsAnimator);
			}
		}
	}
}
