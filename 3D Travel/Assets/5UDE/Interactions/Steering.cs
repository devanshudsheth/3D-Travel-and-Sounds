/*
Copyright ©2017. The University of Texas at Dallas. All Rights Reserved. 

Permission to use, copy, modify, and distribute this software and its documentation for 
educational, research, and not-for-profit purposes, without fee and without a signed 
licensing agreement, is hereby granted, provided that the above copyright notice, this 
paragraph and the following two paragraphs appear in all copies, modifications, and 
distributions. 

Contact The Office of Technology Commercialization, The University of Texas at Dallas, 
800 W. Campbell Road (AD15), Richardson, Texas 75080-3021, (972) 883-4558, 
otc@utdallas.edu, https://research.utdallas.edu/otc for commercial licensing opportunities.

IN NO EVENT SHALL THE UNIVERSITY OF TEXAS AT DALLAS BE LIABLE TO ANY PARTY FOR DIRECT, 
INDIRECT, SPECIAL, INCIDENTAL, OR CONSEQUENTIAL DAMAGES, INCLUDING LOST PROFITS, ARISING 
OUT OF THE USE OF THIS SOFTWARE AND ITS DOCUMENTATION, EVEN IF THE UNIVERSITY OF TEXAS AT 
DALLAS HAS BEEN ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

THE UNIVERSITY OF TEXAS AT DALLAS SPECIFICALLY DISCLAIMS ANY WARRANTIES, INCLUDING, BUT 
NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR 
PURPOSE. THE SOFTWARE AND ACCOMPANYING DOCUMENTATION, IF ANY, PROVIDED HEREUNDER IS 
PROVIDED "AS IS". THE UNIVERSITY OF TEXAS AT DALLAS HAS NO OBLIGATION TO PROVIDE 
MAINTENANCE, SUPPORT, UPDATES, ENHANCEMENTS, OR MODIFICATIONS.
*/

using UnityEngine;
using System.Collections;
using System;

public class Steering : MonoBehaviour
{

    // Enumerate the states of steering
    public enum SteeringState
    {
        NotSteering,
        SteeringForward,
        SteeringBackward,
        Hop,
        Teleport
    };

    // Inspector parameters
    [Tooltip("The tracking device used to determine absolute direction for steering.")]
    public CommonTracker tracker;

    [Tooltip("The controller joystick used to determine relative direction (forward/backward) and speed.")]
    public CommonAxis joystick;

    [Tooltip("A button required to be pressed to activate steering.")]
    public CommonButton btnTouchpad;

    [Tooltip("A button required to be pressed to activate hopping.")]
    public CommonButton btnTrigger;


    [Tooltip("A button required to be pressed to teleport.")]
    public CommonButton btnTeleport;


    [Tooltip("The space that is translated by this interaction. Usually set to the physical tracking space.")]
    public CommonSpace space;

    [Tooltip("The median speed for movement expressed in meters per second.")]
    public float speed = 1.0f;

    // Private interaction variables
    private SteeringState state;
    private bool hopFlag;
    private bool teleportFlag;

    // World coordinates
    private const float TOP = -8.0f;
    private const float X_MID = -3.5f;
    private const float BOTTOM = 4.0f;
    private const float LEFT = -6.0f;
    private const float Z_MID = 2.0f;
    private const float RIGHT = 9.0f;

    // Vectors for room positions
    private Vector3 office;
    private Vector3 breakroom;
    private Vector3 bathroom;

    // Called at the end of the program initialization
    void Start()
    {

        // Set initial steering state to not steering
        state = SteeringState.NotSteering;
        hopFlag = true;
        teleportFlag = true;

        office = new Vector3(0, 0, 0);
        breakroom = new Vector3(0, 0, 5);
        bathroom = new Vector3(-5, 0, 5);
    }

    // FixedUpdate is not called every graphical frame but rather every physics frame
    void FixedUpdate()
    {
        // If state is not steering
        if (state == SteeringState.NotSteering)
        {

            ResetHeight();
            // If the joystick is pressed forward and the btnTouchpad is pressed
            if (joystick.GetAxis().y > 0.0f && btnTouchpad.GetPress())
            {

                // Change state to steering forward
                state = SteeringState.SteeringForward;
            }

            // If the joystick is pressed backward and the btnTouchpad is pressed
            else if (joystick.GetAxis().y < 0.0f && btnTouchpad.GetPress())
            {

                // Change state to steering backward
                state = SteeringState.SteeringBackward;
            }
            else if (btnTrigger.GetPress())
            {
                state = SteeringState.Hop;
            }

            else if (btnTeleport.GetPress())
            {
                state = SteeringState.Teleport;
            }
            // Process current not steering state
            else
            {

                // Nothing to do for not steering
            }
        }

        // If state is steering forward
        else if (state == SteeringState.SteeringForward)
        {

            // If the btnTouchpad is not pressed
            if (!btnTouchpad.GetPress())
            {

                // Change state to not steering 
                state = SteeringState.NotSteering;
            }

            // If the joystick is pressed backward and the btnTouchpad is pressed
            else if (joystick.GetAxis().y < 0.0f && btnTouchpad.GetPress())
            {

                // Change state to steering backward
                state = SteeringState.SteeringBackward;
            }

            // Process current steering forward state
            else
            {

                // Added to avoid flying up or down while moving
                Vector3 direction = tracker.transform.forward;
                direction.y = 0.0f;

                // Translate the space based on the tracker's absolute forward direction and the joystick's forward value
                space.transform.position += joystick.GetAxis().y * direction * speed * Time.deltaTime;
            }
        }

        // If state is steering backward
        else if (state == SteeringState.SteeringBackward)
        {

            // If the btnTouchpad is not pressed
            if (!btnTouchpad.GetPress())
            {

                // Change state to not steering 
                state = SteeringState.NotSteering;
            }

            // If the joystick is pressed forward and the btnTouchpad is pressed
            else if (joystick.GetAxis().y > 0.0f && btnTouchpad.GetPress())
            {

                // Change state to steering forward
                state = SteeringState.SteeringForward;
            }

            // Process current steering backward state
            else
            {

                // Added to avoid flying up or down while moving
                Vector3 direction = tracker.transform.forward;
                direction.y = 0.0f;

                // Translate the space based on the tracker's absolute forward direction and the joystick's backward value
                space.transform.position += joystick.GetAxis().y * direction * speed * Time.deltaTime;
            }
        }

        // if state is hop
        else if (state == SteeringState.Hop)
        {

            // If the btnTrigger is not pressed
            if (!btnTrigger.GetPress())
            {
                state = SteeringState.NotSteering;
            }
            else
            {
                Hop();
            }
        }

        // if state is teleport
        else if (state == SteeringState.Teleport)
        {

            // If the btnTeleport is not pressed
            if (!btnTeleport.GetPress())
            {
                // Return state to NotSteering
                state = SteeringState.NotSteering;
                // Set teleportFlag to true, to let the user teleport again.
                teleportFlag = true;
            }
            else
            {
                // ensures that the user can teleport only one level when they press the trigger
                if (teleportFlag)
                {
                    // move to another room
                    Teleport();
                }
            }
        }



    }

    // brings the user back to ground
    private void ResetHeight()
    {
        Vector3 direction = space.transform.position;
        direction.y = 0.0f;
        space.transform.position = direction;
    }

    // makes the user hop in place
    private void Hop()
    {
        /* 
         * if true user can start the upward movement again. 
         * ensures the user returns to ground before moving up again.
         */
        if (hopFlag)
        {
            space.transform.Translate(Vector3.up * Time.deltaTime);
            if (space.transform.position.y >= 0.5)
            {
                hopFlag = !hopFlag;
            }
        }
        // user continues to come down till they hit the ground
        else
        {
            space.transform.Translate(Vector3.down * Time.deltaTime);
            if (space.transform.position.y <= 0.0)
            {
                hopFlag = !hopFlag;
            }
        }
    }

    // moving from one room to another
    private void Teleport()
    {
        // coordinates of the user's position
        float z = space.transform.position.z;
        float x = space.transform.position.x;

        // if office, move to the breakroom
        if (IsOffice(x, z))
        {
            space.transform.position = breakroom;
        }
        // if breakroom, move to bathroom
        else if(IsBreakroom(x, z))
        {
            space.transform.position = bathroom;
        }
        // if bathroom, move to office
        else
        {
            space.transform.position = office;
        }
        // flag to ensure only one teleportation per click
        teleportFlag = !teleportFlag;
    }

    // checks if the user is in office
    private bool IsOffice(float x, float z)
    {
        if(z > LEFT && z < Z_MID && x < BOTTOM && x > TOP)
        {
            return true;
        }
        return false;
    }


    // checks if the user is in breakroom
    private bool IsBreakroom(float x, float z)
    {
        if (z > Z_MID && z < RIGHT && x < BOTTOM && x > X_MID)
        {
            return true;
        }
        return false;
    }
}