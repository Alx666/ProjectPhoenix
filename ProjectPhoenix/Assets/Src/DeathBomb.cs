using UnityEngine;
using System.Collections;
using System;

public class DeathBomb : MonoBehaviour
{
    public float BombTimer;
    public float VelocityThreshold;
    public float PulseTime = 0.2f;
    public float StartingFrequency = 1.0f;
    public AudioSource Audio;

    private MadMaxActor m_hActor;
    private Rigidbody m_hRigidbody;
    private Light m_hLight;
    private IDetonator currentState;

    void Awake()
    {
        m_hActor = GetComponent<MadMaxActor>();
        m_hRigidbody = GetComponent<Rigidbody>();
        m_hLight = GetComponentInChildren<Light>();

        LightBulb lightBulb = new LightBulb(m_hLight);
        StateInactive inactive = new StateInactive(this, lightBulb);
        StateActive active = new StateActive(this, lightBulb);
        StateExplode explode = new StateExplode(this, lightBulb);

        inactive.Active = active;
        active.Inactive = inactive;
        active.Explode = explode;
        explode.Inactive = inactive;

        inactive.OnStateEnter();
        currentState = inactive;

    }
    void Update()
    {
        currentState.OnStateUpdate();
    }

    //public IEnumerator LightPulse()
    //{

    //    yield return new WaitForSeconds(0.2f);
    //    m_hLight.enabled = false;
    //}

    public interface IDetonator
    {
        void OnStateEnter();
        IDetonator OnStateUpdate();

    }
    private class StateInactive : IDetonator
    {
        private DeathBomb deathBomb;
        private LightBulb lightBulb;
        public StateActive Active { get; internal set; }

        public StateInactive(DeathBomb deathBomb, LightBulb light)
        {
            this.deathBomb = deathBomb;
            lightBulb = light;
        }

        public void OnStateEnter()
        {
            lightBulb.Pulse(false);
        }

        public IDetonator OnStateUpdate()
        {
            //check player status if he is dead there's non need to compare velocity
            //if (deathBomb.m_hActor.IsDead)
            //{
            //    return this;
            //}
            if (deathBomb.m_hRigidbody.velocity.magnitude < deathBomb.VelocityThreshold)
            {
                Active.OnStateEnter();
                deathBomb.currentState = Active;
                return Active;
            }
            return this;
        }
    }

    private class StateActive : IDetonator
    {
        public StateExplode Explode { get; internal set; }
        public StateInactive Inactive { get; internal set; }

        private DeathBomb deathBomb;
        private LightBulb lightBulb;

        private float counter;
        private float frequency;

        public StateActive(DeathBomb deathBomb, LightBulb light)
        {
            this.deathBomb = deathBomb;
            lightBulb = light;
        }

        public void OnStateEnter()
        {
            counter = deathBomb.BombTimer;
            frequency = deathBomb.StartingFrequency;
        }

        public IDetonator OnStateUpdate()
        {
            if (deathBomb.m_hRigidbody.velocity.magnitude > deathBomb.VelocityThreshold)
            {
                //cooldown to avoid immediate state switch
                counter += Time.deltaTime;
                if (counter > deathBomb.BombTimer)
                {
                    Inactive.OnStateEnter();
                    deathBomb.currentState = Inactive;
                    return Inactive;
                }
            }

            counter -= Time.deltaTime;
            float countRate = counter / deathBomb.BombTimer;

            if (countRate < 0f)
            {
                Explode.OnStateEnter();
                deathBomb.currentState = Explode;
                return Explode;
            }
            else if (countRate > 0.0f && countRate < 0.25f)
            {
                //passare ad una coroutine che aspetta la frequenza e che chiama un'altra courotine che fa l'accensione per x secondi
                frequency = deathBomb.StartingFrequency * 0.25f;
                lightBulb.Pulse(true, frequency);
            }
            else if (countRate > 0.25f && countRate < 0.5f)
            {
                frequency = deathBomb.StartingFrequency * 0.5f;
                lightBulb.Pulse(true, frequency);
            }
            else if (countRate > 0.5f && countRate < 0.75f)
            {
                lightBulb.Pulse(true, frequency);
            }
            deathBomb.currentState = this;
            return this;
        }
    }

    private class StateExplode : IDetonator
    {
        private DeathBomb deathBomb;
        private LightBulb lightBulb;
        public StateInactive Inactive { get; internal set; }

        public StateExplode(DeathBomb deathBomb, LightBulb light)
        {
            this.deathBomb = deathBomb;
            lightBulb = light;
        }


        public void OnStateEnter()
        {
            deathBomb.m_hActor.Die(deathBomb.m_hActor);
        }

        public IDetonator OnStateUpdate()
        {
            Inactive.OnStateEnter();
            deathBomb.currentState = Inactive;
            return Inactive;
        }
    }

    private class LightBulb
    {
        public float Frequency { get; set; }

        private Light light;
        private float lightPulseCounter;
        private float lightPulseDuration = 0.5f;
        public LightBulb(Light lightBulb)
        {
            light = lightBulb;
            light.enabled = false;
            lightPulseCounter = lightPulseDuration;
        }

        public void Pulse(bool IsActive, float Frequency = 1.0f)
        {
            if (IsActive)
            {
                float waitTime = Frequency;
                while (waitTime > 0.0f)
                {
                    light.enabled = false;
                    waitTime -= Time.deltaTime;
                }

                light.enabled = true;
                float pulseWaitTime = lightPulseDuration;
                while (pulseWaitTime > 0.0f)
                {
                    pulseWaitTime -= Time.deltaTime;
                }
                light.enabled = false;
            }
            else
            {
                return;
            }
        }
    }
}
