using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Jacobian : MonoBehaviour
{
    [SerializeField] private uint m_ChainLength;
    [SerializeField] private uint m_Iterations;
    [SerializeField] protected Transform[] m_Joints;
    [SerializeField] protected Vector3[] m_Positions;
    [SerializeField] protected Vector3[] m_Angles;
    [SerializeField] private Transform m_TargetTransform;
    [SerializeField] protected float[] m_BoneLengths;
    [SerializeField] protected float m_LimbLength;
    [SerializeField] Quaternion[] m_InitialRotJoint;
    [SerializeField] Quaternion m_InitialRotTarget;
    [SerializeField] Quaternion m_InitialRotRoot;
    [SerializeField] protected float m_Epsilon;
    [SerializeField] Vector3[] m_InitialDir;
    [SerializeField] float[,] m_1;
    [SerializeField] float[,] m_2;

    // Start is called before the first frame update
    void Start()
    {

        float[,] matrix =
        {
            {1, 2, 3 },
            {4, 5, 6 },
            {7, 8, 9 },
            {10, 11, 12 },
            {13, 14, 15 }
        };

        Vector3 test = new Vector3(1, 2, 3);


        Debug.Log(MatrixMult(matrix, test));
    }

    private void Awake()
    {
        Init();
    }

    void Init()
    {
        m_Joints = new Transform[m_ChainLength + 1];
        m_Positions = new Vector3[m_ChainLength + 1];
        m_Angles = new Vector3[m_ChainLength + 1];
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

    // Update is called once per frame
    void Update()
    {
    }
    private void LateUpdate()
    {


        //if ((m_TargetTransform.position - m_Joints[0].position).sqrMagnitude >= Mathf.Pow(m_LimbLength, 2))
        //{
        //    Vector3 lsDirection = (m_TargetTransform.position - m_Joints[0].position).normalized;

        //    for (int i = 1; i < m_Joints.Length; i++)
        //    {
        //        //the limb reaches for the target ending in a straight limb
        //        m_Joints[i].position = m_Joints[i-1].position + lsDirection * m_BoneLengths[i - 1];
        //    }

        //    for(int i = 0; i < m_Joints.Length; i++)
        //    {
        //        if (i == m_Positions.Length - 1)
        //        {
        //            m_Joints[i].rotation = m_TargetTransform.rotation * Quaternion.Inverse(m_InitialRotTarget) * m_InitialRotJoint[i];
        //        }
        //        else
        //        {
                    
        //        }
        //    }

        //}

        if ((m_Joints[m_Positions.Length - 1].position - m_TargetTransform.position).sqrMagnitude < Mathf.Pow(m_Epsilon, 2))
            return;

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
        else //reachable
        {
            Vector3 temp = Vector3.zero;
            int ii = 0;
            while (ii < m_Iterations)
            {

                float[,] lsDeltaOrientation = CalcOrientation();
                for (int i = 0; i < m_Joints.Length - 1; i++)
                {
                    temp = new Vector3(lsDeltaOrientation[i, 0] * Time.fixedDeltaTime, lsDeltaOrientation[i, 1] * Time.fixedDeltaTime, lsDeltaOrientation[i, 2] * Time.fixedDeltaTime); //might have to convert to quaternions
                    Vector3.Dot(temp, m_TargetTransform.position);
                    Quaternion temp_quat = Quaternion.Euler(temp);
                    //Quaternion fromTo = Quaternion.FromToRotation()
                    m_Joints[i].rotation *= temp_quat;

                }

                ii++;

                if ((m_Joints[m_Joints.Length - 1].position - m_TargetTransform.position).sqrMagnitude < Mathf.Pow(m_Epsilon, 2))
                    return;
            }

            return;

        }

        //resolve when out of range
        for (int i = 0; i < m_Positions.Length; i++)
        {
            if (i == m_Positions.Length - 1)
            {
                m_Joints[i].rotation = m_TargetTransform.rotation * Quaternion.Inverse(m_InitialRotTarget) * m_InitialRotJoint[i];
            }
            else
            {
                m_Joints[i].rotation = Quaternion.FromToRotation(m_InitialDir[i], m_Positions[i + 1] - m_Positions[i]) * m_InitialRotJoint[i];
            }

            m_Joints[i].position = m_Positions[i];
        }

    }
    private float[,] CalcOrientation()
    {
        float[,] lsJacobian = CalcJacobian();
        Vector3 lsV = m_TargetTransform.position - m_Joints[m_Joints.Length - 1].position;
        float[,] lsDeltaOrientation = MatrixMult(lsJacobian, lsV);
        return lsDeltaOrientation;
    }
    private float[,] CalcJacobian()
    {
        float[,] lsJacobian = new float[3, m_Joints.Length - 1];

        Vector3 dist = Vector3.zero;
        //no adjustment made to end effector
        for (int i = 0; i < m_Joints.Length - 1; i++)
        {
            dist = m_Joints[m_Joints.Length - 1].transform.position - m_Joints[i].transform.position;
            //Zi x (Pe - Pi)
            Vector3 lsTemp = Vector3.Cross(m_Joints[i].transform.forward.normalized, dist);
            //column major
            lsJacobian[0, i] = lsTemp.x;
            lsJacobian[1, i] = lsTemp.y;
            lsJacobian[2, i] = lsTemp.z;
        }

        //transposed to row major
        return Transpose(lsJacobian);
    }
    private float CalcAngle(Vector3 argAxis, Vector3 argPosA, Vector3 argPosB)
    {
        float lsAngle = Vector3.Angle(argAxis, (argPosA - argPosB).normalized);

        return Vector3.Cross(argAxis, ((argPosA - argPosB).normalized)).z < 0f ? -lsAngle : lsAngle;
    }
    private float[,] Transpose(float[,] argMatrix)
    {
        float[,] lsTransposedMatrix = new float[m_Joints.Length - 1, 3];
        for(int i = 0; i < argMatrix.GetLength(0); i++)
        {
            for(int j = 0; j < argMatrix.GetLength(1); j++)
            {
                lsTransposedMatrix[j, i] = argMatrix[i, j];
            }
        }

        return lsTransposedMatrix;
    }
    private float[,] MatrixMult(float[,] argMatrix, Vector3 argVec)
    {

        float[] jacobian = new float[argMatrix.GetLength(0)];

        for (int i = 0; i < argMatrix.GetLength(0); i++)
        {
            for (int j = 0; j < 3; j++)
            {
                argMatrix[i, j] = argVec[j] * argMatrix[i, j];
            }
        }

        return argMatrix;

        //for(int i = 0; i < argMatrix.GetLength(0); i++)
        //{
        //    jacobian[i] = argMatrix[i, 0] * argMatrix[i, 1] * argMatrix[i, 2];
        //}
        //return jacobian;
    }
}
