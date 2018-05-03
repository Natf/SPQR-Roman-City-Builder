using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Well : TileObject
{

	public override void OnPlace()
	{
		base.OnPlace();

		UseRandomRotation();
		ModelInitialize();
	}
}
