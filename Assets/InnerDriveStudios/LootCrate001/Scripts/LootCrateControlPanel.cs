using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/**
 * Simple loot crate control script for the UI.
 * 
 * @author J.C. Wichman - Inner Drive Studios
 */
public class LootCrateControlPanel : MonoBehaviour {

	[SerializeField] private Text statusText;
	[SerializeField] private Text steamText;
	[SerializeField] private Text materialText;
	[SerializeField] private LootCrate lootCrate;

	[SerializeField] private Material[] materials;
	[SerializeField] private bool autoPlayOnStartup = false;

	private MeshRenderer[] renderers;
	private int _currentMaterialIndex = 0;

	private void Awake()
	{
		renderers = lootCrate.GetComponentsInChildren<MeshRenderer>();

		if (autoPlayOnStartup) StartCoroutine(autoPlay());
	}

	private void Update()
	{
		//update the status if required
		if (statusText != null)
		{
			string status = "Status:";

			if (lootCrate.IsOpen())
			{
				status += "OPEN";
			}
			else if (lootCrate.IsClosed())
			{
				status += "CLOSED";
			} else
			{
				status += " - ";
			}

			status += " / ";

			if (lootCrate.IsOpening())
			{
				status += "OPENING";
			}
			else if (lootCrate.IsClosing())
			{
				status += "CLOSING";
			} else
			{
				status += "-";
			}

			statusText.text = status;
		}
		//update the material info if required
		if (materialText != null)
		{
			if (materials.Length == 0)
			{
				materialText.text = "-";
			}
			else
			{
				if (_currentMaterialIndex >= 0)
				{
					materialText.text = "" + (_currentMaterialIndex + 1) + "/" + materials.Length;
				}
				else
				{
					materialText.text = "R";
				}
			}
		}

		if (steamText != null)
		{
			steamText.text = "Steam mode:"+lootCrate.GetSteamMode();
		}
	}

	public void NextMaterial()
	{
		if (materials.Length == 0) return;
		_currentMaterialIndex = (_currentMaterialIndex + 1) % materials.Length;
		updateMaterial();
	}

	public void PreviousMaterial()
	{
		if (materials.Length == 0) return;
		_currentMaterialIndex = (_currentMaterialIndex - 1 + materials.Length) % materials.Length;
		updateMaterial();
	}

	public void RandomMaterial()
	{
		if (materials.Length == 0) return;
		_currentMaterialIndex = -1;
		updateMaterial();
	}

	private void updateMaterial()
	{
		for (int i = 0; i < renderers.Length; i++)
		{
			renderers[i].sharedMaterial = 
				materials[_currentMaterialIndex>-1?_currentMaterialIndex:Random.Range(0, materials.Length)];
		}
	}

	private IEnumerator autoPlay()
	{
		yield return new WaitForSeconds(2);

		while (true)
		{
			lootCrate.Open();
			yield return new WaitForSeconds(4f);
			lootCrate.Close();
			yield return new WaitForSeconds(1f);
			NextMaterial();
			yield return new WaitForSeconds(2f);
		}
	}


}
