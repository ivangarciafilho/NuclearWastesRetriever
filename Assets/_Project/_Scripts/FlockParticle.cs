using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlockParticle : MonoBehaviour
{
    public Vector3 baseSpeeds = new Vector3(3f, 30f, 3F);
    public Vector3 speedsDeviations = new Vector3(0.333f, 0.333f, 0.333f);
    public Vector3 speedInversionIntervals = new Vector3(18f, 6f, 18f);
    public Vector3 inversionIntervalDeviations = new Vector3(0.333f, 0.111f, 0.333f);

    private Transform itsTransform;
    private Vector3 currentRotation;
    private float currentYSpeed = 180f;
    private float currentXSpeed = 180f;
    private float currentZSpeed = 180f;
    private float smoothing;


    private void Start()
    {
        itsTransform = transform;

        StartCoroutine(GenerateRandomXRotation());
        StartCoroutine(GenerateRandomYRotation());
        StartCoroutine(GenerateRandomZRotation());
    }

    private void Update()
    {
        smoothing = Time.smoothDeltaTime;

        currentRotation.x = currentXSpeed * smoothing;
        currentRotation.y = currentYSpeed * smoothing;
        currentRotation.z = currentZSpeed * smoothing;

        itsTransform.Rotate(currentRotation, Space.Self);
    }

    private IEnumerator GenerateRandomXRotation()
    {

        while (true)
        {
            var min = baseSpeeds.x * (1 - speedsDeviations.x);
            var max = baseSpeeds.x * (1 + speedsDeviations.x);
            var range = max - min;

            var inverseSpeed = -currentXSpeed;
            inverseSpeed += Random.Range(-range, +range);

            if (inverseSpeed > 0)
            {
                inverseSpeed = inverseSpeed > max ? max : inverseSpeed;
                inverseSpeed = inverseSpeed < min ? min : inverseSpeed;
            }
            else
            {
                inverseSpeed = inverseSpeed < -max ? -max : inverseSpeed;
                inverseSpeed = inverseSpeed > -min ? -min : inverseSpeed;
            }

            currentXSpeed = inverseSpeed;

            yield return new WaitForSeconds(Random.
                Range(speedInversionIntervals.x * (1 - inversionIntervalDeviations.x), speedInversionIntervals.x * (1 + inversionIntervalDeviations.x)));
        }
    }

    private IEnumerator GenerateRandomYRotation()
    {

        while (true)
        {
            var min = baseSpeeds.y * (1 - speedsDeviations.y);
            var max = baseSpeeds.y * (1 + speedsDeviations.y);
            var range = max - min;

            var inverseSpeed = -currentYSpeed;
            inverseSpeed += Random.Range(-range, +range);

            if (inverseSpeed > 0)
            {
                inverseSpeed = inverseSpeed > max ? max : inverseSpeed;
                inverseSpeed = inverseSpeed < min ? min : inverseSpeed;
            }
            else
            {
                inverseSpeed = inverseSpeed < -max ? -max : inverseSpeed;
                inverseSpeed = inverseSpeed > -min ? -min : inverseSpeed;
            }

            currentYSpeed = inverseSpeed;

            yield return new WaitForSeconds(Random.
                Range(speedInversionIntervals.y * (1 - inversionIntervalDeviations.y), speedInversionIntervals.y * (1 + inversionIntervalDeviations.y)));
        }
    }

    private IEnumerator GenerateRandomZRotation()
    {

        while (true)
        {
            var min = baseSpeeds.z * (1 - speedsDeviations.z);
            var max = baseSpeeds.z * (1 + speedsDeviations.z);
            var range = max - min;

            var inverseSpeed = -currentZSpeed;
            inverseSpeed += Random.Range(-range, +range);

            if (inverseSpeed > 0)
            {
                inverseSpeed = inverseSpeed > max ? max : inverseSpeed;
                inverseSpeed = inverseSpeed < min ? min : inverseSpeed;
            }
            else
            {
                inverseSpeed = inverseSpeed < -max ? -max : inverseSpeed;
                inverseSpeed = inverseSpeed > -min ? -min : inverseSpeed;
            }

            currentZSpeed = inverseSpeed;

            yield return new WaitForSeconds(Random.
                Range(speedInversionIntervals.z * (1 - inversionIntervalDeviations.z), speedInversionIntervals.z * (1 + inversionIntervalDeviations.z)));
        }
    }


}
