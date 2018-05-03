using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Prefect : Walker {
    
    public const int BUILDING_LAYER = 10;
    
    public List<BurnableTileObject> firesToPutOut = new List<BurnableTileObject>();
    public BurnableTileObject firePuttingOut = null;
    private NavMeshAgent navMeshAgent;
    
    public override void OnPlace()
    {
        base.OnPlace();
        
    	navMeshAgent = gameObject.GetComponent(typeof(NavMeshAgent)) as NavMeshAgent;
    }
    
    public void AddFire(BurnableTileObject fireToPutOut)
    {
        if (!firesToPutOut.Contains(fireToPutOut)) {
            firesToPutOut.Add(fireToPutOut);
        }
    }
    
	protected override void AtNewTile(Vector3 position)
	{
		base.AtNewTile(position);
        
        ResetBurningRiskForRoadBuildings(position);
        
        CheckIfFiresToPutOut();
	}
    
    protected void ResetBurningRiskForRoadBuildings(Vector3 position)
    {
        int layerMask = 1 << BUILDING_LAYER;
        
        Collider[] hitColliders = Physics.OverlapSphere(position, 4.0f, layerMask);
        
        foreach (Collider hit in hitColliders) {
            BurnableTileObject burnable = hit.gameObject.GetComponent(typeof(BurnableTileObject)) as BurnableTileObject;
            
            if (burnable != null) {
                burnable.ResetFireRisk();
            }
        }
    }
    
    protected void CheckIfFiresToPutOut()
    {
        if (firesToPutOut.Count > 0) {
            StartCoroutine("PutOutNearestFires");
            currentStatus = PUTTING_OUT_FIRE_STATUS;
        }
    }
    
    protected void SetGoToNearestFire()
    {
        BurnableTileObject closestBurnable = null;
        float closestDistance = 0.0f;
        
        for (int i = firesToPutOut.Count - 1; i >= 0; i --) {
            if (firesToPutOut[i] == null || !firesToPutOut[i].onFire) {
                firesToPutOut.RemoveAt(i);
            } else {
                float distanceBetween = Vector3.Distance(transform.position, firesToPutOut[i].gameObject.transform.position);
                
                if (closestDistance > distanceBetween || closestBurnable == null) {
                    closestBurnable = firesToPutOut[i];
                    closestDistance = distanceBetween;
                }
            }
        }
        
        if (closestBurnable != null) {
            firePuttingOut = closestBurnable;
            navMeshAgent.SetDestination(closestBurnable.gameObject.transform.position);
            
            StartCoroutine("CheckAtFire");
        }
    }
    
    IEnumerator PutOutNearestFires()
    {
        while (firesToPutOut.Count > 0) {
            if (firePuttingOut == null) {
                SetGoToNearestFire();
            }
            
            yield return new WaitForSeconds(0.1f);
        }
        
        UpdateWalkDestination();
        currentStatus = PATROLLING_STATUS;
    }
    
    IEnumerator CheckAtFire()
	{
		while ((firePuttingOut != null)) {            
			if (!navMeshAgent.pathPending) {
				if (navMeshAgent.remainingDistance <= navMeshAgent.stoppingDistance) {
		        	if (!navMeshAgent.hasPath || navMeshAgent.velocity.sqrMagnitude == 0f) {
                        firePuttingOut.StopFire();
                        firesToPutOut.Remove(firePuttingOut);
						firePuttingOut = null;
                        navMeshAgent.isStopped = true;
                        navMeshAgent.ResetPath();
                        ResetBurningRiskForRoadBuildings(transform.position);
						yield break;
		         	}
		     	} else if (navMeshAgent.velocity.sqrMagnitude < 1.0f) {
					 // moving too slow so maybe stuck, get new path
					 navMeshAgent.SetDestination(firePuttingOut.gameObject.transform.position);
					 yield return new WaitForSeconds(1.0f);
					 // wait a bit longer to allow us to climb some speed back
					 // and prevent bursts of new path calls
				}
		 	}

			yield return new WaitForSeconds(0.2f);
		}
	}
}
