using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuNavigation : MonoBehaviour
{

    private Animator screenAnimator;
    private GameObject splashObj, creditsObj, characterSelectObj, fightObj;

    // Start is called before the first frame update
    void Start()
    {
        screenAnimator = GameObject.Find("Background").GetComponent<Animator>();
        splashObj = GameObject.Find("SplashCanvas");
        creditsObj = GameObject.Find("CreditsCanvas");
        characterSelectObj = GameObject.Find("CharacterSelectCanvas");
        fightObj = GameObject.Find("FightCanvas");
        characterSelectObj.transform.GetChild(0).gameObject.SetActive(false);
        creditsObj.transform.GetChild(0).gameObject.SetActive(false);
        fightObj.transform.GetChild(0).gameObject.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void startGame()
    {
        SceneManager.LoadScene("MainScene");
    }

    public void characterSelect()
    {
        screenAnimator.SetTrigger("characterSelect");
        splashObj.transform.GetChild(0).gameObject.SetActive(false);
        characterSelectObj.transform.GetChild(0).gameObject.SetActive(true);
        // switchScene("MainScene");
    }
    public void credits()
    {
        screenAnimator.SetTrigger("credits");
        splashObj.transform.GetChild(0).gameObject.SetActive(false);
        creditsObj.transform.GetChild(0).gameObject.SetActive(true);
    }
    public void splash()
    {
        screenAnimator.SetTrigger("splash");
        creditsObj.transform.GetChild(0).gameObject.SetActive(false);
        characterSelectObj.transform.GetChild(0).gameObject.SetActive(false);
        splashObj.transform.GetChild(0).gameObject.SetActive(true);
    }
    public void fight ()
    {
        screenAnimator.SetTrigger("fight");
        creditsObj.transform.GetChild(0).gameObject.SetActive(false);
        characterSelectObj.transform.GetChild(0).gameObject.SetActive(false);
        splashObj.transform.GetChild(0).gameObject.SetActive(false);
        fightObj.transform.GetChild(0).gameObject.SetActive(true);
    }
}
