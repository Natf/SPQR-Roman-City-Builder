using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[System.Serializable]
public class House : Building {

	const int WATER_SOURCE_LAYER = 9;

	public GameObject[] houseLevels;
	NavMeshObstacle navMeshObstacle;
	GameObject currentModel = null;
	public int currentHouseLevel = 0;

	public int maxSupportedHabitants = 5;
	public int reservedInhabitants = 0;
	public int inhabitants = 0;

	PopulationController populationController;

	public float rangeFromWell = 10.0f;

	string[] rankOneNeeds = {"water"};

	public override void OnPlace()
	{
		canBurn = false;

		base.OnPlace();

		navMeshObstacle = gameObject.GetComponent(typeof(NavMeshObstacle)) as NavMeshObstacle;

		UpdateHouseLevel();

		populationController = GameObject.Find("PopulationController").GetComponent(typeof(PopulationController)) as PopulationController;
		populationController.ChangeMaxPopulation(maxSupportedHabitants);
	}
// as we can expect there to be more houses than static stuff like wells we should really have the wells post
// they are offering water on their creation and only periodically renotify houses. We can then notify houses when
// they're taken away as well.
// this way we can store the list of requirements that each house has and then get them to check them in order
// each check which is faster than doing the hit detection for each house
	IEnumerator CheckForNeeds()
	{
		while(true) {
			CheckForWater();
			yield return new WaitForSeconds(5.0f);
    	}
	}

	void CheckForWater()
	{
		int layerMask = 1 << WATER_SOURCE_LAYER;

			if (currentHouseLevel > 0) {
				Collider[] hitColliders = Physics.OverlapSphere(transform.position, rangeFromWell, layerMask);

		    if (hitColliders.Length > 0 && currentHouseLevel < 2) {
					currentHouseLevel++;
					ChangeMaxSupportedPopulation(5);
					UpdateHouseLevel();
				} else if (hitColliders.Length == 0 && currentHouseLevel > 1) {
					currentHouseLevel = 1;
					ChangeMaxSupportedPopulation((maxSupportedHabitants * -1) + 5);
					UpdateHouseLevel();
				}
			}
	}

	public void ChangeMaxSupportedPopulation(int change)
	{
		populationController.ChangeMaxPopulation(change);
		maxSupportedHabitants += change;
	}

	public void Occupy()
	{
		if (currentHouseLevel == 0) {
			currentHouseLevel ++;
			UpdateHouseLevel();
			StartCoroutine("CheckForNeeds");
		}
	}

	public void UpdateHouseLevel()
	{
		currentModel = SetChildObject(houseLevels[currentHouseLevel], currentModel);

		if (currentHouseLevel == 1) {
			canBurn = true;
			StartCoroutine("UpdateRisks");
			UseRandomRotation();
			navMeshObstacle.enabled = true;
		} else if (currentHouseLevel == 0) {
			navMeshObstacle.enabled = false;
		}

		ModelInitialize();
	}

	public override GameObject GetModelBeingDisplayedForTile()
	{
		return currentModel;
	}

	void OnDestroy() {
		populationController.ChangePopulation(inhabitants * -1);
		populationController.ChangeMaxPopulation(maxSupportedHabitants * -1);
	}

	protected override void BuildingBurntDown()
	{
		base.BuildingBurntDown();

		populationController.ChangePopulation(inhabitants * -1);
		populationController.ChangeMaxPopulation(maxSupportedHabitants * -1);

		Destroy(navMeshObstacle);
		StopCoroutine("CheckForNeeds");
	}
}
