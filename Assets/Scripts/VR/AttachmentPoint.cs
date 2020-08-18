using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;

public class AttachmentPoint : MonoBehaviour
{
    internal AttachableObject m_mainObject;
    internal ConnectionObject m_connectionObject;

    internal ConfigurableJoint m_joint;
    public bool m_attachToSelf = false;


    private void OnTriggerEnter(Collider other)
    {
        ConnectionObject _co = other.GetComponent<ConnectionObject>();
        if(_co != null)
        {
            m_connectionObject = _co;
            _co.GetComponent<Rigidbody>().isKinematic = true;
            _co.transform.SetParent(transform);
            _co.transform.localPosition = Vector3.zero;
            other.enabled = false;
            return;
        }

        //is it an attachment point?
        AttachmentPoint _ap = other.GetComponent<AttachmentPoint>();
        if (_ap != null)
        {   
            if (m_joint == null && m_connectionObject != null)
            {
                CreateAttachment(_ap);
            }
        }
    }

    void CreateAttachment(AttachmentPoint other)
    {
        //can't connect object to itself if not allowed
        if (other.m_mainObject == m_mainObject && !m_attachToSelf) return;

        ConfigurableJoint _j = m_mainObject.gameObject.AddComponent<ConfigurableJoint>();

        //set joint position to attachment point position
        _j.anchor = transform.localPosition;

        _j.connectedBody = other.m_mainObject.GetComponent<Rigidbody>();

        //anchor joint to other's position
        _j.autoConfigureConnectedAnchor = false;
        _j.connectedAnchor = other.transform.localPosition;

        //lock off motion
        _j.xMotion = ConfigurableJointMotion.Locked;
        _j.yMotion = ConfigurableJointMotion.Locked;
        _j.zMotion = ConfigurableJointMotion.Locked;

        if(m_connectionObject.m_objectType == ConnectionType.Glue)
        {
            _j.angularXMotion = _j.angularYMotion = _j.angularZMotion = ConfigurableJointMotion.Locked;
        }

        _j.enableCollision = true;

        //break force
        _j.breakForce = 450f;

        m_joint = _j;
        other.m_joint = _j;
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
