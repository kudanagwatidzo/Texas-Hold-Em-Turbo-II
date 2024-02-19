using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using TMPro;


public class RockPaperScissorsScript : MonoBehaviour
{
    // Todo, enum from controller input to RPS

    private float timerDuration = 5.5f;
    private string p1Option, p2Option, winner;
    private bool isP1Locked, isP2Locked, rpsCompleted;
    public TextMeshProUGUI timerVisual;

    // Start is called before the first frame update
    void Start()
    {
        p1Option = "";
        p2Option = "";
        winner = "";
        isP1Locked = false;
        isP2Locked = false;
        rpsCompleted = false;
        timerVisual = GameObject.Find("Timer").GetComponent<TextMeshProUGUI>();
    }

    // Update is called once per frame
    void Update()
    {
        timerDuration -= Time.deltaTime;
        float seconds = Mathf.FloorToInt(timerDuration % 60);
        float milliseconds = (int)(timerDuration * 100f) % 100;

        if (rpsCompleted) timerVisual.text = winner + " Wins!";
        else timerVisual.text = string.Format("{0:00}:{1:00}", seconds, milliseconds);
        // When the timer hits 0, start rock paper scissors sequence
        if (timerDuration < 0 && !rpsCompleted)
        {
            winner = playRockPaperScissors();
        }
        else
        {
            readRPSInputs();
        }
    }

    private string playRockPaperScissors()
    {
        Debug.Log("Starting RPS");
        // If two players tie, reset the RPS situation
        if (string.Equals(p1Option, p2Option))
        {
            resetRPS();
            return "Tied";
        }
        else
        {
            rpsCompleted = true;
            return checkRPS();
        }

    }

    private string checkRPS ()
    {
        Debug.Log("Player 1 Chose: " + p1Option);
        Debug.Log("Player 2 Chose: " + p2Option);
        if (p1Option == "") return "p2";
        else if (p2Option == "") return "p1";
        else if ((p1Option == "rock" && p2Option == "scissors") || 
            (p1Option == "paper" && p2Option == "rock") || 
            (p1Option == "scissors" && p2Option == "paper")) return "p1";
        else return "p2";
    }

    private void resetRPS ()
    {
        timerDuration = 5.5f;
        p1Option = "";
        p2Option = "";
        isP1Locked = false;
        isP2Locked = false;
    }

    private void readRPSInputs ()
    {
        float p1Horizontal = Input.GetAxis("Player1Horizontal");
        float p2Horizontal = Input.GetAxis("Player2Horizontal");
        float p1Vertical = Input.GetAxis("Player1Vertical");
        float p2Vertical = Input.GetAxis("Player2Vertical");

        if (!isP1Locked)
        {
            if (Math.Abs(p1Horizontal) > 0.6 || p1Vertical > 0.6)
            {
                if (p1Horizontal > 0.6)
                {
                    p1Option = "rock";
                }
                else if (p1Horizontal < -0.6)
                {
                    p1Option = "scissors";
                }
                else if (p1Vertical > 0.6)
                {
                    p1Option = "paper";
                }
                isP1Locked = true;
                Debug.Log("P1 has locked: " + p1Option);
            }
        }
        if (!isP2Locked)
        {
            if (Math.Abs(p2Horizontal) > 0.6 || p2Vertical > 0.6)
            {
                if (p2Horizontal > 0.6)
                {
                    p2Option = "rock";
                }
                else if (p2Horizontal < -0.6)
                {
                    p2Option = "scissors";
                }
                else if (p2Vertical > 0.6)
                {
                    p2Option = "paper";
                }
                isP2Locked = true;
                Debug.Log("P2 has locked: " + p2Option);
            }
        }
    }
}
