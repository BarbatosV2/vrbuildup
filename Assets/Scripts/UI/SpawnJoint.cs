using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR.InteractionSystem;

public class SpawnJoint : MonoBehaviour
{
    public List<GameObject> attachmentPrefabs;
    public RadialMenu radialMenu = null;

    public void attachBalljoint()
    {
        print("stop");
        GameObject ballJoint = Instantiate(attachmentPrefabs[0], transform.position, Quaternion.identity);
       // ballJoint.GetComponent<Rigidbody>().useGravity = false;
        radialMenu.interactingHand.AttachObject(ballJoint, GrabTypes.Trigger);
        
        print("why");
    }

   

    /*private void SetMaterial(Material newMaterial)
    {
        MeshRenderer renderer = GetComponent<MeshRenderer>();
        renderer.material = newMaterial;
    }*/
}
