
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
public sealed class Sea : MyMonoBehaviour
{
    public sealed class Buoy
    {
        public GameObject   itsGameObject;
        public Transform    itsTransform;
        public Rigidbody    itsRigidbody;


        public Buoy(GameObject itsGameObject, Transform itsTransform, Rigidbody itsRigidbody)
        {
            this.itsGameObject  =   itsGameObject;
            this.itsTransform   =   itsTransform;
            this.itsRigidbody   =   itsRigidbody;
        }
    }


    public static       Sea         seaInstance { get; private set; }
    public static       float       tideHeight = 0f;
    public static       Buoy[]      buoys;
    public static       Int16       amountOfbuoys;
    public              Transform   surfaceMoonlight;
    public              Transform   underwaterMoonlight;
    public static       Transform   reefs;

    private void Awake()
    {
        if (seaInstance != null)
            if (seaInstance != this)
                DestroyImmediate(gameObject);
        seaInstance = this;

        itsTransform = GetComponent<Transform>();
        DontDestroyOnLoad(gameObject);
    }

    private IEnumerator Start()
    {
        while (gameManager == null) yield return null;
        while (protagonist == null) yield return null;
        yield return null;
        Initialize();
    }


    protected override void Initialize()
    {
        base.Initialize();

        var _worldBuoys = GameObject.FindGameObjectsWithTag("Buoy");
        List<Buoy> _newBuoys = new List<Buoy>();

        foreach (var _buoy in _worldBuoys)
            _newBuoys.Add(new Buoy(_buoy.gameObject, _buoy.transform, _buoy.GetComponent<Rigidbody>()));

        buoys = _newBuoys.ToArray();
        amountOfbuoys = (Int16)buoys.Length;

        GameManager.BeforeFixedUpdate += ApplyBuyoance;
        GameManager.AfterSlowLateUpdate += MoveMoonlight;
    }

    private void MoveMoonlight()
    {
        surfaceMoonlight.localPosition = new Vector3
            (protagonist.itsLocalPosition.x, surfaceMoonlight.localPosition.y, protagonist.itsLocalPosition.z);
        underwaterMoonlight.localPosition = new Vector3
            (protagonist.itsLocalPosition.x, underwaterMoonlight.localPosition.y, protagonist.itsLocalPosition.z);
    }

    private int _index;
    private float buoyAltitude;
    private Transform buoyTransform;
    private Rigidbody buoyRigidBody;
    private void ApplyBuyoance()
    {
        tideHeight = (itsTransform.localPosition.y);

        for (int i = 0; i < amountOfbuoys; i++)
        {
            buoyTransform = buoys[i].itsTransform;
            buoyAltitude = buoyTransform.position.y;
            buoyRigidBody = buoys[i].itsRigidbody;
            buoyRigidBody.useGravity = false;

            //Add Gravity Based On Mass (to avoid spreading scripts across every game object)
            buoyRigidBody.AddForce((gravity * buoyRigidBody.mass), ForceMode.Acceleration);

            if (buoyAltitude < tideHeight)
            {
                buoyRigidBody.drag = 3f;
                buoyRigidBody.angularDrag = 27f;

                buoyRigidBody.AddForce((-gravity
                    + (-gravity * (Mathf.Abs(buoyAltitude - tideHeight)))),
                        ForceMode.Acceleration);
            }
            else
            {
                buoyRigidBody.drag = 0.0111f;
                buoyRigidBody.angularDrag = 0.0999f;
            }
        }
    }



    public static void NewBuoy(GameObject _newBuoyGameObject, Transform _newBuoyTransform, Rigidbody _newBuoyRigidbody)
    {
        var _newBuoy = new Buoy(_newBuoyGameObject, _newBuoyTransform, _newBuoyRigidbody);
        List<Buoy> _buoys = buoys.ToList();
        _buoys.Add(_newBuoy);
        buoys = _buoys.ToArray();
        amountOfbuoys = (Int16)buoys.Length;
    }
}
