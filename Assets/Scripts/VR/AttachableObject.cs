using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Valve.VR;
using Valve.VR.InteractionSystem;

public class AttachableObject : MonoBehaviour
{
    public List<Attachment> m_attachments;
    //float m_breakForce = 600f;
    //List<Vector3> m_forceVectors = new List<Vector3>();
    Rigidbody rigidBody;

    public Hand m_heldHand;

    public List<GameObject> m_connectedObjects;
    public Vector3 m_lastPosition;

    //private void OnCollisionStay(Collision collision)
    //{
    //    Vector3 v = collision.impulse / Time.fixedDeltaTime;
    //    if(!m_forceVectors.Contains(v)) m_forceVectors.Add(v);
    //    print("adding " + v);
    //}

    private void Awake()
    {
        rigidBody = GetComponent<Rigidbody>();
    }

    // Start is called before the first frame update
    void Start()
    {

    }

    private void HandHoverUpdate(Hand hand)
    {
        GrabTypes startingGrabType = hand.GetGrabStarting();

        if (startingGrabType != GrabTypes.None)
        {
            m_heldHand = hand;
            Teleport.Player.AddListener(TeleportBringAttachedObjects);
        }
    }

    void TeleportBringAttachedObjects(TeleportMarkerBase b)
    {
        if(m_heldHand != null)
        {
            // get the object w/ all the connections
            m_connectedObjects.Clear();
            FindFullObject(this, m_connectedObjects);

            // actually move the object(s)
            Vector3 moveDelta = transform.position - m_lastPosition;
            foreach(GameObject go in m_connectedObjects)
            {
                if (go == gameObject) continue;
                go.transform.position += moveDelta;
            }
        }
    }

    private void LateUpdate()
    {
        m_lastPosition = transform.position;
    }

    private void FixedUpdate()
    { 
        //Vector3 totalForce = Vector3.zero;// (Physics.gravity * rigidBody.mass);
        //print("totalForce begin: " + totalForce);
        //foreach (Vector3 v in m_forceVectors)
        //{
        //    totalForce += v;
        //}
        //if (totalForce.magnitude > m_breakForce)
        //{
        //    print("total: " + totalForce + ", breaking");
        //    gameObject.SetActive(false);
        //}

        //m_forceVectors.Clear();
    }

    public void FindFullObject(AttachableObject start, List<GameObject> result)
    {
        result.Add(start.gameObject);

        foreach (Attachment a in start.m_attachments)
        {
            if (result.Contains(a.gameObject)) continue;

            result.Add(a.gameObject);

            foreach (AttachableObject ao in a.m_connectedObjects.Keys)
            {
                if (result.Contains(ao.gameObject)) continue;

                FindFullObject(ao, result);
            }
        }
    }
   
    // Update is called once per frame
    void Update()
    {
        if (m_heldHand && m_heldHand.IsGrabEnding(gameObject))
        {
            m_heldHand = null;
            Teleport.Player.RemoveListener(TeleportBringAttachedObjects);
        }
    }
}
