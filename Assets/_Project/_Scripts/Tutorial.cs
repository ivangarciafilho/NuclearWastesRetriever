using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tutorial : MonoBehaviour
{
    public enum Tutorial_State
    {
        FIRST,
        SECOND,
        SONAR,
        NUCLEARWASTE,
        MENU
    };

    public TextMesh text;
    public GameObject textObject;
    public Animator textAnimator;
    const string firstUserKey = "first_man_on_land";
    bool firstUser = false;
    Tutorial_State state = Tutorial_State.FIRST;
    bool holdOn = false;
    public static bool completedTutorial = false;

    float secondsPressingSubmergeButton = 0.0f;

    void Start()
    {
        int firstUserResult = PlayerPrefs.GetInt(firstUserKey);

        if(firstUserResult == 1)
        {
            textObject.SetActive(false);
            completedTutorial = true;
        }
        else
        {
            firstUser = true;

#if !UNITY_EDITOR
            PlayerPrefs.SetInt(firstUserKey, 1);
#endif
        }
    }

    IEnumerator ChangeState(float seconds, Tutorial_State ts, string str)
    {
        holdOn = true;
        yield return new WaitForSeconds(seconds);
        state = ts;
        text.text = str;
        textAnimator.SetBool("FadeOut", false);
        holdOn = false;
    }

    void Update()
    {
        if(firstUser && !holdOn)
        {
            if(state == Tutorial_State.FIRST)
            {
                if (Input.GetKey(KeyCode.S) && secondsPressingSubmergeButton < 3.1f)
                {
                    secondsPressingSubmergeButton += Time.deltaTime;
                }

                if (secondsPressingSubmergeButton >= 3.1f)
                {
                    textAnimator.SetBool("FadeOut", true);
                    secondsPressingSubmergeButton = 0.0f;
                    StartCoroutine(ChangeState(6f, Tutorial_State.SECOND, "Press A or D to turn right or left\nor both to move forward\nQ and E to backwards"));
                }
            }
            else if(state == Tutorial_State.SECOND)
            {
                if ((Input.GetKey(KeyCode.E) || Input.GetKey(KeyCode.Q) || Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.D)) && secondsPressingSubmergeButton < 3.1f)
                {
                    secondsPressingSubmergeButton += Time.deltaTime;
                }

                if (secondsPressingSubmergeButton >= 3.1f)
                {
                    textAnimator.SetBool("FadeOut", true);
                    secondsPressingSubmergeButton = 0.0f;
                    StartCoroutine(ChangeState(6f, Tutorial_State.SONAR, "The green particles represents a sonar that \nshows you where the Nuclear Waste is,\nfollow them"));
                }
            }
            else if(state == Tutorial_State.SONAR)
            {
                secondsPressingSubmergeButton += Time.deltaTime;

                if (secondsPressingSubmergeButton >= 12f)
                {
                    textAnimator.SetBool("FadeOut", true);
                    secondsPressingSubmergeButton = 0.0f;

                    StartCoroutine(ChangeState(8f, Tutorial_State.NUCLEARWASTE, "When you reach the Nuclear Waste,\nPress SPACE to make it emerge from the water"));
                }
            }
            else if(state == Tutorial_State.NUCLEARWASTE)
            {
                secondsPressingSubmergeButton += Time.deltaTime;
                if (secondsPressingSubmergeButton >= 13f)
                {
                    textAnimator.SetBool("FadeOut", true);
                    secondsPressingSubmergeButton = 0.0f;

                    StartCoroutine(ChangeState(3f, Tutorial_State.MENU, "Press ESC for Menu"));
                    completedTutorial = true;
                }
            }
            else if(state == Tutorial_State.MENU)
            {
                secondsPressingSubmergeButton += Time.deltaTime;
                if (secondsPressingSubmergeButton >= 8f || Input.GetKeyDown(KeyCode.Escape))
                {
                    textAnimator.SetBool("FadeOut", true);
                    secondsPressingSubmergeButton = 0.0f;
                    firstUser = false;
                }
            }
        }
    }
}
