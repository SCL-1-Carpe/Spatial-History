using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MagicLeap;
using UnityEngine.XR.MagicLeap;
using Utils;

public class CharacterController : MonoBehaviour
{
    public float walkSpeed = 0.005f;
    private Animator animator;
    [SerializeField] ControllerConnectionHandler controller;

    [SerializeField] GameObject camera;

    public Transform floor;

    private Vector3 prePos;

    private bool isWaving = false;

    void Start()
    {
        animator = GetComponent<Animator>();
        prePos = transform.position;
    }


    void Update()
    {
        float x = controller.ConnectedController.Touch1PosAndForce.x;
        float y = controller.ConnectedController.Touch1PosAndForce.y;

        if (Utility.Distance2D_GT(x, y, 0.3f) && !isWaving)
        {
            animator.SetBool("isWalking", true);

            Vector3 d = (transform.position - camera.transform.position).normalized;
            d.y *= 0;

            float axis = Vector3.Angle(Vector3.forward, d);
            if (camera.transform.position.x > 0) axis *= -1;

            transform.position = new Vector3(transform.position.x, floor.position.y, transform.position.z) + Quaternion.Euler(0, axis, 0) * new Vector3(x, 0, y) * walkSpeed;

            Vector3 diff = transform.position - prePos;
            transform.rotation = Quaternion.LookRotation(diff);

            prePos = transform.position;
        }
        else
        {
            animator.SetBool("isWalking", false);
        }
    }

    public IEnumerator Wave()
    {
        animator.SetTrigger("isWaving");
        isWaving = true;
        yield return new WaitForSeconds(4f);
        isWaving = false;
    }

}
