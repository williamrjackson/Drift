using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrackPegDrift : MonoBehaviour {

    public Canvas RadialImageCanvas;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    void OnTriggerEnter2D(Collider2D other)
    {
        RadialImageCanvas.transform.position = transform.position;
        Vector2 tada = (other.transform.position - transform.position).normalized;
        float angle = Mathf.Atan2(tada.x, tada.y) * Mathf.Rad2Deg;
        RadialImageCanvas.transform.eulerAngles = new Vector3(RadialImageCanvas.transform.eulerAngles.x, RadialImageCanvas.transform.eulerAngles.y, angle); 
        print("in");
    }
    void OnTriggerExit2D(Collider2D other)
    {
        print("Out!");
    }
}
