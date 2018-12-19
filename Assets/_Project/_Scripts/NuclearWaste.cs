using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EnhancedBehaviours;

public sealed class NuclearWaste : MyMonoBehaviour
{
    [SerializeField] private float cullingRadius = 45;
    [SerializeField] private MonoBehaviour[] volumetricLights;
    [SerializeField] private ParticleSystem[] itsParticleSystems = null;

    private ParticleSystem.MainModule[] itsParticleSystemsMainModules = null;
    private float[] defaultSimulationSpeeds;

    private int amountOfParticles;

    private IEnumerator Start()
    {
        while (gameManager == null) yield return null;
        yield return null;

        Initialize();
        amountOfParticles = itsParticleSystems.Length;

        defaultSimulationSpeeds = new float[amountOfParticles];
        for (int i = 0; i < amountOfParticles; i++)
            defaultSimulationSpeeds[i] = itsParticleSystems[i].main.simulationSpeed;


        itsParticleSystemsMainModules = new ParticleSystem.MainModule[amountOfParticles];
        for (int i = 0; i < amountOfParticles; i++)
            itsParticleSystemsMainModules[i] = itsParticleSystems[i].main;

        MyGameManager.AfterSlowFixedUpdate += HandleParticlesCrossfade;
    }


    public enum rigidBodyStatus
    {
        sleeping,
        awaken,
    }

    public rigidBodyStatus currrentStatus;
    public rigidBodyStatus previousStatus;

    private void HandleParticlesCrossfade()
    {
        currrentStatus = itsRigidbody.IsSleeping() ? rigidBodyStatus.sleeping : rigidBodyStatus.awaken;

        if (currrentStatus == previousStatus) return;
        if (currrentStatus == rigidBodyStatus.awaken) { FadeOut(); return; }
        if (currrentStatus == rigidBodyStatus.sleeping) { FadeIn(); return; }
    }

    private void FadeIn()
    {
        for (int i = 0; i < amountOfParticles; i++)
        {
            itsParticleSystemsMainModules[i].simulationSpeed = defaultSimulationSpeeds[i];
            itsParticleSystemsMainModules[i].loop = true;

            if (itsParticleSystems[i].isPlaying == false
                || itsParticleSystems[i].isPaused == true)
            {
                itsParticleSystems[i].Play();
            }
        }
    }

    private void FadeOut()
    {
        for (int i = 0; i < amountOfParticles; i++)
        {
            itsParticleSystemsMainModules[i].simulationSpeed = 6f;
            itsParticleSystemsMainModules[i].loop = false;
        }
    }

    private void OnDisable()
    {
        MyGameManager.AfterSlowLateUpdate -= HandleParticlesCrossfade;
    }
}