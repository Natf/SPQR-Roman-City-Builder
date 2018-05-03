using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Farmer : TaskUnit
{
	private const float FIELD_RANGE = 10.0f;
	Farmland farmLand;

	public void FarmlandGrown()
	{
		if (farmLand != null) {
			farmLand.ResetFarmland();
			MoveBackHome();
		}
	}

	protected override void GetDestination()
	{
		if (farmLand != null) {
			farmLand.SetFarmer(null);
			farmLand = null;
		}

		int layerMask = 1 << TileLayers.FARMLAND_LAYER;

        Collider[] hitColliders = Physics.OverlapSphere(transform.position, FIELD_RANGE, layerMask);

        int i = 0;
        while (i < hitColliders.Length)
        {
            if (hitColliders[i].gameObject != gameObject) {
                GameObject farmTile = hitColliders[i].gameObject;
				farmLand = farmTile.GetComponent(typeof(Farmland)) as Farmland;

                if (farmLand != null && !farmLand.HasFarmer()) {
					farmLand.SetFarmer(this);
					navMeshAgent.SetDestination(farmTile.transform.position);
					return;
                }
            }
            i++;
        }

		Destroy(gameObject);
		// if we get here we couldn't find a free piece of farmland
	}

	protected override void DoTask()
	{
		farmLand.StartGrowing();
	}

	protected void MoveBackHome()
	{
		Destroy(gameObject);
	}
}
