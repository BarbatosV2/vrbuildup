using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Attachment : MonoBehaviour
{
    public Collider m_attachmentCollider;
    public Dictionary<Collider, ConfigurableJoint> m_connectedObjects = new Dictionary<Collider, ConfigurableJoint>();
    public ConnectionType m_connectionType;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    //attaching another object to this
    private void OnCollisionEnter(Collision other)
    {
        print("colliding");
        //ensure the other object can actually be attached to
        AttachableObject _att = other.transform.GetComponent<AttachableObject>();
        if (_att != null && !m_connectedObjects.ContainsKey(other.collider))
        {
            //point of contact between the attachment and the object
            Vector3 _contactpoint = other.GetContact(0).point;
            CreateAttachment(_att, other.collider, _contactpoint);

            //ignore collisions in future between these objects as they're technically "the same"
            Physics.IgnoreCollision(m_attachmentCollider, other.collider, true);
        }
    }

    void CreateAttachment(AttachableObject otherAtt, Collider col, Vector3 contactPointWorld)
    {
        ConfigurableJoint _j = gameObject.AddComponent<ConfigurableJoint>();

        _j.anchor = transform.InverseTransformPoint(contactPointWorld) / 1.2f;
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

        if(m_connectionType == ConnectionType.BallJoint && m_connectedObjects.Count == 0)
        {
            transform.position = contactPointWorld;
            //_j.anchor = Vector3.zero;
            _j.angularXMotion = _j.angularYMotion = _j.angularZMotion = ConfigurableJointMotion.Locked;
        }

        if (m_connectionType == ConnectionType.Glue)
        {
            _j.angularXMotion = _j.angularYMotion = _j.angularZMotion = ConfigurableJointMotion.Locked;
        }

        //_j.configuredInWorldSpace = true;
        _j.enableCollision = true;

        //break force
        _j.breakForce = 450f;

        m_connectedObjects.Add(col, _j);
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
        foreach (Collider disc in m_connectedObjects.Where(o => (o.Value == null)).Select(o => o.Key).ToList())
        {
            Physics.IgnoreCollision(m_attachmentCollider, disc, false);
            m_connectedObjects.Remove(disc);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
