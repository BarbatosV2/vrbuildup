using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Valve.VR.InteractionSystem.Sample
{
    public class MoveUI : UIElement
    {
        public GameObject ui;
        int position1 = 0;
        int position2 = 0;
        int position3 = 0;
        bool uimove = false;
        // Start is called before the first frame update
        protected override void Awake()
        {
            base.Awake();
        }

        protected override void OnButtonClick()
        {
            base.OnButtonClick();
            if (uimove == false)
            {
                uimove = true;

                while (ui.transform.position.z <= ui.transform.position.z + 100)
                {
                    ui.transform.position += new Vector3(0, 0, 1);
                }
                uimove = false;
            }

        }


    }
}
