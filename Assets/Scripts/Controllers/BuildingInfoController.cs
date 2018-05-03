using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class BuildingInfoController : MonoBehaviour
{
	public GameObject buildingInfoPanel;
	public Text buildingName;
	public Text buildingDescription;
	public Text landFertility;
	public GameObject buildingRequirements;
	public Text buildingWorkerCount;
	public Text buildingRoadRequired;

	private Vector3 beginDrag;
	private Vector3 lastPosition;
	private bool infoOpen = false;
	private TileController tileController;

	void Start()
	{
		CloseBuildingInfo();
		tileController = GameObject.Find("GameUI").GetComponent(typeof(TileController)) as TileController;
	}

	void Update()
	{
		if (Input.GetMouseButtonDown(1) && !tileController.PlacingTile()) {
			if (infoOpen) {
				CloseBuildingInfo();
			} else {
				TryToGetInfo();
			}
		}
	}

	public void TryToGetInfo()
	{
		Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
		RaycastHit hit;

		if (Physics.Raycast(ray, out hit, 500)) {
			TileObject tileObject = hit.collider.gameObject.GetComponent(typeof(TileObject)) as TileObject;
			if (tileObject != null) {
				ShowTileInfo(tileObject.tileProperties);
			}
		}
	}

	public void BeginDrag()
	{
		beginDrag = Input.mousePosition;
	}

	public void OnDrag()
	{
		buildingInfoPanel.transform.position += (Input.mousePosition - beginDrag);

		lastPosition = buildingInfoPanel.transform.position;

		beginDrag = Input.mousePosition;
	}

	public void ShowBuildingInfo(BuildingProperties buildingProperties)
	{
		buildingRequirements.SetActive(true);
		buildingWorkerCount.text = "todo get current workers/" + buildingProperties.workersRequired + " Employees";

		// if (walkerBuilding.requiresRoadAccess && !walkerBuilding.HasRoadAccess()) {
		// 	buildingRoadRequired.gameObject.SetActive(true);
		// } else {
		// 	buildingRoadRequired.gameObject.SetActive(false);
		// }
	}

	public void ShowTileInfo(TileProperties tileProperties)
	{
		if (tileProperties.buildingProperties != null){
			ShowBuildingInfo(tileProperties.buildingProperties);
		} else {
			buildingRequirements.SetActive(false);
		}

		infoOpen = true;
		buildingName.text = tileProperties.name;
		buildingDescription.text = tileProperties.description;
		buildingInfoPanel.transform.position = lastPosition;

		landFertility.text = "Land is todo get fertility% fertile.";
		buildingInfoPanel.SetActive(true);
	}

	public void CloseBuildingInfo()
	{
		lastPosition = buildingInfoPanel.transform.position;
		buildingInfoPanel.transform.position = new Vector3(-1 * Screen.width, -100, 0);
		infoOpen = false;
		buildingInfoPanel.SetActive(false);
	}
}
