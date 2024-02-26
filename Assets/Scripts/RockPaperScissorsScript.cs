using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEditor;
using TMPro;


public class RockPaperScissorsScript : MonoBehaviour
{
    // Todo, enum from controller input to RPS
    private int winner, loser;
    private float timerDuration = 10f;
    private bool gameOver;
    private string p1Option, p2Option;
    private float p1Lockout, p2Lockout;
    private int currentP1Power, currentP2Power, totalP1Power, totalP2Power;
    private List<(string, string)>[] playerHands = new List<(string, string)>[2];
    private GameObject[] players = new GameObject[2];
    private GameObject[] playerControls = new GameObject[2];
    private DeckScript DECK_FRAMEWORK;
    private Dictionary<string, Sprite> _cards;
    public TextMeshProUGUI timerVisual, totalP1, totalP2, currentP1, currentP2;

    // Start is called before the first frame update
    void Start()
    {
        // Reset game
        gameOver = false;
        p1Option = p2Option = "";
        winner = -1;
        currentP1Power = currentP2Power = totalP1Power = totalP2Power = 0;
        p1Lockout = p2Lockout = 0f;
        // Grab the text game objects
        timerVisual = GameObject.Find("Timer").GetComponent<TextMeshProUGUI>();
        totalP1 = GameObject.Find("Player1Total").GetComponent<TextMeshProUGUI>();
        totalP2 = GameObject.Find("Player2Total").GetComponent<TextMeshProUGUI>();
        currentP1 = GameObject.Find("Player1Current").GetComponent<TextMeshProUGUI>();
        currentP2 = GameObject.Find("Player2Current").GetComponent<TextMeshProUGUI>();
        // Set up deck framework
        DECK_FRAMEWORK = GameObject.Find("Deck").GetComponent<DeckScript>();

        loadCards();

        players[0] = GameObject.Find("Player1");
        players[1] = GameObject.Find("Player2");
    }

    // Update is called once per frame
    void Update()
    {
        if (!gameOver)
        {
            timerDuration -= Time.deltaTime;
            p1Lockout -= Time.deltaTime;
            p2Lockout -= Time.deltaTime;
            float seconds = Mathf.FloorToInt(timerDuration % 60);
            float milliseconds = (int)(timerDuration * 100f) % 100;

            timerVisual.text = string.Format("{0:00}:{1:00}", seconds, milliseconds);
            totalP1.text = totalP1Power.ToString();
            totalP2.text = totalP2Power.ToString();
            currentP1.text = currentP1Power.ToString();
            currentP2.text = currentP2Power.ToString();
        }
        // End the game loop
        if (timerDuration < 0 && !gameOver)
        {
            gameOver = true;
            if (totalP1Power > totalP2Power) winner = 0;
            else if (totalP2Power > totalP1Power) winner = 1;

            if (winner != -1)
            {
                loser = 1 - winner;
                players[0].transform.GetChild(0).gameObject.SetActive(false);
                players[1].transform.GetChild(0).gameObject.SetActive(false);

                players[winner].GetComponent<Animator>().SetTrigger("Attack");
                players[loser].GetComponent<Animator>().SetTrigger("OnDeath");

                timerVisual.text = "Player " + (winner + 1).ToString() + " Wins!";
            }
            else
            {
                timerVisual.text = "TIED!";
            }


        }

        readInputs();

        checkOptions();

        evaluateHand();

        showHand();
    }

    /*
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
            return checkOptions();
        }

    }
    */

    private void checkOptions()
    {
        if (p1Option != "")
        {
            if (p1Option == "draw")
            {
                playerHands[0].Add(DECK_FRAMEWORK.draw());
                if (playerHands[0].Count > 5) playerHands[0].RemoveAt(0);
            }
            if (p1Option == "attack")
            {
                totalP1Power += currentP1Power;
                currentP1Power = 0;
                playerHands[0].Clear();
            }
            // Reset the option and lock the player out
            p1Option = "";
            p1Lockout = 0.25f;

        }
        if (p2Option != "")
        {
            if (p2Option == "draw")
            {
                playerHands[1].Add(DECK_FRAMEWORK.draw());
                if (playerHands[1].Count > 5) playerHands[1].RemoveAt(0);
            }
            if (p2Option == "attack")
            {
                totalP2Power += currentP2Power;
                currentP2Power = 0;
                playerHands[1].Clear();
            }
            // Reset the option and lock the player out
            p2Option = "";
            p2Lockout = 0.25f;
        }
    }

    private void resetGame()
    {
        timerDuration = 5.5f;
        p1Option = "";
        p2Option = "";
        players[0].transform.GetChild(0).gameObject.SetActive(true);
        players[1].transform.GetChild(0).gameObject.SetActive(true);
        players[0].GetComponent<Animator>().SetBool("Stance", false);
        players[1].GetComponent<Animator>().SetBool("Stance", false);
    }

    private void readInputs()
    {
        float p1Horizontal = Input.GetAxis("Player1Horizontal");
        float p2Horizontal = Input.GetAxis("Player2Horizontal");
        float p1Vertical = Input.GetAxis("Player1Vertical");
        float p2Vertical = Input.GetAxis("Player2Vertical");

        if (p1Lockout < 0)
        {
            if (Math.Abs(p1Horizontal) > 0.6 || p1Vertical > 0.6)
            {
                if (p1Horizontal > 0.6)
                {
                    p1Option = "draw";
                }
                else if (p1Horizontal < -0.6)
                {
                    p1Option = "wildcard";
                }
                else if (p1Vertical > 0.6)
                {
                    p1Option = "attack";
                }
                // Make the controls disappear and start run animation
                // players[0].transform.GetChild(0).gameObject.SetActive(false);
                // players[0].GetComponent<Animator>().SetBool("Stance", true);

                Debug.Log("P1 has locked: " + p1Option);
            }
        }


        if (p2Lockout < 0)
        {
            if (Math.Abs(p2Horizontal) > 0.6 || p2Vertical > 0.6)
            {
                if (p2Horizontal > 0.6)
                {
                    p2Option = "draw";
                }
                else if (p2Horizontal < -0.6)
                {
                    p2Option = "wildcard";
                }
                else if (p2Vertical > 0.6)
                {
                    p2Option = "attack";
                }
                // Make the controls disappear and start run animation
                // players[1].transform.GetChild(0).gameObject.SetActive(false);
                // players[1].GetComponent<Animator>().SetBool("Stance", true);

                Debug.Log("P2 has locked: " + p2Option);
            }
        }
    }

    // Render the current hands
    private void showHand()
    {
        // For each card in the hand, update the sprite of the nearest blank card
        (string, string)[] exportedP1Hand = DECK_FRAMEWORK.sortHand(playerHands[0].ToArray());
        for (int i = 0; i < exportedP1Hand.Length; i++)
        {
            SpriteRenderer currentCardRenderer = GameObject.Find("Player1Hand").transform.GetChild(i).gameObject.GetComponent<SpriteRenderer>();
            currentCardRenderer.sprite = _cards[exportedP1Hand[i].Item1 + exportedP1Hand[i].Item2];
        }
        for (int j = exportedP1Hand.Length; j < 5; j++)
        {
            SpriteRenderer currentCardRenderer = GameObject.Find("Player1Hand").transform.GetChild(j).gameObject.GetComponent<SpriteRenderer>();
            currentCardRenderer.sprite = _cards["blank"];
        }
        (string, string)[] exportedP2Hand = DECK_FRAMEWORK.sortHand(playerHands[1].ToArray());
        for (int i = 0; i < exportedP2Hand.Length; i++)
        {
            SpriteRenderer currentCardRenderer = GameObject.Find("Player2Hand").transform.GetChild(i).gameObject.GetComponent<SpriteRenderer>();
            currentCardRenderer.sprite = _cards[exportedP2Hand[i].Item1 + exportedP2Hand[i].Item2];
        }
        for (int j = exportedP2Hand.Length; j < 5; j++)
        {
            SpriteRenderer currentCardRenderer = GameObject.Find("Player2Hand").transform.GetChild(j).gameObject.GetComponent<SpriteRenderer>();
            currentCardRenderer.sprite = _cards["blank"];
        }
    }

    private void loadCards()
    {
        Sprite[] SpritesData = Resources.LoadAll<Sprite>("Cards");
        _cards = new Dictionary<string, Sprite>();
        for (int i = 0; i < SpritesData.Length; i++)
        {
            _cards.Add(SpritesData[i].name, SpritesData[i]);
        }
        playerHands[0] = DECK_FRAMEWORK.drawNumber(3);
        playerHands[1] = DECK_FRAMEWORK.drawNumber(3);
    }

    private void evaluateHand()
    {
        (string, string)[] exportedP1Hand = playerHands[0].ToArray();
        (string, string)[] exportedP2Hand = playerHands[1].ToArray();

        currentP1Power = DECK_FRAMEWORK.evaluateHand(exportedP1Hand);
        currentP2Power = DECK_FRAMEWORK.evaluateHand(exportedP2Hand);

        Debug.Log("P1 Score: " + currentP1Power.ToString());
        Debug.Log("P2 Score: " + currentP2Power.ToString());
    }

}
