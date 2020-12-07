using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Ubik.XR;

public class HandAnimationController : MonoBehaviour
{
    HandController leftHand;
    HandController rightHand;

    Animator leftHandAnimator;
    Animator rightHandAnimator;

    // Start is called before the first frame update
    void Start()
    {
        GameObject playerObject = GameObject.FindObjectOfType<PlayerController>().gameObject;
        rightHand = playerObject.transform.Find("Right Hand").gameObject.GetComponent<HandController>();
        leftHand = playerObject.transform.Find("Left Hand").gameObject.GetComponent<HandController>();

        leftHand.GripPress.AddListener(OnLeftGrasp);
        rightHand.GripPress.AddListener(OnRightGrasp);

        leftHandAnimator = gameObject.transform.GetChild(0).Find("LeftHand").gameObject.GetComponent<Animator>();
        rightHandAnimator = gameObject.transform.GetChild(0).Find("InvertedLeftHand").gameObject.GetComponent<Animator>();
        leftHandAnimator.Play("IdleAnimation");
        rightHandAnimator.Play("IdleAnimation");
    }

    void OnLeftGrasp(bool grasped)
    {

        leftHandAnimator.SetBool("IsGrabbing", grasped);

        Debug.Log("onleftGrasp");

    }

    void OnRightGrasp(bool grasped)
    {
        rightHandAnimator.SetBool("IsGrabbing", grasped);
        Debug.Log("onRightGrasp");
    }
}
