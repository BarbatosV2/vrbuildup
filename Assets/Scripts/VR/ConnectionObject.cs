using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ConnectionType
{
    Glue,
    BallJoint
}

public class ConnectionObject : MonoBehaviour
{
    public ConnectionType m_objectType;
    public AttachmentPoint m_attachmentPoint;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
