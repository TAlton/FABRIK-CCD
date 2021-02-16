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
