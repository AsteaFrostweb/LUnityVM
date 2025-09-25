using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoundToCenter : MonoBehaviour
{
    public float radius = 300f;

    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (transform.position.magnitude > radius)
            Center();
    }

    private void Center() 
    {
        Vector3 offset = -transform.position;
        transform.position = new Vector3(0, 0, 0);
        foreach (Transform t in GameObject.FindObjectsByType(typeof(Transform), FindObjectsSortMode.None)) 
        {
            if(t != transform && t.parent == null) 
            t.position += offset;
        }
    }
}
