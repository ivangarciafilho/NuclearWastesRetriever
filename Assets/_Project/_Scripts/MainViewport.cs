using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EnhancedBehaviours;

public class MainViewport : MyMonoBehaviour.MyMainViewport
{
                        public static Transform itsPivotTransform;
    [SerializeField]    private  Vector3 cameraOffset = new Vector3(0f,9f,-30f);
                        public static Vector3 itsPivotLocalPosition;

    private IEnumerator Start()
    {
        while (gameManager == null) yield return null;
        while (protagonist == null) yield return null;
        yield return null;

        Initialize();
    }

    private void FollowProtagonist()
    {
        itsPivotLocalPosition = itsPivotTransform.localPosition;

        itsPivotTransform.localPosition = Vector3.
            Lerp(itsPivotLocalPosition, (protagonist.itsLocalPosition
                + (protagonist.itsLocalRotation * cameraOffset)), thriceSmoothDeltatime);
    }

    private void FocusOnProtagonist()
    {
        itsRotation = itsTransform.rotation;

        itsTransform.rotation = Quaternion.Lerp(itsRotation, Quaternion.
            LookRotation(((protagonist.itsLocalPosition - itsPivotLocalPosition).
                normalized) * 9f, vector3Up), thriceSmoothDeltatime);
    }

    protected override void Initialize()
    {
        base.Initialize();
        itsPivotTransform = new GameObject().transform;
        itsPivotTransform.name = "MainViewportController";
        itsPivotTransform.position = protagonist.transform.position;
        itsTransform.SetParent(itsPivotTransform);
        itsPivotTransform.hierarchyCapacity = itsPivotTransform.hierarchyCount;
        itsTransform.localPosition = vector3Zero;
        itsTransform.localRotation = quaternionIdentity;
        DontDestroyOnLoad(itsPivotTransform.gameObject);
        GameManager.AfterLateUpdate += FollowProtagonist;
        GameManager.AfterLateUpdate += FocusOnProtagonist;
    }
}
