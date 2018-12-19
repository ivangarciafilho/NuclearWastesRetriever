using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Linq.Expressions;
using UnityEngine;
using UnityEngine.UI;
using EnhancedBehaviours;
using Random = UnityEngine.Random;

[DisallowMultipleComponent]
public sealed class RetrievedWastesDisplay : MyMonoBehaviour
{
    public enum DisplayStatus
    {
        fadeIn,
        fadeOut,
        iddle
    }

    public  static  RetrievedWastesDisplay   retrievedWastesDisplayInstance                    { get; private set; }
    public  static  Text                     itsText;
    public  static  int                      score;
    public  static  DisplayStatus            currentDisplayStatus;
    public  static  float                    previousUpdate;

    private void Awake()
    {
        if (retrievedWastesDisplayInstance != null)
            if (retrievedWastesDisplayInstance != this)
                DestroyImmediate(gameObject);
        retrievedWastesDisplayInstance = this;
        DontDestroyOnLoad(transform.root.gameObject);
    }

    private IEnumerator Start()
    {
        while (gameManager == null) yield return null;
        while (protagonist == null) yield return null;
        yield return null;
        Initialize();
    }

    protected override void Initialize()
    {
        base.Initialize();
        itsTransform = transform.root;
        itsText = GetComponent<Text>();
        itsText.CrossFadeAlpha(0f, 1f, false);

        previousUpdate = 0;
        score = 0;
        currentDisplayStatus = DisplayStatus.iddle;

        WasteRetrieverBalloon.OnRetrieveEvent += UpdateAndDisplayScore;
    }

    public void UpdateAndDisplayScore()
    {
        score++;

        if (currentDisplayStatus != DisplayStatus.iddle) return;

        itsTransform.localPosition = protagonist.itsLocalPosition + vector3Up;

        itsText.text = String.Format("+ 1 ({0}) ", score);
        itsText.CrossFadeAlpha(1f, 3f, false);

        currentDisplayStatus = DisplayStatus.fadeIn;
        previousUpdate = time;

        GameManager.BeforeUpdate += HandleDisplayFade;
    }

    private void HandleDisplayFade()
    {
        if (currentDisplayStatus == DisplayStatus.iddle) return;
        itsTransform.LookAt(mainViewport.itsTransform);
        itsTransform.localPosition += (vector3Up * smoothDeltatime);

        if (time > previousUpdate + 3)
            if (currentDisplayStatus == DisplayStatus.fadeIn)
            {
                itsText.CrossFadeAlpha(0f, 6f, false);
                currentDisplayStatus = DisplayStatus.fadeOut;
            }

        if (time > previousUpdate + 9)
            if (currentDisplayStatus == DisplayStatus.fadeOut)
                currentDisplayStatus = DisplayStatus.iddle;

        if (time > previousUpdate + 12)
            GameManager.BeforeUpdate -= HandleDisplayFade;
    }
}