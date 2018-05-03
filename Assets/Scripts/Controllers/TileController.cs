using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

[System.Serializable]
public class TileController : MonoBehaviour {

	public GameObject[] allTiles;
	public EventSystem eventSystem;
	public GameObject moneyControllerObject;
	public Texture2D demolishCursorTexture;
    public Vector2 hotSpot;

	private bool placingTile = false;
	private bool clearingTile = false;
	private GameObject tileToPlace;
	private GameObject hoverTileBeingPlaced = null;
	private MoneyController moneyController;
	private Quaternion defaultRotation;
	private CursorMode cursorMode = CursorMode.Auto;
	private TileObject tileToShowBadModelWhenClearing = null;
	private Material[] hoverTileOriginalMaterials;
	private bool hoverObjectUsingSafe = true;

	void Start()
	{
		moneyController = moneyControllerObject.GetComponent(typeof(MoneyController)) as MoneyController;
		defaultRotation = Quaternion.Euler(new Vector3(-90, 0, 0));
	}

	void LateUpdate()
	{
		if (Input.GetMouseButtonDown(0) && !eventSystem.IsPointerOverGameObject()) {
			if (clearingTile) {
				TryToClearTile();
			} else if (placingTile) {
				TryToPlaceTile();
			}
		}

		if (Input.GetMouseButtonDown(1)) {
			ClearPlacingTile();
		}

		if (hoverTileBeingPlaced != null) {
			PositionCorrectModelAtNearestTile();
		} else if (clearingTile) {
			ShowBadModelForTileNearestMouse();
		}
	}

	public bool PlacingTile()
	{
		return placingTile || clearingTile;
	}

	public void TryToClearTile()
	{
		GameObject clickedTile = GetTileAtMouse();

		if (clickedTile != null) {
			if (clickedTile.tag != "ClearLand") {
				TileObject tileToPlaceScript = tileToPlace.GetComponent(typeof(TileObject)) as TileObject;

				if (moneyController.PurchaseIfPossible(tileToPlaceScript.tileProperties.buildCost)) {
					Vector3 objectPosition = clickedTile.transform.position;
					Quaternion objectRotation = clickedTile.transform.rotation;

					Destroy(clickedTile);
					GameObject tilePlaced = Instantiate(tileToPlace, objectPosition, objectRotation);
					TileObject tileObject = tilePlaced.GetComponent(typeof(TileObject)) as TileObject;

					if (tileObject != null) {
						tileObject.PlayPlaceSound();
					}
				}
			}
		}
	}

	public void TryToPlaceTile()
	{
		GameObject clickedTile = GetTileAtMouse();

		if (clickedTile != null) {
			if (clickedTile.tag == "ClearLand") {
				TileObject tileToPlaceScript = tileToPlace.GetComponent(typeof(TileObject)) as TileObject;

				if (moneyController.PurchaseIfPossible(tileToPlaceScript.tileProperties.buildCost)) {
					Vector3 objectPosition = clickedTile.transform.position;
					Quaternion objectRotation = clickedTile.transform.rotation;

					Destroy(clickedTile);
					GameObject tilePlaced = Instantiate(tileToPlace, objectPosition, objectRotation);
					TileObject tileObject = tilePlaced.GetComponent(typeof(TileObject)) as TileObject;

					if (tileObject != null) {
						tileObject.PlayPlaceSound();
					}
				}
			}
		}
	}

	public void ClearPlacingTile()
	{
		clearingTile = false;
		placingTile = false;
		Cursor.SetCursor(null, hotSpot, cursorMode);
		ClearBadModelForPreviousHover();

		ClearHoverTile();
	}

	public void SelectGrass()
	{
		SelectTile(0);
	}

	public void SelectHouse()
	{
		SelectTile(1);
	}

	public void SelectRoad()
	{
		SelectTile(2);
	}

	public void SelectWell()
	{
		SelectTile(3);
	}

	public void SelectPrefecture()
	{
		SelectTile(4);
	}

	public void SelectFarmlandWheat()
	{
		SelectTile(5);
	}

	private GameObject GetTileAtMouse()
	{
		int layerMask = ~((1 << TileLayers.UNIT_LAYER) | (1 << TileLayers.HOVER_TILE_LAYER));
		Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
		RaycastHit hit;

		if (Physics.Raycast(ray, out hit, 500, layerMask)) {
			return hit.collider.gameObject;
		}

		return null;
	}

	private void SelectTile(int tileNo)
	{
		ClearBadModelForPreviousHover();
		ClearHoverTile();

		placingTile = true;
		tileToPlace = allTiles[tileNo];

		if (tileToPlace.tag == "ClearLand") {
			Cursor.SetCursor(demolishCursorTexture, hotSpot, cursorMode);
			clearingTile = true;
		} else {
			Cursor.SetCursor(null, hotSpot, cursorMode);
			clearingTile = false;
		}

		TileObject tileObject = tileToPlace.GetComponent(typeof(TileObject)) as TileObject;

		if (!clearingTile) {
			InstantiateNewHoverTile(tileObject);
		}
	}

	private void InstantiateNewHoverTile(TileObject tileObject)
	{
		ClearHoverTile();

		TileProperties tileProperties = tileObject.tileProperties;

		if (tileProperties.hoverObject != null) {
			hoverTileBeingPlaced = Instantiate(tileProperties.hoverObject);
			hoverTileBeingPlaced.layer = TileLayers.HOVER_TILE_LAYER;
		}

		HideGrassMaterialForHoverObject();

		if (hoverTileBeingPlaced != null) {
			PositionCorrectModelAtNearestTile();
		} else if (clearingTile) {
			ShowBadModelForTileNearestMouse();
		}
	}

	private void ClearHoverTile()
	{
		if (hoverTileBeingPlaced != null) {
			hoverObjectUsingSafe = true;
			Destroy(hoverTileBeingPlaced);
			hoverTileBeingPlaced = null;
		}
	}

	private void PositionCorrectModelAtNearestTile()
	{
		GameObject gameObjectAtMouse = GetTileAtMouse();

		if (gameObjectAtMouse != null) {
			TileObject tileObject = gameObjectAtMouse.GetComponent(typeof(TileObject)) as TileObject;

			if (tileObject != null) {
				if (gameObjectAtMouse.tag == "ClearLand") {
					hoverTileBeingPlaced.transform.position = gameObjectAtMouse.transform.position;
					SetHoverObjectSafeMaterials();
				} else {
					hoverTileBeingPlaced.transform.position = gameObjectAtMouse.transform.position + new Vector3(0f, 0.001f, 0f);
					SetHoverObjectBadMaterials();
				}
			}
		}
	}

	private void ShowBadModelForTileNearestMouse()
	{
		GameObject gameObjectAtMouse = GetTileAtMouse();

		if (gameObjectAtMouse != null && gameObjectAtMouse.tag != "ClearLand") {
			TileObject tileObject = gameObjectAtMouse.GetComponent(typeof(TileObject)) as TileObject;

			if (tileObject != null) {
				if (tileToShowBadModelWhenClearing != tileObject) {
					ClearBadModelForPreviousHover();
				}

				tileToShowBadModelWhenClearing = tileObject;
				tileObject.ShowBadModel();

				return;
			}
		}

		ClearBadModelForPreviousHover();
	}

	private void ClearBadModelForPreviousHover()
	{
		if (tileToShowBadModelWhenClearing != null) {
			tileToShowBadModelWhenClearing.HideBadModel();
			tileToShowBadModelWhenClearing = null;
		}
	}

	private void SetHoverObjectSafeMaterials()
	{
		if (!hoverObjectUsingSafe) {
			hoverObjectUsingSafe = true;

			hoverTileBeingPlaced.transform.localScale = new Vector3(1f, 1f, 1f);

			MeshRenderer meshRenderer = hoverTileBeingPlaced.GetComponent<MeshRenderer>();
			meshRenderer.materials = hoverTileOriginalMaterials;
		}
	}

	private void SetHoverObjectBadMaterials()
	{
		if (hoverObjectUsingSafe) {
			hoverObjectUsingSafe = false;

			hoverTileBeingPlaced.transform.localScale = new Vector3(1.0001f, 1.0001f, 1.0001f);

			MeshRenderer meshRenderer = hoverTileBeingPlaced.GetComponent<MeshRenderer>();
			Material[] materials = meshRenderer.materials;
			hoverTileOriginalMaterials = meshRenderer.materials;

			for (int i = 0; i < materials.Length; i++) {
				if (!materials[i].shader.name.Contains("Polygon Wind")) {
					Material lerpedMaterial = new Material(materials[i]);
					lerpedMaterial.Lerp(materials[i], TileObject.redHoverMaterial, 0.5f);
					materials[i] = lerpedMaterial;
				} else {
					materials[i] = TileObject.redHoverMaterial;
				}
			}

			meshRenderer.materials = materials;
		}
	}

	private void HideGrassMaterialForHoverObject()
	{
		if (hoverTileBeingPlaced != null) {
			MeshRenderer meshRenderer = hoverTileBeingPlaced.GetComponent<MeshRenderer>();
			Material[] materials = meshRenderer.materials;
			hoverTileOriginalMaterials = meshRenderer.materials;

			for (int i = 0; i < materials.Length; i++) {
				if(materials[i].name.Contains("GrassColourable")) {
					materials[i] = TileObject.hiddenMaterial;
				}
			}

			meshRenderer.materials = materials;
		}
	}
}
