using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Attachment : MonoBehaviour
{
    public Collider m_attachmentCollider;
    public AttachableObject m_mainObject;
    public ConnectionType m_connectionType;

    public ConfigurableJoint m_confJoint;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    //attaching this to other object
    private void OnCollisionEnter(Collision collision)
    {
        //ensure the other object can actually be attached to
        AttachableObject _att = collision.transform.GetComponent<AttachableObject>();
        if(_att != null)
        {
            //point of contact between the attachment and the object
            Vector3 _contactPoint = collision.GetContact(0).point;

            //center the attachment onto the attached point (rather than an edge)
            transform.position = _contactPoint;

            //add a FixedJoint between this and the object (not allowed to parent as that screws up taking it off)
            FixedJoint _j = gameObject.AddComponent<FixedJoint>();
            _j.connectedBody = _att.GetComponent<Rigidbody>(); 
            _j.breakForce = 900f;


            //set to trigger to prevent any more OnCollisionEnters
            m_attachmentCollider.isTrigger = true;
        }

    }
    //attaching another object to this
    private void OnTriggerEnter(Collider other)
    {
        //ensure the other object can actually be attached to
        AttachableObject _att = other.transform.GetComponent<AttachableObject>();
        if(_att != null)
        {
            CreateAttachment(_att);
        }
    }

    void CreateAttachment(AttachableObject otherAtt)
    {
        ConfigurableJoint _j = m_mainObject.gameObject.AddComponent<ConfigurableJoint>();

        _j.anchor = transform.localPosition;
        _j.connectedBody = otherAtt.GetComponent<Rigidbody>();

        //anchor joint to other's position
        _j.autoConfigureConnectedAnchor = false;
        //_j.axis = Vector3.zero;

        //set joint position to attachment point position
        _j.connectedAnchor = transform.localPosition;

        //lock off motion
        _j.xMotion = ConfigurableJointMotion.Locked;
        _j.yMotion = ConfigurableJointMotion.Locked;
        _j.zMotion = ConfigurableJointMotion.Locked;

        if (m_connectionType == ConnectionType.Glue)
        {
            _j.angularXMotion = _j.angularYMotion = _j.angularZMotion = ConfigurableJointMotion.Locked;
        }

        _j.configuredInWorldSpace = true;
        _j.enableCollision = true;

        //break force
        _j.breakForce = 450f;

        m_confJoint = _j;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
