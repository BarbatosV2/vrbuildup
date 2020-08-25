using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Attachment : MonoBehaviour
{
    public Collider m_attachmentCollider;
    public List<AttachableObject> m_objects;
    public ConnectionType m_connectionType;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    ////attaching this to other object
    //private void OnCollisionEnter(Collision collision)
    //{
    //    //ensure the other object can actually be attached to
    //    AttachableObject _att = collision.transform.GetComponent<AttachableObject>();
    //    if(_att != null && !m_objects.Contains(_att))
    //    {
    //        CreateAttachment(_att);
    //        ////point of contact between the attachment and the object
    //        //Vector3 _contactPoint = collision.GetContact(0).point;

    //        ////center the attachment onto the attached point (rather than an edge)
    //        //transform.position = _contactPoint;

    //        ////add a FixedJoint between this and the object (not allowed to parent as that screws up taking it off)
    //        //FixedJoint _j = gameObject.AddComponent<FixedJoint>();
    //        //_j.connectedBody = _att.GetComponent<Rigidbody>(); 
    //        //_j.breakForce = 900f;

    //        //m_objects.Add(_att);

    //        ////set to trigger to prevent any more OnCollisionEnters
    //        //m_attachmentCollider.isTrigger = true;
    //    }

    //}

    //attaching another object to this
    private void OnCollisionEnter(Collision other)
    {
        //ensure the other object can actually be attached to
        AttachableObject _att = other.transform.GetComponent<AttachableObject>();
        if (_att != null && !m_objects.Contains(_att))
        {
            //m_attachmentCollider.isTrigger = true;
            //point of contact between the attachment and the object
            Vector3 _contactpoint = other.GetContact(0).point;
            CreateAttachment(_att, _contactpoint);

            Physics.IgnoreCollision(m_attachmentCollider, other.collider);
            //transform.GetComponent<Rigidbody>().isKinematic = true;
        }
    }

    ////attaching another object to this
    //private void OnTriggerEnter(Collider other)
    //{
    //    print("triggerero");
    //    //ensure the other object can actually be attached to
    //    AttachableObject _att = other.transform.GetComponent<AttachableObject>();
    //    if (_att != null && !m_objects.Contains(_att))
    //    {
    //        CreateAttachment(_att);
    //    }
    //}

    void CreateAttachment(AttachableObject otherAtt, Vector3 contactPointWorld)
    {
        ConfigurableJoint _j = gameObject.AddComponent<ConfigurableJoint>();
        m_objects.Add(otherAtt);

        _j.anchor = transform.InverseTransformPoint(contactPointWorld)/1.2f;
        _j.connectedBody = otherAtt.GetComponent<Rigidbody>();

        //anchor joint to other's position
        _j.autoConfigureConnectedAnchor = false;
        //_j.axis = Vector3.zero;

        //set joint position to attachment point position
        _j.connectedAnchor = otherAtt.transform.InverseTransformPoint(contactPointWorld) / 1.2f;

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
    }

    private void OnJointBreak(float breakForce)
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
