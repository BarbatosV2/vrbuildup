using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ATAttachment : MonoBehaviour
{
    public ConnectionType m_connectionType;
    public bool m_canAttach = true;

    private Collider m_collider;

    // Start is called before the first frame update
    void Start()
    {
        m_collider = GetComponent<Collider>();
    }

    private void OnCollisionEnter(Collision other)
    {
        if (!m_canAttach) return;

        //ensure the other object can actually be attached to
        AttachableObject _att = other.transform.GetComponent<AttachableObject>();
        
        if (_att != null)
        {
            

            //@TODO: make traditional-joint attachments work
            if (_att.transform.IsChildOf(transform.root)) return;

            //point of contact between the attachment and the object
            ContactPoint _contactpoint = other.GetContact(0);

            //ignore collisions in future between these objects as they're technically "the same"
            Physics.IgnoreCollision(m_collider, other.collider, true);

            bool _isAlreadyParented = _att.transform.parent.GetComponent<Attachment>() != null;
            CreateArticulatedAttachment(_isAlreadyParented, _att, other, _contactpoint);

        }
    }

    void CreatePhysXAttachment(AttachableObject otherAtt, Collision collision, ContactPoint contactPointWorld)
    {

    }

    void CreateArticulatedAttachment(bool isAlreadyParented, AttachableObject otherAtt, Collision collision, ContactPoint contactPointWorld)
    {
        /*
        1) set attachableobject to child of this
        2) set offset to contact point
        3) change settings depending on connection type
         */

        otherAtt.transform.SetParent(transform);

        otherAtt.m_artBody.anchorPosition = otherAtt.transform.InverseTransformPoint(contactPointWorld.point);

        switch(m_connectionType)
        {
            case ConnectionType.BallJoint: otherAtt.m_artBody.jointType = ArticulationJointType.SphericalJoint; break;
            default: break;
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
