using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ATAttachment : MonoBehaviour
{
    public ConnectionType m_connectionType;
    public bool m_canAttach = true;

    private Collider m_collider;
    Transform m_childTransform;

    ArticulationBody m_childArticulation;

    // Start is called before the first frame update
    void Start()
    {
        m_collider = GetComponent<Collider>();
        m_childTransform = transform.GetChild(0);
        m_childArticulation = GetComponentInChildren<ArticulationBody>();
    }

    private void OnCollisionEnter(Collision other)
    {
        if (!m_canAttach) return;

        //ensure the other object can actually be attached to
        AttachableObject _att = other.transform.GetComponent<AttachableObject>();
        
        if (_att != null)
        {
            //point of contact between the attachment and the object
            ContactPoint _contactpoint = other.GetContact(0);

            //@TODO: make traditional-joint attachments work
            if (_att.transform.IsChildOf(transform.root))
            {
                CreatePhysXAttachment(_att,other,_contactpoint);
                return;
            }

            //ignore collisions in future between these objects as they're technically "the same"
            Physics.IgnoreCollision(m_collider, other.collider, true);

            bool _isAlreadyParented = _att.transform.parent?.GetComponentInParent<ATAttachment>() != null;
            CreateArticulatedAttachment(_isAlreadyParented, _att, other, _contactpoint);

        }
    }

    void CreatePhysXAttachment(AttachableObject otherAtt, Collision collision, ContactPoint contactPointWorld)
    {

    }

    void CreateArticulatedAttachment(bool isAlreadyParented, AttachableObject otherAtt, Collision collision, ContactPoint contactPointWorld)
    {
        /*
        1) set attachableobject to child of this (unless it already *has* a parent, in which case:                                                                              
        2) set offset to contact point
        3) change settings depending on connection type
         */

        // set attachableobject to child.... unless it already has a parent
        if (!isAlreadyParented)
        {
            otherAtt.transform.SetParent(m_childTransform);
        }
        else
        {
            //@TODO: make connecting two separately-parented objects work (pls no rebalancing)
        }

        // set offset to contact point
        otherAtt.m_artBody.anchorPosition = otherAtt.transform.InverseTransformPoint(contactPointWorld.point);

        // change articulated body settings depending on connection type
        switch(m_connectionType)
        {
            case ConnectionType.Glue: otherAtt.m_artBody.jointType = ArticulationJointType.FixedJoint; break;
            case ConnectionType.BallJoint: otherAtt.m_artBody.jointType = ArticulationJointType.SphericalJoint; break;
            default: break;
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void FixedUpdate()
    {
        m_childArticulation.TeleportRoot(transform.position,transform.rotation);
    }
}
