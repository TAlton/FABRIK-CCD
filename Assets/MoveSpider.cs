using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveSpider : MonoBehaviour
{
    [SerializeField] KeyCode forward_;
    [SerializeField] KeyCode back_;
    [SerializeField] KeyCode left_;
    [SerializeField] KeyCode right_;
    // Start is called before the first frame update
    void Start()
    {
        
    }
    private void Awake()
    {
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKey(forward_))
        {
            float x_ = 10 * Time.deltaTime;
            this.transform.position += this.transform.forward * x_;
            BroadcastMessage("UpdateTargets");
        }
        if (Input.GetKey(back_))
        {
            float x_ = 10 * Time.deltaTime;
            this.transform.position += -this.transform.forward * x_;
            BroadcastMessage("UpdateTargets");
        }
        if (Input.GetKey(left_))
        {
            float y_ = -180 * Time.deltaTime;
            Vector3 temp = new Vector3(0, y_, 0);
            this.transform.Rotate(temp);
            BroadcastMessage("UpdateTargets");
        }
        if (Input.GetKey(right_))
        {
            float y_ = 180 * Time.deltaTime;
            Vector3 temp = new Vector3(0, y_, 0);
            this.transform.Rotate(temp);
            BroadcastMessage("UpdateTargets");
        }
    }
}
