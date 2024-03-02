using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

public class DeckScript : MonoBehaviour
{

    System.Random _random = new System.Random();

    private string[] _suits = new string[] { "spades", "diamonds", "clubs", "hearts" };
    private string[] _values = new string[] { "A", "A", "10", "10", "10", "J", "J", "J", "Q", "Q", "Q", "K", "K" };
    private (string, string)[] _shuffled;
    private Queue<(string, string)> _deck = new Queue<(string, string)>();

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {

    }

    public (string, string) draw()
    {
        if (_deck.Count > 0)
        {
            return _deck.Dequeue();
        }
        else
        {
            return ("blank", "");
        }
    }

    public List<(string, string)> drawNumber(int n)
    {
        List<(string, string)> _drawn = new List<(string, string)>();
        for (int i = 0; i < n; i++)
        {
            _drawn.Add(draw());
        }
        return _drawn;
    }

    public (string, string)[] sortHand((string, string)[] cards)
    {
        cards = _sortBySuit(cards);
        List<(string, int)> rankedCards = _sortByRank(cards);
        List<(string, string)> deranked = new List<(string, string)>();
        foreach ((string, int) card in rankedCards)
        {
            switch (card.Item2)
            {
                case 11:
                    deranked.Add((card.Item1, "J"));
                    break;
                case 12:
                    deranked.Add((card.Item1, "Q"));
                    break;
                case 13:
                    deranked.Add((card.Item1, "K"));
                    break;
                case 14:
                    deranked.Add((card.Item1, "A"));
                    break;
                default:
                    deranked.Add((card.Item1, card.Item2.ToString()));
                    break;
            }
        }
        return deranked.ToArray();
    }

    public int evaluateHand((string, string)[] cards)
    {
        // Royal Flush
        if (_isFlush(cards) && _isStraight(cards))
            return 10;
        // 4 of a kind
        else if (_isFourOfKind(cards))
            return 8;
        // Full house
        else if (_isFullHouse(cards))
            return 6;
        // Regular flush
        else if (_isFlush(cards))
            return 5;
        // 5 Straight
        else if (_isStraight(cards))
            return 4;
        // Three of a kind
        else if (_isThreeOfKind(cards))
            return 3;
        // Two pairs
        else if (_isTwoPair(cards))
            return 2;
        // One pair
        else if (_isOnePair(cards))
            return 1;
        else
            return 0;
    }

    public void createDeck()
    {
        _shuffled = new (string, string)[52];
        int count = 0;
        // Initialize the deck
        for (int i = 0; i < _suits.Length; i++)
        {
            for (int j = 0; j < _values.Length; j++)
            {
                _shuffled[52 - count - 1] = (_suits[i], _values[j]);
                count++;
            }
        }
        // Shuffle the deck
        count = _shuffled.Length;
        while (count > 1)
        {
            int i = _random.Next(count--);
            (_shuffled[i], _shuffled[count]) = (_shuffled[count], _shuffled[i]);
        }
        // Place it back
        for (int i = 0; i < _shuffled.Length; i++)
        {
            _deck.Enqueue(_shuffled[i]);
        }
    }

    // Poker hand evaluation algorithms from Emory
    private List<(string, int)> _sortByRank((string, string)[] cards)
    {
        List<(string, int)> rankedCards = new List<(string, int)>();
        // Sort deck by rank
        foreach ((string, string) card in cards)
        {
            switch (card.Item2)
            {
                case "J":
                    rankedCards.Add((card.Item1, 11));
                    break;
                case "Q":
                    rankedCards.Add((card.Item1, 12));
                    break;
                case "K":
                    rankedCards.Add((card.Item1, 13));
                    break;
                case "A":
                    rankedCards.Add((card.Item1, 14));
                    break;
                default:
                    rankedCards.Add((card.Item1, Int32.Parse(card.Item2)));
                    break;
            }
        }
        rankedCards.Sort((x, y) => x.Item2.CompareTo(y.Item2));
        return rankedCards;
    }

    private (string, string)[] _sortBySuit((string, string)[] cards)
    {
        (string, string)[] copy = cards;
        Array.Sort(copy, (x, y) => y.Item1.CompareTo(x.Item1));
        return copy;
    }
    private bool _isFlush((string, string)[] cards)
    {
        if (cards.Length != 5) return false;
        cards = _sortBySuit(cards);
        return (cards[0].Item1 == cards[4].Item1);
    }

    private bool _isStraight((string, string)[] cards)
    {
        if (cards.Length != 5) return false;

        List<(string, int)> rankedCards = _sortByRank(cards);

        // Check for Ace hand
        if (rankedCards[4].Item2 == 14)
        {
            /* =================================
                Check straight using an Ace
                ================================= */
            bool a = rankedCards[0].Item2 == 2 && rankedCards[1].Item2 == 3 &&
                        rankedCards[2].Item2 == 4 && rankedCards[3].Item2 == 5;
            bool b = rankedCards[0].Item2 == 10 && rankedCards[1].Item2 == 11 &&
                        rankedCards[2].Item2 == 12 && rankedCards[3].Item2 == 13;

            return (a || b);
        }
        else
        {
            /* ===========================================
            General case: check for increasing values
            =========================================== */
            int testRank = rankedCards[0].Item2 + 1;
            for (int i = 1; i < 5; i++)
            {
                if (rankedCards[i].Item2 != testRank)
                    return false;        // Straight failed...
                testRank++;   // Next card in hand
            }
            return true;        // Straight found !
        }
    }

    private bool _isFourOfKind((string, string)[] cards)
    {
        if (cards.Length != 5) return false;

        List<(string, int)> rankedCards = _sortByRank(cards);

        /* ------------------------------------------------------
           Check for: 4 cards of the same rank 
                  + higher ranked unmatched card 
       ------------------------------------------------------- */
        bool a1 = rankedCards[0].Item2 == rankedCards[1].Item2 &&
             rankedCards[1].Item2 == rankedCards[2].Item2 &&
             rankedCards[2].Item2 == rankedCards[3].Item2;


        /* ------------------------------------------------------
           Check for: lower ranked unmatched card 
                  + 4 cards of the same rank 
       ------------------------------------------------------- */
        bool a2 = rankedCards[1].Item2 == rankedCards[2].Item2 &&
             rankedCards[2].Item2 == rankedCards[3].Item2 &&
             rankedCards[3].Item2 == rankedCards[4].Item2;

        return (a1 || a2);
    }

    private bool _isFullHouse((string, string)[] cards)
    {
        if (cards.Length != 5) return false;

        List<(string, int)> rankedCards = _sortByRank(cards);

        /* ------------------------------------------------------
           Check for: x x x y y
       ------------------------------------------------------- */
        bool a1 = rankedCards[0].Item2 == rankedCards[1].Item2 &&
             rankedCards[1].Item2 == rankedCards[2].Item2 &&
             rankedCards[3].Item2 == rankedCards[4].Item2;

        /* ------------------------------------------------------
           Check for: x x y y y
       ------------------------------------------------------- */
        bool a2 = rankedCards[0].Item2 == rankedCards[1].Item2 &&
             rankedCards[2].Item2 == rankedCards[3].Item2 &&
             rankedCards[3].Item2 == rankedCards[4].Item2;

        return (a1 || a2);
    }

    private bool _isThreeOfKind((string, string)[] cards)
    {
        if (cards.Length != 5)
            return false;

        if (_isFourOfKind(cards) || _isFullHouse(cards))
            return false;        // The hand is not 3 of a kind (but better)          

        List<(string, int)> rankedCards = _sortByRank(cards);

        /* ------------------------------------------------------
           Check for: x x x a b
       ------------------------------------------------------- */
        bool a1 = rankedCards[0].Item2 == rankedCards[1].Item2 &&
             rankedCards[1].Item2 == rankedCards[2].Item2;

        /* ------------------------------------------------------
           Check for: a x x x b
       ------------------------------------------------------- */
        bool a2 = rankedCards[1].Item2 == rankedCards[2].Item2 &&
             rankedCards[2].Item2 == rankedCards[3].Item2;

        /* ------------------------------------------------------
           Check for: a b x x x
       ------------------------------------------------------- */
        bool a3 = rankedCards[2].Item2 == rankedCards[3].Item2 &&
             rankedCards[3].Item2 == rankedCards[4].Item2;

        return (a1 || a2 || a3);
    }

    private bool _isTwoPair((string, string)[] cards)
    {
        if (cards.Length != 5)
            return false;

        if (_isFourOfKind(cards) || _isFullHouse(cards) || _isThreeOfKind(cards))
            return false;        // The hand is not 2 pairs (but better)      

        List<(string, int)> rankedCards = _sortByRank(cards);

        /* --------------------------------
           Checking: a a  b b x
       -------------------------------- */
        bool a1 = rankedCards[0].Item2 == rankedCards[1].Item2 &&
             rankedCards[2].Item2 == rankedCards[3].Item2;

        /* ------------------------------
           Checking: a a x  b b
       ------------------------------ */
        bool a2 = rankedCards[0].Item2 == rankedCards[1].Item2 &&
             rankedCards[3].Item2 == rankedCards[4].Item2;

        /* ------------------------------
           Checking: x a a  b b
       ------------------------------ */
        bool a3 = rankedCards[1].Item2 == rankedCards[2].Item2 &&
             rankedCards[3].Item2 == rankedCards[4].Item2;

        return (a1 || a2 || a3);
    }

    private bool _isOnePair((string, string)[] cards)
    {
        if (cards.Length != 5)
            return false;

        if (_isFourOfKind(cards) || _isFullHouse(cards) || _isThreeOfKind(cards) || _isTwoPair(cards))
            return (false);        // The hand is not one pair (but better)       

        List<(string, int)> rankedCards = _sortByRank(cards);

        /* --------------------------------
           Checking: a a x y z
       -------------------------------- */
        bool a1 = rankedCards[0].Item2 == rankedCards[1].Item2;

        /* --------------------------------
           Checking: x a a y z
       -------------------------------- */
        bool a2 = rankedCards[1].Item2 == rankedCards[2].Item2;

        /* --------------------------------
           Checking: x y a a z
       -------------------------------- */
        bool a3 = rankedCards[2].Item2 == rankedCards[3].Item2;

        /* --------------------------------
           Checking: x y z a a
       -------------------------------- */
        bool a4 = rankedCards[3].Item2 == rankedCards[4].Item2;

        bool[] conditions = new bool[] { a1, a2, a3, a4 };

        return (a1 || a2 || a3 || a4);
    }
}
