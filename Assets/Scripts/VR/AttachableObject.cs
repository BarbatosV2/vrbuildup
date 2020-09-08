using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;

public class AttachableObject : MonoBehaviour
{
    public List<Attachment> m_attachments;
    float m_breakForce = 600f;
    Vector3 forceVector = Vector3.zero;
    public Vector3 m_lastVector;

    private void OnCollisionStay(Collision collision)
    {
        forceVector += (collision.impulse / Time.fixedDeltaTime);
        print("adding " + forceVector);
    }

    // Start is called before the first frame update
    void Start()
    {

    }

    private void LateUpdate()
    {
      
    }

    private void FixedUpdate()
    {
        //print(forceVector);
        if (forceVector.magnitude > m_breakForce)
        {
            gameObject.SetActive(false);
        }
        m_lastVector = forceVector;
        forceVector = Vector3.zero;
    }

    // Update is called once per frame
    void Update()
    {
    }
}
