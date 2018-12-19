using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Linq.Expressions;
using UnityEngine;
using EnhancedBehaviours;
using Random = UnityEngine.Random;

[DisallowMultipleComponent]
public sealed class WasteRetrieverBalloon : MyMonoBehaviour
{
    public  static      WasteRetrieverBalloon   wasteRetrieverBalloonInstance       { get; private set; }
    public  static      SpringJoint             itsSpringJoint;
    public  static      bool                    busy;
    public  static      Transform               currentWasteBeingDragged;
    private             GameObject              itsBalloonSac;
    private readonly    Vector3                 anchorOffset                    =   new Vector3(0f, -5f, 0f);
    private readonly    Vector3                 balloonDefaultPosition          =   new Vector3(0f, 900f, 0f);

    public delegate void OnRetrieveDelegate();
    public static event OnRetrieveDelegate OnRetrieveEvent;

    public delegate void OnRetrievedDelegate();
    public static event OnRetrievedDelegate OnRetrievedEvent;

    private void Awake()
    {
        if (wasteRetrieverBalloonInstance != null)
            if (wasteRetrieverBalloonInstance != this)
                DestroyImmediate(gameObject);

        wasteRetrieverBalloonInstance = this;
        DontDestroyOnLoad(gameObject);
    }

    private IEnumerator Start()
    {
        while (gameManager == null) yield return null;
        while (protagonist == null) yield return null;
        Initialize();
    }

    protected override void Initialize()
    {
        base.Initialize();
        itsTransform.localPosition = balloonDefaultPosition;
        itsRigidbody.isKinematic = true;
        itsBalloonSac = GetComponentInChildren<SkinnedMeshRenderer>().gameObject;
        itsBalloonSac.SetActive(false);
        itsSpringJoint = GetComponent<SpringJoint>();
        busy = false;
    }

    private const float retrieveDuration = 18f;
    private float _releaseWasteSchedule = 0f;
    public void Retrieve()
    {
        busy = true;

        itsSpringJoint.connectedAnchor              =   Vector3.zero;
        itsSpringJoint.connectedBody                =   null;
        itsSpringJoint.autoConfigureConnectedAnchor =   false;
        itsSpringJoint.autoConfigureConnectedAnchor =   true;

        var _closestNuclearWaste = WasteDetectionSystem.closestNuclearWaste;
        _closestNuclearWaste.itsCapsuleCollider.enabled = false;

        itsTransform.localPosition = _closestNuclearWaste.itsTransform.localPosition
            + _closestNuclearWaste.itsTransform.up;

        itsTransform.localRotation = Quaternion.identity;

        itsRigidbody.mass = protagonist.itsRigidbody.mass;
        itsRigidbody.drag = protagonist.itsRigidbody.drag;
        itsRigidbody.angularDrag = protagonist.itsRigidbody.angularDrag;
        itsRigidbody.velocity = vector3Zero;

        itsSpringJoint.connectedBody = _closestNuclearWaste.itsRigidbody;

        itsSpringJoint.anchor = anchorOffset;

        itsRigidbody.isKinematic = false;

        itsBalloonSac.SetActive(true);

        currentWasteBeingDragged = _closestNuclearWaste.itsTransform;
        if (OnRetrieveEvent != null) OnRetrieveEvent();
        GameManager.BeforeFixedUpdate += DragWasteToTheSurface;
        _releaseWasteSchedule = time + retrieveDuration;
    }

    private void DragWasteToTheSurface()
    {
        itsRigidbody.mass = (Mathf.Clamp((Mathf.Lerp(itsRigidbody.mass,
                (itsRigidbody.mass - 9f), fixedDeltatime)), 1, 600f));

        if (time < _releaseWasteSchedule) return;

        if (OnRetrievedEvent != null) OnRetrievedEvent();

        busy = false;

        itsSpringJoint.connectedBody = null;

        currentWasteBeingDragged.position = new Vector3
            (Random.Range(-300f, 300f),10f, Random.Range(-300f, 300f));

        //TODO MAKE THE CODE BELOW MORE ELEGANT
        WasteDetectionSystem.closestNuclearWaste.itsRigidbody.velocity = vector3Zero;
        WasteDetectionSystem.closestNuclearWaste.itsRigidbody.angularVelocity = vector3Zero;
        WasteDetectionSystem.closestNuclearWaste.itsCapsuleCollider.enabled = true;

        currentWasteBeingDragged = null;

        itsBalloonSac.SetActive(false);
        itsTransform.localPosition = balloonDefaultPosition;
        itsRigidbody.isKinematic = true;
        GameManager.BeforeFixedUpdate -= DragWasteToTheSurface;
    }
}
