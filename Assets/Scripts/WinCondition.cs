using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum WinRequirement
{
    ObjectsDestroyed
}

public class WinCondition : MonoBehaviour
{
    public List<GameObject> m_affectedObjects;

    [Tooltip("Lose instead of win when this condition is met.")]
    public bool m_invertCondition;

    


    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
