using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScreenEffects : MonoBehaviour
{

    private ShakeBehavior cameraShake;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void screenShake ()
    {
        cameraShake = GameObject.Find("Main Camera").GetComponent<ShakeBehavior>();
        cameraShake.TriggerShake();
    }
    
}
