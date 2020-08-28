using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;
using Valve.VR.InteractionSystem;

public class ThrowableSpecial : MonoBehaviour
{
    public Hand.AttachmentFlags m_attachmentFlags;
    public ReleaseStyle releaseVelocityStyle = ReleaseStyle.GetFromHand;
    protected VelocityEstimator velocityEstimator;

    private List<Hand> holdingHands = new List<Hand>();
    private List<Rigidbody> holdingBodies = new List<Rigidbody>();
    private List<Vector3> holdingPoints = new List<Vector3>();
    private List<ConfigurableJoint> holdingJoints = new List<ConfigurableJoint>();

    private List<Rigidbody> rigidBodies = new List<Rigidbody>();

    // Start is called before the first frame update
    void Start()
    {
        GetComponentsInChildren<Rigidbody>(rigidBodies);
        velocityEstimator = GetComponent<VelocityEstimator>();
    }

    //private void OnDrawGizmos()
    //{
    //    Gizmos.color = Color.red;
    //    for (int i = 0; i < holdingHands.Count; i++)
    //    {
    //        Gizmos.DrawSphere(holdingBodies[i].transform.TransformPoint(holdingPoints[i]),0.05f);
    //    }
    //}

    // Update is called once per frame
    void Update()  
    {
        for (int i = 0; i < holdingHands.Count; i++)
        {
            holdingHands[i].skeleton.transform.position = holdingBodies[i].transform.TransformPoint(holdingPoints[i]); 

            if (holdingHands[i].IsGrabEnding(this.gameObject))
            {
                PhysicsDetach(holdingHands[i]);
            }
        }
    }

    private void HandHoverUpdate(Hand hand)
    {
        GrabTypes startingGrabType = hand.GetGrabStarting();

        if (startingGrabType != GrabTypes.None)
        {
            PhysicsAttach(hand, startingGrabType);
        }
    }

    //-------------------------------------------------
    private bool PhysicsDetach(Hand hand)
    {
        int i = holdingHands.IndexOf(hand);

        if (i != -1)
        {
            holdingHands[i].DetachObject(gameObject, false);

            // Allow the hand to do other things
            holdingHands[i].HoverUnlock(null);

            // Delete any existing joints from the hand
            Destroy(holdingHands[i].GetComponent<ConfigurableJoint>());

            Util.FastRemove(holdingHands, i);
            Util.FastRemove(holdingBodies, i);
            Util.FastRemove(holdingPoints, i);
            Util.FastRemove(holdingJoints, i);

            return true;
        }

        return false;
    }

    //-------------------------------------------------
    private void PhysicsAttach(Hand hand, GrabTypes startingGrabType)
    {
        PhysicsDetach(hand);

        Rigidbody holdingBody = null;
        Vector3 holdingPoint = Vector3.zero;

        // The hand should grab onto the nearest rigid body
        float closestDistance = float.MaxValue;
        for (int i = 0; i < rigidBodies.Count; i++)
        {
            float distance = Vector3.Distance(rigidBodies[i].worldCenterOfMass, hand.transform.position);
            if (distance < closestDistance)
            {
                holdingBody = rigidBodies[i];
                closestDistance = distance;
            }
        }

        // Couldn't grab onto a body
        if (holdingBody == null)
            return;

        // Create a configurable joint from the hand to the holding body
        Rigidbody handRigidbody = Util.FindOrAddComponent<Rigidbody>(hand.gameObject);
        handRigidbody.isKinematic = true;

        ConfigurableJoint handJoint = hand.gameObject.AddComponent<ConfigurableJoint>();
        handJoint.connectedBody = holdingBody;
        handJoint.xMotion = handJoint.yMotion = handJoint.zMotion = ConfigurableJointMotion.Locked;

        ConfigurableJointMotion _m = ConfigurableJointMotion.Locked;

        handJoint.angularXMotion = handJoint.angularYMotion = handJoint.angularZMotion = _m;

        // Don't let the hand interact with other things while it's holding us
        hand.HoverLock(null);

        // Affix this point
        Vector3 offset = hand.transform.position - holdingBody.worldCenterOfMass;
        offset = Mathf.Min(offset.magnitude, 1.0f) * offset.normalized;
        holdingPoint = holdingBody.transform.InverseTransformPoint(holdingBody.worldCenterOfMass + offset);

        hand.AttachObject(this.gameObject, startingGrabType, m_attachmentFlags);

        // Update holding list
        holdingHands.Add(hand);
        holdingBodies.Add(holdingBody);
        holdingPoints.Add(holdingPoint);
        holdingJoints.Add(handJoint);
    }

    //-------------------------------------------------
    protected virtual void OnDetachedFromHand(Hand hand)
    {

        hand.HoverUnlock(null);

        Vector3 velocity;
        Vector3 angularVelocity;

        GetReleaseVelocities(hand, out velocity, out angularVelocity);

        for (int i = 0; i < rigidBodies.Count; i++)
        {
            rigidBodies[i].velocity = velocity;
            rigidBodies[i].angularVelocity = angularVelocity;
        }
    }


    public virtual void GetReleaseVelocities(Hand hand, out Vector3 velocity, out Vector3 angularVelocity)
    {
        hand.GetEstimatedPeakVelocities(out velocity, out angularVelocity);

        //if (releaseVelocityStyle != ReleaseStyle.NoChange)
        //{
        //    float scaleFactor = 1.0f;
        //    if (scaleReleaseVelocityThreshold > 0)
        //    {
        //        scaleFactor = Mathf.Clamp01(scaleReleaseVelocityCurve.Evaluate(velocity.magnitude / scaleReleaseVelocityThreshold));
        //    }

        //    velocity *= (scaleFactor * scaleReleaseVelocity);
        //}
    }

}
