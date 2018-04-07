using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Drift : MonoBehaviour {
    public float maxSpeed = 5f;
    public float accelerationTime = 1f;
    public float rotateDegreesPerSec = 180f;
    public TouchAxisCtrl touchAxis;
    public TrailRenderer[] trails;

    Transform m_VelDir;
    Rigidbody2D m_RigidBody;
    float m_AppliedSpeed = 0;

    void Start()
    {
        m_RigidBody = GetComponent<Rigidbody2D>();
        // Velocity direction is parented by the car transform, which allows it to adopt it's rotations, 
        // while simultaneously getting it's own rotation. That's the "drifty" part.
        m_VelDir = new GameObject().transform;
        m_VelDir.parent = transform;
        m_VelDir.localPosition = Vector3.zero;
        m_VelDir.localEulerAngles = Vector3.zero;
    }
    void Update()
    {
        // Set applied speed to the car's actual speed. This is for the cases where the car's travel
        // is interrupted by a collision, and needs to build back up to max again.
        m_AppliedSpeed = m_RigidBody.velocity.magnitude;

        // If we're not currently travelling at max speed...
        if (m_AppliedSpeed < maxSpeed)
        {
            // See if we crashed into something, causing us to stop. Make sure we're not still trying to drift/skid. 
            if (m_AppliedSpeed < .5f)
                m_VelDir.localEulerAngles = Vector3.zero;
            // Gradually build speed.
            m_AppliedSpeed += maxSpeed * Time.deltaTime * accelerationTime;
        }
        float zVal;
        // Represent the difference between the car's pointing direction and travel direction as a float ranging -1 to 1. 
        float angleOffset = Remap((Mathf.DeltaAngle(transform.eulerAngles.z, m_VelDir.transform.eulerAngles.z)), -90, 90, -1, 1);
        if (touchAxis.GetAxis("InverseHorizontal") == 0)
        {
            // If not actively turning, straighten out gradually.
            zVal = transform.eulerAngles.z +  m_AppliedSpeed * angleOffset / 3;
        }
        else
        {
            // Turn over n degrees per second, scaled by joystick input value.
            zVal = transform.eulerAngles.z + rotateDegreesPerSec * Time.deltaTime * touchAxis.GetAxis("InverseHorizontal");
        }
        // Apply new rotation
        transform.eulerAngles = new Vector3(transform.eulerAngles.x, transform.eulerAngles.y, zVal);
        // Determine travel velocity. The car may be moving perfectly "sideways," traveling 90 deg from car's forward.
        m_VelDir.localEulerAngles = new Vector3(0, 0, Mathf.LerpAngle(m_VelDir.localEulerAngles.z, touchAxis.GetAxis("Horizontal") * 90f, Time.deltaTime));

        // Manage the transparency of trail renderers. Not perfect, since the whole trail fades when the car straightens out.
        // Ideally, "skidmarks" would be permanent.
        foreach (TrailRenderer t in trails)
        {
            Color newAlpha = t.startColor;
            newAlpha.a = Mathf.Abs(angleOffset);
            t.startColor = newAlpha;
        }

    }

    void LateUpdate()
    {
        // Move the car
        m_RigidBody.velocity = m_VelDir.up * m_AppliedSpeed;
    }

    // Linear remap
    float Remap(float val, float srcMin, float srcMax, float destMin, float destMax)
    {
        return Mathf.Lerp(destMin, destMax, Mathf.InverseLerp(srcMin, srcMax, val));
    }
}
