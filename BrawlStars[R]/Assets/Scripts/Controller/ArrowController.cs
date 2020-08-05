using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArrowController : MonoBehaviour
{
    public bool isMine = false;
    public bool isCombo = false;
    public Vector3 shooterPos;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }
    void OnTriggerEnter(Collider collision)
    {
        var collider = collision.gameObject;
        if (collider.tag == "wall")
        {
            if(isCombo){
                if(isMine) {
                    GameManager.CallNet("destroy_wall", new object[]{GameManager.user_id, collider.transform.position.x, collider.transform.position.z});
                }
                Destroy(collider);

            }
            Destroy(gameObject);
        }
        var shootDir = transform.position - shooterPos;
        if (collision.gameObject.tag == "enermy")
        {
            if (!isMine) return; //not myself arrow no effects
            if (isCombo)
            {
                var heroController = collision.gameObject.GetComponent<PlayerController>();
                var targetPos = transform.position + shootDir.normalized * 2;
                // heroController.ExecuteRepel(transform.position + shootDir.normalized * 2);
                GameManager.CallNet("combo_damage", new object[]{
                    GameManager.user_id,
                    collision.gameObject.GetComponent<PlayerController>().user_id,
                    GameManager.combo_damage,
                    targetPos.x,
                    targetPos.z
                });
            }
            else
            {
                GameManager.CallNet("normal_damage", new object[]{
                    GameManager.user_id,
                    collision.gameObject.GetComponent<PlayerController>().user_id,
                    GameManager.normal_damage
                });
            }
            Destroy(gameObject);
        }
    }
}
