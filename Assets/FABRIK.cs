using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class FABRIK : MonoBehaviour
{
    [SerializeField] private uint m_ChainLength;
    [SerializeField] private uint m_Iterations;
    [SerializeField] private Transform m_TargetTransform;
    [SerializeField] private Transform m_ChildTransform;
    [SerializeField] protected float[] m_BoneLengths;
    [SerializeField] protected float m_LimbLength;
    [SerializeField] protected Transform[] m_Joints;
    [SerializeField] protected Vector3[] m_Postions;
    [SerializeField] protected float m_Delta;

    // Start is called before the first frame update
    void Start()
    {
        
    }
    private void Awake()
    {
        Init();
    }
    void Init()
    {
        m_Joints                        = new Transform[m_ChainLength + 1];
        m_Postions                      = new Vector3[m_ChainLength + 1];
        m_BoneLengths                   = new float[m_ChainLength];
        m_LimbLength                    = 0;
        Transform lsCurrentTransform    = this.transform;

        for(int i = m_Joints.Length - 1; i >= 0; i--)
        {
            m_Joints[i]                 = lsCurrentTransform;

            if(m_Joints.Length - 1 == i)
            {

            } else
            {
                m_BoneLengths[i]        = (m_Joints[i + 1].position - lsCurrentTransform.position).magnitude;
                m_LimbLength            += m_BoneLengths[i];
            }

            lsCurrentTransform          = lsCurrentTransform.parent;
        }
    }
    // Update is called once per frame
    void Update()
    {
        
    }
    private void LateUpdate()
    {
        ResolveFABRIK();
    }
    private void ResolveFABRIK()
    {
        if (null == m_TargetTransform) return;
        if (m_BoneLengths.Length != m_ChainLength) Init();

        for(int i = 0; i < m_Joints.Length; i++)
        {
            m_Postions[i]               = m_Joints[i].position;
        }
        //checks if the target is reachable by the limb
        if ((m_TargetTransform.position - m_Joints[0].position).sqrMagnitude >= Mathf.Pow(m_LimbLength, 2))
        {
            //not reachable
            Vector3 lsDirection         = (m_TargetTransform.position - m_Postions[0]).normalized;
            
            for(int i = 1; i < m_Postions.Length; i++)
            {
                //the limb reaches for the target ending in a straight limb
                m_Postions[i]           = m_Postions[i - 1] + lsDirection * m_BoneLengths[i - 1];
            }

        }  else
        {
            //reachable
            for(int i = 0; i < m_Iterations; i++)
            {
                //backwards iteration
                for(int ii = m_Postions.Length - 1; ii > 0; ii--)
                {
                    if(ii == m_Postions.Length - 1)
                    {
                        m_Postions[ii]   = m_TargetTransform.position;
                    } else
                    {
                        //checking that the distances of joints are constrained to bone length
                        m_Postions[ii]  = m_Postions[ii + 1] + (m_Postions[ii] - m_Postions[ii + 1]).normalized * m_BoneLengths[ii];
                    }
                }
                //forward iteration
                for(int ii = 1; ii < m_Postions.Length; ii++)
                {
                    m_Postions[ii]      = m_Postions[ii - 1] + (m_Postions[ii] - m_Postions[ii - 1]).normalized * m_BoneLengths[ii - 1];
                }
                //if distance from target and end effector is within our delta we break
                if ((m_Postions[m_Postions.Length - 1] - m_TargetTransform.position).sqrMagnitude < Mathf.Pow(m_Delta, 2)) break;
            }
        }

        for(int i = 0; i < m_Postions.Length; i++)
        {
            m_Joints[i].position    = m_Postions[i];
        }
    }
    private void OnDrawGizmos()
    {
        Transform lsCurrentTransform    = this.transform;

        for(int i = 0; i < m_ChainLength && lsCurrentTransform != null && lsCurrentTransform.parent != null; i++)
        {
            float lsScale               = Vector3.Distance(lsCurrentTransform.position, lsCurrentTransform.parent.position) * 0.1f;
            Handles.matrix              = Matrix4x4.TRS(lsCurrentTransform.position, Quaternion.FromToRotation(Vector3.up,
                                                                                        lsCurrentTransform.parent.position - lsCurrentTransform.position),
                                                                                            new Vector3(lsScale, Vector3.Distance(lsCurrentTransform.parent.position,
                                                                                            lsCurrentTransform.position), lsScale));
            Handles.color               = Color.green;

            Handles.DrawWireCube(Vector3.up * 0.5f, Vector3.one);

            lsCurrentTransform          = lsCurrentTransform.parent;
        }
    }

}
