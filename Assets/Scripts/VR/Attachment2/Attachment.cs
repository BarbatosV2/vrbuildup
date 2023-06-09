﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Valve.VR.InteractionSystem;

public enum ConnectionType
{ 
    BallJoint,
    Glue,
    GearMotor
}

public class Attachment : MonoBehaviour
{
    public Collider m_attachmentCollider;
    public Dictionary<AttachableObject, ConfigurableJoint> m_connectedObjects = new Dictionary<AttachableObject, ConfigurableJoint>();
    private ConfigurableJoint m_firstJoint;

    private Collider collider;
    private Rigidbody rigidBody;

    public ConnectionType m_connectionType;
    public bool m_canAttach = true;

    // Start is called before the first frame update
    void Start()
    {
        collider = GetComponent<Collider>();
        rigidBody = GetComponent<Rigidbody>();
    }

    //attaching another object to this
    private void OnCollisionEnter(Collision other)
    {
        if (!m_canAttach) return;
        //ensure the other object can actually be attached to
        AttachableObject _att = other.transform.GetComponent<AttachableObject>();
        if (_att != null && !m_connectedObjects.ContainsKey(_att))
        {
            //point of contact between the attachment and the object
            ContactPoint _contactpoint = other.GetContact(0);

            //ignore collisions in future between these objects as they're technically "the same"
            Physics.IgnoreCollision(m_attachmentCollider, other.collider, true);

            CreateAttachment(_att, other, _contactpoint);

        }
    }
    private void HandHoverUpdate(Hand hand)
    {
        GrabTypes startingGrabType = hand.GetGrabStarting();

        if (startingGrabType != GrabTypes.None)
        {
            print("should remove");
            RemoveAllAttachments();
        }
    }

    void RemoveAllAttachments()
    {
        ConfigurableJoint[] joints = GetComponents<ConfigurableJoint>();
        foreach(ConfigurableJoint j in joints.ToList())
        {
            Destroy(j);
        }
        StartCoroutine(IgnoreNewCollisions());
    }

    public IEnumerator IgnoreNewCollisions()
    {
        m_canAttach = false;
        yield return LateUpdateJoints();
        yield return new WaitForSeconds(0.5f);
        m_canAttach = true;
    }

    void CreateGearMotor(ContactPoint contactPointWorld, Collision col, ConfigurableJoint j)
    {
        Vector3 norm = Vector3.zero;
        j.anchor = Vector3.zero;



        //transform.rotation = Quaternion.FromToRotation(Vector3.forward, norm);

        j.angularXMotion = j.angularYMotion = j.angularZMotion = ConfigurableJointMotion.Locked;
    }

    void CreateAttachment(AttachableObject otherAtt, Collision col, ContactPoint contactPointWorld)
    {
        //create the configurable joint between the attachment and the object
        ConfigurableJoint _j = gameObject.AddComponent<ConfigurableJoint>();

        //inset the attachment if necessary
        Vector3 _attPoint = (contactPointWorld.point);
        if(m_connectionType == ConnectionType.Glue)
        {
            float inset = Vector3.Distance(contactPointWorld.point, transform.TransformPoint(GetComponentInChildren<Rigidbody>().centerOfMass)) / 2f;
            Vector3 offset = (contactPointWorld.normal.normalized * inset / 8f);

            //don't autosnap when there's already something
            if (m_connectedObjects.Count != 0) offset = Vector3.zero;

            _attPoint = (contactPointWorld.point - offset);

            transform.position = _attPoint;
        }

        if(m_connectionType == ConnectionType.GearMotor)
        {
            if (Physics.Raycast(transform.position, (contactPointWorld.point - transform.position), out RaycastHit hit, 300f, ~LayerMask.GetMask("Attachment")))
            {
                _j.connectedAnchor = hit.transform.InverseTransformPoint(hit.point);
                
                //transform.position = Vector3.MoveTowards(hit.point, hit.point + hit.normal, collider.bounds.size.z*1.5f);
                Vector3 pos = hit.point + (hit.normal * (((BoxCollider)collider).size.z * 0.95f));
                transform.position = pos;

                _attPoint = pos;

                transform.rotation = Quaternion.FromToRotation(Vector3.forward, hit.normal);// * transform.rotation;

                //norm = hit.normal;
                //transform.rotation = Quaternion.Euler(Vector3.Cross(-hit.normal, Vector3.up));
            }
        }


        _j.anchor = transform.InverseTransformPoint(_attPoint);
        _j.connectedBody = otherAtt.GetComponent<Rigidbody>(); 

        //anchor joint to other's position
        _j.autoConfigureConnectedAnchor = false;

        //set joint position to attachment point position
        _j.connectedAnchor = otherAtt.transform.InverseTransformPoint(_attPoint);

        //lock off motion
        _j.xMotion = ConfigurableJointMotion.Locked;
        _j.yMotion = ConfigurableJointMotion.Locked;
        _j.zMotion = ConfigurableJointMotion.Locked;

        switch(m_connectionType)
        {
            case ConnectionType.BallJoint:
                if (m_connectedObjects.Count == 0)
                {
                    _j.angularXMotion = _j.angularYMotion = _j.angularZMotion = ConfigurableJointMotion.Locked;
                }
                break;
            case ConnectionType.Glue:
                _j.angularXMotion = _j.angularYMotion = _j.angularZMotion = ConfigurableJointMotion.Locked;
                JointDrive _d = new JointDrive
                {
                    positionSpring = 1000f,
                    positionDamper = 1000f
                };
                _j.angularXDrive = _j.angularYZDrive = _d;
                break;
            case ConnectionType.GearMotor:
                CreateGearMotor(contactPointWorld, col, _j);
                if (m_connectedObjects.Count >= 1)
                {
                    otherAtt.transform.SetParent(transform);
                    otherAtt.GetComponent<Rigidbody>().useGravity = false;
                }
                break;
            default: break;
        }

        //_j.configuredInWorldSpace = true;
        _j.enableCollision = true;

        //break force
        //_j.breakForce = 750f;

        if (m_connectedObjects.Count == 0) m_firstJoint = _j;
        m_connectedObjects.Add(otherAtt, _j);
        otherAtt.m_attachments.Add(this);
    }

    private void OnJointBreak(float breakForce)
    {
        print("joint broke!");
        StartCoroutine(LateUpdateJoints());
    }

    /// <summary>
    /// have to do this after the frame ends as joints don't reliably break on the same frame as OnJointBreak is called
    /// </summary> 
    IEnumerator LateUpdateJoints()
    {
        yield return new WaitForEndOfFrame();
        //find all objects' collider without the associated joint (i.e. the joint(s) that broke)
        foreach (Collider disc in m_connectedObjects.Where(o => (o.Value == null)).Select(o => o.Key.GetComponent<Collider>()).ToList())
        {
            if (disc == null) continue;
            AttachableObject a = disc.GetComponentInParent<AttachableObject>();
            if(a) a.m_attachments.Remove(this);
            yield return new WaitForEndOfFrame();
            Physics.IgnoreCollision(m_attachmentCollider, disc, false);
            m_connectedObjects.Remove(a);
        }
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if(m_connectionType == ConnectionType.GearMotor && m_connectedObjects.Count >= 2)
        {
            // unlock the Z rotation (spin!)
            m_firstJoint.angularZMotion = ConfigurableJointMotion.Free;
            rigidBody.AddTorque(transform.forward * 3f);//MoveRotation(Quaternion.Euler(transform.rotation.eulerAngles + new Vector3(0, 0, Time.deltaTime * 15f)));
        }
    }
}
