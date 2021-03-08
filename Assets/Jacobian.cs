using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Jacobian : MonoBehaviour
{
    [SerializeField] private uint m_ChainLength;
    [SerializeField] protected Transform[] m_Joints;
    [SerializeField] protected Vector3[] m_Positions;
    [SerializeField] private Transform m_TargetTransform;
    [SerializeField] protected float[] m_BoneLengths;
    [SerializeField] protected float m_LimbLength;
    [SerializeField] Quaternion[] m_InitialRotJoint;
    [SerializeField] Quaternion m_InitialRotTarget;
    [SerializeField] Quaternion m_InitialRotRoot;
    [SerializeField] protected float m_Epsilon;
    [SerializeField] Vector3[] m_InitialDir;

    // Start is called before the first frame update
    void Start()
    {
        m_Joints = new Transform[m_ChainLength + 1];
        m_Positions = new Vector3[m_ChainLength + 1];

        m_InitialRotTarget = m_TargetTransform.rotation;
        Transform lsCurrentTransform = this.transform;

        if (null == m_TargetTransform)
        {
            m_TargetTransform = new GameObject(gameObject.name + " Target").transform;
            m_TargetTransform.position = transform.position;
        }

        for (int i = m_Joints.Length - 1; i >= 0; i--)
        {
            m_Joints[i] = lsCurrentTransform;
            m_InitialRotJoint[i] = lsCurrentTransform.rotation;

            if (m_Joints.Length - 1 == i)
            {
                //leaf node
                m_InitialDir[i] = m_TargetTransform.position - lsCurrentTransform.position;
            }
            else
            {
                m_InitialDir[i] = m_Joints[i + 1].position - lsCurrentTransform.position;
                m_BoneLengths[i] = (m_Joints[i + 1].position - lsCurrentTransform.position).magnitude;
                m_LimbLength += m_BoneLengths[i];
            }

            lsCurrentTransform = lsCurrentTransform.parent;
        }

    }

    // Update is called once per frame
    void Update()
    {
        
        while(Vector3.Distance(m_Joints[m_Joints.Length - 1].position,m_TargetTransform.position) > m_Epsilon)
        {
            Vector3[] lsDeltaOrientation = CalcOrientation();
            for (int i = 0; i < m_Joints.Length - 1; i++)
            {
                m_Joints[i].eulerAngles = lsDeltaOrientation[i]; //might have to convert to quaternions
            }
        }
    }
    private void LateUpdate()
    {
        List<float> lsAngles = new List<float>();

        for(int i  = 0; i < m_Joints.Length - 1; i++)
        {
            lsAngles.Add(CalcAngle(Vector3.up, m_Joints[i].transform.position, m_Joints[i - 1].transform.position));
        }
        CalcJacobian(lsAngles);
    }
    private Vector3 CalcOrientation()
    {
        float[,] lsJacobian = CalcJacobian();
        Vector3 lsV = m_TargetTransform.position - this.transform.position;
        //return new Vector3[] lsJacobian * lsV; //need to implement MatrixVector mult
    }
    private float[,] CalcJacobian()
    {
        float[,] lsJacobian = new float[m_Joints.Length - 1, 3];

        for(int i= 0; i < m_Joints.Length - 1; i++)
        {
            Vector3 lsTemp = Vector3.Cross(m_Joints[i].forward, m_Joints[m_Joints.Length - 1].transform.position - m_Joints[i].transform.position);
            lsJacobian[i, 0] = lsTemp.x;
            lsJacobian[i, 1] = lsTemp.y;
            lsJacobian[i, 2] = lsTemp.z;
        }

        return lsJacobian;
    }
    private float CalcAngle(Vector3 argAxis, Vector3 argPosA, Vector3 argPosB)
    {
        float lsAngle = Vector3.Angle(argAxis, (argPosA - argPosB).normalized);

        return Vector3.Cross(argAxis, ((argPosA - argPosB).normalized)).z < 0f ? -lsAngle : lsAngle;
    }
}
