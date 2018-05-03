using System;﻿
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class TileObject : MonoBehaviour, ITileObject {

	public static Material redHoverMaterial;
	public static Material hiddenMaterial;
	public static Color fullyFertile = new Color(0.33f, 0.8f, 0.14f, 1.0f);
    public static Color barren = new Color(0.88f, 0.88f, 0.33f, 1.0f); // 225 225 84

	// tile properties
	public TileProperties tileProperties;

	public float fertility;

	// Road access vars
	protected bool roadAbove = false;
	protected bool roadBelow = false;
	protected bool roadLeft = false;
	protected bool roadRight = false;

	protected AudioSource audioSource = null;
	protected Material[] originalMaterials;
	protected bool badModelBeingUsed = false;
	private MaterialPropertyBlock propertyBlock;

	void Start()
	{
		OnPlace();
	}

	public virtual void OnPlace()
	{
		transform.rotation = Quaternion.Euler(new Vector3(-90, 0 ,0));
		propertyBlock = new MaterialPropertyBlock();
	}

	public virtual void ModelInitialize()
	{
		SetColourOfGrassBasedOnFertility();
	}

    private void SetColourOfGrassBasedOnFertility()
    {
        MeshRenderer meshRenderer = GetModelBeingDisplayedForTile().GetComponent<MeshRenderer>();

        meshRenderer.GetPropertyBlock(propertyBlock);

        fertility = Mathf.PerlinNoise(transform.position.x / 50, transform.position.z / 50);

        fertility = BlockyContrastFertility(fertility, 1.5f);
        fertility = AddContrastToFertility(fertility, 1.5f);

        fertility += MapController.mapBaseFertility;

        if (fertility > 1.0f) {
            fertility = 1.0f;
        } else if (fertility <= 0.0f) {
            fertility = 0.0f;
        }

        propertyBlock.SetColor("_GrassColor", Color.Lerp(barren, fullyFertile, fertility));

        meshRenderer.SetPropertyBlock(propertyBlock);
    }

    private float AddContrastToFertility(float originalFertility, float contrastValue)
    {
        float newFertility = Mathf.Pow(originalFertility + 0.5f, contrastValue) - (Mathf.Pow(1.5f, contrastValue) - 1.0f);

        if (newFertility < 0.0f) {
            return 0.0f;
        }

        return newFertility;
    }

    private float BlockyContrastFertility(float originalFertility, float contrastValue)
    {
        if (originalFertility > 0.5f) {
            return originalFertility + ((originalFertility - 0.5f) / contrastValue);
        } else {
            return originalFertility / contrastValue;
        }
    }

	public virtual GameObject GetHoverObject()
	{
		return Instantiate(tileProperties.hoverObject);
	}

	public virtual void HideBadModel()
	{
		if (badModelBeingUsed) {
			GameObject currentModelDisplayed = GetModelBeingDisplayedForTile();

			if (currentModelDisplayed != null) {
				badModelBeingUsed = false;
				MeshRenderer meshRenderer = currentModelDisplayed.GetComponent<MeshRenderer>();
				meshRenderer.materials = originalMaterials;
			}
		}
	}

	public virtual void ShowBadModel(GameObject currentModelDisplayed)
	{
		if (currentModelDisplayed != null) {
			badModelBeingUsed = true;

			MeshRenderer meshRenderer = currentModelDisplayed.GetComponent<MeshRenderer>();
			Material[] materials = meshRenderer.materials;
			originalMaterials = meshRenderer.materials;

			for (int i = 0; i < materials.Length; i++) {
				if(!materials[i].name.Contains("GrassColourable")) {
					if (materials[i].shader.name != "Polygon Wind") {
						Material lerpedMaterial = new Material(materials[i]);
						lerpedMaterial.Lerp(materials[i], redHoverMaterial, 0.5f);
						materials[i] = lerpedMaterial;
					} else {
						materials[i] = redHoverMaterial;
					}
				}
			}

			meshRenderer.materials = materials;
		}
	}

	public virtual void ShowBadModel()
	{
		if (!badModelBeingUsed) {
			GameObject currentModelDisplayed = GetModelBeingDisplayedForTile();
			ShowBadModel(currentModelDisplayed);
		}
	}

	public virtual GameObject SetChildObject(GameObject newChild, GameObject oldChild = null)
	{
		if (oldChild != null) {
			Destroy(oldChild);
		}

		newChild = Instantiate(newChild, transform.position, transform.rotation);
		newChild.transform.parent = gameObject.transform;

		if (badModelBeingUsed) {
			ShowBadModel(newChild);
		}

		return newChild;
	}

	public virtual TileProperties GetTileProperties()
	{
		return tileProperties;
	}

// todo would be good to save this at some point
	public virtual GameObject GetModelBeingDisplayedForTile()
	{
		MeshRenderer meshRenderer = this.gameObject.GetComponent<MeshRenderer>();
		if (meshRenderer != null) {
			return this.gameObject;
		} else {
			meshRenderer = this.gameObject.transform.GetChild(0).gameObject.GetComponent<MeshRenderer>();

			if (meshRenderer != null) {
				return this.gameObject.transform.GetChild(0).gameObject;
			}
		}

		return null;
	}

	public void PlayPlaceSound()
	{
		audioSource = GetComponent<AudioSource>();

		if (audioSource != null) {
			audioSource.Play();
		}
	}

	protected void UseRandomRotation()
	{
		int random = UnityEngine.Random.Range(0, 4);

		if (random < 1) {
			transform.rotation = Quaternion.Euler(new Vector3(-90, 0, 0));
		} else if (random < 2) {
			transform.rotation = Quaternion.Euler(new Vector3(-90, 0, 90));
		} else if (random < 3) {
			transform.rotation = Quaternion.Euler(new Vector3(-90, 0, 180));
		} else {
			transform.rotation = Quaternion.Euler(new Vector3(-90, 0, 270));
		}
	}

    protected void DetectAdjacentBuildings(Action<GameObject> callback = null)
    {
        int layerMask = 1 << TileLayers.BUILDING_LAYER;

        Collider[] hitColliders = Physics.OverlapSphere(transform.position, 2.0f, layerMask);

        int i = 0;
        while (i < hitColliders.Length)
        {
            if (hitColliders[i].gameObject != gameObject) {
                GameObject building = hitColliders[i].gameObject;

                if (
                    (building.transform.position.x > transform.position.x && building.transform.position.z > transform.position.z) ||
                    (building.transform.position.x < transform.position.x && building.transform.position.z > transform.position.z) ||
                    (building.transform.position.x > transform.position.x && building.transform.position.z < transform.position.z) ||
                    (building.transform.position.x < transform.position.x && building.transform.position.z < transform.position.z)
                ) {
                    // ignore corner pieces
                    i++;
                    continue;
                }

                if (callback != null) {
                    callback(building);
                }
            }
            i++;
        }
    }

    protected void DetectAdjacentRoads(bool notifyUpdateToAdjacentRoad = false)
    {
        roadAbove = false;
        roadBelow = false;
        roadLeft = false;
        roadRight = false;

        int layerMask = 1 << TileLayers.ROAD_LAYER;

        Collider[] hitColliders = Physics.OverlapSphere(transform.position, 2.0f, layerMask);

        int i = 0;
        while (i < hitColliders.Length)
        {
            if (hitColliders[i].gameObject != gameObject) {
                GameObject road = hitColliders[i].gameObject;

                if (
                    (road.transform.position.x > transform.position.x && road.transform.position.z > transform.position.z) ||
                    (road.transform.position.x < transform.position.x && road.transform.position.z > transform.position.z) ||
                    (road.transform.position.x > transform.position.x && road.transform.position.z < transform.position.z) ||
                    (road.transform.position.x < transform.position.x && road.transform.position.z < transform.position.z)
                ) {
                    // ignore corner pieces
                    i++;
                    continue;
                }

                if (road.transform.position.x > transform.position.x) {
                    roadAbove = true;
                } else if (road.transform.position.x < transform.position.x) {
                    roadBelow = true;
                } else if (road.transform.position.z > transform.position.z) {
                    roadRight = true;
                } else if (road.transform.position.z < transform.position.z) {
                    roadLeft = true;
                }

                if (notifyUpdateToAdjacentRoad) {
                    Road roadScript = (Road) road.GetComponent(typeof(Road));
                    roadScript.AlignRoad();
                }
            }
            i++;
        }
    }
}
