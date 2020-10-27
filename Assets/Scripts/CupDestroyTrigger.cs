using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CupDestroyTrigger : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    private void OnTriggerEnter(Collider other)
    {
        CoffeeCup cup = other.GetComponentInParent<CoffeeCup>();
        if(cup)
        {
            Destroy(other.gameObject);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
