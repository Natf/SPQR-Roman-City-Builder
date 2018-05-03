using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Immigrant : Unit {

	int populationValue = 5;
	public GameObject houseToMoveTo;
	NavMeshAgent navMeshAgent;

	PopulationController populationController;

	void Start()
	{
		populationController = GameObject.Find("PopulationController").GetComponent(typeof(PopulationController)) as PopulationController;
		populationController.ChangePopulation(populationValue);

		navMeshAgent = gameObject.GetComponent(typeof(NavMeshAgent)) as NavMeshAgent;

		InhabitAFreeHouse();
	}

	void InhabitAFreeHouse()
	{
		StartCoroutine("FindAFreeHouse", GameObject.FindGameObjectsWithTag("House"));
	}

	IEnumerator FindAFreeHouse(GameObject[] houses)
	{
		foreach (GameObject house in houses) {
			if (house != null) {
				House houseScript = house.GetComponent<House>();
				if (houseScript.reservedInhabitants < houseScript.maxSupportedHabitants) {
					houseScript.reservedInhabitants += populationValue;
					houseToMoveTo = house;
					navMeshAgent.SetDestination(house.transform.position);
					StartCoroutine("CheckAtHouse");
					yield break;
				}
			}
			yield return 0;
		}

		DestroyUnit(); // if we can't find a house destroy
	}

	IEnumerator CheckAtHouse()
	{
		while ((houseToMoveTo != null)) {
			if (!navMeshAgent.pathPending) {
				if (navMeshAgent.remainingDistance <= navMeshAgent.stoppingDistance) {
		        	if (!navMeshAgent.hasPath || navMeshAgent.velocity.sqrMagnitude == 0f) {
						OccupyHouse();
						yield break;
		         	}
		     	} else if (navMeshAgent.velocity.sqrMagnitude < 1.0f) {
					 // moving too slow so maybe stuck, get new path
					 navMeshAgent.SetDestination(houseToMoveTo.transform.position);
					 yield return new WaitForSeconds(1.0f);
					 // wait a bit longer to allow us to climb some speed back
					 // and prevent bursts of new path calls
				}
		 	}

			yield return new WaitForSeconds(0.2f);
		}

		DestroyUnit();
	}

	void DestroyUnit()
	{
		populationController.ChangePopulation(populationValue * -1);
		Destroy(gameObject);
	}

	void OccupyHouse() {
		if (houseToMoveTo == null) {
			DestroyUnit();
			return;
		}

		House houseToMoveToScript = houseToMoveTo.GetComponent<House>();
		houseToMoveToScript.inhabitants += populationValue;
		houseToMoveToScript.Occupy();

		Destroy(gameObject);
	}
}
