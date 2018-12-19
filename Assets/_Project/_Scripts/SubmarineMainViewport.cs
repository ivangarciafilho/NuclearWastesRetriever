using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EnhancedBehaviours;
using FishFlock;

public class SubmarineMainViewport : MyMonoBehaviour.MyMainViewport
{
                        public static Transform itsPivotTransform;
    [SerializeField]    private  Vector3 cameraOffset = new Vector3(0f,9f,-30f);
    [SerializeField]    private Transform bubbleStreamTransform;
    [SerializeField]    private ParticleSystem bubbleStreamParticle;
    public static Vector3 itsPivotLocalPosition;

    FishFlockController fishController;
    //List<MeshRenderer> fishRenderers = new List<MeshRenderer>();

    Camera cam;


    private IEnumerator Start()
    {
        while (gameManager == null) yield return null;
        while (protagonist == null) yield return null;
        yield return null;

        Initialize();

        cam = GetComponent<Camera>();



        if (fishController)
        {
            //while (fishController.agentsObjects.Count <= 0) yield return null;

            //for (int i = 0; i < fishController.agentsObjects.Count; i++)
            //{
            //    GameObject obj = fishController.agentsObjects[i];
            //    MeshRenderer obj_renderer = obj.GetComponentInChildren<MeshRenderer>();

            //    fishRenderers.Add(obj_renderer);
            //}
        }
    }

    private void FollowProtagonist()
    {
        itsPivotLocalPosition = itsPivotTransform.localPosition;
        //itsPivotTransform.localPosition = Vector3.
        //    Lerp(itsPivotLocalPosition, (protagonist.itsLocalPosition
        //        + (protagonist.itsLocalRotation * cameraOffset)), thriceSmoothDeltatime);
    }

    private void FocusOnProtagonist()
    {
        itsRotation = itsTransform.rotation;

        //itsTransform.rotation = Quaternion.Lerp(itsRotation, Quaternion.
        //    LookRotation(((protagonist.itsLocalPosition - itsPivotLocalPosition).
        //        normalized) * 9f, vector3Up), thriceSmoothDeltatime);
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
        SubmarineGameManager.AfterLateUpdate += FollowProtagonist;
        SubmarineGameManager.AfterLateUpdate += FocusOnProtagonist;
        SubmarineGameManager.AfterSlowFixedUpdate += BubbleStreams;
    }

    //private void Update()
    //{
    //    if (!fishController) return;

    //    for(int i = 0; i < fishRenderers.Count; i++)
    //    {
    //       MeshRenderer rend = fishRenderers[i];
    //       FishBehaviour behaviour = fishController.behaviours[i];

    //       if(!RendererHelper.IsVisibleFrom(rend, cam))
    //       {
    //           if (rend.enabled)
    //               rend.enabled = false;

    //           behaviour.isVisible = false;
    //       }
    //       else
    //       {
    //           if (!rend.enabled)
    //               rend.enabled = true;

    //           behaviour.isVisible = true;
    //       }

    //       fishController.behaviours[i] = behaviour;
    //    }
    //}//TAKING 2ms from proc


    private void BubbleStreams()
    {
        if (SubmarineProtagonist.lightsFlicking) return;

        for ( int i = 0; i < 6; i++ )
        {
            bubbleStreamTransform.localPosition = protagonist.itsLocalPosition;
            bubbleStreamTransform.localPosition = bubbleStreamTransform.localPosition - (vector3Up * 12f);
            bubbleStreamTransform.rotation = quaternionIdentity;
            bubbleStreamTransform.Rotate( 0f,Random.Range( -180f,180f ), 0f );
            bubbleStreamTransform.localPosition
                = bubbleStreamTransform.localPosition - ( bubbleStreamTransform.forward * ( 40f - ( 3 * i ) ) );

            bubbleStreamParticle.Play();
        }
    }
}
