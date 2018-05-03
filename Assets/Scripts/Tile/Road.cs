using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Road : TileObject {

	public Mesh straightRoad;
	public Mesh cornerRoad;
	public Mesh junction;
	public Mesh crossroads;

	public override void OnPlace()
	{
		base.OnPlace();
		AlignRoad(true);
		DetectAdjacentBuildings(NotifyUpdateToAdjacentBuilding);
		ModelInitialize();
	}

	public void AlignRoad(bool notifyUpdateToAdjacentRoad = false)
	{
		DetectAdjacentRoads(notifyUpdateToAdjacentRoad);

		if (roadAbove && roadBelow && roadLeft && roadRight) {
			GetComponent<MeshFilter>().sharedMesh = crossroads;
		}
		// junctions
		else if (roadAbove && roadRight && roadBelow) {
			GetComponent<MeshFilter>().sharedMesh = junction;
			transform.rotation = Quaternion.Euler(new Vector3(-90, 0, 180));
		}  else if (roadRight && roadBelow && roadLeft) {
			GetComponent<MeshFilter>().sharedMesh = junction;
			transform.rotation = Quaternion.Euler(new Vector3(-90, 0, 90));
		} else if (roadBelow && roadLeft && roadAbove) {
			GetComponent<MeshFilter>().sharedMesh = junction;
			transform.rotation = Quaternion.Euler(new Vector3(-90, 0, 0));
		} else if (roadLeft && roadAbove && roadRight) {
			GetComponent<MeshFilter>().sharedMesh = junction;
			transform.rotation = Quaternion.Euler(new Vector3(-90, 0, 270));
		}
		// corners
		else if (roadAbove && roadRight) {
			GetComponent<MeshFilter>().sharedMesh = cornerRoad;
			transform.rotation = Quaternion.Euler(new Vector3(-90, 0, 90));
		} else if (roadRight && roadBelow) {
			GetComponent<MeshFilter>().sharedMesh = cornerRoad;
			transform.rotation = Quaternion.Euler(new Vector3(-90, 0, 0));
		} else if (roadBelow && roadLeft) {
			GetComponent<MeshFilter>().sharedMesh = cornerRoad;
			transform.rotation = Quaternion.Euler(new Vector3(-90, 0, 270));
		} else if (roadLeft && roadAbove) {
			GetComponent<MeshFilter>().sharedMesh = cornerRoad;
			transform.rotation = Quaternion.Euler(new Vector3(-90, 0, 180));
		}
	 	// straight roads
		else if (roadAbove || roadBelow) {
			GetComponent<MeshFilter>().sharedMesh = straightRoad;
			transform.rotation = Quaternion.Euler(new Vector3(-90, 0, 90));
		} else if (roadLeft || roadRight) {
			GetComponent<MeshFilter>().sharedMesh = straightRoad;
			transform.rotation = Quaternion.Euler(new Vector3(-90, 0, 0));
		} else {
			GetComponent<MeshFilter>().sharedMesh = straightRoad;
		}
	}

	private void OnDestroy()
	{
		// gameObject.layer = 0;
		DetectAdjacentRoads(true);
		DetectAdjacentBuildings(NotifyUpdateToAdjacentBuilding);
	}

	void NotifyUpdateToAdjacentBuilding(GameObject building)
	{
		WalkerBuilding buildingScript = (WalkerBuilding) building.GetComponent(typeof(WalkerBuilding));

		if (buildingScript != null) {
			buildingScript.UpdateRoadAccess();
		}
	}
}
