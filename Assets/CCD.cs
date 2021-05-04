using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CCD : MonoBehaviour
{
    [SerializeField] private uint m_ChainLength;
    [SerializeField] private uint m_Iterations;
    [SerializeField] private Transform m_TargetTransform;
    [SerializeField] private Transform m_PoleTransform;
    [SerializeField] protected float[] m_BoneLengths;
    [SerializeField] protected float m_LimbLength;
    [SerializeField] protected Transform[] m_Joints;
    [SerializeField] protected Vector3[] m_Positions;
    [SerializeField] protected float m_Epsilon;
    [SerializeField] Vector3[] m_InitialDir;
    [SerializeField] Quaternion[] m_InitialRotJoint;
    [SerializeField] Quaternion m_InitialRotTarget;
    [SerializeField] Quaternion m_InitialRotRoot;
    private void Awake()
    {
        Init();
    }
    void Init()
    {
        m_Joints = new Transform[m_ChainLength + 1];
        m_Positions = new Vector3[m_ChainLength + 1];
        m_BoneLengths = new float[m_ChainLength];
        m_LimbLength = 0;
        m_InitialDir = new Vector3[m_ChainLength + 1];
        m_InitialRotJoint = new Quaternion[m_ChainLength + 1];

        if (null == m_TargetTransform)
        {
            m_TargetTransform = new GameObject(gameObject.name + " Target").transform;
            m_TargetTransform.position = transform.position;
        }

        m_InitialRotTarget = m_TargetTransform.rotation;
        Transform lsCurrentTransform = this.transform;

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
    // Start is called before the first frame update
    void Start()
    {



    }

    // Update is called once per frame
    void LateUpdate()
    {

        for (int i = 0; i < m_Joints.Length; i++)
        {
            m_Positions[i] = m_Joints[i].position;
        }

        for (int i = 0; i < m_Iterations; i++)
        {
            for (int j = m_Joints.Length - 1; j >= 0; j--)
            {
                Vector3 lsJointToEffector = m_Joints[m_Joints.Length - 1].position - m_Joints[j].position;
                Vector3 lsBoneToTarget = m_TargetTransform.position - m_Joints[j].position;
                Quaternion lsFromTo = Quaternion.FromToRotation(lsJointToEffector, lsBoneToTarget);
                Quaternion lsNewRot = lsFromTo * m_Joints[j].rotation;

                m_Joints[j].rotation = lsNewRot;
            }

            if ((m_Positions[m_Positions.Length - 1] - m_TargetTransform.position).sqrMagnitude < Mathf.Pow(m_Epsilon, 2)) break;
        }
    }
}
