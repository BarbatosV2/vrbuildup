using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Valve.VR.InteractionSystem;

public enum ConnectionType
{ 
    BallJoint,
    Glue
}

public class Attachment : MonoBehaviour
{
    public Collider m_attachmentCollider;
    public Dictionary<AttachableObject, ConfigurableJoint> m_connectedObjects = new Dictionary<AttachableObject, ConfigurableJoint>();
    public ConnectionType m_connectionType;
    public bool m_canAttach = true;

    // Start is called before the first frame update
    void Start()
    {
        
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

            CreateAttachment(_att, other.collider, _contactpoint);

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

    void CreateAttachment(AttachableObject otherAtt, Collider col, ContactPoint contactPointWorld)
    {
        //create the configurable joint between the attachment and the object
        ConfigurableJoint _j = gameObject.AddComponent<ConfigurableJoint>();

        //inset the attachment if necessary
        Vector3 _attPoint = (contactPointWorld.point);
        if(m_connectionType != ConnectionType.BallJoint)
        {
            float inset = Vector3.Distance(contactPointWorld.point, transform.TransformPoint(GetComponentInChildren<Rigidbody>().centerOfMass)) / 2f;
            Vector3 offset = (contactPointWorld.normal.normalized * inset / 8f);

            //don't autosnap when there's already something
            if (m_connectedObjects.Count != 0) offset = Vector3.zero;

            _attPoint = (contactPointWorld.point - offset);

            transform.position = _attPoint;
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

        if(m_connectionType == ConnectionType.BallJoint && m_connectedObjects.Count == 0)
        {
            //transform.position = contactPointWorld.point;
            //_j.anchor = Vector3.zero;
            _j.angularXMotion = _j.angularYMotion = _j.angularZMotion = ConfigurableJointMotion.Locked;
        }

        if (m_connectionType == ConnectionType.Glue)
        {
            _j.angularXMotion = _j.angularYMotion = _j.angularZMotion = ConfigurableJointMotion.Locked;
            JointDrive _d = new JointDrive
            {
                positionSpring = 1000f,
                positionDamper = 1000f
            };
            _j.angularXDrive = _j.angularYZDrive = _d;
        }

        //_j.configuredInWorldSpace = true;
        _j.enableCollision = true;

        //break force
        _j.breakForce = 750f;

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
            Physics.IgnoreCollision(m_attachmentCollider, disc, false);
            m_connectedObjects.Remove(a);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
