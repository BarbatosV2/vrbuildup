using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;

public class AttachableObject : MonoBehaviour
{
    public List<AttachmentPoint> m_attachmentPoints;
    public bool m_multiAttach = false;

    private void Awake()
    {
        foreach(AttachmentPoint p in m_attachmentPoints)
        {
            p.m_mainObject = this;
        }
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
