using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Dreamteck.Splines.Examples
{
    public class RollerCoaster : MonoBehaviour
    {
        public float          speed         = 10f;
        public float          minSpeed      = 1f;
        public float          maxSpeed      = 20f;
        public float          frictionForce = 0.1f;
        public float          gravityForce  = 1f;
        public float          slopeRange    = 60f;
        public AnimationCurve speedGain;
        public AnimationCurve speedLoss;
        public float          brakeSpeed;
        public float          brakeReleaseSpeed;

        public  CoasterSound[] sounds;
        public  AudioSource    brakeSound;
        public  AudioSource    boostSound;
        public  float          soundFadeLength = 0.15f;
        private float          addForce;
        private float          brakeForce;

        private float          brakeTime;
        private SplineFollower follower;


        // Use this for initialization
        private void Start()
        {
            follower              =  GetComponent<SplineFollower>();
            follower.onEndReached += OnEndReached;
            Cursor.lockState      =  CursorLockMode.Locked;
        }

        // Update is called once per frame
        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape)) Cursor.lockState = CursorLockMode.None;
            var dot = Vector3.Dot(transform.forward, Vector3.down);
            var dotPercent = Mathf.Lerp(-slopeRange / 90f, slopeRange / 90f, (dot + 1f) / 2f);
            speed -= Time.deltaTime * frictionForce * (1f - brakeForce);
            var speedAdd     = 0f;
            var speedPercent = Mathf.InverseLerp(minSpeed, maxSpeed, speed);
            if (dotPercent > 0f)
                speedAdd = gravityForce * dotPercent * speedGain.Evaluate(speedPercent) * Time.deltaTime;
            else
                speedAdd = gravityForce * dotPercent * speedLoss.Evaluate(1f - speedPercent) * Time.deltaTime;
            speed += speedAdd * (1f - brakeForce);
            speed =  Mathf.Clamp(speed, minSpeed, maxSpeed);
            if (addForce > 0f)
            {
                var lastAdd = addForce;
                addForce =  Mathf.MoveTowards(addForce, 0f, Time.deltaTime * 30f);
                speed    += lastAdd - addForce;
            }

            follower.followSpeed = speed;
            follower.followSpeed *= 1f - brakeForce;
            if (brakeTime > Time.time) brakeForce = Mathf.MoveTowards(brakeForce, 1f, Time.deltaTime * brakeSpeed);
            else brakeForce = Mathf.MoveTowards(brakeForce, 0f, Time.deltaTime * brakeReleaseSpeed);

            speedPercent = Mathf.Clamp01(speed / maxSpeed) * (1f - brakeForce);
            for (var i = 0; i < sounds.Length; i++)
            {
                if (speedPercent < sounds[i].startPercent - soundFadeLength ||
                    speedPercent > sounds[i].endPercent   + soundFadeLength)
                {
                    if (sounds[i].source.isPlaying) sounds[i].source.Pause();
                    continue;
                }

                if (!sounds[i].source.isPlaying) sounds[i].source.UnPause();

                var volume = 1f;
                if (speedPercent < sounds[i].startPercent + soundFadeLength)
                    volume = Mathf.InverseLerp(sounds[i].startPercent, sounds[i].startPercent + soundFadeLength,
                                               speedPercent);
                else if (speedPercent > sounds[i].endPercent)
                    volume = Mathf.InverseLerp(sounds[i].endPercent + soundFadeLength, sounds[i].endPercent,
                                               speedPercent);
                var pitchPercent = Mathf.InverseLerp(sounds[i].startPercent, sounds[i].endPercent, speedPercent);
                sounds[i].source.volume = volume;
                sounds[i].source.pitch  = Mathf.Lerp(sounds[i].startPitch, sounds[i].endPitch, pitchPercent);
            }
        }

        private void OnEndReached(double last)
        {
            //Detect when the wagon has reached the end of the spline
            var computers   = new List<SplineComputer>();
            var connections = new List<int>();
            var connected   = new List<int>();
            follower.spline.GetConnectedComputers(computers, connections, connected, 1.0, follower.direction,
                                                  true); //Get the avaiable connected computers at the end of the spline
            if (computers.Count == 0) return;
            //Do not select computers that are not connected at the first point so that we don't reverse direction
            for (var i = 0; i < computers.Count; i++)
                if (connected[i] != 0)
                {
                    computers.RemoveAt(i);
                    connections.RemoveAt(i);
                    connected.RemoveAt(i);
                    i--;
                }

            var distance =
                follower.CalculateLength(0.0, follower.result.percent); //Get the excess distance after looping
            follower.spline =
                computers[Random.Range(0, computers.Count)]; //Change the spline computer to the new spline
            follower.SetDistance(distance);                  //Set the excess distance along the new spline
        }

        public void AddBrake(float time)
        {
            brakeTime = Time.time + time;
            brakeSound.Stop();
            brakeSound.Play();
        }

        public void RemoveBrake()
        {
            brakeTime = 0f;
        }

        public void AddForce(float amount)
        {
            addForce = amount;
            boostSound.Stop();
            boostSound.Play();
        }

        [Serializable]
        public class CoasterSound
        {
            public float       startPercent;
            public float       endPercent = 1f;
            public AudioSource source;
            public float       startPitch = 1f;
            public float       endPitch   = 1f;
        }
    }
}