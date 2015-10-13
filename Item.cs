using UnityEngine;
using System.Collections;
using System;

//[System.Serializable]
public class Item : MonoBehaviour
{

public enum ItemType {BATTERY, TORCH, CROWBAR};
public enum Quality {COMMON,UNCOMMON,RARE,EPIC,LEGENDARY,ARTIFACT};


	public ItemType type;
	
	public Quality quality;
	
	public Sprite spriteNeutral;
	public Sprite spriteHighlighted;
	public int maxSize; // defines how many we can stack - if its #1, it can't be stacked
	
	public float strength, intellect, agility, stamina; // these are set in the inspector
	public string itemName;
	public string description;

	public void Use ()
	{
		switch (type)
		{
			case ItemType.BATTERY:
				Debug.Log("I just used a Battery");
				break;
			case ItemType.TORCH:
				Debug.Log("I just used a Torch");
				break;
			case ItemType.CROWBAR:
				Debug.Log("I just used a Crowbar");
				break;
		}
	}
	
	public string GetToolTip()
	{
		string stats = string.Empty;
		string color = string.Empty;
		string newLine = string.Empty;
		
		if (description != string.Empty)
		{
			newLine = "\n";
		}
		
		switch (quality)
		{
		case Quality.COMMON:
			color = "white";
			break;
		case Quality.UNCOMMON:
			color = "lime";
			break;
		case Quality.RARE:
			color = "navy";
			break;
		case Quality.EPIC:
			color = "magenta";
			break;
		case Quality.LEGENDARY:
			color = "orange";
			break;
		case Quality.ARTIFACT:
			color = "red";
			break;
		}
		
		if (strength > 0)
		{
			stats += "\n+" + strength.ToString() + " Strength";
		}
		if (intellect > 0)
		{
			stats += "\n+" + intellect.ToString() + " Intellect";
		}
		if (agility > 0)
		{
			stats += "\n+" + agility.ToString() + " Agility";
		}
		if (stamina > 0)
		{
			stats += "\n+" + stamina.ToString() + " Stamina";
		}
		
		// this next line sets up the description in the tooltip
		return string.Format("<color=" + color + "><size=26>{0}</size></color><size=24><i><color=lime>" + newLine + "{1}</color></i>{2}</size>", itemName, description, stats);
	}
}




