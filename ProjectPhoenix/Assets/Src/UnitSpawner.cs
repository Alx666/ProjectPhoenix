using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System;
using Graph;

public class UnitSpawner : MonoBehaviour
{
	public Transform			SpawnLocator;
	public Transform			ExitLocator;
	public int					MaxUnits;			//Limite Max di Spawn della barracks
	public float				MinSpawnTime;
	public float				MaxSpawnTime;
	public string				CURRENT_STATE;
	public List<SpawnChance>	Spawns;

	private List<GameObject>	units;
	private int					Count { get; set; }         //Tiene traccia di quanti veicoli sono attivi al momento
	private Animator			animator;
    private int toOpen			= Animator.StringToHash("ToOpen");
	private int toClose			= Animator.StringToHash( "ToClose" );

	private StateIdle			idle;
	private StateReady			ready;
	private StateWait			wait;
	private StateSpawn			spawn;
	private StateOpen			open;
	private StatePush			push;
	private StateClose			close;

	private IState				currentState;
	

	public void Awake()
	{
		if ( !Mathf.Approximately( Spawns.Sum( hS => hS.Chance ), 1f ) )
			throw new UnityException( this.gameObject.name + ": Sum of chances must be == 1" );

		animator = GetComponent<Animator>();

		units = new List<GameObject>();

		idle			= new StateIdle( this );
		ready			= new StateReady(this);
		wait			= new StateWait( this );
		spawn			= new StateSpawn( this );
		open			= new StateOpen( this );
		push			= new StatePush( this );
		close			= new StateClose( this );
        
        ready.Wait = wait;
        wait.Spawn = spawn;
        spawn.Open = open;
        open.Push = push;
        push.Close = close;
        close.Ready = ready;

		currentState = ready;
		ready.OnStateEnter();
	}

	public void Update()
	{
		currentState = currentState.OnStateUpdate();
		CURRENT_STATE = currentState.ToString();
	}

	[Serializable]
	public class SpawnChance
	{
		public GameObject Unit;
        public Graph<POI> Graph;

		[Range( 0f, 1f )]
		public float Chance;

	}

	public interface IState
	{
        void OnStateEnter();
		IState OnStateUpdate();
	}

	private class StateIdle : IState
	{
		private UnitSpawner unitSpawner;

		public StateIdle( UnitSpawner unitSpawner )
		{
			this.unitSpawner = unitSpawner;
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
	private class StateReady : IState
	{
		private UnitSpawner owner;

		public StateReady( UnitSpawner owner )
		{
			this.owner = owner;
		}

		public StateWait Wait { get; internal set; }

		public void OnStateEnter()
		{

		}

		public IState OnStateUpdate()
		{
			if ( owner.Count >= 0 &&	owner.Count < owner.MaxUnits )
			{
				Wait.OnStateEnter();
				return Wait;
			}

			return this;
			
        }
		public override string ToString()
		{
			return "READY";
		}
	}
	private class StateWait : IState
	{
		private UnitSpawner owner;
		private float spawnTime;

		public StateWait( UnitSpawner owner )
		{
			this.owner = owner;
		}

		public StateSpawn Spawn { get; internal set; }

		public void OnStateEnter()
		{
			spawnTime = UnityEngine.Random.Range( owner.MinSpawnTime, owner.MaxSpawnTime );
		}

		public IState OnStateUpdate()
		{
			spawnTime -= Time.deltaTime;
			if ( spawnTime <= 0f )
			{
				Spawn.OnStateEnter();
				return Spawn;
			}
			return this;
		}
		public override string ToString()
		{
			return "WAIT";
		}
	}
	private class StateSpawn : IState
	{
		private UnitSpawner owner;

		public StateSpawn( UnitSpawner owner )
		{
			this.owner = owner;
		}

		public StateOpen Open { get; internal set; }

		public void OnStateEnter()
		{
			float spawnResult = (float)UnityEngine.Random.Range( 0, 100 ) / 100;
			for ( int i = 0; i < owner.Spawns.Count; i++ )
			{
				spawnResult -= owner.Spawns[i].Chance;
				if ( spawnResult <= 0f )
				{
                    SpawnChance unit = owner.Spawns[i];
                    Spawn(unit);
					break;
				}
			}
			owner.Count++;
		}

        private void Spawn(SpawnChance unit)
        {
            GameObject spawned = Instantiate(unit.Unit, owner.SpawnLocator.position, owner.SpawnLocator.rotation) as GameObject;
            owner.units.Add(spawned);
            spawned.GetComponent<IControllerAI>().Graph = unit.Graph;
            //GlobalFactory.GetInstance( SpawnChances[i].Unit );
        }
        public IState OnStateUpdate()
		{
			Open.OnStateEnter();
			return Open;
		}

		public override string ToString()
		{
			return "SPAWN";
		}
	}
	private class StateOpen : IState
	{
		private UnitSpawner owner;

		public StatePush Push { get; internal set; }
		
        public StateOpen( UnitSpawner owner )
		{
			this.owner = owner;
		}
		public void OnStateEnter()
		{
            if (owner.animator == null)
                return;
                owner.animator.SetTrigger( owner.toOpen );
		}
		public IState OnStateUpdate()
		{
            if (owner.animator == null)
            {
                Push.OnStateEnter();
                return Push;
            }

            if (owner.animator.IsInTransition(0) || owner.animator.GetCurrentAnimatorStateInfo(0).normalizedTime < 1 ) 
			{
				return this;
			}

			Push.OnStateEnter();
			return Push;
		}

		public override string ToString()
		{
			return "OPEN";
		}

	}
	private class StatePush : IState
	{
		private UnitSpawner owner;
		private int tweenId;

		public StatePush( UnitSpawner owner )
		{
			this.owner = owner;
		}

		public StateClose Close { get; internal set; }

		public void OnStateEnter()
		{
			LTDescr d = LeanTween.move( owner.units.Last(), owner.ExitLocator.position, 3f );
			tweenId = d.id;
			d.setEase( LeanTweenType.easeInOutCubic );
		}

		public IState OnStateUpdate()
		{
			if ( LeanTween.isTweening( tweenId ) )
			{
				return this;
			}

            //else
            IControllerAI ai = owner.units.Last().GetComponent<IControllerAI>();

            Debug.Log("DELETEME!");
            ai.Target = new GameObject();
            ai.Target.transform.position = new Vector3(327f, 0f, 121f);
            ai.Patrol();
            //Deleteme

            Close.OnStateEnter();
			return Close;
		}

		public override string ToString()
		{
			return "CLOSE";
		}
	}
	private class StateClose : IState
	{
		private UnitSpawner owner;

		public StateClose( UnitSpawner owner )
		{
			this.owner = owner;
		}

		public StateReady Ready { get; internal set; }

		public void OnStateEnter()
		{
            if (owner.animator == null)
                return;
			owner.animator.SetTrigger( owner.toClose );
		}

		public IState OnStateUpdate()
		{
            if (owner.animator == null)
            {
                Ready.OnStateEnter();
                return Ready;
            }

            if ( owner.animator.IsInTransition(0) || owner.animator.GetCurrentAnimatorStateInfo(0).normalizedTime < 1f )
			{
				return this;
			}

			Ready.OnStateEnter();
			return Ready;
		}

		public override string ToString()
		{
			return "CLOSE";
		}
	}

}