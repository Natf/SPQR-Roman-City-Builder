using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Tile", menuName = "TileProperties")]
public class TileProperties : ScriptableObject
{
    public new string name;
	[TextArea(5,20)]
	public string description;
	public int buildCost;
	public int clearCost;
	public GameObject hoverObject;
    public BuildingProperties buildingProperties = null;
    public Vector2 tileHalfBox = new Vector2(0.5f, 0.5f);
}
