using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VRTK;

public class HandController : MonoBehaviour {

    //References
    private Animator m_animation;
    public VRTK_ControllerEvents m_handEvent;
    public VRTK_InteractGrab m_handGrabEvent;

    //HandStatus
    public HandButtonsState handButtonsState;
    private GameObject grabbed;

    public List<Vector3> lastPositions;
    public List<Quaternion> lastRotations;

    private void Start()
    {
        m_animation = GetComponent<Animator>();
        lastPositions = new List<Vector3>();
        lastPositions.Add(transform.position);
        lastRotations = new List<Quaternion>();
        lastRotations.Add(transform.rotation);
        m_animation.SetTrigger(Natural);
        SetupEvents();
    }


    private void Update()
    {
        AnimateHand();
        SaveHandPosition();
        CalculateHandState();

    }

    //-----------------------------------------------
    //-------------------Gestures--------------------
    //-----------------------------------------------

    public enum HandState
    {
        Neutral,
        Pointing,
        Fist,
        ThumbsUp,
        FourFingers,
        TBI
    }

    public HandState handState;
    public Transform indexPoint;

    private void SaveHandPosition()
    {
        float threshold = 0.01f;
        if (Vector3.Distance(transform.position, lastPositions[lastPositions.Count-1]) > threshold ||
            Vector3.Distance(transform.rotation.eulerAngles, lastRotations[lastRotations.Count-1].eulerAngles) > threshold)
        {
            lastPositions.Add(transform.position);
            lastRotations.Add(transform.rotation);
            if (lastPositions.Count > 200)
            {
                lastPositions.RemoveAt(0);
                lastRotations.RemoveAt(0);
                //lastPositions.TrimExcess();
            }
        }
    }

    void CalculateHandState()
    {
        if (handButtonsState.TriggerPressed && handButtonsState.GripPressed && (handButtonsState.ButtonAPressed || handButtonsState.ButtonBPressed || handButtonsState.TouchpadPressed))
        {
            handState = HandState.Fist;
        }
        else if (!handButtonsState.TriggerPressed && !handButtonsState.GripPressed && (handButtonsState.ButtonAPressed || handButtonsState.ButtonBPressed || handButtonsState.TouchpadPressed))
        {
            handState = HandState.FourFingers;
        }
        else if (handButtonsState.TriggerPressed && handButtonsState.GripPressed)
        {
            if (grabbed == null)
            {
                handState = HandState.ThumbsUp;
            }
            else
            {
                handState = HandState.TBI;
            }
        }
        else if (handButtonsState.GripPressed)
        {
            if (grabbed == null)
            {
                handState = HandState.Pointing;
            }
            else
            {
                handState = HandState.TBI;
            }
        }
        else if (handButtonsState.TriggerPressed)
        {
            handState = HandState.TBI;
        }
        else
        {
            handState = HandState.Neutral;
        }
    }


    //-----------------------------------------------
    //-------------------Animation-------------------
    //-----------------------------------------------

    //Animator Hashes
    int Idle = Animator.StringToHash("Idle");
    int Point = Animator.StringToHash("Point");
    int GrabLarge = Animator.StringToHash("GrabLarge");
    int GrabSmall = Animator.StringToHash("GrabSmall");
    int GrabStickUp = Animator.StringToHash("GrabStickUp");
    int GrabStickFront = Animator.StringToHash("GrabStickFront");
    int ThumbUp = Animator.StringToHash("ThumbUp");
    int Fist = Animator.StringToHash("Fist");
    int Gun = Animator.StringToHash("GrabStickFront");
    int GunShoot = Animator.StringToHash("GrabStickUp");
    int PushButton = Animator.StringToHash("PushButton");
    int Spread = Animator.StringToHash("Spread");
    int MiddleFinger = Animator.StringToHash("MiddleFinger");
    int Peace = Animator.StringToHash("Peace");
    int OK = Animator.StringToHash("OK");
    int Phone = Animator.StringToHash("Phone");
    int Rock = Animator.StringToHash("Rock");
    int Natural = Animator.StringToHash("Natural");
    int Number3 = Animator.StringToHash("Number3");
    int Number4 = Animator.StringToHash("Number4");


    private void AnimateHand()
    {
        grabbed = m_handGrabEvent.GetGrabbedObject();
        if (handButtonsState.TriggerPressed && handButtonsState.GripPressed && (handButtonsState.ButtonAPressed || handButtonsState.ButtonBPressed || handButtonsState.TouchpadPressed))
        {
            m_animation.SetTrigger(Fist);
        }
        else if (!handButtonsState.TriggerPressed && !handButtonsState.GripPressed && (handButtonsState.ButtonAPressed || handButtonsState.ButtonBPressed || handButtonsState.TouchpadPressed))
        {
            m_animation.SetTrigger(Number4);
        }
        else if (handButtonsState.TriggerPressed && handButtonsState.GripPressed)
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
        else if (handButtonsState.GripPressed)
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
        else if (handButtonsState.TriggerPressed)
        {
            m_animation.SetTrigger(Number3);
        }
        else
        {
            m_animation.SetTrigger(Natural);
        }
    }

    //-----------------------------------------------
    //-------------Event Receivers-------------------
    //-----------------------------------------------
    private void SetupEvents()
    {
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


    private void DoGrabObject(object sender, VRTK.ObjectInteractEventArgs e)
    {
        AnimateHand();
    }

    private void DoTriggerPressed(object sender, VRTK.ControllerInteractionEventArgs e)
    {
        handButtonsState.TriggerPressed = true;
        AnimateHand();
    }
    private void DoTriggerReleased(object sender, VRTK.ControllerInteractionEventArgs e)
    {
        handButtonsState.TriggerPressed = false;
        AnimateHand();

    }
    private void DoButtonAPressed(object sender, VRTK.ControllerInteractionEventArgs e)
    {
        handButtonsState.ButtonAPressed = true;
        AnimateHand();

    }
    private void DoButtonAReleased(object sender, VRTK.ControllerInteractionEventArgs e)
    {
        handButtonsState.ButtonAPressed = false;
        AnimateHand();

    }
    private void DoButtonBPressed(object sender, VRTK.ControllerInteractionEventArgs e)
    {
        handButtonsState.ButtonBPressed = true;
        AnimateHand();

    }
    private void DoButtonBReleased(object sender, VRTK.ControllerInteractionEventArgs e)
    {
        handButtonsState.ButtonBPressed = false;
        AnimateHand();

    }
    private void DoGripPressed(object sender, VRTK.ControllerInteractionEventArgs e)
    {
        handButtonsState.GripPressed = true;
        AnimateHand();

    }
    private void DoGripReleased(object sender, VRTK.ControllerInteractionEventArgs e)
    {
        handButtonsState.GripPressed = false;
        AnimateHand();

    }
    private void DoTouchPadPressed(object sender, VRTK.ControllerInteractionEventArgs e)
    {
        handButtonsState.TouchpadPressed = true;
        AnimateHand();

    }
    private void DoTouchPadReleased(object sender, VRTK.ControllerInteractionEventArgs e)
    {
        handButtonsState.TouchpadPressed = false;
        AnimateHand();

    }



}
