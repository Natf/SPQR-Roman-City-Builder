using System;﻿
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class BurnableTileObject : TileObject {
    
    public const string PATH_TO_FIRE_MODEL = "Models/FireModel";
    public const string PATH_TO_RUBBLE_MODEL = "Models/RubbleModel";
    public const float RANGE_TO_CALL_PREFECT = 100.0f;
    
	public bool canBurn;
	public float fireChanceIncrease = 0.05f;

    // risk variables
    public bool onFire = false;
    public float fireChance = 0.0f;
    protected GameObject fireObject;
    protected GameObject rubbleObject;

    // our main function called by building
    public void FireCheck()
    {
        if (!onFire && canBurn) {
            fireChance += fireChanceIncrease;

            if (fireChance >= 1.0f) {
                if (UnityEngine.Random.Range(0, 10) < (1 + Math.Floor((fireChance - 1.0f) * 9))) {
                    StartFire();
                }
            }
        }
    }

	public void ResetFireRisk()
	{
		fireChance = 0.0f;
	}

	public void StopFire()
	{
		if (onFire && fireObject != null) {
			onFire = false;
			fireChance = 0.0f;
			Destroy(fireObject);
            StopCoroutine("OnFire");
            StopCoroutine("BurnDown");

			if (gameObject.name == "Rubble") {
				gameObject.tag = "Untagged";
				Destroy(this);
			}
		}
	}
    
	IEnumerator BurnDown()
	{
		yield return new WaitForSeconds(15.0f);

		BuildingBurntDown();

		yield return new WaitForSeconds(5.0f);

		StopFire();

		yield break;
	}
	
	IEnumerator OnFire()
	{
		while (true) {
			DetectAdjacentBuildings(TryToSpreadFire);
            AlertNearbyPrefectsOfFire();
			yield return new WaitForSeconds(1.0f);
		}
	}

	protected virtual void BuildingBurntDown()
	{
		SetRubbleObjectOnly();
		canBurn = false;
		StopCoroutine("UpdateRisks");
	}

	protected void StartFire()
	{
		onFire = true;

		GameObject fireModel = Resources.Load(PATH_TO_FIRE_MODEL, typeof(GameObject)) as GameObject;
		fireObject = Instantiate(fireModel, transform.position, transform.rotation) as GameObject;
		fireObject.transform.parent = gameObject.transform;
		
		StartCoroutine("OnFire");
		StartCoroutine("BurnDown");
        
        AlertNearbyPrefectsOfFire();
	}
    
    protected void AlertNearbyPrefectsOfFire()
    {
        int layerMask = 1 << TileLayers.UNIT_LAYER;

        Collider[] hitColliders = Physics.OverlapSphere(transform.position, RANGE_TO_CALL_PREFECT, layerMask);

        int i = 0;
        while (i < hitColliders.Length)
        {
            if (hitColliders[i].gameObject != gameObject) {
                GameObject unit = hitColliders[i].gameObject;
                
                if (unit.tag == "Prefect") {
                    Prefect prefectScript = unit.GetComponent(typeof(Prefect)) as Prefect;
                    
                    if (prefectScript != null) {
                        prefectScript.AddFire(this);
                    }
                }
            }
            i++;
        }
    }
	
	protected void TryToSpreadFire(GameObject burnable)
	{
		BurnableTileObject burnableScript = (BurnableTileObject) burnable.GetComponent(typeof(TileObject));
		
		if (burnableScript != null) {
			burnableScript.AddToFireRiskFromNearbyBurningBuilding();
		}
	}
	
	protected void AddToFireRiskFromNearbyBurningBuilding()
	{
		if (!onFire && canBurn) {
			fireChance += 0.1f;
		}
	}

	protected void SetRubbleObjectOnly()
	{
		gameObject.name = "Rubble";

		foreach (Transform child in transform) {
			if (child.gameObject != fireObject) {
				Destroy(child.gameObject);
			}
		}

		GameObject rubbleModel = Resources.Load(PATH_TO_RUBBLE_MODEL, typeof(GameObject)) as GameObject;
		rubbleObject = Instantiate(rubbleModel, transform.position, transform.rotation) as GameObject;
		rubbleObject.transform.parent = gameObject.transform;
	}
}