using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MoneyController : MonoBehaviour {

	public int startingMoney = 10000;
	public int currentMoney;

	public Text moneyCounter;

	void Start()
	{
		currentMoney = startingMoney;
		ShowDenariiValue();
	}

	public bool PurchaseIfPossible(int cost)
	{
		if ((currentMoney - cost) > 0) {
			currentMoney -= cost;
			ShowDenariiValue();
			return true;
		}

		return false;
	}

	void ShowDenariiValue()
	{
		moneyCounter.text = "" + currentMoney;
		
		if (currentMoney < 0) {
			moneyCounter.color = new Color(0.5f, 0f, 0f);
		} else {
			moneyCounter.color = new Color(0f, 0f, 0f);
		}
	}
}
