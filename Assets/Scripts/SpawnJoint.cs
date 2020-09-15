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
        radialMenu.interactingHand.AttachObject(ballJoint, GrabTypes.Grip);
    }

   

    /*private void SetMaterial(Material newMaterial)
    {
        MeshRenderer renderer = GetComponent<MeshRenderer>();
        renderer.material = newMaterial;
    }*/
}
