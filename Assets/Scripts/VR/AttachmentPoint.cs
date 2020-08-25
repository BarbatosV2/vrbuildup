using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;

[Serializable]
public class AttachmentPosition
{
    public ConnectionType m_type;
    public GameObject m_gameObject;
}

public class AttachmentPoint : MonoBehaviour
{
    public List<AttachmentPosition> m_attachmentPositions = new List<AttachmentPosition>();
    internal AttachableObject m_mainObject;
    internal ConnectionObject m_connectionObject;

    internal ConfigurableJoint m_joint;
    internal SphereCollider m_coll;
    public bool m_attachToSelf = false;

    float areaRefreshRate = 4f;
    float currentRate = 4f;


    private void OnTriggerEnter(Collider other)
    {
        print("collided");
        ConnectionObject _co = other.GetComponent<ConnectionObject>();
        if(_co != null && m_connectionObject == null && _co.m_attachmentPoint == null)
        {
            print("collided");
            _co.GetComponent<Rigidbody>().isKinematic = true;
            _co.GetComponent<SphereCollider>().enabled = false;
            _co.transform.SetParent(transform);

            //set joint position to attachment point position
            Vector3 pos = m_attachmentPositions.Find(o => (o.m_type == _co.m_objectType)).m_gameObject.transform.position;
            _co.transform.position = pos;

            _co.m_attachmentPoint = this;
            m_connectionObject = _co;

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
                return;
            }
        }

        print("invalid collision");
    }

    void CreateAttachment(AttachmentPoint other)
    {
        //can't connect object to itself if not allowed
        if (other.m_mainObject == m_mainObject && !m_attachToSelf) return;

        ConfigurableJoint _j = m_mainObject.gameObject.AddComponent<ConfigurableJoint>();

        //m_coll.enabled = false;

        _j.anchor = transform.localPosition + m_connectionObject.transform.localPosition;
        _j.connectedBody = other.m_mainObject.GetComponent<Rigidbody>();

        //anchor joint to other's position
        _j.autoConfigureConnectedAnchor = false;
        //_j.axis = Vector3.zero;

        //set joint position to attachment point position
        Vector3 pos = other.m_attachmentPositions.Find(o => (o.m_type == m_connectionObject.m_objectType)).m_gameObject.transform.localPosition;
        _j.connectedAnchor = other.transform.localPosition + pos;

        //lock off motion
        _j.xMotion = ConfigurableJointMotion.Locked;
        _j.yMotion = ConfigurableJointMotion.Locked;
        _j.zMotion = ConfigurableJointMotion.Locked;

        if(m_connectionObject.m_objectType == ConnectionType.Glue)
        { 
            _j.angularXMotion = _j.angularYMotion = _j.angularZMotion = ConfigurableJointMotion.Locked;
        }

        //_j.configuredInWorldSpace = true;
        _j.enableCollision = true;

        //break force
        _j.breakForce = 450f;

        m_joint = _j;
        other.m_joint = _j;
    }

    private void OnJointBreak(float breakForce)
    {
        //Destroy(m_joint);
        //m_coll.enabled = true;
        //m_joint = null;
    }

    // Start is called before the first frame update
    void Start()
    {
        m_coll = GetComponent<SphereCollider>();   
    }

    // Update is called once per frame
    void Update()
    {
        if (currentRate == 0)
        {
            currentRate = areaRefreshRate;
            
        }
        else currentRate--;
    }
}
