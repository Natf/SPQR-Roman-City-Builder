using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;
using System.IO;
using System.Xml.Linq;
using UnityEngine.AI;
// using System.Diagnostics;

[System.Serializable]
public class MapController : MonoBehaviour {
	public const string PATH_TO_RESOURCE_FOLDER = "Assets/Resources/";
	public const string PATH_TO_TILES = "Tiles";
	public const string PATH_TO_SCENERY = "Scenery/";

	public static float mapBaseFertility = 0.5f;

	public Dictionary<string, GameObject> mapTiles;

	private XDocument mapFile;
	private IEnumerable<XElement> mapRows;
	private bool finishedLoading = false;
	private string[,] mapData;
	private NavMeshController navMeshController;

	public static Object[] grassPatches;

	void Start()
	{
		Debug.Log("loading...");
		navMeshController = GameObject.Find("WorldNavMesh").GetComponent(typeof(NavMeshController)) as NavMeshController;
		TileObject.redHoverMaterial = Resources.Load("Materials/DeleteHover") as Material;
		TileObject.hiddenMaterial = new Material(TileObject.redHoverMaterial);
		TileObject.hiddenMaterial.color = new Color(0f, 0f, 0f, 0f);
		DontDestroyOnLoad (gameObject);
		LoadAllTiles();
		// ReadMapFromXML("Test");
		GenerateFlatLandMap(100, 100);
	}

	void Update() {
		if (finishedLoading) {
			finishedLoading = false;
			Debug.Log("Finished Reading");
			BakeNavMesh();
			StartCoroutine("InstantiateMap");
		}
	}

	public static GameObject GetRandomGrassPatch()
	{
		if (grassPatches != null) {
			return (GameObject) grassPatches[Random.Range(0, grassPatches.Length)];
		}

		return null;
	}

	void BakeNavMesh()
	{
		Debug.Log("Baking Nav Mesh...");
		navMeshController.ScaleAndBake(mapData.GetLength(0), mapData.GetLength(1));
	}

	void LoadAllTiles()
	{
		Object[] mapTilesToLoad = Resources.LoadAll(PATH_TO_TILES, typeof(GameObject));
		mapTiles = new Dictionary<string, GameObject>();

		foreach (GameObject tile in mapTilesToLoad) {
			mapTiles.Add(tile.name, tile);
		}

		grassPatches = Resources.LoadAll(PATH_TO_SCENERY, typeof(GameObject));

		TileObject.redHoverMaterial = Resources.Load("Materials/DeleteHover") as Material;

		Debug.Log(grassPatches);
	}

	void GenerateFlatLandMap(int width, int height)
	{
		Debug.Log("Generating flat map tiles...");
		mapData = new string[width, height];

		for (int x = 0; x < width; x++) {
			for (int y = 0; y < width; y++) {
				mapData[x, y] = "ClearLand";
			}
		}

		finishedLoading = true;
	}

	void ReadMapFromXML (string mapName) {
		mapFile = XDocument.Load("Assets/Maps/" + mapName + ".cbmap");
		XElement mapMeta = mapFile.Element("map_data").Element("map_meta");

		int mapWidth = int.Parse(mapMeta.Element("map_x_size").Value);
		int mapHeight = int.Parse(mapMeta.Element("map_y_size").Value);

		mapData = new string[mapWidth, mapHeight];

		mapRows = mapFile.Descendants("map_tile_row");

		foreach (var mapRow in mapRows) {
			foreach (var mapTile in mapRow.Elements()) {
				int x = int.Parse(mapRow.Attribute("x").Value);
				int y = int.Parse(mapTile.Attribute("y").Value);
				string mapTileValue = mapTile.Value;

				mapData[x, y] = mapTileValue;
			}
		}

		finishedLoading = true;
	}

	IEnumerator InstantiateMap()
	{
		Debug.Log("Instantiating world...");
		GameObject tileToInstantiate;

		for(int x=0; x < mapData.GetLength(0); x++) {
    		for(int y=0; y < mapData.GetLength(1); y++) {
				if (mapTiles.ContainsKey(mapData[x, y])) {
					tileToInstantiate = mapTiles[mapData[x, y]];
			 	} else {
					Debug.LogError("No tile found for - " + mapData[x,y] + ". Placing ClearLand instead.");

					tileToInstantiate = mapTiles["ClearLand"];
				}

				Instantiate(tileToInstantiate, new Vector3(x*2, 0, y*2), transform.rotation);
			}
			yield return new WaitForSeconds(0.01f);
		}
	}
}
