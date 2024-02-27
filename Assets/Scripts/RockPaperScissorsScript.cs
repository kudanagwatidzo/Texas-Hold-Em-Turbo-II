using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEditor;
using TMPro;


public class RockPaperScissorsScript : MonoBehaviour
{
    System.Random _random = new System.Random();
    private int winner, loser;
    private float timerDuration = 10f;
    private bool gameOver;
    private string p1Option, p2Option;
    private float p1Lockout, p2Lockout;
    private int[] playerHealth = new int[2];
    private int[] playerPower = new int[2]; 
    private List<(string, string)>[] playerHands = new List<(string, string)>[2];
    private GameObject[] players = new GameObject[2];
    private GameObject[] playerControls = new GameObject[2];
    private DeckScript DECK_FRAMEWORK;
    private Dictionary<string, Sprite> _cards, _health;
    public TextMeshProUGUI timerVisual, currentP1, currentP2;

    // Start is called before the first frame update
    void Start()
    {
        // Reset game
        gameOver = false;
        p1Option = p2Option = "";
        winner = -1;
        playerHealth[0] = playerHealth[1] = 10;
        playerPower[0] = playerPower[1] = 0;
        p1Lockout = p2Lockout = 0f;
        // Grab the text game objects
        timerVisual = GameObject.Find("Timer").GetComponent<TextMeshProUGUI>();
        currentP1 = GameObject.Find("Player1Current").GetComponent<TextMeshProUGUI>();
        currentP2 = GameObject.Find("Player2Current").GetComponent<TextMeshProUGUI>();
        // Set up deck framework
        DECK_FRAMEWORK = GameObject.Find("Deck").GetComponent<DeckScript>();

        loadCards();

        loadHealth();

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
            currentP1.text = playerPower[0].ToString();
            currentP2.text = playerPower[1].ToString();
        }
        // End the game loop
        if (timerDuration < 0 && !gameOver)
        {
            gameOver = true;
            if (playerHealth[0] > playerHealth[1]) winner = 0;
            else if (playerHealth[1] > playerHealth[0]) winner = 1;

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

        showHealth();
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
                playerHealth[1] -= playerPower[0];
                playerPower[0] = 0;
                playerHands[0].Clear();
            }
            if (p1Option == "wildcard")
            {
                wildcard(0);
            }
            // Reset the option and lock the player out
            p1Option = "";
            p1Lockout = 0.5f;

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
                playerHealth[0] -= playerPower[1];
                playerPower[1] = 0;
                playerHands[1].Clear();
            }
            if (p2Option == "wildcard")
            {
                wildcard(1);
            }
            // Reset the option and lock the player out
            p2Option = "";
            p2Lockout = 0.5f;
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

    private void loadHealth()
    {
        Sprite[] SpritesData = Resources.LoadAll<Sprite>("health-bar");
        _health = new Dictionary<string, Sprite>();
        for (int i = 0; i < SpritesData.Length; i++)
        {
            _health.Add(SpritesData[i].name, SpritesData[i]);
        }
    }

    private void evaluateHand()
    {
        (string, string)[] exportedP1Hand = playerHands[0].ToArray();
        (string, string)[] exportedP2Hand = playerHands[1].ToArray();

        playerPower[0] = DECK_FRAMEWORK.evaluateHand(exportedP1Hand);
        playerPower[1] = DECK_FRAMEWORK.evaluateHand(exportedP2Hand);

        Debug.Log("P1 Score: " + playerPower[0].ToString());
        Debug.Log("P2 Score: " + playerPower[1].ToString());
    }

    private void showHealth ()
    {
        SpriteRenderer player1Health = GameObject.Find("Player1Health").GetComponent<SpriteRenderer>();
        SpriteRenderer player2Health = GameObject.Find("Player2Health").GetComponent<SpriteRenderer>();
        player1Health.sprite = _health["health-bar_" + playerHealth[0].ToString()];
        player2Health.sprite = _health["health-bar_" + playerHealth[1].ToString()];
    }

    private void wildcard (int player)
    {
        int opponent = 1 - player;
        int dice = _random.Next(1, 7);
        switch (dice)
        {
            case 1:
                playerHands[player].Clear();
                break;
            case 2:
                if (playerHands[opponent].Count > 0) playerHands[opponent].RemoveAt(0);
                break;
            case 3:
                if (playerHands[player].Count > 0) playerHands[player].RemoveAt(0);
                break;
            case 4:
                if (playerHands[player].Count > 2)
                {
                    playerHands[player].RemoveAt(0);
                    playerHands[player].RemoveAt(0);
                }
                else playerHands[player].Clear();
                break;
            case 5:
                if (playerHands[player].Count > 2)
                {
                    playerHands[player].RemoveAt(0);
                    playerHands[player].RemoveAt(0);
                }
                else playerHands[player].Clear();
                playerHands[player].Add(("hearts", "A"));
                playerHands[player].Add(("spades", "A"));
                break;
            case 6: 
                if (playerHands[player].Count > 3)
                {
                    playerHands[player].RemoveAt(0);
                    playerHands[player].RemoveAt(0);
                    playerHands[player].RemoveAt(0);
                }
                else playerHands[player].Clear();

                playerHands[player].Add(("hearts", "K"));
                playerHands[player].Add(("clubs", "Q"));
                playerHands[player].Add(("spades", "J"));
                break;
        } 

    }
}
