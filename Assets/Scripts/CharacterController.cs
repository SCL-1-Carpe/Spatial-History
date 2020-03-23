using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MagicLeap;
using UnityEngine.XR.MagicLeap;

public class CharacterController : MonoBehaviour
{
    public float walkSpeed;
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

        if (Mathf.Abs(x) > 0.3f || Mathf.Abs(y) > 0.3f)
        {
            animator.SetBool("isWalk", true);
            Vector3 d = (transform.position - camera.transform.position).normalized;
            d.y *= 0;
            float axis = Vector3.Angle(new Vector3(0, 0, 1), d);

            if (camera.transform.position.x > 0)
                axis *= -1;

            transform.position = new Vector3(transform.position.x, floor.position.y, transform.position.z) + Quaternion.Euler(0, axis, 0) * new Vector3(x, 0, y) * walkSpeed;

            Vector3 diff = transform.position - prePos;
            prePos = transform.position;
            transform.rotation = Quaternion.LookRotation(diff);
        }
        else
            animator.SetBool("isWalk", false);
    }

}
