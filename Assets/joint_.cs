using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class joint_ : MonoBehaviour
{
    [SerializeField] public Vector3 rotation_freedom_positive_;
    [SerializeField] public Vector3 rotation_freedom_negative_;
    [SerializeField] public Vector3 translation_freedom_positive;
    [SerializeField] public Vector3 translation_freedom_negative;
    [SerializeField] public Vector3 origin_position;
    [SerializeField] public Vector3 origin_rotation;
    // Start is called before the first frame update
    void Start()
    {
        
    }
    private void Awake()
    {
        origin_position = this.transform.position;
        origin_rotation = this.transform.rotation.eulerAngles;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
