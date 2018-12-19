using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EnhancedBehaviours;


public class OscilateVolume : MyMonoBehaviour
{
    public float maximumVolumeOverride = 0f;
    public float minimumVolumeOverride = 0f;

    private float targetVolume;
    private float oscilationSpeed;
    [SerializeField] private Vector2 volumeRange;
    [SerializeField] private Vector2 oscilationSpeedRange;
    [SerializeField] private AudioSource itsAudioSource;

    private void OnEnable()
    {
        GenerateNewOscilation();
        MyGameManager.AfterUpdate += HandleOscilation;
    }

    private void OnDisable()
    {
        MyGameManager.AfterUpdate -= HandleOscilation;
    }

    float volumeAdjustment;
    private void HandleOscilation()
    {
        volumeAdjustment = Mathf.MoveTowards(itsAudioSource.volume, targetVolume, smoothDeltatime * oscilationSpeed);
        volumeAdjustment = Mathf.Clamp(volumeAdjustment,
            minimumVolumeOverride != 0f ? minimumVolumeOverride : 0f,
            maximumVolumeOverride != 0f ? maximumVolumeOverride : 1f);


        itsAudioSource.volume = volumeAdjustment;
        if (itsAudioSource.volume == volumeAdjustment) GenerateNewOscilation();
    }

    private void GenerateNewOscilation()
    {
        targetVolume = Random.Range(volumeRange.x, volumeRange.y);
        oscilationSpeed = Random.Range(oscilationSpeedRange.x, oscilationSpeedRange.y);
    }
}
