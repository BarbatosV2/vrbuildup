using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;
using Valve.VR.InteractionSystem;

public class ThrowableSpecialAT : MonoBehaviour
{
    public Hand.AttachmentFlags m_attachmentFlags;
    protected VelocityEstimator velocityEstimator;

    private List<Hand> holdingHands = new List<Hand>();
    private List<Rigidbody> holdingBodies = new List<Rigidbody>();
    public List<Vector3> holdingPoints = new List<Vector3>();
    private List<ConfigurableJoint> holdingJoints = new List<ConfigurableJoint>();

    private List<Rigidbody> rigidBodies = new List<Rigidbody>();

    // Start is called before the first frame update
    void Start()
    {
        rigidBodies.Add(GetComponentInParent<Rigidbody>());
        velocityEstimator = GetComponent<VelocityEstimator>();
    }

    //private void OnDrawGizmos()
    //{
    //    Gizmos.color = Color.red;
    //    for (int i = 0; i < holdingHands.Count; i++)
    //    {
    //        Gizmos.DrawSphere(holdingHands[i].transform.position, 0.05f);
    //    }
    //}

    // Update is called once per frame
    void Update()  
    {
        for (int i = 0; i < holdingHands.Count; i++)
        {
            if(holdingHands.Count > 1)
            {
                holdingHands[i].skeleton.transform.position = holdingBodies[i].transform.TransformPoint(holdingPoints[i]);
            }
            
            //two-handed grabbing should detach if the player's hands disagree (move too close to each other, etc)
            if (Vector3.Distance(holdingHands[i].transform.position,holdingHands[i].skeleton.transform.position) > 0.25f)
            {
                PhysicsDetach(holdingHands[i]);
                continue;
            }

            //limit the x-rotation and lock Y and Z if holding with two hands
            //reasoning behind this is because X rotation (lateral rotation) is often not wanted when using two hands to grab
            //but still can happen, obviously, so limit it to ~60 degrees
            holdingJoints[i].angularXMotion = (holdingHands.Count > 1) ? ConfigurableJointMotion.Limited : ConfigurableJointMotion.Locked;
            holdingJoints[i].angularYMotion = holdingJoints[i].angularZMotion = (holdingHands.Count > 1) ? ConfigurableJointMotion.Free : ConfigurableJointMotion.Locked;

            if (holdingHands[i].IsGrabEnding(this.gameObject))
            {
                PhysicsDetach(holdingHands[i]);
            }
        }
    }

    IEnumerator ResetJointHandles()
    {
        for (int i = 0; i < holdingJoints.Count; i++)
        {
            holdingJoints[i].connectedBody = null;
        }
        yield return new WaitForEndOfFrame();
        for (int i = 0; i < holdingJoints.Count; i++)
        {
            holdingJoints[i].connectedBody = holdingBodies[i];
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

        //The hand should grab onto the nearest rigid body
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

        Rigidbody handRigidbody = Util.FindOrAddComponent<Rigidbody>(hand.gameObject);
        handRigidbody.isKinematic = true;

        ConfigurableJoint handJoint = hand.gameObject.AddComponent<ConfigurableJoint>();
        Debug.Log("context for handj", hand.gameObject);
        handJoint.connectedBody = holdingBody;
        handJoint.xMotion = handJoint.yMotion = handJoint.zMotion = ConfigurableJointMotion.Locked;

        // Don't let the hand interact with other things while it's holding us
        hand.HoverLock(null);

        // Affix this point
        Vector3 offset = hand.transform.position - holdingBody.worldCenterOfMass;
        offset = Mathf.Min(offset.magnitude, 1.0f) * offset.normalized;

        Collider col = holdingBody.GetComponentInChildren<Collider>();

        Vector3 closestToMesh = Physics.ClosestPoint(holdingBody.worldCenterOfMass + offset,col,col.transform.position,col.transform.rotation);
        holdingPoint = holdingBody.transform.InverseTransformPoint(closestToMesh);

        //if not already holding, move object to meet hand
        if(true)//if(holdingHands.Count == 0)
        {
            Vector3 moveVector = hand.transform.position - closestToMesh;
            //print(moveVector);
            holdingBody.transform.position += moveVector;
        }

        // Create a configurable joint from the hand to the holding body
        ConfigurableJointMotion _m = ConfigurableJointMotion.Limited;
        handJoint.angularXMotion = handJoint.angularYMotion = handJoint.angularZMotion = _m;

        SoftJointLimit _l = new SoftJointLimit
        {
            limit = 50f
        };
        handJoint.lowAngularXLimit = handJoint.angularYLimit = handJoint.angularZLimit = _l;

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

        StartCoroutine(ResetJointHandles());

        Vector3 velocity;
        Vector3 angularVelocity;

        GetReleaseVelocities(hand, out velocity, out angularVelocity);

        //still technically holding it 
        if (holdingHands.Count > 1) return;

        for (int i = 0; i < rigidBodies.Count; i++)
        {
            rigidBodies[i].velocity = velocity;
            rigidBodies[i].angularVelocity = angularVelocity;
        }
    }

    public virtual void GetReleaseVelocities(Hand hand, out Vector3 velocity, out Vector3 angularVelocity)
    {
        hand.GetEstimatedPeakVelocities(out velocity, out angularVelocity);
    }

}
