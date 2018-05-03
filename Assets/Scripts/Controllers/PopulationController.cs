using System;﻿
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PopulationController : MonoBehaviour {

	public int population = 0;
	public int maxPopulation = 0;
	public int employedPopulation = 0;
	public int totalNumberOfJobs = 0;
	public float unemploymentValue = 0.0f;
	public float understaffedValue = 0.0f;

	float immigrantEnticement = 1.0f;

	public GameObject immigrantObject;

	public Text populationCounter;
	public GameObject populationHoverToolTip;

	void Start () {
		transform.position = new Vector3(1, 0, 1);
		populationHoverToolTip.SetActive(false);
		UpdatePopulationCounter();

		// main loop for checking population
		StartCoroutine("CheckForNewImmigrants");
	}

	IEnumerator CheckForNewImmigrants()
	{
    	while(true) {
		if (immigrantEnticement > 0.5f && (population < maxPopulation)) {
			Instantiate(immigrantObject, transform.position, transform.rotation);
		}
		if ((maxPopulation - population) > 5) {
			yield return new WaitForSeconds(0.2f);
		} else {
			yield return new WaitForSeconds(1.0f);
		}
    }
	}

	public void ChangeMaxPopulation (int populationChange)
	{
		maxPopulation += populationChange;
		UpdatePopulationCounter();
	}

	public void ChangePopulation (int populationChange)
	{
		population += populationChange;
		UpdatePopulationCounter();
		UpdateEmplymentValues();
	}

	void UpdatePopulationCounter()
	{
		if (populationCounter != null) {
			populationCounter.text = "" + population;

			if ((population == maxPopulation) || (immigrantEnticement < 0.5f)) {
				populationCounter.color = new Color(0.5f, 0f, 0f);
			} else {
				populationCounter.color = new Color(0f, 0f, 0f);
			}
		}
	}

	public void ShowPopulationHoverToolTip()
	{
		if ((population == maxPopulation) && (immigrantEnticement > 0.5f)) {
			populationHoverToolTip.SetActive(true);
		}
	}

	public void HidePopulationHoverToolTip()
	{
		populationHoverToolTip.SetActive(false);
	}

	public int GetAvailableWorkersForBuilding(int numberOfJobs)
	{
		if (unemploymentValue > 0) {
			return numberOfJobs;
		} else if (understaffedValue > 0) {
			return numberOfJobs - ((int) Math.Ceiling(numberOfJobs * understaffedValue));
		}

		return 0;
	}

	public void ChangeTotalNumberOfJobs(int numberOfJobs)
	{
		totalNumberOfJobs += numberOfJobs;
		
		UpdateEmplymentValues();
	}
	
	private void UpdateEmplymentValues()
	{
		if (population > 0) {
			unemploymentValue = (float) (population - totalNumberOfJobs) / (float) population;
		}

		if (totalNumberOfJobs > 0) {
			understaffedValue = (float) (totalNumberOfJobs - population) / (float) totalNumberOfJobs;
		}
	}
}
