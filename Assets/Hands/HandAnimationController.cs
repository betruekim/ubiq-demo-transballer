using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Ubiq.XR;

public class HandAnimationController : MonoBehaviour
{
    HandController leftHand;
    HandController rightHand;

    Animator leftHandAnimator;
    Animator rightHandAnimator;

    // Start is called before the first frame update
    void Start()
    {
        foreach (var item in XRPlayerController.Singleton.handControllers)
        {
            if(item.Left)
            {
                item.GripPress.AddListener(OnLeftGrasp);
            }
            if(item.Right)
            {
                item.GripPress.AddListener(OnRightGrasp);
            }
        }

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
