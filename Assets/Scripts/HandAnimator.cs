using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VRTK;

public class HandAnimator : MonoBehaviour {

    private Animator m_animation;
    public VRTK_ControllerEvents m_handEvent;
    public VRTK_InteractGrab m_handGrabEvent;

    // Use this for initialization

    //int Idle = Animator.StringToHash("Idle");
    int Point = Animator.StringToHash("Point");
    int GrabLarge = Animator.StringToHash("GrabLarge");
    int GrabSmall = Animator.StringToHash("GrabSmall");
    //int GrabStickUp = Animator.StringToHash("GrabStickUp");
    //int GrabStickFront = Animator.StringToHash("GrabStickFront");
    int ThumbUp = Animator.StringToHash("ThumbUp");
    int Fist = Animator.StringToHash("Fist");
    int Gun = Animator.StringToHash("GrabStickFront");
    int GunShoot = Animator.StringToHash("GrabStickUp");
    //int PushButton = Animator.StringToHash("PushButton");
    //int Spread = Animator.StringToHash("Spread");
    //int MiddleFinger = Animator.StringToHash("MiddleFinger");
    //int Peace = Animator.StringToHash("Peace");
    //int OK = Animator.StringToHash("OK");
    //int Phone = Animator.StringToHash("Phone");
    //int Rock = Animator.StringToHash("Rock");
    int Natural = Animator.StringToHash("Natural");
    int Number3 = Animator.StringToHash("Number3");
    //int Number4 = Animator.StringToHash("Number4");


    public bool TriggerPressed;
    public bool GripPressed;
    public bool ButtonAPressed;
    public bool ButtonBPressed;
    public bool MenuButtonPressed;
    public bool TouchpadPressed;

    private GameObject grabbed;

    private void Start()
    {
        m_animation = GetComponent<Animator>();
        m_animation.SetTrigger(Natural);

        m_handEvent = GetComponentInParent<VRTK_ControllerEvents>();
        m_handGrabEvent = GetComponentInParent<VRTK_InteractGrab>();


        m_handEvent.TriggerPressed += new VRTK.ControllerInteractionEventHandler(DoTriggerPressed);
        m_handEvent.TriggerReleased += new VRTK.ControllerInteractionEventHandler(DoTriggerReleased);

        m_handEvent.GripPressed += new VRTK.ControllerInteractionEventHandler(DoGripPressed);
        m_handEvent.GripReleased += new VRTK.ControllerInteractionEventHandler(DoGripReleased);

        m_handEvent.TouchpadPressed += new VRTK.ControllerInteractionEventHandler(DoTouchPadPressed);
        m_handEvent.TouchpadReleased += new VRTK.ControllerInteractionEventHandler(DoTouchPadReleased);

        m_handEvent.ButtonOnePressed += new VRTK.ControllerInteractionEventHandler(DoButtonAPressed);
        m_handEvent.ButtonOneReleased += new VRTK.ControllerInteractionEventHandler(DoButtonAReleased);

        m_handEvent.ButtonTwoPressed += new VRTK.ControllerInteractionEventHandler(DoButtonBPressed);
        m_handEvent.ButtonTwoReleased += new VRTK.ControllerInteractionEventHandler(DoButtonBReleased);

        m_handGrabEvent.ControllerGrabInteractableObject += new ObjectInteractEventHandler(DoGrabObject);
    }

    

    private void AnimateHand()
    {
        grabbed = m_handGrabEvent.GetGrabbedObject();
        if (TriggerPressed && GripPressed && (ButtonAPressed || ButtonBPressed || TouchpadPressed))
        {
            m_animation.SetTrigger(Fist);
        }
        else if (TriggerPressed && GripPressed)
        {
            if (grabbed == null)
            {
                m_animation.SetTrigger(ThumbUp);
            }
            else if (grabbed.tag == "Gun")
            {
                m_animation.SetTrigger(GunShoot);
            }
            else
            {
                m_animation.SetTrigger(GrabSmall);
            }
        }
        else if (GripPressed)
        {
            if (grabbed == null)
            {
                m_animation.SetTrigger(Point);
            }
            else if (grabbed.tag == "Gun")
            {
                m_animation.SetTrigger(Gun);
            }
            else
            {
                m_animation.SetTrigger(GrabLarge);
            }
        }
        else if (TriggerPressed)
        {
            m_animation.SetTrigger(Number3);
        }
        else
        {
            m_animation.SetTrigger(Natural);
        }
    }

    private void DoGrabObject(object sender, VRTK.ObjectInteractEventArgs e)
    {
        AnimateHand();
    }

    private void DoTriggerPressed(object sender, VRTK.ControllerInteractionEventArgs e)
    {
        TriggerPressed = true;
        AnimateHand();
    }
    private void DoTriggerReleased(object sender, VRTK.ControllerInteractionEventArgs e)
    {
        TriggerPressed = false;
        AnimateHand();

    }
    private void DoButtonAPressed(object sender, VRTK.ControllerInteractionEventArgs e)
    {
        ButtonAPressed = true;
        AnimateHand();

    }
    private void DoButtonAReleased(object sender, VRTK.ControllerInteractionEventArgs e)
    {
        ButtonAPressed = false;
        AnimateHand();

    }
    private void DoButtonBPressed(object sender, VRTK.ControllerInteractionEventArgs e)
    {
        ButtonBPressed = true;
        AnimateHand();

    }
    private void DoButtonBReleased(object sender, VRTK.ControllerInteractionEventArgs e)
    {
        ButtonBPressed = false;
        AnimateHand();

    }
    private void DoGripPressed(object sender, VRTK.ControllerInteractionEventArgs e)
    {
        GripPressed = true;
        AnimateHand();

    }
    private void DoGripReleased(object sender, VRTK.ControllerInteractionEventArgs e)
    {
        GripPressed = false;
        AnimateHand();

    }
    private void DoTouchPadPressed(object sender, VRTK.ControllerInteractionEventArgs e)
    {
        TouchpadPressed = true;
        AnimateHand();

    }
    private void DoTouchPadReleased(object sender, VRTK.ControllerInteractionEventArgs e)
    {
        TouchpadPressed = false;
        AnimateHand();

    }



}
