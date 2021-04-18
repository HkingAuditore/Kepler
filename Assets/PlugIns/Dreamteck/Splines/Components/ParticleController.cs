using UnityEngine;

namespace Dreamteck.Splines
{
    [ExecuteInEditMode]
    [AddComponentMenu("Dreamteck/Splines/Users/Particle Controller")]
    public class ParticleController : SplineUser
    {
        public enum EmitPoint
        {
            Beginning,
            Ending,
            Random,
            Ordered
        }

        public enum MotionType
        {
            None,
            UseParticleSystem,
            FollowForward,
            FollowBackward,
            ByNormal,
            ByNormalRandomized
        }

        public enum Wrap
        {
            Default,
            Loop
        }

        [HideInInspector] public ParticleSystem _particleSystem;

        [HideInInspector] public bool volumetric;

        [HideInInspector] public bool emitFromShell;

        [HideInInspector] public Vector2 scale = Vector2.one;

        [HideInInspector] public EmitPoint emitPoint = EmitPoint.Beginning;

        [HideInInspector] public MotionType motionType = MotionType.UseParticleSystem;

        [HideInInspector] public Wrap wrapMode = Wrap.Default;

        [HideInInspector] public float minCycles = 1f;

        [HideInInspector] public float maxCycles = 1f;

        private int        birthIndex;
        private Particle[] controllers = new Particle[0];
        private int        particleCount;

        private ParticleSystem.Particle[] particles = new ParticleSystem.Particle[0];

        protected override void Reset()
        {
            base.Reset();
            updateMethod = UpdateMethod.LateUpdate;
            if (_particleSystem == null) _particleSystem = GetComponent<ParticleSystem>();
        }

        protected override void LateRun()
        {
            if (_particleSystem == null) return;
            var maxParticles = _particleSystem.main.maxParticles;
            if (particles.Length != maxParticles)
            {
                particles = new ParticleSystem.Particle[maxParticles];
                var newControllers = new Particle[maxParticles];
                for (var i = 0; i < newControllers.Length; i++)
                {
                    if (i >= controllers.Length) break;
                    newControllers[i] = controllers[i];
                }

                controllers = newControllers;
            }

            particleCount = _particleSystem.GetParticles(particles);

            var isLocal = _particleSystem.main.simulationSpace == ParticleSystemSimulationSpace.Local;

            var particleSystemTransform = _particleSystem.transform;

            for (var i = particleCount - 1; i >= 0; i--)
            {
                if (isLocal) TransformParticle(ref particles[i], particleSystemTransform);
                if (controllers[i] == null)
                {
                    controllers[i] = new Particle();
                    OnParticleBorn(i);
                    if (isLocal) InverseTransformParticle(ref particles[i], particleSystemTransform);
                    continue;
                }

                var life = particles[i].startLifetime - particles[i].remainingLifetime;
                if (life <= Time.deltaTime && controllers[i].lifeTime > life) OnParticleBorn(i);
                if (isLocal) InverseTransformParticle(ref particles[i], particleSystemTransform);
            }

            for (var i = 0; i < particleCount; i++)
            {
                if (controllers[i] == null) controllers[i] = new Particle();
                if (isLocal) TransformParticle(ref particles[i], particleSystemTransform);
                HandleParticle(i);
                if (isLocal) InverseTransformParticle(ref particles[i], particleSystemTransform);
            }

            _particleSystem.SetParticles(particles, particleCount);
        }

        private void TransformParticle(ref ParticleSystem.Particle particle, Transform trs)
        {
            particle.position = trs.TransformPoint(particle.position);
            particle.velocity = trs.TransformDirection(particle.velocity);
        }

        private void InverseTransformParticle(ref ParticleSystem.Particle particle, Transform trs)
        {
            particle.position = trs.InverseTransformPoint(particle.position);
            particle.velocity = trs.InverseTransformDirection(particle.velocity);
        }

        private void HandleParticle(int index)
        {
            var lifePercent = particles[index].remainingLifetime / particles[index].startLifetime;
            if (motionType == MotionType.FollowBackward || motionType == MotionType.FollowForward ||
                motionType == MotionType.None)
            {
                Evaluate(controllers[index].GetSplinePercent(wrapMode, particles[index], motionType), evalResult);
                ModifySample(evalResult);
                particles[index].position = evalResult.position;
                if (volumetric)
                {
                    var right  = -Vector3.Cross(evalResult.forward, evalResult.up);
                    var offset = controllers[index].startOffset;
                    if (motionType != MotionType.None)
                        offset = Vector2.Lerp(controllers[index].startOffset, controllers[index].endOffset,
                                              1f - lifePercent);
                    particles[index].position += right         * offset.x * scale.x * evalResult.size +
                                                 evalResult.up * offset.y * scale.y * evalResult.size;
                }

                particles[index].velocity   = evalResult.forward;
                particles[index].startColor = controllers[index].startColor * evalResult.color;
            }

            controllers[index].remainingLifetime = particles[index].remainingLifetime;
            controllers[index].lifeTime          = particles[index].startLifetime - particles[index].remainingLifetime;
        }

        private void OnParticleDie(int index)
        {
        }

        private void OnParticleBorn(int index)
        {
            birthIndex++;
            var percent = 0.0;
            var emissionRate = Mathf.Lerp(_particleSystem.emission.rateOverTime.constantMin,
                                          _particleSystem.emission.rateOverTime.constantMax, 0.5f);
            var expectedParticleCount = emissionRate * _particleSystem.main.startLifetime.constantMax;
            if (birthIndex > expectedParticleCount) birthIndex = 0;
            switch (emitPoint)
            {
                case EmitPoint.Beginning:
                    percent = 0f;
                    break;
                case EmitPoint.Ending:
                    percent = 1f;
                    break;
                case EmitPoint.Random:
                    percent = Random.Range(0f, 1f);
                    break;
                case EmitPoint.Ordered:
                    percent = expectedParticleCount > 0 ? birthIndex / expectedParticleCount : 0f;
                    break;
            }

            Evaluate(percent, evalResult);
            ModifySample(evalResult);
            controllers[index].startColor        = particles[index].startColor;
            controllers[index].startPercent      = percent;
            controllers[index].startLifetime     = particles[index].startLifetime;
            controllers[index].remainingLifetime = particles[index].remainingLifetime;

            controllers[index].cycleSpeed = Random.Range(minCycles, maxCycles);
            var circle = Vector2.zero;
            if (volumetric)
            {
                if (emitFromShell)
                    circle  = Quaternion.AngleAxis(Random.Range(0f, 360f), Vector3.forward) * Vector2.right;
                else circle = Random.insideUnitCircle;
            }

            controllers[index].startOffset = circle                  * 0.5f;
            controllers[index].endOffset   = Random.insideUnitCircle * 0.5f;


            var right = Vector3.Cross(evalResult.forward, evalResult.up);
            particles[index].position = evalResult.position                                                          +
                                        right         * controllers[index].startOffset.x * evalResult.size * scale.x +
                                        evalResult.up * controllers[index].startOffset.y * evalResult.size * scale.y;

            var forceX = _particleSystem.forceOverLifetime.x.constantMax;
            var forceY = _particleSystem.forceOverLifetime.y.constantMax;
            var forceZ = _particleSystem.forceOverLifetime.z.constantMax;
            if (_particleSystem.forceOverLifetime.randomized)
            {
                forceX = Random.Range(_particleSystem.forceOverLifetime.x.constantMin,
                                      _particleSystem.forceOverLifetime.x.constantMax);
                forceY = Random.Range(_particleSystem.forceOverLifetime.y.constantMin,
                                      _particleSystem.forceOverLifetime.y.constantMax);
                forceZ = Random.Range(_particleSystem.forceOverLifetime.z.constantMin,
                                      _particleSystem.forceOverLifetime.z.constantMax);
            }

            var time          = particles[index].startLifetime - particles[index].remainingLifetime;
            var forceDistance = new Vector3(forceX, forceY, forceZ) * 0.5f * (time * time);

            var startSpeed = _particleSystem.main.startSpeed.constantMax;

            if (motionType == MotionType.ByNormal)
            {
                particles[index].position += evalResult.up * startSpeed *
                                             (particles[index].startLifetime - particles[index].remainingLifetime);
                particles[index].position += forceDistance;
                particles[index].velocity =  evalResult.up * startSpeed + new Vector3(forceX, forceY, forceZ) * time;
            }
            else if (motionType == MotionType.ByNormalRandomized)
            {
                var normal = Quaternion.AngleAxis(Random.Range(0f, 360f), evalResult.forward) * evalResult.up;
                particles[index].position += normal * startSpeed *
                                             (particles[index].startLifetime - particles[index].remainingLifetime);
                particles[index].position += forceDistance;
                particles[index].velocity =  normal * startSpeed + new Vector3(forceX, forceY, forceZ) * time;
            }

            HandleParticle(index);
        }

        public class Particle
        {
            internal float   cycleSpeed;
            internal Vector2 endOffset = Vector2.zero;
            internal float   lifeTime;
            internal float   remainingLifetime;
            internal Color   startColor = Color.white;
            internal float   startLifetime;
            internal Vector2 startOffset = Vector2.zero;
            internal double  startPercent;

            internal double GetSplinePercent(Wrap wrap, ParticleSystem.Particle particle, MotionType motionType)
            {
                var lifePercent = particle.remainingLifetime / particle.startLifetime;
                if (motionType == MotionType.FollowBackward) lifePercent = 1f - lifePercent;
                switch (wrap)
                {
                    case Wrap.Default: return DMath.Clamp01(startPercent + (1f - lifePercent) * cycleSpeed);
                    case Wrap.Loop:
                        var loopPoint                  = startPercent + (1.0 - lifePercent) * cycleSpeed;
                        if (loopPoint > 1.0) loopPoint -= Mathf.FloorToInt((float) loopPoint);
                        return loopPoint;
                }

                return 0.0;
            }
        }
    }
}