using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveSpider : MonoBehaviour
{
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
        if (Input.GetKey(KeyCode.W))
        {
            float x_ = 10 * Time.deltaTime;
            this.transform.position += this.transform.forward * x_;
            BroadcastMessage("UpdateTargets");
        }
        if (Input.GetKey(KeyCode.R))
        {
            float x_ = 10 * Time.deltaTime;
            this.transform.position += -this.transform.forward * x_;
            BroadcastMessage("UpdateTargets");
        }
        if (Input.GetKey(KeyCode.A))
        {
            float y_ = -180 * Time.deltaTime;
            Vector3 temp = new Vector3(0, y_, 0);
            this.transform.Rotate(temp);
            BroadcastMessage("UpdateTargets");
        }
        if (Input.GetKey(KeyCode.S))
        {
            float y_ = 180 * Time.deltaTime;
            Vector3 temp = new Vector3(0, y_, 0);
            this.transform.Rotate(temp);
            BroadcastMessage("UpdateTargets");
        }
    }
}
