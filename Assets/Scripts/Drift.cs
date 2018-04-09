using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Drift : MonoBehaviour {
    public float maxSpeed = 5f;
    public float accelerationTime = 1f;
    public float rotateDegreesPerSec = 180f;
    public TouchAxisCtrl touchAxis;
    public float trackSegLength = .15f;
    public int traceSegCount = 100;
    public Transform[] wheels;
    public Material trailMaterial;

    Rigidbody2D m_RigidBody;
    private Transform m_VelDir;
    float m_AppliedSpeed = 0;
    private List<WheelTrack> m_WheelTracks;
    private Vector3 m_LastPos;

    void Start()
    {
        m_RigidBody = GetComponent<Rigidbody2D>();
        // Velocity direction is parented by the car transform, which allows it to adopt it's rotations, 
        // while simultaneously getting it's own rotation. That's the "drifty" part.
        m_VelDir = new GameObject().transform;
        m_VelDir.parent = transform;
        m_VelDir.localPosition = Vector3.zero;
        m_VelDir.localEulerAngles = Vector3.zero;
        m_LastPos = transform.position;
        m_WheelTracks = new List<WheelTrack>();
        for (int i = 0; i < wheels.Length; i++)
        {
            WheelTrack wheel = new WheelTrack();
            wheel.Init(wheels[i], trailMaterial, traceSegCount);
            m_WheelTracks.Add(wheel);
        }

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
            zVal = transform.eulerAngles.z + m_AppliedSpeed * angleOffset / 3;
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

        // If we've traveled the track segment lengh, make a new segment.
        if (Vector3.Distance(transform.position, m_LastPos) > trackSegLength)
        {
            // Remember this for next time to see how far we've traveled.
            m_LastPos = transform.position;

            // Determine track segment transparency. 
            Color newAlpha = Color.black;
            newAlpha.a = Mathf.Min(Mathf.Abs(angleOffset), .5f);
            foreach (WheelTrack wheel in m_WheelTracks)
            {
                wheel.AddSegment(newAlpha);
            }
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

    private class WheelTrack
    {
        private List<Transform> lines;
        private Vector3 lastPos;
        private Transform transform;
        private Transform lineHolder;
        private int lineIndex = 0;
        private Material mat;
        private int segmentCount;
        
        public void Init(Transform tf, Material material, int segCount)
        {
            lines = new List<Transform>();
            transform = tf;
            mat = material;
            segmentCount = segCount;
            lineHolder = new GameObject(transform.name + "Tracks").transform;
        }
        public void AddSegment(Color color)
        { 
            // Until we have enough segments, make new ones.
            if (lines.Count < segmentCount)
            {
                GameObject go = new GameObject();
                go.transform.parent = lineHolder;

                // Set up line renderer
                LineRenderer newLine = go.AddComponent<LineRenderer>();
                newLine.material = mat;
                newLine.startWidth = newLine.endWidth = .05f;
                newLine.positionCount = 2;

                // Add to our list of lines
                lines.Add(go.transform);
            }

            // Set up the line the way we want it.
            LineRenderer line = lines[lineIndex].GetComponent<LineRenderer>();
            line.SetPosition(0, transform.position);
            line.SetPosition(1, lastPos);
            line.startColor = line.endColor = color;

            lastPos = transform.position;
            // Loop back to the oldest segment next time (unless there's room)
            lineIndex = (lineIndex + 1) % segmentCount;
        }
    }
}
