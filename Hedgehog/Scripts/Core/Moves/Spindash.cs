﻿using UnityEngine;

namespace Hedgehog.Core.Moves
{
    public class Spindash : Move
    {
        /// <summary>
        /// Input for charging.
        /// </summary>
        [SerializeField]
        [Tooltip("Input for charging.")]
        public string ChargeButton;

        /// <summary>
        /// The lowest speed possible after releasing, in units per second.
        /// </summary>
        [SerializeField, PhysicsFoldout]
        [Tooltip("The lowest speed possible after releasing, in units per second.")]
        public float BasePower;

        /// <summary>
        /// The bonus speed given for charging up once, in units per second.
        /// </summary>
        [SerializeField, PhysicsFoldout]
        [Tooltip("The bonus speed given for charging up once, in units per second.")]
        public float ChargePower;

        /// <summary>
        /// Decay rate of the bonus speed from charging up. Current charge decreases by 
        /// itself times ChargePowerDecay each second.
        /// </summary>
        [SerializeField, PhysicsFoldout]
        [Tooltip("Decay rate of the bonus speed from charging up. Current charge decreases by " +
                 "itself times ChargePowerDecay each second.")]
        public float ChargePowerDecay;

        /// <summary>
        /// Maximum total bonus speed from charging up, in units per second.
        /// </summary>
        [SerializeField, PhysicsFoldout]
        [Tooltip("Maximum total bonus speed from charging up, in units per second.")]
        public float MaxChargePower;

        /// <summary>
        /// Current charge power, or zero if not currently active.
        /// </summary>
        public float CurrentChargePower;

        /// <summary>
        /// An audio clip to play when the spindash is charged.
        /// </summary>
        [SoundFoldout]
        [Tooltip("An audio clip to play when the spindash is charged.")]
        public AudioClip ChargeSound;

        /// <summary>
        /// How many charges it takes to go from minimum pitch to maximum pitch.
        /// </summary>
        [SoundFoldout]
        [Tooltip("How many charges it takes to go from minimum pitch to maximum pitch.")]
        public int ChargePitchSteps;

        /// <summary>
        /// The minimum pitch of the charge sound.
        /// </summary>
        [SoundFoldout]
        [Tooltip("The minimum pitch of the charge sound.")]
        public float ChargePitchMin;

        /// <summary>
        /// The maximum pitch of the charge sound.
        /// </summary>
        [SoundFoldout]
        [Tooltip("The maximum pitch of the charge sound.")]
        public float ChargePitchMax;

        private AudioSource ChargeAudioSource;

        public override void Reset()
        {
            base.Reset();

            ChargeButton = "Jump";

            ChargePower = 0.6f;
            MaxChargePower = 2.4f;
            BasePower = 4.8f;
            ChargePowerDecay = 1.875f;

            ChargeSound = null;
        }

        public override void Awake()
        {
            base.Awake();
            CurrentChargePower = 0.0f;
        }

        public override void Start()
        {
            base.Start();

            if (ChargeSound == null) return;

            ChargeAudioSource = new GameObject {name = "Charge Audio Source"}.AddComponent<AudioSource>();
            ChargeAudioSource.clip = ChargeSound;
            ChargeAudioSource.transform.SetParent(transform);
        }

        public override bool Available
        {
            get { return Manager.IsActive<Duck>(); }
        }

        public override bool ShouldPerform
        {
            get { return Input.GetButton(ChargeButton); }
        }

        public override bool ShouldEnd
        {
            get { return !Manager.IsActive<Duck>(); }
        }

        public override void OnActiveEnter(State previousState)
        {
            CurrentChargePower = 0.0f;

            if (ChargeAudioSource == null) return;
            ChargeAudioSource.pitch = ChargePitchMin;
            ChargeAudioSource.Play();
        }

        public override void OnActiveUpdate()
        {
            CurrentChargePower -= CurrentChargePower*ChargePowerDecay*Time.deltaTime;

            if (Input.GetButtonDown(ChargeButton))
            {
                Charge();
            }

            if (CurrentChargePower > MaxChargePower) CurrentChargePower = MaxChargePower;
        }

        public void Charge()
        {
            CurrentChargePower += ChargePower;

            if (ChargeAudioSource != null)
            {
                ChargeAudioSource.pitch += (ChargePitchMax - ChargePitchMin)/ChargePitchSteps;
                if (ChargeAudioSource.pitch > ChargePitchMax) ChargeAudioSource.pitch = ChargePitchMax;
                ChargeAudioSource.Play();
            }
        }

        public override void OnActiveExit()
        {
            if(Controller.FacingForward)
                Controller.GroundVelocity += BasePower + CurrentChargePower;
            else
                Controller.GroundVelocity -= BasePower + CurrentChargePower;

            Manager.Perform<Roll>(true, true);

            if(ChargeAudioSource != null)
                ChargeAudioSource.Stop();
        }
    }
}
