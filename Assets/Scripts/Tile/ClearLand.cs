using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClearLand : TileObject
{
    private LODGroup lodGroup;

    public void CameraAtCloseRange()
    {
        lodGroup.enabled = true;
    }

    public void CameraAtFarRange()
    {
        lodGroup.enabled = false;
    }

    public override void OnPlace()
    {
        base.OnPlace();

        lodGroup = GetComponent<LODGroup>();
        lodGroup.enabled = false;

        CameraControls.CameraAtCloseRange.AddListener(CameraAtCloseRange);
        CameraControls.CameraAtFarRange.AddListener(CameraAtFarRange);

        ModelInitialize();
        AddRandomGrassPatches();
    }

    private void AddRandomGrassPatches()
    {
        float randomOffset = Random.Range(0.0f, 1.0f) * -1;

        for (float fertilityForGrassLeft = (fertility + randomOffset); fertilityForGrassLeft > 0.2f; fertilityForGrassLeft -= 0.75f) {

    		Instantiate(
                MapController.GetRandomGrassPatch(),
                transform.position + new Vector3(Random.Range(0.0f, 2.0f) - 1.0f, 0, Random.Range(0.0f, 2.0f) - 1.0f),
                transform.rotation,
                gameObject.transform
            );
        }
    }
}
