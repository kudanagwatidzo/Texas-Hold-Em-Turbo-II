using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using TMPro;


public class RockPaperScissorsScript : MonoBehaviour
{
    // Todo, enum from controller input to RPS
    private int winner, loser;
    private float timerDuration = 5.5f;
    private string p1Option, p2Option;
    private bool isP1Locked, isP2Locked, rpsCompleted;

    private GameObject[] players = new GameObject[2];
    public TextMeshProUGUI timerVisual;

    // Start is called before the first frame update
    void Start()
    {
        // Reset RPS 
        winner = 0;
        p1Option = "";
        p2Option = "";
        isP1Locked = false;
        isP2Locked = false;
        rpsCompleted = false;
        // Grab the needed game objects
        timerVisual = GameObject.Find("Timer").GetComponent<TextMeshProUGUI>();
        players[0] = GameObject.Find("Player1");
        players[1] = GameObject.Find("Player2");
    }

    // Update is called once per frame
    void Update()
    {
        timerDuration -= Time.deltaTime;
        float seconds = Mathf.FloorToInt(timerDuration % 60);
        float milliseconds = (int)(timerDuration * 100f) % 100;

        if (rpsCompleted) timerVisual.text = "Player " + (winner + 1).ToString() + " Wins!";
        else timerVisual.text = string.Format("{0:00}:{1:00}", seconds, milliseconds);
        // When the timer hits 0, start rock paper scissors sequence
        if (timerDuration < 0 && !rpsCompleted)
        {
            winner = playRockPaperScissors() - 1;
            loser = 1 - winner;

            if (winner >= 0)
            {
                players[winner].GetComponent<Animator>().SetTrigger("Attack");
                players[loser].GetComponent<Animator>().SetTrigger("OnDeath");
            }

        }
        else
        {
            readRPSInputs();
        }
    }

    private int playRockPaperScissors()
    {
        Debug.Log("Starting RPS");
        // If two players tie, reset the RPS situation
        if (string.Equals(p1Option, p2Option))
        {
            resetRPS();
            return 0;
        }
        else
        {
            rpsCompleted = true;
            return checkRPS();
        }

    }

    private int checkRPS ()
    {
        Debug.Log("Player 1 Chose: " + p1Option);
        Debug.Log("Player 2 Chose: " + p2Option);
        if (p1Option == "") return 2;
        else if (p2Option == "") return 1;
        else if ((p1Option == "rock" && p2Option == "scissors") || 
            (p1Option == "paper" && p2Option == "rock") || 
            (p1Option == "scissors" && p2Option == "paper")) return 1;
        else return 2;
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
