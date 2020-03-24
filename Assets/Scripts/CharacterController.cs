using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MagicLeap;
using UnityEngine.XR.MagicLeap;

public class CharacterController : MonoBehaviour
{
    public float walkSpeed = 0.005f;
    public float runSpeed = 0.01f;
    private Animator animator;
    [SerializeField] ControllerConnectionHandler controller;

    [SerializeField] GameObject camera;

    public Transform floor;

    private Vector3 prePos;

    void Start()
    {
        animator = GetComponent<Animator>();
        prePos = transform.position;
    }


    void Update()
    {
        float x = controller.ConnectedController.Touch1PosAndForce.x;
        float y = controller.ConnectedController.Touch1PosAndForce.y;

        if (x*x + y*y > 0.3f*0.3f)
        {
            float speed = 0;
            if (x * x + y * y > 0.7f * 0.7f)
            {
                animator.SetBool("isWalking", false);
                animator.SetBool("isRunning", true);
                speed = runSpeed;
            }
            else
            {
                animator.SetBool("isRunning", false);
                animator.SetBool("isWalking", true);
                speed = walkSpeed;
            }

            Vector3 d = (prePos - camera.transform.position).normalized;
            d.y *= 0;

            float axis = Vector3.Angle(new Vector3(0, 0, 1), d);
            if (camera.transform.position.x > 0) axis *= -1;

            transform.position = new Vector3(prePos.x, floor.position.y, prePos.z) + Quaternion.Euler(0, axis, 0) * new Vector3(x, 0, y) * speed;

            Vector3 diff = transform.position - prePos;
            transform.rotation = Quaternion.LookRotation(diff);

            prePos = transform.position;
        }
        else
        {
            animator.SetBool("isWalking", false);
            animator.SetBool("isWalking", false);
        }
    }

}
