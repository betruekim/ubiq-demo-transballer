using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bob : MonoBehaviour
{
    public float bobSpeedMin;
    public float bobSpeedMax;
    public float bobHeightMin;
    public float bobHeightMax;

    private float y0;
    private float bobHeight;
    private float bobSpeed;
    void Start()
    {
        bobHeight = Random.Range(bobHeightMin, bobHeightMax);
        bobSpeed = Random.Range(bobSpeedMin, bobSpeedMax);

        y0 = transform.position.y;
    }

    void Update()
    {
        float newY = y0 + bobHeight * Mathf.Sin(bobSpeed * Time.time);
        transform.position = new Vector3(transform.position.x, newY, transform.position.z);
    }
}
