using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class NavMeshController : MonoBehaviour
{
	public void ScaleAndBake(int xTiles, int yTiles)
	{
		Renderer renderer = GetComponent<Renderer>();
        renderer.enabled = true;

		transform.localScale = new Vector3(xTiles * 0.2f, 1.0f, yTiles * 0.2f);
		transform.position = new Vector3(xTiles - 1.0f, 0.0f, yTiles - 1.0f);

		NavMeshSurface navSurface = gameObject.GetComponent<NavMeshSurface>();

		navSurface.BuildNavMesh();

		renderer.enabled = false;

		BoxCollider collider = GetComponent<BoxCollider>();
		collider.enabled = false;
	}
}
