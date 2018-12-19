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
public sealed class WasteDetectionSystem : MyMonoBehaviour
{
    public enum SonarStatus
    {
        ping,
        pong
    }

    public struct NuclearWaste
    {
        public GameObject itsGameObject;
        public Transform itsTransform;
        public Rigidbody itsRigidbody;
        public CapsuleCollider itsCapsuleCollider;

        public NuclearWaste(GameObject itsGameObject, Transform itsTransform, Rigidbody itsRigidbody, CapsuleCollider itsCapsuleCollider)
        {
            this.itsGameObject = itsGameObject;
            this.itsTransform = itsTransform;
            this.itsRigidbody = itsRigidbody;
            this.itsCapsuleCollider = itsCapsuleCollider;
        }
    }

                        public  static  WasteDetectionSystem    wasteDetectionSystemInstance    { get; private set; }
                        public  static  SonarStatus             currentSonarStatus;
    [SerializeField]    private         AudioSource             sonarPingAudioSource;
    [SerializeField]    private         AudioSource             sonarPongAudioSource;
    [SerializeField]    private         Transform               sonarPingFeedbackTransform;
    [SerializeField]    private         ParticleSystem          sonarPingFeedbackParticles;
    [SerializeField]    private         Transform               sonarPongFeedbackTransform;
    [SerializeField]    private         ParticleSystem          sonarPongFeedbackParticles;

                        public  static  NuclearWaste            closestNuclearWaste;
                        public  static  NuclearWaste[]          nuclearWastes;
                        public  static  int                     amountOfNuclearWastes;

                        //Cross Reference to avoid GetComponent<>()
                        public  static  Dictionary<NuclearWaste, Vector3>   nuclearWasteDireciton;
                        public  static  Dictionary<NuclearWaste, Vector3>   nuclearWastePosition;
                        public  static  Dictionary<NuclearWaste, float>     nuclearWasteDistance;

    private void Awake()
    {
        if (wasteDetectionSystemInstance != null)
            if (wasteDetectionSystemInstance != this)
                DestroyImmediate(gameObject);

        wasteDetectionSystemInstance = this;
        DontDestroyOnLoad(gameObject);
    }

    private IEnumerator Start()
    {
        while (gameManager == null) yield return null;
        while (protagonist == null) yield return null;
        while (MyMainViewport.viewportInstance == null) yield return null;

        yield return null;
        Initialize();
    }

    protected override void Initialize()
    {
        base.Initialize();
        if (nuclearWasteDistance == null)       nuclearWasteDistance        =   new Dictionary<NuclearWaste, float>();
        if (nuclearWastePosition == null)       nuclearWastePosition        =   new Dictionary<NuclearWaste, Vector3>();
        if (nuclearWasteDireciton == null)      nuclearWasteDireciton       =   new Dictionary<NuclearWaste, Vector3>();

        //Initialize main Variables
        var nuclearWasteGameobjects = GameObject.FindGameObjectsWithTag("NuclearWaste");
        amountOfNuclearWastes = nuclearWasteGameobjects.Length;

        List<Transform>     _wasteTransforms    =   new List<Transform>();
        List<Rigidbody>     _wasteRigidbodies   =   new List<Rigidbody>();
        List<NuclearWaste>  _nuclearWastes      =   new List<NuclearWaste>();

        for (int i = 0; i < amountOfNuclearWastes; i++)
        {
            var _newNuclearWaste = new NuclearWaste(nuclearWasteGameobjects[i], nuclearWasteGameobjects[i].transform,
                nuclearWasteGameobjects[i].GetComponent<Rigidbody>(), nuclearWasteGameobjects[i].GetComponent<CapsuleCollider>());
            _nuclearWastes.Add(_newNuclearWaste);

            nuclearWasteGameobjects[i].transform.hierarchyCapacity = nuclearWasteGameobjects[i].transform.hierarchyCount;

            if (!nuclearWastePosition.ContainsKey(_newNuclearWaste))
                nuclearWastePosition.Add(_newNuclearWaste, _newNuclearWaste.itsTransform.localPosition);

            if (!nuclearWasteDireciton.ContainsKey(_newNuclearWaste))
                nuclearWasteDireciton.Add(_newNuclearWaste, _newNuclearWaste.itsTransform.localPosition - protagonist.itsTransform.localPosition);

            if (!nuclearWasteDistance.ContainsKey(_newNuclearWaste))
                nuclearWasteDistance.Add(_newNuclearWaste, Vector3.Distance(_newNuclearWaste.itsTransform.localPosition,protagonist.itsTransform.localPosition));
        }
        nuclearWastes           =   _nuclearWastes.ToArray();
        closestNuclearWaste = nuclearWasteDistance.OrderBy(_waste => _waste.Value).FirstOrDefault().Key;

        currentSonarStatus = SonarStatus.ping;
        GameManager.AfterVerySlowFixedUpdate += DetectNuclearWaste;
    }

    private Vector3 _wasteDirection;
    private Vector3 _protagonistPosition;
    private void DetectNuclearWaste()
    {
        if (currentSonarStatus == SonarStatus.ping)
        {
            if(SubmarineProtagonist.lightsFlicking) return;

            currentSonarStatus = SonarStatus.pong;
            sonarPingFeedbackTransform.SetPositionAndRotation(protagonist.itsLocalPosition,protagonist.itsLocalRotation );
            sonarPingFeedbackParticles.Play();
            sonarPingAudioSource.Play();
        }
        else
        {
            if (SubmarineProtagonist.lightsFlicking) return;

            _protagonistPosition = protagonist.itsLocalPosition;

            for (int i = 0; i < amountOfNuclearWastes; i++)
            {
                nuclearWastePosition[nuclearWastes[i]]  =   nuclearWastes[i].itsTransform.localPosition;
                nuclearWasteDireciton[nuclearWastes[i]] =   (nuclearWastePosition[nuclearWastes[i]] - _protagonistPosition);
                nuclearWasteDistance[nuclearWastes[i]]  =   nuclearWasteDireciton[nuclearWastes[i]].sqrMagnitude;
            }

            closestNuclearWaste = nuclearWasteDistance.OrderBy(_waste => _waste.Value).FirstOrDefault().Key;
            _wasteDirection = nuclearWasteDireciton[closestNuclearWaste].normalized;

            sonarPongFeedbackTransform.localPosition = (nuclearWasteDistance[closestNuclearWaste] > 90)
                ? _protagonistPosition + (_wasteDirection * 45f)
                : nuclearWastePosition[closestNuclearWaste];

            sonarPongFeedbackTransform.LookAt(_protagonistPosition);
            sonarPongFeedbackTransform.Rotate(90f,0f,0f);


            sonarPongFeedbackParticles.Play();
            sonarPongAudioSource.Play();
            currentSonarStatus = SonarStatus.ping;
        }
    }
}
