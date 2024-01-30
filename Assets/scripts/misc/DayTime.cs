using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DayTime : MonoBehaviour
{
    public GameObject sun;
    public float speed;
    private void Start()
    {
        sun = gameObject;
    }
    private void Update()
    {
        var rot = sun.transform.eulerAngles;
        sun.transform.eulerAngles = new Vector3(rot.x,(rot.y+speed*Time.deltaTime),rot.z);
    }
}
