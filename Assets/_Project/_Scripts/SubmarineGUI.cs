using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[Serializable]
public struct GUI_Item
{
    public string name;
    public GameObject obj;
    public Transform transform;
    public Vector2 size;
    public Vector2 offset;
    public bool checkPressingClick;
    public UnityEvent onClick;
};

public class SubmarineGUI : MonoBehaviour
{
    public static SubmarineGUI instance;

    public GUI_Item[] items;

    [Space(10)]
    public Camera cam;
    public float scaling_speed = 1.2f;
    public bool drawGizmos = false;

    Vector3 scale_up_vec = new Vector3(0.19f, 0.19f, 1f);
    Vector3 scale_normal_vec = new Vector3(0.16f, 0.16f, 1f);

    Transform currSelectedButtonTransform;
    int currSelectedButtonIndex;

    public GameObject guiObject;

    private void Awake()
    {
        instance = this;
    }

    void UpdateMainMenu()
    {
        if (items == null || items.Length == 0) return;
        currSelectedButtonTransform = null;

        Vector2 mousePos = cam.ScreenToWorldPoint(Input.mousePosition);

        for (int i = 0; i < items.Length; i++)
        {
            Transform buttonTransform = items[i].transform;

            if (items[i].obj.activeInHierarchy)
            {
                Vector3 pos = items[i].transform.position;
                pos.x += items[i].offset.x;
                pos.y += items[i].offset.y;
                pos.z = 1f;

                Vector2 size = items[i].size / 2;

                if (mousePos.x >= (pos.x - size.x) && mousePos.x <= (pos.x + size.x) &&
                    mousePos.y >= (pos.y - size.y) && mousePos.y <= (pos.y + size.y))
                {
                    currSelectedButtonTransform = items[i].transform;
                    currSelectedButtonIndex = i;
                }
                else
                {
                    buttonTransform.localScale = Vector3.Lerp(buttonTransform.localScale,
                                                                scale_normal_vec,
                                                                Time.deltaTime * scaling_speed);
                }
            }
        }

        if (currSelectedButtonTransform == null) return;

        currSelectedButtonTransform.localScale = Vector3.Lerp(currSelectedButtonTransform.localScale,
                                                            scale_up_vec,
                                                            Time.deltaTime * scaling_speed);

        if (Input.GetMouseButtonDown(0) && currSelectedButtonTransform)
        {
            items[currSelectedButtonIndex].onClick.Invoke();
        }
    }

    private void OnDrawGizmos()
    {
        if (!drawGizmos) return;

        for (int i = 0; i < items.Length; i++)
        {
            if (!items[i].obj.activeInHierarchy) continue;

            Vector3 pos = items[i].transform.position;
            pos.x += items[i].offset.x;
            pos.y += items[i].offset.y;
            pos.z = 1f;
            Vector2 size = items[i].size;

            Gizmos.DrawWireCube(pos, size);
        }
    }

    private void OnEnable()
    {
        GameManager.AfterUpdate += UpdateMainMenu;
    }

    private void OnDisable()
    {
        GameManager.AfterUpdate -= UpdateMainMenu;
    }
}
