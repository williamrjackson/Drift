using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Drift : MonoBehaviour {
    public float maxSpeed = 5f;
    public float rotateSpeed = 200f;
    public TouchAxisCtrl touchAxis;
    public TrailRenderer[] trails;

    Transform m_VelDir;
    Rigidbody2D m_RigidBody;
    float m_AppliedSpeed = 0;

    void Start()
    {
        m_RigidBody = GetComponent<Rigidbody2D>();
        m_VelDir = new GameObject().transform;
        m_VelDir.parent = transform;
        m_VelDir.localPosition = Vector3.zero;
        m_VelDir.localEulerAngles = Vector3.zero;
    }
    void Update()
    {
        m_AppliedSpeed = m_RigidBody.velocity.magnitude;
        if (m_AppliedSpeed < maxSpeed)
        {
            m_AppliedSpeed += maxSpeed * Time.deltaTime;
        }
        float zVal;
        float angleOffset = Remap((Mathf.DeltaAngle(transform.eulerAngles.z, m_VelDir.transform.eulerAngles.z)), -90, 90, -1, 1);
        if (touchAxis.GetAxis("InverseHorizontal") == 0)
        {
            zVal = transform.eulerAngles.z +  m_AppliedSpeed * angleOffset / 3;
        }
        else
        {
            zVal = transform.eulerAngles.z + rotateSpeed * Time.deltaTime * touchAxis.GetAxis("InverseHorizontal");
        }
        transform.eulerAngles = new Vector3(transform.eulerAngles.x, transform.eulerAngles.y, zVal);
        m_VelDir.localEulerAngles = new Vector3(0, 0, Mathf.LerpAngle(m_VelDir.localEulerAngles.z, touchAxis.GetAxis("Horizontal") * 90f, Time.deltaTime));

        foreach (TrailRenderer t in trails)
        {
            Color newAlpha = t.startColor;
            newAlpha.a = Mathf.Abs(angleOffset);
            t.startColor = newAlpha;
        }

    }

    void LateUpdate()
    {
        m_RigidBody.velocity = m_VelDir.up * m_AppliedSpeed;
    }

    float Remap(float val, float srcMin, float srcMax, float destMin, float destMax)
    {
        return Mathf.Lerp(destMin, destMax, Mathf.InverseLerp(srcMin, srcMax, val));
    }
}
