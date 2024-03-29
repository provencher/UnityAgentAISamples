﻿using UnityEngine;
using UnityEngine.Networking;

namespace Complete
{
    public class TankMovement : NetworkBehaviour
    {
        public int m_PlayerNumber = 1;              // Used to identify which tank belongs to which player.  This is set by this tank's manager.
        public float m_Speed = 12f;                 // How fast the tank moves forward and back.
        public float m_TurnSpeed = 180f;            // How fast the tank turns in degrees per second.
        public AudioSource m_MovementAudio;         // Reference to the audio source used to play engine sounds. NB: different to the shooting audio source.
        public AudioClip m_EngineIdling;            // Audio to play when the tank isn't moving.
        public AudioClip m_EngineDriving;           // Audio to play when the tank is moving.
		public float m_PitchRange = 0.2f;           // The amount by which the pitch of the engine noises can vary.


        private string m_MovementAxisName;          // The name of the input axis for moving forward and back.
        private string m_TurnAxisName;              // The name of the input axis for turning.
        private Rigidbody m_Rigidbody;              // Reference used to move the tank.
        private float m_MovementInputValue;         // The current value of the movement input.
        private float m_TurnInputValue;             // The current value of the turn input.
        private float m_OriginalPitch;              // The pitch of the audio source at the start of the scene.
        private Rigidbody2D m_Rigidbody2D;        

        public bool is2D = false;

        public GameObject Renderers;

      
        private void Awake ()
        {
            if (GetComponent<Rigidbody>())
            {
                m_Rigidbody = GetComponent<Rigidbody>();
            }
            else
            {
                m_Rigidbody2D = GetComponent<Rigidbody2D>();
                is2D = true;
            }
        }
        public override void OnStartLocalPlayer()
        {
            //GetComponent<MeshRenderer>().material.color = Color.blue;
            Initialize();
            if (Renderers)
            {
                foreach(Transform child in Renderers.transform)
                {
                    if (child.GetComponent<MeshRenderer>())
                    {
                        child.GetComponent<MeshRenderer>().material.color = Color.blue;
                    }
                }
            }

            foreach (var l in GameObject.FindGameObjectsWithTag("Landmark"))
            {                
                l.GetComponent<MeshRenderer>().enabled = false;
            }            
        }

        private void OnEnable()
        {
            if (!is2D)
            {
                // When the tank is turned on, make sure it's not kinematic.
                m_Rigidbody.isKinematic = false;
            }
            else
            {
                m_Rigidbody2D.isKinematic = false;
            }
            

            // Also reset the input values.
            m_MovementInputValue = 0f;
            m_TurnInputValue = 0f;
        }


        private void OnDisable ()
        {
            if (!is2D)
            {
                // When the tank is turned off, set it to kinematic so it stops moving.
                m_Rigidbody.isKinematic = true;
            }
            else
            {
                m_Rigidbody2D.isKinematic = true;
            }           
        }

        bool initialized = false;
        public void Initialize()
        {
            initialized = true;
            // The axes names are based on player number.
            m_MovementAxisName = "Vertical" + m_PlayerNumber;
            m_TurnAxisName = "Horizontal" + m_PlayerNumber;

            // Store the original pitch of the audio source.
            m_OriginalPitch = m_MovementAudio.pitch;

            if (FindObjectOfType<TagGameLogic>())
            {
                //FindObjectOfType<TagGameLogic>().SetupGame();
            }
            transform.rotation = Quaternion.Euler(270, 0, 0);            
        }


        private void Update ()
        {
            if (!isLocalPlayer)
            {
                return;
            }

            if (initialized)
            {
                // Store the value of both input axes.
                m_MovementInputValue = Input.GetAxis(m_MovementAxisName);
                m_TurnInputValue = Input.GetAxis(m_TurnAxisName);

                EngineAudio();
            }           
        }


        private void EngineAudio ()
        {
            // If there is no input (the tank is stationary)...
            if (Mathf.Abs (m_MovementInputValue) < 0.1f && Mathf.Abs (m_TurnInputValue) < 0.1f)
            {
                // ... and if the audio source is currently playing the driving clip...
                if (m_MovementAudio.clip == m_EngineDriving)
                {
                    // ... change the clip to idling and play it.
                    m_MovementAudio.clip = m_EngineIdling;
                    m_MovementAudio.pitch = Random.Range (m_OriginalPitch - m_PitchRange, m_OriginalPitch + m_PitchRange);
                    m_MovementAudio.Play ();
                }
            }
            else
            {
                // Otherwise if the tank is moving and if the idling clip is currently playing...
                if (m_MovementAudio.clip == m_EngineIdling)
                {
                    // ... change the clip to driving and play.
                    m_MovementAudio.clip = m_EngineDriving;
                    m_MovementAudio.pitch = Random.Range(m_OriginalPitch - m_PitchRange, m_OriginalPitch + m_PitchRange);
                    m_MovementAudio.Play();
                }
            }
        }


        private void FixedUpdate ()
        {
            // Adjust the rigidbodies position and orientation in FixedUpdate.
            Move ();
            Turn ();
        }


        private void Move ()
        {
            // Create a vector in the direction the tank is facing with a magnitude based on the input, speed and the time between frames.
            Vector3 movement = transform.forward * m_MovementInputValue * m_Speed * transform.localScale.x * Time.deltaTime;

            if (!is2D)
            {
                // Apply this movement to the rigidbody's position.
                m_Rigidbody.MovePosition(m_Rigidbody.position + movement);
            }
            else
            {
                m_Rigidbody2D.MovePosition(m_Rigidbody2D.position + (Vector2)movement);
            }            
        }


        private void Turn ()
        {
            // Determine the number of degrees to be turned based on the input, speed and time between frames.
            float turn = m_TurnInputValue * m_TurnSpeed * Time.deltaTime;

            // Make this into a rotation in the y axis.
            Quaternion turnRotation = Quaternion.Euler (0f, turn, 0f);

            if (!is2D)
            {
                // Apply this rotation to the rigidbody's rotation.
                m_Rigidbody.MoveRotation(m_Rigidbody.rotation * turnRotation);
            }
            else
            {
                transform.rotation *= turnRotation;
            }
        }
    }
}