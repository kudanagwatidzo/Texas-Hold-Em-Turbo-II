using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;
using UnityEngine;
using UnityEditor.Animations;
using UnityEditor;
using TMPro;


public class RockPaperScissorsScript : MonoBehaviour
{
    System.Random _random = new System.Random();
    private int winner, loser;
    private float timerDuration;
    private bool roundOver = false;
    private string p1Option, p2Option;
    private int[] playerHealth = new int[2];
    private int[] playerPower = new int[2];
    private float[,] playerLockouts = new float[2, 3];
    private Animator[,] playerControls = new Animator[2, 3];
    public List<int> rounds = new List<int>();
    private List<(string, string)>[] playerHands = new List<(string, string)>[2];
    private GameObject[] players = new GameObject[2];
    private GameObject[] assists = new GameObject[2];
    private DeckScript DECK_FRAMEWORK;
    private Dictionary<string, Sprite> _cards, _health;
    public Sprite[] wildcardSprites = new Sprite[3];
    public AnimatorController[] assistAnimators = new AnimatorController[3];
    private TextMeshProUGUI timerVisual, currentP1, currentP2;
    private GameObject textDescription, menu;

    // Start is called before the first frame update
    void Start()
    {
        // Grab the necessary game objects on screen
        timerVisual = GameObject.Find("Timer").GetComponent<TextMeshProUGUI>();
        currentP1 = GameObject.Find("Player1Current").GetComponent<TextMeshProUGUI>();
        currentP2 = GameObject.Find("Player2Current").GetComponent<TextMeshProUGUI>();
        // Grab players
        players[0] = GameObject.Find("Player1");
        players[1] = GameObject.Find("Player2");
        // Grab assists: TODO
        assists[0] = GameObject.Find("Assist1");
        assists[1] = GameObject.Find("Assist2");
        
        textDescription = GameObject.Find("Descriptions");
        menu = GameObject.Find("Menus");

        loadCharacterSelectData();
        // Set up deck framework
        DECK_FRAMEWORK = GameObject.Find("Deck").GetComponent<DeckScript>();
        // Start playthrough
        resetGame();
    }

    // Update is called once per frame
    void Update()
    {
        if (!roundOver)
        {
            timerDuration -= Time.deltaTime;
            float seconds = Mathf.FloorToInt(timerDuration % 60);
            float milliseconds = (int)(timerDuration * 100f) % 100;

            timerVisual.text = string.Format("{0:00}:{1:00}", seconds, milliseconds);
            currentP1.text = playerPower[0].ToString();
            currentP2.text = playerPower[1].ToString();

            updateLockouts();

            readInputs();

            checkInputs();

            evaluateHand();

            showHand();

            showHealth();
        }
        // End the game loop
        if ((timerDuration < 0 || playerHealth[0] <= 0 || playerHealth[1] <= 0) && !roundOver)
        {
            roundOver = true;
            if (playerHealth[0] > playerHealth[1])
            {
                winner = 0;
                rounds.Add(-1);
            }
            else if (playerHealth[1] > playerHealth[0])
            {
                winner = 1;
                rounds.Add(1);
            }
            else
            {
                winner = -1;
            }
            // Display winner of the round
            if (winner != -1)
            {
                loser = 1 - winner;
                players[0].transform.GetChild(0).gameObject.SetActive(false);
                players[1].transform.GetChild(0).gameObject.SetActive(false);
                textDescription.SetActive(false);
                players[winner].transform.Find("PlayerSprite").GetComponent<Animator>().SetTrigger("Attack");
                players[loser].transform.Find("PlayerSprite").GetComponent<Animator>().SetTrigger("OnDeath");

                timerVisual.text = "Player " + (winner + 1).ToString() + " Wins Round " + rounds.Count.ToString() + "!";
            }
            else
            {
                timerVisual.text = "Round " + rounds.Count.ToString() + " is Tied!";
            }
            // Check if best of 3 is completed
            if (rounds.Count > 1 && rounds.AsQueryable().Sum() != 0)
            {
                menu.SetActive(true);
            }
            else
            {
                Invoke("resetGame", 5.0f);
            }
        }
    }

    private void loadCharacterSelectData ()
    {
        int p1Assist = PlayerPrefs.GetInt("Player1Wildcard", 1);
        int p2Assist = PlayerPrefs.GetInt("Player2Wildcard", 1);
        // Set wildcard icons from character select
        GameObject.Find("P1WildcardSprite").GetComponent<SpriteRenderer>().sprite = wildcardSprites[p1Assist];
        GameObject.Find("P2WildcardSprite").GetComponent<SpriteRenderer>().sprite = wildcardSprites[p2Assist];
        // Set wildcard assist sprites
        GameObject.Find("Assist1").GetComponent<Animator>().runtimeAnimatorController = assistAnimators[p1Assist];
        GameObject.Find("Assist2").GetComponent<Animator>().runtimeAnimatorController = assistAnimators[p2Assist];
    }

    private void resetGame()
    {

        timerDuration = 10f;

        p1Option = p2Option = "";
        winner = -1;
        playerHealth[0] = playerHealth[1] = 10;
        playerPower[0] = playerPower[1] = 0;

        players[0].transform.GetChild(0).gameObject.SetActive(true);
        players[1].transform.GetChild(0).gameObject.SetActive(true);

        textDescription.SetActive(true);
        menu.SetActive(false);

        loadControls();

        loadCards();

        loadHealth();

        roundOver = false;

        players[0].transform.Find("PlayerSprite").GetComponent<Animator>().Rebind();
        players[0].transform.Find("PlayerSprite").GetComponent<Animator>().Update(0f);

        players[1].transform.Find("PlayerSprite").GetComponent<Animator>().Rebind();
        players[1].transform.Find("PlayerSprite").GetComponent<Animator>().Update(0f);
    }

    private void loadCards()
    {
        Sprite[] SpritesData = Resources.LoadAll<Sprite>("Cards");
        _cards = new Dictionary<string, Sprite>();
        for (int i = 0; i < SpritesData.Length; i++)
        {
            _cards.Add(SpritesData[i].name, SpritesData[i]);
        }

        DECK_FRAMEWORK.createDeck();

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

    private void loadControls()
    {
        playerControls[0, 0] = GameObject.Find("Player1Controls").transform.GetChild(0).GetChild(0).GetComponent<Animator>();
        playerControls[0, 1] = GameObject.Find("Player1Controls").transform.GetChild(1).GetChild(0).GetComponent<Animator>();
        playerControls[0, 2] = GameObject.Find("Player1Controls").transform.GetChild(2).GetChild(0).GetComponent<Animator>();
        playerControls[1, 0] = GameObject.Find("Player2Controls").transform.GetChild(0).GetChild(0).GetComponent<Animator>();
        playerControls[1, 1] = GameObject.Find("Player2Controls").transform.GetChild(1).GetChild(0).GetComponent<Animator>();
        playerControls[1, 2] = GameObject.Find("Player2Controls").transform.GetChild(2).GetChild(0).GetComponent<Animator>();
    }

    private void checkInputs()
    {
        if (p1Option != "")
        {
            if (p1Option == "draw")
            {
                draw(0);
            }
            if (p1Option == "attack")
            {
                attack(0);
            }
            if (p1Option == "wildcard")
            {
                wildcard(0);
            }
            // Reset the option
            p1Option = "";

        }
        if (p2Option != "")
        {
            if (p2Option == "draw")
            {
                draw(1);
            }
            if (p2Option == "attack")
            {
                attack(1);
            }
            if (p2Option == "wildcard")
            {
                wildcard(1);
            }
            // Reset the option and lock the player out
            p2Option = "";
        }
    }

    private void readInputs()
    {
        float p1Horizontal = Input.GetAxis("Player1Horizontal");
        float p2Horizontal = Input.GetAxis("Player2Horizontal");
        float p1Vertical = Input.GetAxis("Player1Vertical");
        float p2Vertical = Input.GetAxis("Player2Vertical");

        if (Math.Abs(p1Horizontal) > 0.6 || p1Vertical > 0.6)
        {
            if (p1Horizontal > 0.6 && playerLockouts[0, 0] < 0)
            {
                p1Option = "draw";
            }
            else if (p1Vertical > 0.6 && playerControls[0, 1].GetBool("canAttack"))
            {
                p1Option = "attack";
            }
            else if (p1Horizontal < -0.6 && playerLockouts[0, 2] < 0)
            {
                p1Option = "wildcard";
            }
        }

        if (Math.Abs(p2Horizontal) > 0.6 || p2Vertical > 0.6)
        {
            if (p2Horizontal > 0.6 && playerLockouts[1, 0] < 0)
            {
                p2Option = "draw";
            }
            else if (p2Vertical > 0.6 && playerControls[1, 1].GetBool("canAttack"))
            {
                p2Option = "attack";
            }
            else if (p2Horizontal < -0.6 && playerLockouts[1, 2] < 0)
            {
                p2Option = "wildcard";
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

    private void evaluateHand()
    {
        (string, string)[] exportedP1Hand = playerHands[0].ToArray();
        (string, string)[] exportedP2Hand = playerHands[1].ToArray();

        playerPower[0] = DECK_FRAMEWORK.evaluateHand(exportedP1Hand);
        playerPower[1] = DECK_FRAMEWORK.evaluateHand(exportedP2Hand);

        if (playerPower[0] > 0) playerControls[0, 1].SetBool("canAttack", true);
        else playerControls[0, 1].SetBool("canAttack", false);

        if (playerPower[1] > 0) playerControls[1, 1].SetBool("canAttack", true);
        else playerControls[1, 1].SetBool("canAttack", false);
    }

    private void showHealth()
    {
        SpriteRenderer player1Health = GameObject.Find("Player1Health").GetComponent<SpriteRenderer>();
        SpriteRenderer player2Health = GameObject.Find("Player2Health").GetComponent<SpriteRenderer>();

        if (playerHealth[0] < 0) player1Health.sprite = _health["health-bar_0"];
        else player1Health.sprite = _health["health-bar_" + playerHealth[0].ToString()];

        if (playerHealth[1] < 0) player2Health.sprite = _health["health-bar_0"];
        else player2Health.sprite = _health["health-bar_" + playerHealth[1].ToString()];
    }

    private void updateLockouts()
    {
        // Update the lockout timers for players, update corresponding visuals
        for (int i = 0; i < 2; i++)
        {
            for (int j = 0; j < 3; j++)
            {
                playerLockouts[i, j] -= Time.deltaTime;
                playerControls[i, j].SetFloat("lockout", playerLockouts[i, j]);
            }
            if (playerLockouts[i, 2] > 0)
                GameObject.Find("P" + (i + 1).ToString() + "WildcardSprite").GetComponent<SpriteRenderer>().color = Color.black;
            else
                GameObject.Find("P" + (i + 1).ToString() + "WildcardSprite").GetComponent<SpriteRenderer>().color = Color.white;
        }
    }

    private void draw(int player)
    {
        // Draw 1 card and discard until 5 or less
        playerHands[player].Add(DECK_FRAMEWORK.draw());
        for (int i = 0; i < playerHands[player].Count - 5; i++)
        {
            playerHands[player].RemoveAt(0);
        }
        // Update animation for draw button
        playerLockouts[player, 0] = 0.3f;
    }

    private void attack(int attacker)
    {
        int defender = 1 - attacker;
        playerHealth[defender] -= playerPower[attacker];
        playerPower[attacker] = 0;
        playerHands[attacker].Clear();
        players[attacker].transform.Find("PlayerSprite").GetComponent<Animator>().SetTrigger("Attack");
    }

    private void wildcard(int player)
    {
        assists[player].GetComponent<Animator>().SetTrigger("assistEffect");
        int opponent = 1 - player;
        // We have 0-2 for selected wildcards.
        int selectedWildcard = PlayerPrefs.GetInt("Player" + (player + 1).ToString() + "Wildcard", 0);
        int dice = _random.Next(1, 5);
        switch (selectedWildcard)
        {
            // Aggressive hand
            case 0:
                _handSteal(dice, player);
                break;
            // Support hand
            case 1:
                switch (dice)
                {
                    case 1:
                        if (playerHands[player].Count > 3)
                        {
                            playerHands[player].RemoveAt(0);
                            playerHands[player].RemoveAt(0);
                            playerHands[player].RemoveAt(0);
                        }
                        else playerHands[player].Clear();
                        playerHands[player].Add(("hearts", "Q"));
                        playerHands[player].Add(("clubs", "Q"));
                        playerHands[player].Add(("spades", "Q"));
                        break;
                    case 2:
                        if (playerHands[player].Count > 2)
                        {
                            playerHands[player].RemoveAt(0);
                            playerHands[player].RemoveAt(0);
                        }
                        else playerHands[player].Clear();
                        playerHands[player].Add(("hearts", "A"));
                        playerHands[player].Add(("spades", "A"));
                        break;
                    case 3:
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
                    case 4:
                        if (playerHands[player].Count > 4)
                        {
                            playerHands[player].RemoveAt(0);
                            playerHands[player].RemoveAt(0);
                            playerHands[player].RemoveAt(0);
                            playerHands[player].RemoveAt(0);
                        }
                        else playerHands[player].Clear();

                        playerHands[player].Add(("hearts", "J"));
                        playerHands[player].Add(("clubs", "J"));
                        playerHands[player].Add(("spades", "J"));
                        playerHands[player].Add(("diamonds", "J"));
                        break;
                }
                break;
            // Gamble hand
            case 2:
                Debug.Log(dice.ToString());
                switch (dice)
                {
                    case 1:
                        playerHands[player].Clear();
                        break;
                    case 2:
                    case 3:
                        if (playerHands[player].Count > 2)
                        {
                            playerHands[player].RemoveAt(0);
                            playerHands[player].RemoveAt(0);
                        }
                        else playerHands[player].Clear();
                        break;
                    case 4:
                        playerHands[player].Clear();
                        playerHands[player].Add(("hearts", "A"));
                        playerHands[player].Add(("hearts", "K"));
                        playerHands[player].Add(("hearts", "Q"));
                        playerHands[player].Add(("hearts", "J"));
                        playerHands[player].Add(("hearts", "10"));
                        break;
                }
                break;

        }
        playerLockouts[player, 2] = 4f;

    }

    private void _handSteal(int number, int player)
    {
        int opponent = 1 - player;
        List<(string, string)> temp = playerHands[opponent];
        while (playerHands[player].Count > 5 - number)
        {
            playerHands[player].RemoveAt(0);
        }
        int limit;
        if (temp.Count > number)
            limit = number;
        else
            limit = temp.Count;
        for (int i = 0; i < limit; i++)
        {
            playerHands[player].Add(playerHands[opponent][0]);
            playerHands[opponent].RemoveAt(0);
        }
    }
}