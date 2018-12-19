using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EnhancedBehaviours;

public class OscilatePanStereo : MyMonoBehaviour
{
    public float maximumStereoPanOverride = 0f;
    public float minimumStereoPanOverride = 0f;

    private float targetStereoPan;
    private float oscilationSpeed;
    [SerializeField] private Vector2 stereoPanRange;
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

    float stereoPanAdjustment;
    private void HandleOscilation()
    {
        stereoPanAdjustment = Mathf.MoveTowards(itsAudioSource.panStereo, targetStereoPan, smoothDeltatime * oscilationSpeed);
        stereoPanAdjustment = Mathf.Clamp(stereoPanAdjustment,
            minimumStereoPanOverride != 0f ? minimumStereoPanOverride : -1f,
            maximumStereoPanOverride != 0f ? maximumStereoPanOverride : 1f);

        itsAudioSource.panStereo = stereoPanAdjustment;
        if (itsAudioSource.panStereo == stereoPanAdjustment) GenerateNewOscilation();
    }

    private void GenerateNewOscilation()
    {
        targetStereoPan = Random.Range(stereoPanRange.x, stereoPanRange.y);
        oscilationSpeed = Random.Range(oscilationSpeedRange.x, oscilationSpeedRange.y);
    }
}
