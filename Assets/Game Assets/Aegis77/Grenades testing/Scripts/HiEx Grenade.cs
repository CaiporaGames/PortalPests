using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;

namespace Aegis.GrenadeSystem.HiEx
{
    public class HiExGrenade : MonoBehaviour
    {
        [SerializeField] private GameObject bombModel;
        // Explosion effects
        [Header("Explosion Effects")]
        [SerializeField] GameObject explosionEffectPrefab;
        [SerializeField] Vector3 explosionParticleOffset = new Vector3(0, 1, 0);

        // Add this field at the top with your other serialized fields
        [Header("Haptics")]
        [SerializeField] private float hapticMaxDistance = 15f;
        [SerializeField] private float hapticAmplitude = 1f;
        [SerializeField] private float hapticDuration = 0.4f;


        //explosion settings
        [Header("Explosion Settings")]
        [SerializeField] float explosionDelay = 3f;
        [SerializeField] float explosionForce = 1000f;
        [SerializeField] float explosionForceRadius = 5f;

        // Damage settings
        [Header("Damage Settings")]
        [SerializeField] float closeRadius = 1f;
        [SerializeField] float nearRadius = 5f;
        [SerializeField] float farRadius = 7f;

        [SerializeField] float closeDam = 10f;
        [SerializeField] float nearDam = 5f;
        [SerializeField] float farDam = 1f;


        // Audio effects
        [Header("Audio Effects")]
        [SerializeField] AudioSource audioSource;
        [SerializeField] AudioClip impact;
        [SerializeField] AudioClip[] explosionSounds;

        // internal variables
        float countdown;
        bool hasexploded = false;

        private void Awake()
        {
            audioSource = gameObject.GetComponent<AudioSource>();
        }

        IEnumerator ExplosionCountdown()
        {
            while (countdown > 0)
            {
                countdown -= Time.deltaTime;
                yield return null;
            }

            if (!hasexploded)
            {
                Explode();
                hasexploded = true;
            }
        }

        public void StartExplosionTimer()
        {
            if (hasexploded) return;

            countdown = explosionDelay;
            StartCoroutine(ExplosionCountdown());
        }

        //explode function - what happens when the timer reaches 0
        void Explode()
        {

            // instantiate explosion effect at this game object
            GameObject explosionEffect = Instantiate(explosionEffectPrefab, transform.position + explosionParticleOffset, Quaternion.identity);

            Destroy(explosionEffect, 1.9f);

            PlaySoundAtPosition();

            ApplyExplosiveForce();

            ApplyDamage();
            TriggerHaptics();
        }


        //Function to apply damage to the player or to enemies
        void ApplyDamage()
        {
            //There are three radii to apply damage;
            //If a player or enemy is close, near or far from the explosion
            //Damage will be applied to a greater degree the closer the damaged entity is to the blast
            //Adjust the damage output and distance/reach of each radii in the DAMAGE settings above

            //close  objects
            Collider[] closecolliders = Physics.OverlapSphere(transform.position, closeRadius);

            //near objects
            Collider[] nearbycolliders = Physics.OverlapSphere(transform.position, nearRadius);

            //far objects
            Collider[] farcolliders = Physics.OverlapSphere(transform.position, farRadius);

            foreach (Collider closeobject in closecolliders)
            {

                //if an Enemy or player is nearby the explosion, apply damage
                if (closeobject.tag == "Player" || closeobject.tag == "Enemy" || closeobject.tag == "Target")
                {

                    DamageHandler healthobject = closeobject.GetComponent<DamageHandler>();
                    Target targetObject = closeobject.GetComponent<Target>();

                    if (targetObject)
                    {
                        targetObject.Got(closeDam);
                    }

                    if (healthobject)
                    {
                        healthobject.ApplyDamage(closeDam);
                    }
                }

            }

            foreach (Collider nearbyobject in nearbycolliders)
            {

                //if an Enemy or player is nearby the explosion, apply damage
                if (nearbyobject.tag == "Player" || nearbyobject.tag == "Enemy" || nearbyobject.tag == "Target")
                {
                    DamageHandler healthobject = nearbyobject.GetComponent<DamageHandler>();

                    if (healthobject)
                    {
                        healthobject.ApplyDamage(nearDam);
                    }
                }

            }

            foreach (Collider farobject in farcolliders)
            {

                //if an Enemy or player is nearby the explosion, apply damage
                if (farobject.tag == "Player" || farobject.tag == "Enemy" || farobject.tag == "Target")
                {
                    DamageHandler healthobject = farobject.GetComponent<DamageHandler>();

                    if (healthobject)
                    {
                        healthobject.ApplyDamage(farDam);
                    }
                }

            }
        }

        void TriggerHaptics()
        {
            GameObject player = GameObject.FindWithTag("Player");
            if (player == null) return;

            float distance = Vector3.Distance(transform.position, player.transform.position);
            if (distance >= hapticMaxDistance) return;

            float intensity = Mathf.Clamp01(1f - (distance / hapticMaxDistance)) * hapticAmplitude;

            // Delay haptic slightly based on distance (sound travels ~340m/s)
            float delay = distance / 340f;
            StartCoroutine(DelayedHaptic(intensity, delay));
        }

        IEnumerator DelayedHaptic(float intensity, float delay)
        {
            yield return new WaitForSeconds(delay);

            float[] pulses = { 1f, 0.5f, 0.2f };
            float[] gaps   = { 0f, 0.12f, 0.1f };

            for (int i = 0; i < pulses.Length; i++)
            {
                yield return new WaitForSeconds(gaps[i]);
                SendHapticToAllControllers(intensity * pulses[i], 0.15f);
            }
        }

        void SendHapticToAllControllers(float amplitude, float duration)
        {
            // Must query each hand separately
            var leftDevices = new List<InputDevice>();
            var rightDevices = new List<InputDevice>();
            var headDevices = new List<InputDevice>();

            InputDevices.GetDevicesWithCharacteristics(
                InputDeviceCharacteristics.Controller | InputDeviceCharacteristics.Left, leftDevices);
            InputDevices.GetDevicesWithCharacteristics(
                InputDeviceCharacteristics.Controller | InputDeviceCharacteristics.Right, rightDevices);
            InputDevices.GetDevicesWithCharacteristics(
                InputDeviceCharacteristics.HeadMounted, headDevices);

            var all = new List<InputDevice>();
            all.AddRange(leftDevices);
            all.AddRange(rightDevices);
            all.AddRange(headDevices);

            foreach (var device in all)
            {
                if (device.TryGetHapticCapabilities(out HapticCapabilities caps) && caps.supportsImpulse)
                    device.SendHapticImpulse(0, amplitude, duration);
            }
        }

        //Function to apply physics explosive force to objects near the explosion
        void ApplyExplosiveForce()
        {
            //Create a list of all colliders of objects within the radius of the explosion force
            Collider[] colliders = Physics.OverlapSphere(transform.position, explosionForceRadius);

            //for every collider collected, apply an explosive force originating from the position of the explosion
            foreach (Collider nearbyobject in colliders)
            {
                Rigidbody rb = nearbyobject.GetComponent<Rigidbody>();

                if (rb != null)
                {
                    rb.AddExplosionForce(explosionForce, transform.position, explosionForceRadius);
                }
            }
        }

        //Function to play explosion sound effect by instantiating a new object to play that sound at the explosion
        void PlaySoundAtPosition()
        {

            int rand = Random.Range(0, explosionSounds.Length);

            

            audioSource.spatialBlend = 1;
            audioSource.clip = explosionSounds[rand];
            audioSource.Play();
            bombModel.SetActive(false);
            Destroy(gameObject, 5);
        }

        //Function to play an impact sound effect if the thrown grenade hits something, but has not exploded yet
        private void OnCollisionEnter(Collision collision)
        {
            if (collision.gameObject.tag != "Player")
            {
                audioSource.clip = impact;

                audioSource.spatialBlend = 1;

                audioSource.Play();
            }
        }
    }
}
