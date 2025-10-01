using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Game.Utility;

public class FirstPersonSelectionController : MonoBehaviour
{

    
    private void Update()
    {
       
        RaycastHit hit;
        if (Physics.Raycast(transform.position, transform.forward, out hit, Mathf.Infinity))
        {
            Selectable selectable = hit.collider.gameObject.GetComponent<Selectable>();
            if (selectable != null)
            {

                if (GameData.currentSelectable != selectable)
                {
                    GameData.currentSelectable = selectable;
                }


                if (Input.GetMouseButtonDown(0))
                {
                    selectable.action.Invoke();
                }              
            }
            else
            {
                GameData.currentSelectable = null;
            }
        }
        else 
        {
            GameData.currentSelectable = null;
        }

        //will be changes to esc once left editor phase as esc input gets captures by editor before i can use it...
        if (Utility.GetKeyOrDown(KeyCode.F1)) 
        {
    
            GameData.currentFocus = Focus.BASE_FLIGHT;
        }
    }
}
