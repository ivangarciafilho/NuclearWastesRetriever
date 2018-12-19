using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EnhancedBehaviours;

public class OscilatePitch : MyMonoBehaviour
{
    public float maximumPitchOverride = 0f;
    public float minimumPitchOverride = 0f;

    private float targetPitch;
    private float oscilationSpeed;
    [SerializeField] private Vector2 pitchRange;
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

    float pitchAdjustment;
    private void HandleOscilation()
    {
        pitchAdjustment = Mathf.MoveTowards(itsAudioSource.pitch, targetPitch, smoothDeltatime * oscilationSpeed);
        pitchAdjustment = Mathf.Clamp(pitchAdjustment,
            minimumPitchOverride != 0f ? minimumPitchOverride : 0f,
            maximumPitchOverride != 0f ? maximumPitchOverride : 3f);

        itsAudioSource.pitch = pitchAdjustment;
        if (itsAudioSource.pitch == pitchAdjustment) GenerateNewOscilation();
    }

    private void GenerateNewOscilation()
    {
        targetPitch = Random.Range(pitchRange.x, pitchRange.y);
        oscilationSpeed = Random.Range(oscilationSpeedRange.x, oscilationSpeedRange.y);
    }
}
