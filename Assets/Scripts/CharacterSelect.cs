using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;

public class CharacterSelect : MonoBehaviour
{

    private string playerNumber;
    private int index;
    private float lockout;
    private bool lockedIn;
    private Transform crosshair, wildcards, submitKey;
    private MenuNavigation menu;
    private List<Transform> texts;

    // Start is called before the first frame update
    void Start()
    {
        index = 0;
        lockout = 0;
        lockedIn = false;
        texts = new List<Transform>();
        playerNumber = gameObject.name[0].ToString();
        menu = GameObject.Find("Background").GetComponent<MenuNavigation>();
        crosshair = gameObject.transform.Find("Crosshair");
        wildcards = gameObject.transform.Find("Wildcards");
        submitKey = gameObject.transform.Find("Controls").Find("Submit");

        foreach (Transform child in gameObject.transform.Find("Descriptions"))
        {
            texts.Add(child);
            child.gameObject.SetActive(false);
        }

        texts[0].gameObject.SetActive(true);
    }

    // Update is called once per frame
    void Update()
    {

        if (!lockedIn)
        {
            lockout -= Time.deltaTime;

            float pHorizontal = Input.GetAxis("Player" + playerNumber + "Horizontal");
            float pVertical = Input.GetAxis("Player" + playerNumber + "Vertical");

            if ((Math.Abs(pHorizontal) > 0.6 || pVertical > 0.6) && lockout <= 0)
            {
                if (pHorizontal > 0.6)
                    shift(1);
                else if (pHorizontal < -0.6)
                    shift(-1);
                else if (pVertical > 0.6)
                {
                    submit();
                }
                lockout = 0.5f;
            }
        }
    }

    private void shift(int direction)
    {
        index += direction;
        if (index < 0)
            index = 0;
        else if (index > wildcards.childCount - 1)
            index = wildcards.childCount - 1;

        Vector3 submitPosition = new Vector3(wildcards.GetChild(index).position.x, submitKey.position.y, submitKey.position.z);

        crosshair.position = wildcards.GetChild(index).position;
        submitKey.position = submitPosition;

        foreach (Transform child in texts)
        {
            child.gameObject.SetActive(false);
        }

        texts[index].gameObject.SetActive(true);
    }

    private void submit()
    {
        lockedIn = true;
        // Trigger locked in animations
        crosshair.GetComponent<Animator>().SetTrigger("locked");
        submitKey.GetComponent<Animator>().SetTrigger("locked");
        // Update player perf
        PlayerPrefs.SetInt("Player" + playerNumber + "Wildcard", index);
        // Check if both player perfs are set
        int p1 = PlayerPrefs.GetInt("Player1Wildcard", -1);
        int p2 = PlayerPrefs.GetInt("Player2Wildcard", -1);
        if (p1 != -1 && p2 != -1)
            menu.fight();
    }


}
