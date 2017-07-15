using System.Collections;
using UnityEngine;

public class DeathBomb : MonoBehaviour
{
    public float BombTimer = 7f;
    public float VelocityThreshold = 5f;
    public float StartingFrequency = 1f;
    public AudioSource Audio;

    public string STATE_DEBUG = string.Empty;

    private MadMaxActor m_hActor;
    private Rigidbody m_hRigidbody;
    private Light m_hLight;
    private LightBulb m_hBulb;

    private StateInactive inactive;
    private StateActive active;
    private StateExplode explode;

    private IStateDetonator currentState;

    private void Awake()
    {
        m_hActor = GetComponent<MadMaxActor>();
        m_hRigidbody = GetComponent<Rigidbody>();
        m_hLight = GetComponentInChildren<Light>();

        m_hBulb = new LightBulb(this);

        inactive = new StateInactive(this);
        active = new StateActive(this);
        explode = new StateExplode(this);

        inactive.Active = active;
        active.Inactive = inactive;
        active.Explode = explode;
        explode.Inactive = inactive;

        inactive.OnStateEnter();
        currentState = inactive;

        m_hBulb.Reset();
    }

    private void Update()
    {
        currentState = currentState.OnStateUpdate();
        STATE_DEBUG = currentState.ToString();
    }

    private void Pulse(float frequency)
    {
        StartCoroutine(LightPulse(frequency));
    }

    public IEnumerator LightPulse(float duration)
    {
        m_hBulb.TurnOn();
        yield return new WaitForSeconds(duration * 0.5f);
        m_hBulb.TurnOff();

        yield return new WaitForSeconds(duration * 0.5f);
        m_hBulb.Reset();
    }

    public interface IStateDetonator
    {
        void OnStateEnter();

        IStateDetonator OnStateUpdate();
    }

    private class StateInactive : IStateDetonator
    {
        private DeathBomb deathBomb;
        public StateActive Active { get; internal set; }

        public StateInactive(DeathBomb deathBomb)
        {
            this.deathBomb = deathBomb;
        }

        public void OnStateEnter()
        {
            deathBomb.m_hBulb.Reset();
        }

        public IStateDetonator OnStateUpdate()
        {
            if (deathBomb.m_hActor.IsDead)
            {
                return this;
            }
            if (deathBomb.m_hRigidbody.velocity.magnitude < deathBomb.VelocityThreshold)
            {
                Active.OnStateEnter();
                return Active;
            }
            return this;
        }

        public override string ToString()
        {
            return "INACTIVE";
        }
    }

    private class StateActive : IStateDetonator
    {
        public StateExplode Explode { get; internal set; }
        public StateInactive Inactive { get; internal set; }

        private DeathBomb deathBomb;

        private float counter;
        private float frequency;

        public StateActive(DeathBomb deathBomb)
        {
            this.deathBomb = deathBomb;
        }

        public void OnStateEnter()
        {
            counter = deathBomb.BombTimer;
            frequency = deathBomb.StartingFrequency;
        }

        public IStateDetonator OnStateUpdate()
        {
            if (deathBomb.m_hRigidbody.velocity.magnitude > deathBomb.VelocityThreshold)
            {
                //cooldown to avoid immediate state switch
                counter += Time.deltaTime;
                if (counter > deathBomb.BombTimer)
                {
                    Inactive.OnStateEnter();
                    return Inactive;
                }
            }
            else
            {
                counter -= Time.deltaTime;
            }

            if (deathBomb.m_hBulb.PulseReady)
            {
                float countRate = counter / deathBomb.BombTimer;
                if (countRate < 0f)
                {
                    Explode.OnStateEnter();
                    return Explode;
                }
                else if (countRate > 0.0f && countRate < 0.25f)
                {
                    frequency = deathBomb.StartingFrequency * 0.25f;
                    deathBomb.Pulse(frequency);
                }
                else if (countRate > 0.25f && countRate < 0.5f)
                {
                    frequency = deathBomb.StartingFrequency * 0.5f;
                    deathBomb.Pulse(frequency);
                }
                else if (countRate > 0.5f && countRate < 0.75f)
                {
                    deathBomb.Pulse(frequency);
                }
            }

            return this;
        }

        public override string ToString()
        {
            return "ACTIVE";
        }
    }

    private class StateExplode : IStateDetonator
    {
        private DeathBomb deathBomb;
        public StateInactive Inactive { get; internal set; }

        public StateExplode(DeathBomb deathBomb)
        {
            this.deathBomb = deathBomb;
        }

        public void OnStateEnter()
        {
            deathBomb.m_hActor.Die(deathBomb.m_hActor);
        }

        public IStateDetonator OnStateUpdate()
        {
            Inactive.OnStateEnter();
            return Inactive;
        }

        public override string ToString()
        {
            return "EXPLODED";
        }
    }

    private class LightBulb
    {
        public bool PulseReady { get; private set; }
        private DeathBomb owner;

        public LightBulb(DeathBomb owner)
        {
            this.owner = owner;
        }

        public void TurnOn()
        {
            PulseReady = false;
            owner.m_hLight.enabled = true;
        }

        public void TurnOff()
        {
            owner.m_hLight.enabled = false;
        }

        public void Reset()
        {
            PulseReady = true;
            TurnOff();
        }
    }

    //internal void Reset()
    //{
    //    inactive.OnStateEnter();
    //    currentState = inactive;
    //    m_hBulb.Reset();
    //}
}