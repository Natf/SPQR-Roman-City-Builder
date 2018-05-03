using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Farmland : TileObject
{
	public GameObject[] stages;
	public string[] seasonsCanGrowIn;
	public float durationEachStage = 5.0f;
	public int yield;

	private int currentStage = 0;
	private GameObject currentStageModel = null;
	private Farmer farmer = null;

	public override void OnPlace()
	{
		base.OnPlace();

		SetModelForStage();
	}

	public bool HasFarmer()
	{
		return (farmer != null);
	}

	public void SetFarmer(Farmer farmer)
	{
		this.farmer = farmer;
	}

	public void StartGrowing()
	{
		StartCoroutine("Grow");
	}

	public void ResetFarmland()
	{
		StopCoroutine("Grow");
		currentStage = 0;
		SetModelForStage();
		this.farmer = null;
	}

	IEnumerator Grow()
	{
		yield return new WaitForSeconds(5.0f);

		while (currentStage < (stages.Length - 1)) {
			currentStage ++;
			SetModelForStage();
			yield return new WaitForSeconds(durationEachStage);
		}

		NotifyFarmerGrown();
	}

	private void NotifyFarmerGrown()
	{
		if (farmer != null) {
			farmer.FarmlandGrown();
		}
	}

	private void SetModelForStage()
	{
		currentStageModel = SetChildObject(stages[currentStage], currentStageModel);
	}
}
