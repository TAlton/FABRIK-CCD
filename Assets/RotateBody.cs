using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class RotateBody : MonoBehaviour
{
    [SerializeField] GameObject FL_Effector;
    [SerializeField] GameObject FR_Effector;
    [SerializeField] GameObject BL_Effector;
    [SerializeField] GameObject BR_Effector;
    private int effector_count = 4;
    private Vector3 avg_vector;
	[SerializeField] private Vector4 avg_quat;
	[SerializeField] private List<GameObject> list_effectors;
    private Vector3 orientation_;
    [SerializeField] public float px = 0;
    [SerializeField] public float py = 0;
    [SerializeField] public float pz = 0;
    [SerializeField] public Targets targets_;
    Vector3[] left_legs;
    Vector3[] right_legs;

    // Start is called before the first frame update
    void Start()
    {
        list_effectors = new List<GameObject>();
        list_effectors.Add(FL_Effector);
        list_effectors.Add(FR_Effector);
        list_effectors.Add(BL_Effector);
        list_effectors.Add(BR_Effector);
        left_legs = new Vector3[] { FL_Effector.transform.position, BL_Effector.transform.position };
        right_legs = new Vector3[] { FR_Effector.transform.position, BR_Effector.transform.position };
        targets_ = this.GetComponent<Targets>();
    }

    // Update is called once per frame
    void Update()
    {
        foreach(GameObject eff in list_effectors)
        {
            py += eff.transform.position.y;
        }

        py /= effector_count;

        this.transform.position = new Vector3(this.transform.position.x, py + 1, this.transform.position.z);
        targets_.UpdateTargets();

        py = 0;
       
    }

}
