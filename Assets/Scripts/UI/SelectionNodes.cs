using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR.InteractionSystem;

public class SelectionNodes : MonoBehaviour
{
    // Start is called before the first frame update
    public List<Transform> selectionNodesList;

    public void Hide()
    {
        for (int i = 0; i < 5; i++) {
            selectionNodesList[i].gameObject.SetActive(false);
                }
    }

    public void Show(int index, bool value)
    {
        selectionNodesList[index].gameObject.SetActive(value);
    }
}
