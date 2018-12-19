using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using EnhancedBehaviours;

[RequireComponent(typeof(Rigidbody))]
public sealed class Protagonist : MyMonoBehaviour.MyProtagonist
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


    private IEnumerator Start()
    {
        while (gameManager == null) yield return null;
        yield return null;
        Initialize();
    }

    protected override void Initialize()
    {
        base.Initialize();
        itsBuoys = GetComponentsInChildren<Rigidbody>();
        amountOfBuoys = itsBuoys.Length;
        GameManager.BeforeUpdate += HandleUserInputs;
        GameManager.BeforeFixedUpdate += HandleProtagonistMotion;
    }


    private float _distanceToProtagonist;
    private float _squaredDistanceToProtagonist;
    private float _squareRoot;
    private Vector3 _directionToNuclearWaste;
    private Vector3 _absoluteSquaredDifference;
    private void HandleUserInputs()
    {
        //caching
        itsLocalPosition    =   itsTransform.localPosition;
        itsLocalRotation    =   itsTransform.localRotation;

        //Adjusting LeftHelixInput
        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.Q))
        {
            leftHelixMotion = Input.GetKey(KeyCode.A) ?
                Mathf.Lerp(leftHelixMotion, 1f, deltatime)
                : Mathf.Lerp(leftHelixMotion, -1f, deltatime);
        }
        else { leftHelixMotion = Mathf.Lerp(leftHelixMotion, 0f, twiceDeltatime); }

        //Adjusting RightHelixInput
        if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.E))
        {
            rightHelixMotion = Input.GetKey(KeyCode.D) ?
                Mathf.Lerp(rightHelixMotion, 1f, deltatime)
                : Mathf.Lerp(rightHelixMotion, -1f, deltatime);
        }
        else { rightHelixMotion = Mathf.Lerp(rightHelixMotion, 0f, twiceDeltatime); }


        //Rotate Helix
        if (rightHelixMotion > 0.0111f || rightHelixMotion < 0.0111f)
            rightHelixTransform.Rotate(0f, 0f, -720f * rightHelixMotion * smoothDeltatime);

        if (leftHelixMotion > 0.0111f || leftHelixMotion < 0.0111f)
            leftHelixTransform.Rotate(0f, 0f, -720f * leftHelixMotion * smoothDeltatime);

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
            _squareRoot = ((_squareRoot + (_squaredDistanceToProtagonist / _squareRoot)) / 2);
            _squareRoot = ((_squareRoot + (_squaredDistanceToProtagonist / _squareRoot)) / 2);

            _distanceToProtagonist = _squareRoot;

            if (_distanceToProtagonist < 12f)
                WasteRetrieverBalloon.wasteRetrieverBalloonInstance.Retrieve();
        }
    }



    private void HandleProtagonistMotion()
    {
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
}
