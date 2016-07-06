using UnityEngine;
using System.Collections;
using System;
using Graph;

[RequireComponent(typeof(ControllerPlayerHeli))]
public class ProviderAIHelicopter : MonoBehaviour, IControllerAI {

	public float RotationSpeed		= 10f;
	public float StoppingDistance   = 60f;
	public float Tolerance = 0.2f;
	public string CURRENT_STATE;

	private ControllerPlayerHeli	helicopter;

	private StateIdle				idle;
	private StateLift				lift;
	private StatePatrol				patrol;
	private StateLanding			landing;
	private IState					currentState;

	void Awake()
	{
		helicopter		= this.GetComponent<ControllerPlayerHeli>();
		
		idle			= new StateIdle(this);
		lift			= new StateLift(this);
		patrol			= new StatePatrol(this);
		landing			= new StateLanding(this);

		lift.Patrol		= patrol;
		patrol.Landing	= landing;
		patrol.Idle		= idle;
		landing.Idle	= idle;

		currentState = idle;
		idle.OnStateEnter();
	}
	
	// Update is called once per frame
	void Update ()
	{
		currentState = currentState.OnStateUpdate();
		CURRENT_STATE = currentState.ToString();
	}

    #region IControllerAI

    private GameObject target;
    public GameObject Target { get { return target; } set { target = value; } }

    private Graph<POI> graph;
    public Graph<POI> Graph { get { return graph; } set { graph = value; } }

    public void Idle()
    {
        idle.OnStateEnter();
        currentState = idle;
    }

    public void Patrol()
    {
        patrol.OnStateEnter();
        currentState = patrol;
    }

    public void Attack()
    {
    }

    #endregion IControllerAI

    public interface IState
	{
		void OnStateEnter();
		IState OnStateUpdate();
	}

	#region PRIMARY FSM
	private class StateIdle				 : IState
	{
		private ProviderAIHelicopter owner;

		public StateIdle( ProviderAIHelicopter owner )
		{
			this.owner = owner;
		}

		public void OnStateEnter()
		{

		}

		public IState OnStateUpdate()
		{
			return this;
		}

		public override string ToString()
		{
			return "IDLE";
		}
	}

	private class StateLift		 : IState
	{
		private ProviderAIHelicopter owner;

		public StateLift( ProviderAIHelicopter owner )
		{
			this.owner = owner;
		}

		public StatePatrol Patrol { get; internal set; }

		public void OnStateEnter()
		{
			owner.helicopter.BeginUp();
		}

		public IState OnStateUpdate()
		{
			if ( owner.helicopter.transform.position.y >= owner.helicopter.MaxHeight * 0.5f )
			{
				Patrol.OnStateEnter();
				return Patrol;
			}

			return this;
		}

		public override string ToString()
		{
			return "LIFT";
		}
	}

	private class StatePatrol : IState
	{
		private ProviderAIHelicopter owner;

		public StatePatrol( ProviderAIHelicopter owner )
		{
			this.owner = owner;
		}

		public StateIdle Idle { get; internal set; }
		public StateLanding Landing { get; internal set; }

		public void OnStateEnter()
		{
			
		}

		public IState OnStateUpdate()
		{
			if(owner.target != null )
			{
				//Rotate
				Vector3 vDirection		= owner.target.transform.position - owner.helicopter.transform.position;
				Quaternion vRotation	= Quaternion.LookRotation(vDirection);
				Quaternion heliRotation = owner.helicopter.transform.rotation;
				owner.helicopter.transform.rotation = Quaternion.Lerp( heliRotation, vRotation, Time.deltaTime * owner.RotationSpeed );

				//Move
				float vVelocity = owner.helicopter.HeliRigidBody.velocity.magnitude;
                float vDistance = Vector3.Distance( owner.helicopter.transform.position, owner.target.transform.position );
				if ( vDistance > owner.StoppingDistance )
				{
					owner.helicopter.BeginForward();
				}
				else
				{
					owner.helicopter.EndForward();
					if ( vVelocity > 0f && vDistance > owner.Tolerance)
					{
						owner.helicopter.BeginBackward();
						if ( vVelocity <= 0f && vDistance < owner.Tolerance )
						{
							owner.helicopter.EndBackward();
						}
					}
					
				}
				
			}

			return this;
		}

		public override string ToString()
		{
			return "PATROL";
		}
	}

	private class StateLanding : IState
	{
		private ProviderAIHelicopter providerAIHelicopter;

		public StateLanding( ProviderAIHelicopter providerAIHelicopter )
		{
			this.providerAIHelicopter = providerAIHelicopter;
		}

		public StateIdle Idle { get; internal set; }

		public void OnStateEnter()
		{
			throw new NotImplementedException();
		}

		public IState OnStateUpdate()
		{
			throw new NotImplementedException();
		}

		public override string ToString()
		{
			return "LANDING";
		}
	}
	#endregion

	public void ToStateLift()
	{
		lift.OnStateEnter();
		currentState = lift;
	}

}
