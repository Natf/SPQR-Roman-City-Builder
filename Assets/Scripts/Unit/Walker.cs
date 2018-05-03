using System;﻿
﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Walker : Unit {
	public const float TILE_WIDTH = 2.0f;
	
	public const int PATROLLING_STATUS = 0;
	public const int PUTTING_OUT_FIRE_STATUS = 1;
	
	public float walkDuration = 20.0f;
	
	List<Vector3> allDirectionVectors;
	protected bool roadAbove = false;
	protected bool roadBelow = false;
	protected bool roadLeft = false;
	protected bool roadRight = false;
	protected int totalPossibleDirections = 0;
	protected int previousDirection = -1; // 0 - UP, 1 - RIGHT, 2 - DOWN, 3 - LEFT
	protected Vector3 nearestTile;
	protected Vector3 previousNearestTile;
	protected Vector3 targetDestination;
	
	protected int currentStatus;
	
	public Vector3 destination;
	
	void Start()
	{
		OnPlace();
	}
	
	public virtual void OnPlace()
	{
		allDirectionVectors = new List<Vector3>();
		
		// Up
		allDirectionVectors.Add(new Vector3(TILE_WIDTH, 0, 0));
		// Right
		allDirectionVectors.Add(new Vector3(0, 0, TILE_WIDTH));
		// Down
		allDirectionVectors.Add(new Vector3(TILE_WIDTH * -1, 0, 0));
		// Left
		allDirectionVectors.Add(new Vector3(0, 0, TILE_WIDTH * -1));
		
		currentStatus = PATROLLING_STATUS;
		
		UpdateWalkDestination();
		StartCoroutine("EndWalk");
	}
	
	void Update()
	{
		if (currentStatus == PATROLLING_STATUS) {
			if (Vector3.Distance(transform.position, targetDestination) < 0.1) {
				UpdateWalkDestination();
			}
			float step = patrolSpeed * Time.deltaTime;
			transform.position = Vector3.MoveTowards(transform.position, targetDestination, step);
		}
	}
	
	protected void UpdateWalkDestination()
	{
		DetectAdjacentRoads();
		
		if (!(roadBelow || roadAbove || roadLeft || roadRight)) {
			Destroy(gameObject);
		}
		
		Vector3 destination = nearestTile;
		
		if (totalPossibleDirections == 1) {
			if (roadAbove) {
				destination += allDirectionVectors[0];
				previousDirection = 0;
			} else if (roadRight) {
				destination += allDirectionVectors[1];
				previousDirection = 1;
			} else if (roadBelow) {
				destination += allDirectionVectors[2];
				previousDirection = 2;
			} else {
				destination += allDirectionVectors[3];
				previousDirection = 3;
			}
		} else {
			destination += ContinueOrPickRandomDirection();
		}
		
		// this is kinda sloppy
		if (previousNearestTile != nearestTile) {
			RotateToDirection();
			AtNewTile(nearestTile);
		} else {
			RotateToDirection();
		}
		
		targetDestination = destination;
	}
	
	protected virtual void AtNewTile(Vector3 position)
	{
		previousNearestTile = nearestTile;
	}
	
	Vector3 ContinueOrPickRandomDirection()
	{
		List<int> possibleDirections = new List<int>();
		
		if (roadAbove) {
			possibleDirections.Add(0);
		}
		if (roadRight) {
			possibleDirections.Add(1);
		}
		if (roadBelow) {
			possibleDirections.Add(2);
		}
		if (roadLeft) {
			possibleDirections.Add(3);
		}
		
		// remove direction opposite to which we just came as doesn't make sense to just turn back randomly
		if (previousDirection != -1) {
			int reverseDirection = previousDirection + 2;
			
			if (reverseDirection > 3) {
				reverseDirection -= 4;
			}
			
			if (possibleDirections.Contains(reverseDirection)) {
				possibleDirections.Remove(reverseDirection);
			}
		}
		
		previousDirection = possibleDirections[UnityEngine.Random.Range(0, possibleDirections.Count)];
		
		return allDirectionVectors[previousDirection];
	}
	
	void RotateToDirection()
	{
		switch (previousDirection) 
		{
			case 0:
				transform.rotation = Quaternion.Euler(new Vector3(0, 90, 0));
				break;
			case 1:
				transform.rotation = Quaternion.Euler(new Vector3(0, 0, 0));
				break;
			case 2:
				transform.rotation = Quaternion.Euler(new Vector3(0, 270, 0));
				break;
			case 3:
				transform.rotation = Quaternion.Euler(new Vector3(0, 180, 0));
				break;
			default:
				Debug.LogWarning("Invalid direction set " + previousDirection);
			break;
		}
	}
	
	IEnumerator EndWalk()
	{
		yield return new WaitForSeconds(walkDuration);
		
		while (true) {
			// dont destroy the prefect if he's in the middle of a task, seems a bit dumb
			if (currentStatus == PATROLLING_STATUS) {
				Destroy(gameObject);
			}
			
			yield return new WaitForSeconds(1.0f);
		}
	}
	
	protected void DetectAdjacentRoads()
	{
		roadAbove = false;
		roadBelow = false;
		roadLeft = false;
		roadRight = false;
		totalPossibleDirections = 0;

		int layerMask = 1 << TileLayers.ROAD_LAYER;
		
		nearestTile = new Vector3((float)(Math.Round((transform.position.x/2)) *2), 0.0f, (float)(Math.Round(transform.position.z/2)*2));

		Collider[] hitColliders = Physics.OverlapSphere(nearestTile, 2.0f, layerMask);

        int i = 0;
        while (i < hitColliders.Length)
        {
			if (hitColliders[i].gameObject != gameObject) {
				GameObject road = hitColliders[i].gameObject;
				
				if (road.transform.position == nearestTile) {
					// dont want to count the road we're standing on
					i++;
					continue;
				}

				if (
					(road.transform.position.x > nearestTile.x && road.transform.position.z > nearestTile.z) ||
					(road.transform.position.x < nearestTile.x && road.transform.position.z > nearestTile.z) ||
					(road.transform.position.x > nearestTile.x && road.transform.position.z < nearestTile.z) ||
					(road.transform.position.x < nearestTile.x && road.transform.position.z < nearestTile.z)
				) {
					// ignore corner pieces
					i++;
					continue;
				}

				if (road.transform.position.x > nearestTile.x) {
					roadAbove = true;
					totalPossibleDirections++;
				} else if (road.transform.position.x < nearestTile.x) {
					roadBelow = true;
					totalPossibleDirections++;
				} else if (road.transform.position.z > nearestTile.z) {
					roadRight = true;
					totalPossibleDirections++;
				} else if (road.transform.position.z < nearestTile.z) {
					roadLeft = true;
					totalPossibleDirections++;
				}
			}
            i++;
        }
	}
}
