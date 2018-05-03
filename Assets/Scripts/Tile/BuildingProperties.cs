using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Building", menuName = "BuildingProperties")]
public class BuildingProperties : ScriptableObject
{
    public GameObject walkerObject;
	public int workersRequired;
	public int maxWalkers = 2;
	public float workerSpawnRate = 20.0f;
	public GameObject runningModel;
	public GameObject notRunningModel;
	public bool requiresRoadAccess = true;
}
