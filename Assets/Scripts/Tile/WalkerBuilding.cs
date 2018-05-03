using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class WalkerBuilding : Building {

	protected int currentWorkers = 0;
	protected bool running = false;
	protected bool hasRoadAccess = false;
	protected bool foundWorkers = false;
	protected PopulationController populationController;
	protected GameObject currentModel = null;

	public override void OnPlace()
	{
		base.OnPlace();

		UpdateBuildingModel();
		populationController =  GameObject.Find("PopulationController").GetComponent(typeof(PopulationController)) as PopulationController;
		populationController.ChangeTotalNumberOfJobs(tileProperties.buildingProperties.workersRequired);

		UpdateRoadAccess();
		CheckForWorkers();

		if ((hasRoadAccess || !tileProperties.buildingProperties.requiresRoadAccess) && foundWorkers) {
			ChangeRunningValue(true);
		} else {
			StartCoroutine("WaitForRoadAccessAndWorkers");
		}
	}

	public override GameObject GetModelBeingDisplayedForTile()
	{
		return currentModel;
	}

	public void UpdateBuildingModel()
	{
		if (running) {
			currentModel = SetChildObject(tileProperties.buildingProperties.runningModel, currentModel);
			ModelInitialize();
		} else {
			currentModel = SetChildObject(tileProperties.buildingProperties.notRunningModel, currentModel);
			ModelInitialize();
		}
	}

	public int GetCurrentWorkers()
	{
		return currentWorkers;
	}

	public bool HasRoadAccess()
	{
		return hasRoadAccess;
	}

	IEnumerator BuildingRunning()
	{
		while (running) {
			CheckForWorkers();

			if (!foundWorkers) {
				ChangeRunningValue(false);
				yield break;
			}

			InstantiateWalker();

			yield return new WaitForSeconds(tileProperties.buildingProperties.workerSpawnRate / tileProperties.buildingProperties.maxWalkers);
		}
	}

	IEnumerator WaitForRoadAccessAndWorkers()
	{
		while (!running) {
			CheckForWorkers();

			if (foundWorkers && (hasRoadAccess || !tileProperties.buildingProperties.requiresRoadAccess)) {
				ChangeRunningValue(true);
				yield break;
			}

			yield return new WaitForSeconds(1.0f);
		}
	}

	private void CheckForWorkers()
	{
		currentWorkers = populationController.GetAvailableWorkersForBuilding(tileProperties.buildingProperties.workersRequired);
		foundWorkers = (currentWorkers > 0);
	}

	public void UpdateRoadAccess()
	{
		DetectAdjacentRoads();

		hasRoadAccess = (roadAbove || roadLeft || roadRight || roadBelow);

		if (running && !hasRoadAccess && tileProperties.buildingProperties.requiresRoadAccess) {
			ChangeRunningValue(false);
		}
	}

	void ChangeRunningValue(bool setRunningTo)
	{
		if (running == setRunningTo) {
			Debug.LogError("Error tried to change the running state of a building when that state was already active. " + setRunningTo);
			return;
		}

		running = setRunningTo;
		UpdateBuildingModel();

		if (running) {
			StartCoroutine("BuildingRunning");
		} else {
			StartCoroutine("WaitForRoadAccessAndWorkers");
		}
	}

	void OnDestroy()
	{
		populationController.ChangeTotalNumberOfJobs(tileProperties.buildingProperties.workersRequired * -1);
	}

	void InstantiateWalker()
	{
		if (tileProperties.buildingProperties.requiresRoadAccess) {
			List<Vector3> newDestinations = new List<Vector3>();
			float distanceAway = 1.5f;

			if (roadAbove) {
				newDestinations.Add(transform.position + new Vector3(distanceAway,0,0));
			}
			if (roadRight) {
				newDestinations.Add(transform.position + new Vector3(0,0,distanceAway));
			}
			if (roadBelow) {
				newDestinations.Add(transform.position - new Vector3(distanceAway,0,0));
			}
			if (roadLeft) {
				newDestinations.Add(transform.position - new Vector3(0,0,distanceAway));
			}

			int randomDirection = Random.Range(0, newDestinations.Count);

			Instantiate(tileProperties.buildingProperties.walkerObject, newDestinations[randomDirection], Quaternion.Euler(Vector3.zero));
		} else {
			Instantiate(tileProperties.buildingProperties.walkerObject, transform.position, Quaternion.Euler(Vector3.zero));
		}
	}
}
