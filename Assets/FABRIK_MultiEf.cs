using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class FABRIK_MultiEf : MonoBehaviour
{
    [SerializeField] private List<Chain> list_chains_;
    [SerializeField] private FABRIK_Closed_Loop[] fabrik_closed_;
    // Start is called before the first frame update
    void Start()
    {
        foreach (var link in list_chains_)
        {
            link.Init();
        }
        fabrik_closed_ = this.GetComponents<FABRIK_Closed_Loop>();
    }
    private void Awake()
    {

    }
    private void Update()
    {
        
    }

    // Update is called once per frame
    void LateUpdate()
    {
        foreach(var link in list_chains_)
        {
            link.Resolve();
        }

        foreach (var loop in fabrik_closed_)
        {
            if (loop.isActiveAndEnabled)
                loop.PreserveLoop();
        }

    }

    private void OnDrawGizmos()
    {
        //overwriting some gizmos for debugging purposes only no need to change
        foreach(var chain in list_chains_)
        {
            Transform current_transform = chain.end_effector_;
            for (int i = 0; i < chain.chain_length_ && current_transform != null; i++)
            {
                float lsScale = Vector3.Distance(current_transform.position, current_transform.parent.position) * 0.1f;
                Handles.matrix = Matrix4x4.TRS(current_transform.position, Quaternion.FromToRotation(Vector3.up,
                                                                                            current_transform.parent.position - current_transform.position),
                                                                                                new Vector3(lsScale, Vector3.Distance(current_transform.parent.position,
                                                                                                current_transform.position), lsScale));
                Handles.color = Color.green;

                Handles.DrawWireCube(Vector3.up * 0.5f, Vector3.one);

                current_transform = current_transform.parent;
            }
        }
    }
}
[System.Serializable]
struct Chain
{
    [SerializeField] public Transform[] joints_;
    [SerializeField] public int iterations_;
    [SerializeField] public float[] bone_lengths_;
    [SerializeField] public Vector3[] positions_;
    [SerializeField] public float epsilon_;
    [SerializeField] Quaternion[] initial_rot_joint;
    [SerializeField] Quaternion initial_rot_tar_;
    [SerializeField] Quaternion initial_rot_root_;
    [SerializeField] public uint chain_length_;
    [SerializeField] private float limb_length_;
    [SerializeField] private Vector3[] initial_dir_;
    [SerializeField] private Transform target_transform_;
    [SerializeField] private Transform pole_transform_;
    [SerializeField] public Transform end_effector_;
    public void Init()
    {

        joints_ = new Transform[chain_length_ + 1];
        positions_ = new Vector3[chain_length_ + 1];
        bone_lengths_ = new float[chain_length_];
        limb_length_ = 0;
        initial_dir_ = new Vector3[chain_length_ + 1];
        initial_rot_joint = new Quaternion[chain_length_ + 1];
        initial_rot_tar_ = target_transform_.rotation;
        Transform lsCurrentTransform = end_effector_;

        for (int i = joints_.Length - 1; i >= 0; i--)
        {
            joints_[i] = lsCurrentTransform;
            initial_rot_joint[i] = lsCurrentTransform.rotation;

            if (joints_.Length - 1 == i)
            {
                //leaf node
                initial_dir_[i] = target_transform_.position - lsCurrentTransform.position;
            }
            else
            {
                initial_dir_[i] = joints_[i + 1].position - lsCurrentTransform.position;
                bone_lengths_[i] = (joints_[i + 1].position - lsCurrentTransform.position).magnitude;
                limb_length_ += bone_lengths_[i];
            }

            lsCurrentTransform = lsCurrentTransform.parent;

        }
    }

    public void Resolve()
    {
        if (null == target_transform_) return;
        if (bone_lengths_.Length != chain_length_) Init();

        for (int i = 0; i < joints_.Length; i++)
        {
            positions_[i] = joints_[i].position;
        }

        Quaternion lsRootRot = /*(joints_[0].parent != null) ? joints_[0].parent.rotation :*/ Quaternion.identity;
        Quaternion lsRotDelta = lsRootRot * Quaternion.Inverse(initial_rot_root_);

        //checks if the target is reachable by the limb
        if ((target_transform_.position - joints_[0].position).sqrMagnitude >= Mathf.Pow(limb_length_, 2))
        {
            //not reachable
            Vector3 lsDirection = (target_transform_.position - positions_[0]).normalized;

            for (int i = 1; i < positions_.Length; i++)
            {
                //the limb reaches for the target ending in a straight limb
                positions_[i] = positions_[i - 1] + lsDirection * bone_lengths_[i - 1];
            }

        }
        else
        {
            //reachable
            for (int i = 0; i < iterations_; i++)
            {
                //backwards iteration
                for (int ii = positions_.Length - 1; ii > 0; ii--)
                {
                    if (ii == positions_.Length - 1)
                    {
                        positions_[ii] = target_transform_.position;
                    }
                    else
                    {
                        //checking that the distances of joints are constrained to bone length
                        positions_[ii] = positions_[ii + 1] + (positions_[ii] - positions_[ii + 1]).normalized * bone_lengths_[ii];
                    }
                }
                //forward iteration
                for (int ii = 1; ii < positions_.Length; ii++)
                {
                    positions_[ii] = positions_[ii - 1] + (positions_[ii] - positions_[ii - 1]).normalized * bone_lengths_[ii - 1];
                }
                //if distance from target and end effector is within our delta we break
                if ((positions_[positions_.Length - 1] - target_transform_.position).sqrMagnitude < Mathf.Pow(epsilon_, 2)) break;
            }
        }

        //pole
        if (pole_transform_ != null)
        {
            //creates an angle axis for the joints based on the plane from the pole
            for (int i = 1; i < positions_.Length - 1; i++)
            {
                Plane lsPlane = new Plane(positions_[i + 1] - positions_[i - 1], positions_[i - 1]);
                Vector3 lsProjectedPole = lsPlane.ClosestPointOnPlane(pole_transform_.position);
                Vector3 lsProjectedJoint = lsPlane.ClosestPointOnPlane(positions_[i]);
                float lsAngle = Vector3.SignedAngle(lsProjectedJoint - positions_[i - 1], lsProjectedPole - positions_[i - 1], lsPlane.normal);
                positions_[i] = Quaternion.AngleAxis(lsAngle, lsPlane.normal) * (positions_[i] - positions_[i - 1]) + positions_[i - 1];

            }
        }

        for (int i = 0; i < positions_.Length; i++)
        {
            joints_[i].position = positions_[i];
            if (i == positions_.Length - 1)
            {
                //joints_[i].localRotation = target_transform_.rotation * Quaternion.Inverse(initial_rot_tar_) * initial_rot_joint[i];
            }
            else
            {
               //joints_[i].localRotation = Quaternion.FromToRotation(initial_dir_[i], positions_[i + 1] - positions_[i]) * initial_rot_joint[i];
            }
        }
    }
}