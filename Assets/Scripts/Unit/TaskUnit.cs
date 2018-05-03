using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class TaskUnit : Unit
{
    protected NavMeshAgent navMeshAgent;

    void Start()
    {
        navMeshAgent = gameObject.GetComponent(typeof(NavMeshAgent)) as NavMeshAgent;
        GetDestination();
        StartCoroutine("MoveToDestination");
    }

    protected virtual void GetDestination() {}

    protected virtual void AtTargetDestination()
    {
        DoTask();
    }

    protected virtual void DoTask() {}

    protected virtual IEnumerator MoveToDestination()
    {
        while (true) {
			if (!navMeshAgent.pathPending) {
				if (navMeshAgent.remainingDistance <= navMeshAgent.stoppingDistance) {
		        	if (!navMeshAgent.hasPath || navMeshAgent.velocity.sqrMagnitude == 0f) {
                        AtTargetDestination();
						yield break;
		         	}
		     	} else if (navMeshAgent.velocity.sqrMagnitude < 1.0f) {
					 // moving too slow so maybe stuck, get new path
                     GetDestination();
					 yield return new WaitForSeconds(1.0f);
					 // wait a bit longer to allow us to climb some speed back
					 // and prevent bursts of new path calls
				}
		 	}

			yield return new WaitForSeconds(0.2f);
		}
    }
}
