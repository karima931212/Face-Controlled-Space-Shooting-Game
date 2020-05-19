using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(LootCrate))]
public class LootCrateMouseController : MonoBehaviour {

	private LootCrate _lootCrate;

	private void Awake()
	{
		_lootCrate = GetComponent<LootCrate>();
	}

	private void OnMouseDown()
	{
		if (_lootCrate.IsOpeningOrClosing()) return;

		if (_lootCrate.IsOpen())
		{
			_lootCrate.Close();
		} else
		{
			_lootCrate.Open();
		}
	}

}
