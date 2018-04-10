using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TrackPegDrift : MonoBehaviour {

    public Canvas RadialImageCanvas;
    public ParticleSystem particleCelebration;
    private bool m_bIsTriggered;
    private Transform m_Captured;
    private float m_InAngle;
    private Image m_Radial;
    private Vector3 m_InitRadialScale;
    bool m_bJustTriggered;
    bool m_Clockwise;
    float m_LastVal;

	// Use this for initialization
	void Start () {
        m_Radial = RadialImageCanvas.GetComponentInChildren<Image>();
        m_InitRadialScale = RadialImageCanvas.transform.localScale;
	}

	// Update is called once per frame
	void Update () {
		if (m_bIsTriggered)
        {
            Vector3 dirBetween = m_Captured.transform.position - transform.position;
            float angle = EnsurePositiveAngle(Mathf.Atan2(dirBetween.y, dirBetween.x) * Mathf.Rad2Deg);
            float deltaAngle = Mathf.DeltaAngle(m_InAngle, angle);
            if (m_bJustTriggered && deltaAngle != 0)
            {
                m_bJustTriggered = false;
                if (deltaAngle > 0)
                {
                    m_Radial.fillClockwise = false;
                    m_Clockwise = false;
                }
                else
                {
                    m_Radial.fillClockwise = true;
                    m_Clockwise = true;
                }
            }

            float val;
            if (m_Clockwise)
            {
                val = Remap(EnsurePositiveAngle(deltaAngle), 0, 360, 1, 0);
            }
            else
            {
                val = Remap(EnsurePositiveAngle(deltaAngle), 360, 0, 1, 0);
            }

            // Detect either success or reversal
            if (val > .95f)
            {
                if (val - m_LastVal < .5f)
                {
                    m_bIsTriggered = false;
                    RadialImageCanvas.transform.localScale = Vector3.zero;
                    particleCelebration.Play();
                }
                else
                {
                    m_Radial.fillAmount = 0;
                    m_bJustTriggered = true;
                }
            }
            else
            {
                m_Radial.fillAmount = val;
                m_LastVal = val;
            }
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        m_LastVal = 0;
        m_bIsTriggered = true;
        m_bJustTriggered = true;
        m_Captured = other.transform;
        RadialImageCanvas.transform.position = transform.position;
        RadialImageCanvas.transform.localScale = m_InitRadialScale;
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
