using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class FABRIK_Closed_Loop : MonoBehaviour
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
    private Vector3 initial_dir_;

    [SerializeField] List<Vector3> joint_constraints_;

    // Start is called before the first frame update
    void Start()
    {

    }
    private void Awake()
    {
        joint_constraints_ = new List<Vector3>();
        Init();
        foreach(var joint in m_Joints)
        {
            joint_constraints_.Add(joint.localPosition);
        }
    }
    void Init()
    {
        m_Positions = new Vector3[m_ChainLength];
        m_BoneLengths = new float[m_ChainLength];
        m_LimbLength = 0;
        m_InitialDir = new Vector3[m_ChainLength];
        m_InitialRotJoint = new Quaternion[m_ChainLength];

        if (null == m_TargetTransform)
        {
            m_TargetTransform = new GameObject(gameObject.name + " Target").transform;
            m_TargetTransform.position = transform.position;
        }

        m_InitialRotTarget = m_TargetTransform.rotation;
        Transform lsCurrentTransform = m_Joints[0];

        for(int i = 0; i < m_ChainLength; i++)
        {
            m_BoneLengths[i] = (m_Joints[GetNextJointIndex(i)].position - lsCurrentTransform.position).magnitude;
            m_LimbLength += m_BoneLengths[i];

            lsCurrentTransform = m_Joints[GetNextJointIndex(i)];
        }

        //find the midpoint of the vertices and create the initial direction
        Vector3 additive_position = Vector3.zero;
        for (int i = 1; i < m_Joints.Length; i++){
            additive_position += m_Joints[i].position;
        }

        initial_dir_ = ((additive_position / m_Joints.Length) - m_Joints[0].position).normalized;

    }
    // Update is called once per frame
    void Update()
    {

    }
    private void LateUpdate()
    {
    }
    public void PreserveLoop()
    {
        Vector3 temp = (m_Joints[1].position + m_Joints[2].position) / 2;
        //Debug.Log(temp);
        Vector3 dir = (temp - m_Joints[0].position).normalized;
        m_Joints[0].rotation *= Quaternion.FromToRotation(initial_dir_, dir);
        initial_dir_ = dir;
        //m_Joints[0].transform.up = dir;

        for (int i = 0; i < m_Joints.Length; i++)
        {
            m_Joints[i].transform.localPosition = joint_constraints_[i];
        }
    }
    private void OnDrawGizmos()
    {
        Transform lsCurrentTransform = m_Joints[0];
        int ii = 1;
        for (int i = 0; i < m_ChainLength && lsCurrentTransform != null; i++)
        {
            float lsScale = Vector3.Distance(lsCurrentTransform.position, m_Joints[ii].position) * 0.1f;
            Handles.matrix = Matrix4x4.TRS(lsCurrentTransform.position, Quaternion.FromToRotation(Vector3.up,
                                                                                        m_Joints[ii].position - lsCurrentTransform.position),
                                                                                        new Vector3(lsScale, Vector3.Distance(m_Joints[ii].position,
                                                                                        lsCurrentTransform.position), lsScale));
            Handles.color = Color.green;

            Handles.DrawWireCube(Vector3.up * 0.5f, Vector3.one);

            lsCurrentTransform = m_Joints[ii];
            if (ii == m_ChainLength - 1)
            {
                ii = 0;
                continue;
            }
            ii = GetNextJointIndex(ii);
            if (ii == 0) continue;
            ii++;

        }
    }

    private int GetNextJointIndex(int arg_index)
    {
        if(arg_index >= m_ChainLength - 1)
        {
            return 0;
        }
        return arg_index++;
    }
    private int GetPrevJointIndex(int arg_index)
    {
        if (arg_index <= 0)
        {
            return (int)m_ChainLength - 1;
        }
        return arg_index - 1;
    }

}
