using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TrackPegDrift : MonoBehaviour {

    public Canvas RadialImageCanvas;

    private bool m_bIsTriggered;
    private Transform m_Captured;
    private float m_InAngle;
    private Image m_Radial;

	// Use this for initialization
	void Start () {
        m_Radial = RadialImageCanvas.GetComponentInChildren<Image>();
	}
	
	// Update is called once per frame
	void Update () {
		if (m_bIsTriggered)
        {
            Vector3 dirBetween = m_Captured.transform.position - transform.position;
            float angle = EnsurePositiveAngle(Mathf.Atan2(dirBetween.y, dirBetween.x) * Mathf.Rad2Deg);
            float deltaAngle = Mathf.DeltaAngle(m_InAngle, angle);
            float val = Remap(EnsurePositiveAngle(deltaAngle), 0, 360, 1, 0);
            m_Radial.fillAmount = val;
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        m_bIsTriggered = true;
        m_Captured = other.transform;
        RadialImageCanvas.transform.position = transform.position;
        Vector3 vectorTo = other.transform.position - transform.position;
        float angle = EnsurePositiveAngle(Mathf.Atan2(vectorTo.y, vectorTo.x) * Mathf.Rad2Deg);
        m_InAngle = angle;
        RadialImageCanvas.transform.localEulerAngles = new Vector3(0,0,angle); 
    }
    void OnTriggerExit2D(Collider2D other)
    {
        m_bIsTriggered = false;
        m_Radial.fillAmount = 0;
    }

    float Remap(float val, float srcMin, float srcMax, float destMin, float destMax)
    {
        return Mathf.Lerp(destMin, destMax, Mathf.InverseLerp(srcMin, srcMax, val));
    }
    public static float EnsurePositiveAngle(float inputAngle)
    {
        if (inputAngle < 0) { inputAngle += 360; }
        return inputAngle;
    }
}
