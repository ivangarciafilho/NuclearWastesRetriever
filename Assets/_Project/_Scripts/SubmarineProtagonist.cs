using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Audio;
using EnhancedBehaviours;
using Random = UnityEngine.Random;

[RequireComponent(typeof(Rigidbody))]
public sealed class SubmarineProtagonist : MyMonoBehaviour.MyProtagonist
{

    [SerializeField]    private         Transform   leftHelixTransform;
    [SerializeField]    private         Rigidbody   leftBuoy;
    [SerializeField]    private         Transform   rightHelixTransform;
    [SerializeField]    private         Rigidbody   rightBuoy;
    [SerializeField]    private float               engineMaximumSpeed;
    [SerializeField]    private static  Rigidbody[] itsBuoys;
                        private static  int         amountOfBuoys;
                        private static  float       horizontalInput;
                        private static  float       leftHelixMotion;
                        private static  float       rightHelixMotion;
                        private static  GameObject  closestNuclearWasteGameObject;
                        private static  Collider    closestNuclearWasteCollider;
                        private static  Transform   closestNuclearWasteTransform;
                        private static  Rigidbody   closestNuclearWasteRigidbody;

    [Header("Main Spotlights: ")] [Space(18)]
    [SerializeField] [Range(0f, 90f)] private float spotLightAimingSpeed = 9f;
    [SerializeField] private Transform spotlightsAimPivot;


    [SerializeField] private Transform leftSpotLightPivot;
    [SerializeField] private Transform leftSpotLightTarget;
    [SerializeField] private Light leftSpotlight;
    private Quaternion leftSpotlightRotation;
    private Vector3 leftSpotlightPosition;

    [SerializeField] private Transform rightSpotLightPivot;
    [SerializeField] private Transform rightSpotLightTarget;
    [SerializeField] private Light rightSpotlight;
    private Quaternion rightSpotlightRotation;
    private Vector3 rightSpotlightPosition;

    [Header("Engine SFX & VFX: ")][Space(18)]
    [SerializeField] private AudioClip propellerLoopSfx;
    [SerializeField] private AudioClip[] engineLoopSfx;
    [SerializeField] private ParticleSystem[] leftPropellerVfx;
    [SerializeField] private ParticleSystem[] rightPropellerVfx;

    private AudioSource leftPropellerAudioSource;
    private AudioSource rightPropellerAudioSource;
    private List<AudioSource> engineAudioSources;
    private bool engineActivated = false;

    [Header("IMPACT VFX & SFX: ")][Space(18)]
    [SerializeField] private Light[] itsLights;
    [SerializeField] private float[] itsLightsRanges;
    [SerializeField] private float[] itsLightsIntesity;
    [SerializeField] private MonoBehaviour[] itsVolumetricLights;
    [SerializeField] private int amountOfLights;
    [SerializeField] private Material itsLightMaterial;
    [SerializeField] private AudioMixerGroup sfxGroup;
    [SerializeField] private Transform impactVfxPivot;
    [SerializeField] private ParticleSystem impactVfxParticles;
    [SerializeField] private ParticleSystem hullImpactVfxParticles;
    [SerializeField] private AudioClip[] impactSfx;
    [SerializeField] private AudioClip[] metalImpactSfx;
    [SerializeField] private AudioClip[] bubblesImpactSfx;
    [SerializeField] private AudioClip[] rockSlidingSfx;
    [SerializeField] private AudioClip[] electricImpactSfx;
    [SerializeField] private AudioClip[] electricSparksSfx;
    [SerializeField] private AudioClip[] creakSfx;
    [SerializeField] private AudioClip[] metalSqueakSfx;

    private AudioSource impactAudioSource;
    private AudioSource metalImpactAudioSource;
    private AudioSource bubblesImpactAudioSource;
    private AudioSource rockSlidingAudioSource;
    private AudioSource electricImpactAudioSource;
    private List<AudioSource> electricSparksAudioSources;
    private AudioSource creakAudioSource;
    private AudioSource metalSqueakAudioSource;
    private float audioSourceAvailability;
    public static bool lightsFlicking { get; private set; }
    private float stopFlickingAt;
    private Color emissionOn;
    private Color emissionOff;
    private Color defaultEmission;


    private IEnumerator Start()
    {
        while (gameManager == null) yield return null;
        while (MyMainViewport.viewportInstance == null) yield return null;
        Initialize();
    }

    protected override void Initialize()
    {
        base.Initialize();
        itsLights = GetComponentsInChildren< Light >();
        amountOfLights = itsLights.Length;
        itsLightsRanges = new float[amountOfLights];
        itsLightsIntesity = new float[amountOfLights];

        for (int i = 0; i < amountOfLights; i++)
        {
            itsLightsRanges[i] = itsLights[i].range;
            itsLightsIntesity[i] = itsLights[i].intensity;
        }

        itsBuoys = GetComponentsInChildren<Rigidbody>();
        amountOfBuoys = itsBuoys.Length;
        SubmarineGameManager.BeforeUpdate += HandleUserInputs;
        SubmarineGameManager.BeforeFixedUpdate += HandleProtagonistMotion;

        impactAudioSource = gameObject.AddComponent< AudioSource >();
        impactAudioSource.outputAudioMixerGroup = sfxGroup;
        impactAudioSource.priority = 0;

        metalImpactAudioSource = gameObject.AddComponent<AudioSource>();
        metalImpactAudioSource.outputAudioMixerGroup = sfxGroup;
        metalImpactAudioSource.priority = 0;

        creakAudioSource = gameObject.AddComponent<AudioSource>();
        creakAudioSource.outputAudioMixerGroup = sfxGroup;
        creakAudioSource.priority = 0;

        metalSqueakAudioSource = gameObject.AddComponent<AudioSource>();
        metalSqueakAudioSource.outputAudioMixerGroup = sfxGroup;
        metalSqueakAudioSource.priority = 0;

        electricImpactAudioSource = gameObject.AddComponent<AudioSource>();
        electricImpactAudioSource.outputAudioMixerGroup = sfxGroup;
        electricImpactAudioSource.priority = 0;

        bubblesImpactAudioSource = gameObject.AddComponent<AudioSource>();
        bubblesImpactAudioSource.outputAudioMixerGroup = sfxGroup;
        bubblesImpactAudioSource.priority = 0;

        leftPropellerAudioSource = gameObject.AddComponent<AudioSource>();
        leftPropellerAudioSource.clip = propellerLoopSfx;
        leftPropellerAudioSource.outputAudioMixerGroup = sfxGroup;
        leftPropellerAudioSource.panStereo = -0.666f;
        leftPropellerAudioSource.loop = true;
        leftPropellerAudioSource.priority = 0;
        leftPropellerAudioSource.playOnAwake = true;
        leftPropellerAudioSource.Play();

        rightPropellerAudioSource = gameObject.AddComponent<AudioSource>();
        rightPropellerAudioSource.clip = propellerLoopSfx;
        rightPropellerAudioSource.outputAudioMixerGroup = sfxGroup;
        rightPropellerAudioSource.panStereo = 0.666f;
        rightPropellerAudioSource.loop = true;
        rightPropellerAudioSource.priority = 0;
        rightPropellerAudioSource.playOnAwake = true;
        rightPropellerAudioSource.Play();

        engineAudioSources = new List< AudioSource >();
        foreach( var audioClip in engineLoopSfx )
        {
            var newAudiosource = gameObject.AddComponent< AudioSource >();
            newAudiosource.clip = audioClip;
            newAudiosource.outputAudioMixerGroup = sfxGroup;
            newAudiosource.loop = true;
            newAudiosource.priority = 0;
            newAudiosource.playOnAwake = true;
            newAudiosource.Play();
            engineAudioSources.Add(newAudiosource);
            StartCoroutine(OscilatePitch(newAudiosource));
            StartCoroutine(OscilatePanStereo(newAudiosource));
            StartCoroutine(OscilateVolume(newAudiosource));
        }

        electricSparksAudioSources = new List< AudioSource >();
        for( int i = 0; i < 3; i++ )
            electricSparksAudioSources.Add( gameObject.AddComponent<AudioSource>() );

        for( int i = 0; i < 3; i++ )
        {
            electricSparksAudioSources[i].outputAudioMixerGroup = sfxGroup;
            electricSparksAudioSources[i].priority = 0;
        }

        rockSlidingAudioSource = gameObject.AddComponent< AudioSource >();
        rockSlidingAudioSource.outputAudioMixerGroup = sfxGroup;
        rockSlidingAudioSource.priority = 0;

        defaultEmission = itsLightMaterial.GetColor("_EmissionColor");
        emissionOn = defaultEmission * Mathf.GammaToLinearSpace(1.2f);
        emissionOff = defaultEmission * Mathf.GammaToLinearSpace(0f);
    }


    private float _distanceToProtagonist;
    private float _squaredDistanceToProtagonist;
    private float _squareRoot;
    private Vector3 _directionToNuclearWaste;
    private Vector3 _absoluteSquaredDifference;
    private void HandleUserInputs()
    {
        if (SubmarineGameManager.paused) return;

        //caching
        itsLocalPosition    =   itsTransform.localPosition;
        itsLocalRotation    =   itsTransform.localRotation;
        itsForward = itsTransform.forward;

        //Adjusting LeftHelixInput
        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.Q))
        {
            leftHelixMotion = Input.GetKey(KeyCode.A) ?
                Mathf.Lerp(leftHelixMotion, 1f, deltatime)
                : Mathf.Lerp(leftHelixMotion, -1f, deltatime);
        }
        else { leftHelixMotion = Mathf.Lerp(leftHelixMotion, 0f, deltatime); }

        //Adjusting RightHelixInput
        if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.E))
        {
            rightHelixMotion = Input.GetKey(KeyCode.D) ?
                Mathf.Lerp(rightHelixMotion, 1f, deltatime)
                : Mathf.Lerp(rightHelixMotion, -1f, deltatime);
        }
        else { rightHelixMotion = Mathf.Lerp(rightHelixMotion, 0f, deltatime); }

        //Rotate Helix
        if (rightHelixMotion > 0.0111f || rightHelixMotion < 0.0111f) { }
            rightHelixTransform.Rotate(0f, 0f, -720f * rightHelixMotion * smoothDeltatime);

        if (leftHelixMotion > 0.0111f || leftHelixMotion < 0.0111f)
            leftHelixTransform.Rotate(0f, 0f, -720f * leftHelixMotion * smoothDeltatime);

        HandleSpotlightsMovement();
        HandleEngineSfxVfx();

        //Check for ClosestNuclearWaste Distance from Protagonist
        if (WasteRetrieverBalloon.busy) return;
        if (Input.GetKeyDown(KeyCode.Space))
        {

            //Fastest Replacement of Vector3.Distance
            var _closestNuclearWastePosition    =   WasteDetectionSystem.closestNuclearWaste.itsTransform.localPosition;

            _directionToNuclearWaste.x          =   _closestNuclearWastePosition.x - itsLocalPosition.x;
            _directionToNuclearWaste.y          =   _closestNuclearWastePosition.y - itsLocalPosition.y;
            _directionToNuclearWaste.z          =   _closestNuclearWastePosition.z - itsLocalPosition.z;

            _absoluteSquaredDifference.x        =   _directionToNuclearWaste.x * _directionToNuclearWaste.x;
            _absoluteSquaredDifference.y        =   _directionToNuclearWaste.y * _directionToNuclearWaste.z;
            _absoluteSquaredDifference.z        =   _directionToNuclearWaste.z * _directionToNuclearWaste.z;

            _squaredDistanceToProtagonist       =   _absoluteSquaredDifference.x + _absoluteSquaredDifference.y + _absoluteSquaredDifference.z;

            _squareRoot = _squaredDistanceToProtagonist / 3f;

            //Replacement of Square root
            _squareRoot = ((_squareRoot + (_squaredDistanceToProtagonist / _squareRoot)) / 2);
            _squareRoot = ((_squareRoot + (_squaredDistanceToProtagonist / _squareRoot)) / 2);

            _distanceToProtagonist = _squareRoot;

            if (_distanceToProtagonist < 12f)
                WasteRetrieverBalloon.wasteRetrieverBalloonInstance.Retrieve();
        }
    }

    private Quaternion leftSpotlightUpdatedRotation = quaternionIdentity;
    private Quaternion rightSpotlightUpdatedRotation = quaternionIdentity;
    private Vector3 spotLightsDirection = vector3Zero;
    private void HandleSpotlightsMovement()
    {
        spotLightsDirection = spotlightsAimPivot.localPosition;
        spotLightsDirection.z = 0;

        spotLightsDirection.x = Input.GetKey(KeyCode.LeftArrow) ?
            spotLightsDirection.x - (spotLightAimingSpeed * smoothDeltatime) :
            Input.GetKey(KeyCode.RightArrow) ?
            spotLightsDirection.x + (spotLightAimingSpeed * smoothDeltatime)
            : spotLightsDirection.x * (1 - thriceSmoothDeltatime);

        spotLightsDirection.y = Input.GetKey(KeyCode.DownArrow)
            ? spotLightsDirection.y - (spotLightAimingSpeed * smoothDeltatime) : Input.GetKey(KeyCode.UpArrow)
            ? spotLightsDirection.y + (spotLightAimingSpeed * smoothDeltatime) : spotLightsDirection.y * (1 - thriceSmoothDeltatime);

        spotLightsDirection.x = spotLightsDirection.x < -10 ? -10 : spotLightsDirection.x > 10 ? 10 : spotLightsDirection.x;
        spotLightsDirection.y = spotLightsDirection.y < -5 ? -5 : spotLightsDirection.y > 5 ? 5 : spotLightsDirection.y;

        spotlightsAimPivot.localPosition = spotLightsDirection;

        leftSpotlightPosition = leftSpotLightPivot.position;
        leftSpotlightRotation = leftSpotLightPivot.rotation;
        rightSpotlightPosition = rightSpotLightPivot.position;
        rightSpotlightRotation = rightSpotLightPivot.rotation;


        if( Input.GetKey( KeyCode.LeftArrow )
            || Input.GetKey( KeyCode.RightArrow )
            || Input.GetKey( KeyCode.UpArrow )
            || Input.GetKey( KeyCode.DownArrow ) )
        {

            leftSpotlightUpdatedRotation =
                Quaternion.LookRotation( leftSpotLightTarget.position - leftSpotlightPosition, vector3Up );
            rightSpotlightUpdatedRotation =
                Quaternion.LookRotation( rightSpotLightTarget.position - rightSpotlightPosition, vector3Up );
        }

        leftSpotLightPivot.rotation = Quaternion.Lerp(leftSpotlightRotation, leftSpotlightUpdatedRotation,smoothDeltatime);
        rightSpotLightPivot.rotation = Quaternion.Lerp(rightSpotlightRotation, rightSpotlightUpdatedRotation, smoothDeltatime);
    }

    private void HandleEngineSfxVfx()
    {
        leftPropellerAudioSource.volume = Mathf.Abs(leftHelixMotion) * 0.333f;
        leftPropellerAudioSource.pitch = Mathf.Abs(leftHelixMotion);
        leftPropellerAudioSource.priority = ( leftPropellerAudioSource.volume > 0.111f ) ? 0 : 128;

        for( int i = 0; i < 2; i++ )
        {
            var mainModule = leftPropellerVfx[i].main;
            mainModule.loop = leftPropellerAudioSource.volume > 0.111f;
            if (mainModule.loop)
            {
                if (!leftPropellerVfx[i].isPlaying)
                {
                    leftPropellerVfx[i].Play();
                }
            }
        }

        rightPropellerAudioSource.volume = Mathf.Abs(rightHelixMotion) * 0.333f;
        rightPropellerAudioSource.pitch = Mathf.Abs(rightHelixMotion);
        rightPropellerAudioSource.priority = (rightPropellerAudioSource.volume > 0.111f) ? 0 : 128;

        for (int i = 0; i < 2; i++)
        {
            var mainModule = rightPropellerVfx[i].main;
            mainModule.loop = rightPropellerAudioSource.volume > 0.111f;
            if (mainModule.loop)
            {
                if (!rightPropellerVfx[i].isPlaying)
                {
                    rightPropellerVfx[i].Play();
                }
            }
        }
    }


    private IEnumerator OscilateVolume(AudioSource audioSource)
    {
        var targetVolume = Random.Range(0.222f, 0.666f);
        var speed = Random.Range(0.333f, 0.999f);

        while (true)
        {
            if (targetVolume > audioSource.volume)
            {
                audioSource.volume += (smoothDeltatime * speed);
                if ((targetVolume - audioSource.volume) < 0.0111f)
                {
                    targetVolume = Random.Range(0.111f, 0.222f);
                    speed = Random.Range(0.333f, 0.999f);
                }
            }
            else
            {
                audioSource.volume -= (smoothDeltatime * speed);
                if ((audioSource.volume - targetVolume) < 0.0111f)
                {
                    targetVolume = Random.Range(0.222f, 0.333f);
                    speed = Random.Range(0.333f, 0.999f);
                }
            }

            audioSource.volume = Mathf.Min((Mathf.Abs(leftHelixMotion)
                + Mathf.Abs(rightHelixMotion)) / 6f, audioSource.volume);

            audioSource.priority = ( audioSource.volume < 0.0111f ) ? 128 : 0;

            yield return null;
        }
    }

    private IEnumerator OscilatePanStereo(AudioSource audioSource)
    {
        var targetPanStereo = Random.Range( -0.333f,0.333f );
        var speed = Random.Range( 0.111f,0.333f );

        while (true)
        {
            while( audioSource.volume < 0.0111f ) yield return null;

            if (targetPanStereo > audioSource.panStereo)
            {
                audioSource.panStereo += (smoothDeltatime * speed);
                if ((targetPanStereo - audioSource.panStereo) < 0.0111f)
                {
                    targetPanStereo = Random.Range(-0.333f, 0f);
                    speed = Random.Range(0.111f, 0.333f);
                }
            }
            else
            {
                audioSource.panStereo -= (smoothDeltatime * speed);
                if ((audioSource.panStereo - targetPanStereo) < 0.0111f)
                {
                    targetPanStereo = Random.Range(0f, 0.333f);
                    speed = Random.Range(0.111f, 0.333f);
                }
            }

            yield return null;
        }
    }

    private IEnumerator OscilatePitch(AudioSource audioSource)
    {
        var targetPitch = Random.Range(0.888f, 1.111f);
        var speed = Random.Range(0.111f, 0.333f);

        while (true)
        {
            while( audioSource.volume < 0.0111f ){ yield return null; }

            if ( targetPitch > audioSource.pitch )
            {
                audioSource.pitch += ( smoothDeltatime * speed );
                if ((targetPitch - audioSource.pitch) < 0.0111f)
                {
                    targetPitch = Random.Range(0.9666f, 1f);
                    speed = Random.Range(0.111f, 0.333f);
                }
            }
            else
            {
                audioSource.pitch -= (smoothDeltatime * speed);

                if ((audioSource.pitch - targetPitch) < 0.0111f)
                {
                    targetPitch = Random.Range(1f, 1.0333f);
                    speed = Random.Range(0.111f, 0.333f);
                }
            }

            audioSource.pitch = Mathf.Min((Mathf.Abs(leftHelixMotion)
               + Mathf.Abs(rightHelixMotion)) / 2f, audioSource.pitch);

            yield return null;
        }
    }

    private void HandleProtagonistMotion()
    {
        if (SubmarineGameManager.paused) return;

        if (verticalAxisInputKey != 0)
        {
            for (int i = 0; i < amountOfBuoys; i++)
            {
                itsBuoys[i].mass = verticalAxisInputKey < 0 ?
                Mathf.Clamp(Mathf.Lerp(itsRigidbody.mass, itsRigidbody.mass + 6f, fixedDeltatime), 1f, 600)
                : Mathf.Clamp(Mathf.Lerp(itsRigidbody.mass, itsRigidbody.mass - 6f, fixedDeltatime), 1f, 600);
            }
        }

        if (rightHelixMotion > 0.0111f || rightHelixMotion < 0.0111f)
            rightBuoy.AddForce(rightHelixTransform.forward
                * (engineMaximumSpeed * rightHelixMotion * itsRigidbody.drag * fixedDeltatime),
                    ForceMode.Acceleration);

        if (leftHelixMotion > 0.0111f || leftHelixMotion < 0.0111f)
            leftBuoy.AddForce(leftHelixTransform.forward
                * (engineMaximumSpeed * leftHelixMotion * itsRigidbody.drag * fixedDeltatime),
                    ForceMode.Acceleration);
    }


    private void OnCollisionEnter(Collision collision)
    {
        CallOnCollisionEnterCallbacks(collision);
        DamageSfx();
        DamageVfx(collision);
    }

    private void DamageVfx(Collision collision )
    {
        impactVfxPivot.SetPositionAndRotation(collision.contacts[0].point, Quaternion.LookRotation(collision.contacts[0].normal));
        hullImpactVfxParticles.Play();
        impactVfxParticles.Play();
    }

    private void DamageSfx()
    {
        if( time < audioSourceAvailability ) return;

        impactAudioSource.clip = impactSfx[Random.Range( 0, impactSfx.Length )];
        metalImpactAudioSource.clip = metalImpactSfx[Random.Range(0, metalImpactSfx.Length)];
        electricImpactAudioSource.clip = electricImpactSfx[Random.Range(0, electricImpactSfx.Length)];
        creakAudioSource.clip = creakSfx[Random.Range(0, creakSfx.Length)];
        metalSqueakAudioSource.clip = metalSqueakSfx[Random.Range(0, metalSqueakSfx.Length)];

        impactAudioSource.pitch = Random.Range(0.9666f, 1.0333f);
        metalImpactAudioSource.pitch = Random.Range(0.9666f, 1.0333f);
        electricImpactAudioSource.pitch = Random.Range(0.9666f, 1.0333f);
        creakAudioSource.pitch = Random.Range(0.9666f, 1.0333f);
        metalSqueakAudioSource.pitch = Random.Range(0.9666f, 1.0333f);

        impactAudioSource.volume = Random.Range(0.444f, 0.666f);
        metalImpactAudioSource.volume = Random.Range(0.666f, 0.999f);
        electricImpactAudioSource.volume = Random.Range(0.444f, 0.666f);
        creakAudioSource.volume = Random.Range(0.666f, 0.999f);
        metalSqueakAudioSource.volume = Random.Range(0.666f, 0.999f);

        impactAudioSource.Play();
        metalImpactAudioSource.Play();
        electricImpactAudioSource.Play();
        creakAudioSource.Play();
        metalSqueakAudioSource.Play();

        audioSourceAvailability = time + 1f;

        stopFlickingAt = time + Random.Range( 0.333f, 0.999f );

        if (!lightsFlicking)
        {
            StartCoroutine( FlickCabinLights() );
            StartCoroutine(SlowMotion());
        }
    }



    private IEnumerator FlickCabinLights()
    {
        float nextFlick = 0f;
        int currentAvailableElectricSparksAudioSource = 0;
        lightsFlicking = true;

        while (time < stopFlickingAt)
        {
            if( lightsFlicking )
            {
                for( int i = 0; i < amountOfLights; i++ )
                {

                    itsLights[i].intensity = (itsLights[i].intensity < 0.01f) ?
                        0f : (itsLights[i].intensity - (itsLightsIntesity[i] * halfSmoothDeltatime));

                    itsLights[i].range = (itsLights[i].range < 0.01f) ?
                        0f : (itsLights[i].range - (itsLightsRanges[i] * thriceSmoothDeltatime));
                }
            }
            else
            {
                for( int i = 0; i < amountOfLights; i++ )
                {
                    itsLights[i].intensity = (itsLights[i].intensity > 0.666f) ?
                        0.666f : (itsLights[i].intensity + (smoothDeltatime * itsLightsIntesity[i]));

                    itsLights[i].range = (itsLights[i].range > 90f) ?
                        90f : (itsLights[i].range + (itsLightsRanges[i] * smoothDeltatime));
                }
            }

            if (time > nextFlick)
            {
                if (Random.Range( 0f,1f ) < 0.666f)
                {
                    for( int i = 0; i < amountOfLights; i++ )
                    {
                        itsLights[i].enabled = !itsLights[i].enabled;
                    }

                    itsLightMaterial.SetColor("_EmissionColor", itsLights[0].enabled? emissionOn:emissionOff);
                    if(itsLights[0].enabled)
                    {
                        currentAvailableElectricSparksAudioSource =
                            currentAvailableElectricSparksAudioSource > 1 ? 0
                                : currentAvailableElectricSparksAudioSource;

                        electricSparksAudioSources[currentAvailableElectricSparksAudioSource].clip
                            = electricSparksSfx[Random.Range( 0, electricSparksSfx.Length)];
                        electricSparksAudioSources[currentAvailableElectricSparksAudioSource].pitch
                            = Random.Range(0.9666f, 1.0333f);
                        electricSparksAudioSources[currentAvailableElectricSparksAudioSource].volume
                            = Random.Range(0.666f, 0.999f);
                        electricSparksAudioSources[currentAvailableElectricSparksAudioSource].Play();

                        currentAvailableElectricSparksAudioSource++;
                    }
                }
                nextFlick = time + Random.Range(0.0666f, 0.0999f);
            }
            yield return null;
        }

        rockSlidingAudioSource.clip = rockSlidingSfx[Random.Range(0, rockSlidingSfx.Length)];
        rockSlidingAudioSource.pitch = Random.Range(0.9666f, 1.0333f);
        rockSlidingAudioSource.volume = Random.Range(0.666f, 0.999f);
        rockSlidingAudioSource.Play();

        bubblesImpactAudioSource.clip = bubblesImpactSfx[Random.Range(0, bubblesImpactSfx.Length)];
        bubblesImpactAudioSource.pitch = Random.Range(0.9666f, 1.0333f);
        bubblesImpactAudioSource.volume = Random.Range(0.666f, 0.999f);
        bubblesImpactAudioSource.Play();

        lightsFlicking = false;

        for( int i = 0; i < amountOfLights; i++ )
        {
            itsLights[i].enabled = true;
            itsLights[i].intensity = itsLightsIntesity[i];
            itsLights[i].range = itsLightsRanges[i];
        }

        itsLightMaterial.SetColor("_EmissionColor", defaultEmission);

        yield return null;
    }


    private IEnumerator SlowMotion()
    {
        Time.timeScale = 0.111f;
        yield return  new  WaitForSecondsRealtime(0.333f);
        Time.timeScale = 1f;
    }
}
