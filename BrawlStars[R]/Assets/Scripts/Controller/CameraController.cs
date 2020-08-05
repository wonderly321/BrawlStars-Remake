using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField]  
    public Vector3 offset;
    public float smooth;
    public GameObject targetPlayer;
    
    void start()
    {
        targetPlayer = GameObject.FindWithTag("myself");
    }
    // Update is called once per frame
    void Update()
    {
        if(targetPlayer != null)
        {
            transform.position = Vector3.Lerp(transform.position, targetPlayer.transform.position - offset, smooth * Time.deltaTime);
        }
        else
        {
            targetPlayer = GameObject.FindWithTag("myself");
        }
    }
}
