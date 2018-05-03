using System;﻿
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Building : BurnableTileObject {

	// tile properties
	public bool canCollapse;

	public override void OnPlace()
	{
		base.OnPlace();
        
        if (canBurn || canCollapse) {
            StartCoroutine("UpdateRisks");
        }
	}

    IEnumerator UpdateRisks()
    {
        while (true) {
            FireCheck();

            yield return new WaitForSeconds(5.0f); // change this back to 5.0f when not testing
        }
    }
}
