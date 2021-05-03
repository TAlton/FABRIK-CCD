using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum TarIndex { FL = 0, FR, BL, BR };

public class Targets : MonoBehaviour
{
    [SerializeField] public GameObject front_left_tar;
    [SerializeField] public GameObject front_right_tar;
    [SerializeField] public GameObject back_left_tar;
    [SerializeField] public GameObject back_right_tar;
    [SerializeField] public GameObject front_left_temp;
    [SerializeField] public GameObject front_right_temp;
    [SerializeField] public GameObject back_left_temp;
    [SerializeField] public GameObject back_right_temp;
    [SerializeField] public GameObject FL_Effector;
    [SerializeField] public GameObject FR_Effector;
    [SerializeField] public GameObject BL_Effector;
    [SerializeField] public GameObject BR_Effector;
    [SerializeField] private List<GameObject> list_targets_;
    [SerializeField] private List<GameObject> list_effectors_;
    [SerializeField] private List<GameObject> list_temps_;
    [SerializeField] public float epsilon = 1.5f;
    [SerializeField] private Vector3 offset_ = new Vector3(0, 1f, 0);
    private int layer_;
    // Start is called before the first frame update
    void Start()
    {
        list_targets_.Add(front_left_tar);
        list_targets_.Add(front_right_tar);
        list_targets_.Add(back_left_tar);
        list_targets_.Add(back_right_tar);
        list_effectors_.Add(FL_Effector);
        list_effectors_.Add(FR_Effector);
        list_effectors_.Add(BL_Effector);
        list_effectors_.Add(BR_Effector);
        list_temps_.Add(front_left_temp);
        list_temps_.Add(front_right_temp);
        list_temps_.Add(back_left_temp);
        list_temps_.Add(back_right_temp);

        MoveTemps();
        for (int i = 0; i < list_targets_.Count; i++)
        {
            list_targets_[i].transform.position = list_temps_[i].transform.position;
        }
    }
    private void Awake()
    {
        layer_ = (1 << LayerMask.NameToLayer("floor"));
        list_targets_ = new List<GameObject>();
        
    }
    // Update is called once per frame
    void Update()
    {
        MoveTemps();
    }
    public void UpdateTargets()
    {
        for (int i = 0; i < list_targets_.Count; i++)
        {
            if (Vector3.Distance(list_targets_[0].transform.position, list_temps_[0].transform.position) >= epsilon)
            {
                UpdateTarget(TarIndex.FL);
                UpdateTarget(TarIndex.BR);
            } else if(Vector3.Distance(list_targets_[1].transform.position, list_temps_[1].transform.position) >= epsilon)
            {
                UpdateTarget(TarIndex.FR);
                UpdateTarget(TarIndex.BL);
            }
        }
    }
    private void MoveTemps()
    {
        foreach (var obj in list_temps_)
        {
            RaycastHit ray_;
            if (Physics.Raycast((obj.transform.position + offset_), transform.TransformDirection(Vector3.down), out ray_, 10f, layer_))
            {
                obj.transform.position = ray_.point;
                obj.transform.rotation = Quaternion.FromToRotation(obj.transform.up, ray_.normal) * obj.transform.rotation;
            }
        }
    }
    private void UpdateTarget(TarIndex arg_index)
    {
        list_targets_[(int)arg_index].transform.position = list_temps_[(int)arg_index].transform.position;
        list_targets_[(int)arg_index].transform.rotation = list_temps_[(int)arg_index].transform.rotation;
    }
}
