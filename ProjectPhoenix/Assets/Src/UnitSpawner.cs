using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System;

public class UnitSpawner : MonoBehaviour
{

	public List<SpawnChance>	SpawnChances;
	public int					MaxUnits;			//Limite Max di Spawn della barracks
	public float				MinSpawnTime;
	public float				MaxSpawnTime;

	public int					Count { get; private set; }         //Tiene traccia di quanti veicoli sono attivi al momento

	private Pool				pool;
	private bool				spawnbool;
	private float				spawnTime;

	public void Awake()
	{
		if ( !Mathf.Approximately( SpawnChances.Sum( hS => hS.Chance ), 1f ) )
			throw new UnityException( this.gameObject.name + ": Sum of chances must be == 1" );
	}

	public void Start()
	{

	}

	public void Update()
	{
		if ( Count >= 0 && Count < MaxUnits )
		{
			if ( spawnbool == false )
			{ 
				spawnTime = UnityEngine.Random.Range( MinSpawnTime, MaxSpawnTime );
				spawnbool = true;
			}
			spawnTime -= Time.deltaTime;

			if ( spawnTime <= 0 )
			{
				float spawnResult = (float)UnityEngine.Random.Range( 0, 100 )/100;
				Debug.Log( spawnResult );
				for ( int i = 0; i < SpawnChances.Count; i++ )
				{
					spawnResult -= SpawnChances[i].Chance;
					if ( spawnResult <= 0f )
					{
						Instantiate( SpawnChances[i].Unit );
						break;
					}
				}
				

				Count++;
				spawnbool = false;
			}
		}
	}

	[Serializable]
	public class SpawnChance
	{
		public GameObject Unit;

		[Range( 0f, 1f )]
		public float Chance;
	}
	public enum SpawnStrategy
	{

	}
}