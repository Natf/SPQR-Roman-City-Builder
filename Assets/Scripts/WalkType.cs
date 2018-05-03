using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class WalkType : ScriptableObject {
	public int speed;
	public new string name;
	public int walkLevel; // start at 0 meaning no units, 1 meaning road ignorers and 2 meaning all
}
