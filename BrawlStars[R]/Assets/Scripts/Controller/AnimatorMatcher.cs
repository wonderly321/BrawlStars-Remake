using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimatorMatcher : MonoBehaviour
{
    Animator anim;
    PlayerController control;

    void Start()
    {
        anim = GetComponent<Animator>();
        control = GetComponentInParent<PlayerController>();
    }

    private void OnAnimatorMove()   //It updates every frame when animator's animations in play.
    {
        if (control.canMove)
            return;

        // if (!control.onGround)
        //     return;

        control.rigid.drag = 0;
        float multiplier = 3f;

        Vector3 dPosition = anim.deltaPosition;   //storing delta positin of active model's position.         

        dPosition.y = 0f;   //flatten the Y (height) value of root animations.

        Vector3 vPosition = (dPosition * multiplier) / Time.fixedDeltaTime;     //defines the vector 3 value for the velocity.      

        control.rigid.velocity = vPosition; //This will move the root gameObject for matching active model's position.

    }
}