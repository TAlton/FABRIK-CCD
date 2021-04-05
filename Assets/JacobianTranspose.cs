using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JacobianTranspose : MonoBehaviour
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

    private void Awake()
    {
        Init();
    }

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    private void LateUpdate()
    {
        //ResolveFABRIK();
    }
    private void ResolveFABRIK()
    {
        if (null == m_TargetTransform) return;
        if (m_BoneLengths.Length != m_ChainLength) Init();

        for (int i = 0; i < m_Joints.Length; i++)
        {
            m_Positions[i] = m_Joints[i].position;
        }

        Quaternion lsRootRot = (m_Joints[0].parent != null) ? m_Joints[0].parent.rotation : Quaternion.identity;
        Quaternion lsRotDelta = lsRootRot * Quaternion.Inverse(m_InitialRotRoot);

        //checks if the target is reachable by the limb
        if ((m_TargetTransform.position - m_Joints[0].position).sqrMagnitude >= Mathf.Pow(m_LimbLength, 2))
        {
            //not reachable
            Vector3 lsDirection = (m_TargetTransform.position - m_Positions[0]).normalized;

            for (int i = 1; i < m_Positions.Length; i++)
            {
                //the limb reaches for the target ending in a straight limb
                m_Positions[i] = m_Positions[i - 1] + lsDirection * m_BoneLengths[i - 1];
            }

        }
        else
        {
            ////reachable
            //for (int i = 0; i < m_Iterations; i++)
            //{
            //    //backwards iteration
            //    for (int ii = m_Positions.Length - 1; ii > 0; ii--)
            //    {
            //        if (ii == m_Positions.Length - 1)
            //        {
            //            m_Positions[ii] = m_TargetTransform.position;
            //        }
            //        else
            //        {
            //            //checking that the distances of joints are constrained to bone length
            //            m_Positions[ii] = m_Positions[ii + 1] + (m_Positions[ii] - m_Positions[ii + 1]).normalized * m_BoneLengths[ii];
            //        }
            //    }
            //    //forward iteration
            //    for (int ii = 1; ii < m_Positions.Length; ii++)
            //    {
            //        m_Positions[ii] = m_Positions[ii - 1] + (m_Positions[ii] - m_Positions[ii - 1]).normalized * m_BoneLengths[ii - 1];
            //    }
            //    //if distance from target and end effector is within our delta we break
            //    if ((m_Positions[m_Positions.Length - 1] - m_TargetTransform.position).sqrMagnitude < Mathf.Pow(m_Epsilon, 2)) break;
            //}

            for(int i = m_Positions.Length - 1; i > 0; i--)
            {
                Vector3 lsJointToEffector = m_Positions[m_Joints.Length - 1] - m_Positions[i];
                Vector3 lsBoneToTarget = m_TargetTransform.position - m_Positions[i];
                Quaternion lsFromTo = Quaternion.FromToRotation(lsJointToEffector, lsBoneToTarget);
                Quaternion lsNewRot = lsFromTo * m_Joints[i].rotation;

                m_Joints[i].rotation = lsNewRot;
            }
             
        }

        //for (int i = 0; i < m_Positions.Length; i++)
        //{
        //    if (i == m_Positions.Length - 1)
        //    {
        //        m_Joints[i].rotation = m_TargetTransform.rotation * Quaternion.Inverse(m_InitialRotTarget) * m_InitialRotJoint[i];
        //    }
        //    else
        //    {
        //        m_Joints[i].rotation = Quaternion.FromToRotation(m_InitialDir[i], m_Positions[i + 1] - m_Positions[i]) * m_InitialRotJoint[i];
        //    }

        //    m_Joints[i].position = m_Positions[i];
        //}
    }
}
