using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class FABRIK_Joints : MonoBehaviour
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
    List<joint_> list_joint_types_;

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
        list_joint_types_ = new List<joint_>();
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
        list_joint_types_.Add(this.GetComponent<joint_>());

        for (int i = m_Joints.Length - 1; i >= 0; i--)
        {
            m_Joints[i] = lsCurrentTransform;
            list_joint_types_.Add(m_Joints[i].gameObject.GetComponent<joint_>());
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
        ResolveFABRIK();
    }
    private void LateUpdate()
    {

    }
    private void ResolveFABRIK()
    {
        if (null == m_TargetTransform) return;
        if (m_BoneLengths.Length != m_ChainLength) Init();

        //for (int i = 0; i < m_Joints.Length; i++)
        //{
        //    m_Positions[i] = m_Joints[i].position;
        //}

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
            //reachable
            for (int i = 0; i < m_Iterations; i++)
            {
                //backwards iteration
                for (int ii = m_Positions.Length - 1; ii > 0; ii--)
                {
                    if (ii == m_Positions.Length - 1)
                    {
                        m_Positions[ii] = m_TargetTransform.position;
                    }
                    else
                    {
                        //aligning the joint to the next one backwards after the end eff has been moved to the target
                        ////Vector3 dir = (m_Positions[ii] - m_Positions[ii + 1]).normalized
                        m_Positions[ii] = m_Positions[ii + 1] + CheckConstraints(m_Positions[ii], m_Positions[ii + 1], list_joint_types_[ii], ii) * m_BoneLengths[ii];

                    }
                }
                //forward iteration
                for (int ii = 1; ii < m_Positions.Length; ii++)
                {
                    m_Positions[ii] = m_Positions[ii - 1] + CheckConstraints(m_Positions[ii], m_Positions[ii - 1], list_joint_types_[ii], ii) * m_BoneLengths[ii - 1];
                    //check distance
                    //if(Vector3.Distance(m_Positions[ii], m_Positions[ii - 1]) > m_BoneLengths[ii - 1] + list_joint_types_[i].translation_freedom_positive)
                    //check angle
                }
                //if distance from target and end effector is within our delta we break
                if ((m_Positions[m_Positions.Length - 1] - m_TargetTransform.position).sqrMagnitude < Mathf.Pow(m_Epsilon, 2)) break;
            }
        }

        //pole
        if (m_PoleTransform != null)
        {
            for (int i = 1; i < m_Positions.Length - 1; i++)
            {
                Plane lsPlane = new Plane(m_Positions[i + 1] - m_Positions[i - 1], m_Positions[i - 1]);
                Vector3 lsProjectedPole = lsPlane.ClosestPointOnPlane(m_PoleTransform.position);
                Vector3 lsProjectedJoint = lsPlane.ClosestPointOnPlane(m_Positions[i]);
                float lsAngle = Vector3.SignedAngle(lsProjectedJoint - m_Positions[i - 1], lsProjectedPole - m_Positions[i - 1], lsPlane.normal);
                m_Positions[i] = Quaternion.AngleAxis(lsAngle, lsPlane.normal) * (m_Positions[i] - m_Positions[i - 1]) + m_Positions[i - 1];
            }
        }

        for (int i = 0; i < m_Positions.Length; i++)
        {
            if (i == m_Positions.Length - 1)
            {
                //matches the rotation of the end effector to the target
                m_Joints[i].rotation = m_TargetTransform.rotation * Quaternion.Inverse(m_InitialRotTarget) * m_InitialRotJoint[i];
            }
            else
            {
                //rotates the joint from its initial dir to the current direction of the joint and its parent
                m_Joints[i].rotation = Quaternion.FromToRotation(m_InitialDir[i], m_Positions[i + 1] - m_Positions[i]) * m_InitialRotJoint[i];
            }

            m_Joints[i].position = m_Positions[i];
        }
    }
    private void OnDrawGizmos()
    {
        Transform lsCurrentTransform = this.transform;

        for (int i = 0; i < m_ChainLength && lsCurrentTransform != null; i++)
        {
            float lsScale = Vector3.Distance(lsCurrentTransform.position, lsCurrentTransform.parent.position) * 0.1f;
            Handles.matrix = Matrix4x4.TRS(lsCurrentTransform.position, Quaternion.FromToRotation(Vector3.up,
                                                                                        lsCurrentTransform.parent.position - lsCurrentTransform.position),
                                                                                            new Vector3(lsScale, Vector3.Distance(lsCurrentTransform.parent.position,
                                                                                            lsCurrentTransform.position), lsScale));
            Handles.color = Color.green;

            Handles.DrawWireCube(Vector3.up * 0.5f, Vector3.one);

            lsCurrentTransform = lsCurrentTransform.parent;
        }
    }

    private Vector3 CheckConstraints(Vector3 arg_from, Vector3 arg_to, joint_ arg_joint_type, int i)
    {
        Vector3 dir_from_to = (arg_from - arg_to).normalized;
        //direction from prev joint to current joint (current joints forward
        Vector3 dir_prev_from = (m_Joints[i - 1].position - arg_from).normalized;

        float x = AngleOffAroundAxis(dir_from_to, dir_prev_from, Vector3.right);
        float y = AngleOffAroundAxis(dir_from_to, dir_prev_from, Vector3.up);
        float z = AngleOffAroundAxis(dir_from_to, dir_prev_from, Vector3.forward);

        x = (x > arg_joint_type.rotation_freedom_positive_.x ? arg_joint_type.rotation_freedom_positive_.x : x);
        x = (x < arg_joint_type.rotation_freedom_negative_.x ? arg_joint_type.rotation_freedom_negative_.x : x);

        y = (y > arg_joint_type.rotation_freedom_positive_.y ? arg_joint_type.rotation_freedom_positive_.y : y);
        y = (y < arg_joint_type.rotation_freedom_negative_.y ? arg_joint_type.rotation_freedom_negative_.y : y);

        z = (z > arg_joint_type.rotation_freedom_positive_.z ? arg_joint_type.rotation_freedom_positive_.z : z);
        z = (z < arg_joint_type.rotation_freedom_negative_.z ? arg_joint_type.rotation_freedom_negative_.z : z);

        dir_prev_from = new Vector3(x, y, z);
        return dir_prev_from.normalized;
    }

    public static float AngleOffAroundAxis(Vector3 v, Vector3 forward, Vector3 axis, bool clockwise = false)
    {
        Vector3 right;
        if (clockwise)
        {
            right = Vector3.Cross(forward, axis);
            forward = Vector3.Cross(axis, right);
        }
        else
        {
            right = Vector3.Cross(axis, forward);
            forward = Vector3.Cross(right, axis);
        }
        return Mathf.Atan2(Vector3.Dot(v, right), Vector3.Dot(v, forward)) * Mathf.Rad2Deg;
    }

}
