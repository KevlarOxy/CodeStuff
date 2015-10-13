using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System;

public class Inventory : MonoBehaviour 
{

	private List<GameObject> allSlots; // create a list of slots
	private RectTransform inventoryRect;
	private float inventoryWidth, inventoryHieght;
	private static int emptySlots;  // this is used to keep track of how many empty slots we have available
	private static Slot from, to; // used to store items when moving items 'from' one slot, 'to' another
	private static GameObject hoverObject;
	private float hoverYOffset;

	public static int EmptySlots 
	{
		get { return emptySlots; }
		set { emptySlots = value; }
	}

	public int slots; // expose a number of slots available in the inspector
	public int rows; // expose a number of rows available in the inspector
	public float slotPaddingLeft, slotPaddingTop; // make padding available in the inspector
	public float slotSize; // make slotsize available in the inspector
	public GameObject slotPrefab; // make a space for the prefab in the inspector
	public GameObject iconPrefab;
	public Canvas canvas;
	public EventSystem eventSystem;
	private static GameObject clicked;
	
	// split stack variables
	
	public GameObject selectStackSize;
	public Text stackText;
	private int splitAmount;
	private int maxStackCount;
	private static Slot movingSlot; // a store for the objects moving around
	
	// for saving / loading **** ADD MORE PROTOTYPES LIKE THESE BELOW, AS OBJECTS ARE CREATED IN GAME AND WE WANT TO SAVE THEM ****** 
	
	public GameObject torch; 
	public GameObject battery;
	public GameObject crowbar;
	
	// Inventory instance...

	private static Inventory instance;
	public static Inventory Instance
	{
		get
		{
			if (instance == null)
			{
				instance = GameObject.FindObjectOfType<Inventory>(); // find the inventory in the game world
			}
			return Inventory.instance;
		}
	}
	
	// Fade canvas variables

	private static CanvasGroup canvasGroup;
	public static CanvasGroup CanvasGroup 
	{
		get { return canvasGroup; }
	}

	private bool fadingIn;
	private bool fadingOut;
	public float fadeTime;
	
	// Tool Tip Variables
	
	public GameObject tooltipObject;
	private static GameObject tooltip;
	
	public Text sizeTextObject;
	private static Text sizeText;
	
	public Text visualTextObject;
	private static Text visualText;
	
	// Drop item
	
	public GameObject dropItem;
	private static GameObject playerRef; 

	// ************* end variables ****************
	
				
	void Start () 
	{
	
	tooltip = tooltipObject;
	sizeText = sizeTextObject;
	visualText = visualTextObject;
	playerRef = GameObject.Find("PlayerController");
	
	canvasGroup = transform.parent.GetComponent<CanvasGroup>(); // this looks at the parent this script is attached to and gets the 'CanvasGroup' component so we can access it
	canvasGroup.alpha = 0;
	CreatLayout(); // go and setup the initial layout of the slots
	
	movingSlot = GameObject.Find("MovingSlot").GetComponent<Slot>();
	}
	
	//*************
	
	
	void Update()
	{
		if (Input.GetMouseButtonUp(0)) // checks if the user has lifted the left mouse button (left mouse button is zero)
		{
			// Removes the selected item from the inventory
			if (!eventSystem.IsPointerOverGameObject(-1) && from != null) // if we click outside the inventory and the player has picked up an item 
			{
				from.GetComponent<Image>().color = Color.white; // resets the slots colour
				
				foreach (Item item in from.Items) // using the 'from' slot to determine what object should go on the ground
				{
					float angle = UnityEngine.Random.Range(0.0f, Mathf.PI * 2); // a 360 range to drop the item ??? (i odn't understand this - research what that means)
					
					Vector3 v = new Vector3(Mathf.Sin(angle), 0, Mathf.Cos(angle));
					
					v *=30;
					
					GameObject.Instantiate(dropItem,playerRef.transform.position - v, Quaternion.identity);
				}
				
				from.ClearSlot(); // Removes the item from the slot
				Destroy(GameObject.Find("Hover")); // Removes the hover icon
				
				//resets the objects
				to = null; // sets 'to' to null (nothing in it)
				from = null; // sets from to null (noting in it)
				emptySlots++; // adds one space into the slots inventory because we just used / deleted one
			}
			else if (!eventSystem.IsPointerOverGameObject(-1) && !movingSlot.IsEmpty)
			{
				movingSlot.ClearSlot(); // clearing the moving slot
				Destroy (GameObject.Find("Hover")); // destroying the hover game object
			}
		}
		if (hoverObject != null) // checks if the hover objects exists
		{
			Vector2 position;
			RectTransformUtility.ScreenPointToLocalPointInRectangle(canvas.transform as RectTransform, Input.mousePosition, canvas.worldCamera, out position);
			position.Set(position.x, position.y - hoverYOffset);
			hoverObject.transform.position = canvas.transform.TransformPoint(position);
		}
		if (Input.GetKeyDown(KeyCode.I)) // fade the inventory up / down at the press of I
		{
			if(canvasGroup.alpha > 0)
			{
				StartCoroutine("FadeOut");
				PutItemBack();
			}
			else
			{
				StartCoroutine("FadeIn");
			}
		}
	}
	
	public void ShowTooltip(GameObject slot) // putting this GameObject between the brackets reveals a box in the inspector for a (slot) gameobject.
	{
		Slot tmpSlot = slot.GetComponent<Slot>();
		
		if (!tmpSlot.IsEmpty && hoverObject == null) // if the tool tip IS NOT (!) Empty and hover object is null
		{
			visualText.text = tmpSlot.CurrentItem.GetToolTip();
			sizeText.text = visualText.text;
			
			tooltip.SetActive(true);
			
// this next section moves the tool to display beneath the selectd icon, but I want to keep mine in position so have commented it out
//			float xPos = slot.transform.position.x + slotPaddingLeft; 
//			float yPos = slot.transform.position.y - slot.GetComponent<RectTransform>().sizeDelta.y - slotPaddingTop;
//			tooltip.transform.position = new Vector2(xPos, yPos);
		}
	}
	
	
	public void HideToolTip()
	{
		tooltip.SetActive(false);
	}
	
	
	// ***************SAVE INVENTORY - TEMP FOR TESTING ************
	
	public void SaveInventory()
	{
		string content = string.Empty;
		
		for (int i = 0; i < allSlots.Count; i++)
		{
			Slot tmp = allSlots[i].GetComponent<Slot>();
			if (!tmp.IsEmpty)
			{
				content += i + "-" + tmp.CurrentItem.type.ToString() + "-" + tmp.Items.Count.ToString() + ";";
			}
		}
		PlayerPrefs.SetString("content", content);
		PlayerPrefs.SetInt("slots", slots);
		PlayerPrefs.SetInt("rows", rows);
		PlayerPrefs.SetFloat("slotPaddingLeft", slotPaddingLeft);
		PlayerPrefs.SetFloat("slotPaddingTop", slotPaddingTop);
		PlayerPrefs.SetFloat("slotSize", slotSize);
		PlayerPrefs.SetFloat("xPos", inventoryRect.position.x);
		PlayerPrefs.SetFloat("yPos", inventoryRect.position.y);
		PlayerPrefs.Save();
	}
	
	public void LoadInventory()
	{
		string content = PlayerPrefs.GetString("content");
		slots = PlayerPrefs.GetInt("slots");
		rows = PlayerPrefs.GetInt("rows");
		slotPaddingLeft = PlayerPrefs.GetFloat("slotPaddingLeft");
		slotPaddingTop = PlayerPrefs.GetFloat("slotPaddingTop");
		slotSize = PlayerPrefs.GetFloat("slotSize");
	
			
		inventoryRect.position = new Vector3(PlayerPrefs.GetFloat("xPos"), PlayerPrefs.GetFloat("yPos"), inventoryRect.position.z);
		
		CreatLayout();
		
		string[] splitContent = content.Split(';'); // Example [0]"0-TORCH-3;[1]1-BARRERIES-2;" this basically says there are three torches in slot zero and two batteries in slot 1.
		
		for (int x = 0; x < splitContent.Length - 1; x++)
		{
			string[] splitValues = splitContent[x].Split('-');

			int index = Int32.Parse(splitValues[0]);
			
			ItemType type = (ItemType)Enum.Parse (typeof(ItemType), splitValues [1]);
			
			int amount = Int32.Parse(splitValues[2]);
			
			for (int i = 0; i < amount; i++)
			{
				switch (type)
				{
					case ItemType.TORCH:
						allSlots[index].GetComponent<Slot>().AddItem (torch.GetComponent<Item>());
						break;
					case ItemType.BATTERY:
						allSlots[index].GetComponent<Slot>().AddItem (battery.GetComponent<Item>());
						break;
					case ItemType.CROWBAR:
						allSlots[index].GetComponent<Slot>().AddItem (battery.GetComponent<Item>());
						break;
				}
			}
		}
	}
	
	//*************
	
	private void CreatLayout()
	{
		if (allSlots !=null)
		{
			foreach (GameObject go in allSlots)
			{
				Destroy(go);
			}
		}
	
		allSlots = new List<GameObject>(); // Instantiates the allSlots list
		hoverYOffset = slotSize * 0.01f; // offset the hover icon so we can actaully click it.
		emptySlots = slots; // set the number of empty slots to match the created slots at the start
		inventoryWidth = (slots / rows) * (slotSize + slotPaddingLeft) + slotPaddingLeft; // calculate the inventory width size
		inventoryHieght = rows * (slotSize + slotPaddingTop) + slotPaddingTop; // calculate the inventry hieght (number of vertical slots
		inventoryRect = GetComponent<RectTransform>(); // get the rect transform of the inventory rectangle this script is acyaully attached too in the inspector
		inventoryRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, inventoryWidth); // now set the actual calculated width
		inventoryRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, inventoryHieght); // now set the actual calculated hieght
		
		int columns = slots / rows; // 'sots' and 'rows' are public and set in the inspector and used to calculate an int 'columns'
		
		for (int y = 0; y < rows; y++) // begin building rows...
		{
			for (int x = 0; x < columns; x++) // ... and columns
			{
				GameObject newSlot = (GameObject)Instantiate(slotPrefab); // spawn a new slot prefab
				RectTransform slotRect = newSlot.GetComponent<RectTransform>();
				newSlot.name = "Slot";
				newSlot.transform.SetParent(this.transform.parent);
				slotRect.localPosition = inventoryRect.localPosition + new Vector3(slotPaddingLeft * (x + 1) + (slotSize * x), - slotPaddingTop * (y+1) - (slotSize * y));
				slotRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, slotSize * canvas.scaleFactor);
				slotRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, slotSize * canvas.scaleFactor);
				allSlots.Add (newSlot); // add the new slot to the list above
			}
		}
	}
	
	//*************
	
	public bool AddItem(Item item)
	{
		if (item.maxSize == 1) // if maxSize is only set to 1, the item being added is not stackable (stackables are mostly consumables, like batteries)
		{
			// places the item at an empty slot
			print ("maxSize does equal 1 - placing empty item");
			PlaceEmpty(item);
			return true;
		}
		else // if the item is stackable
		{
			foreach (GameObject slot in allSlots) // runs through all sots in the inventory
			{
				Slot tmp = slot.GetComponent<Slot>(); // creates a reference to the slot
				if (!tmp.IsEmpty) // If the item isn't empty
				{
					// checks if the item on the slot is the same type as the item we want to pick up
					if (tmp.CurrentItem.type == item.type && tmp.IsAvailable)
					{
						if (!movingSlot.IsEmpty && clicked.GetComponent<Slot>() == tmp.GetComponent<Slot>())
						{
							continue;
						}
						else
						{
							tmp.AddItem (item); // adds the item to the inventory
							return true;
						}
					}
				}
			}
			if (emptySlots > 0) // places the item on an empty slot
			{
				PlaceEmpty(item);
			}
		}
		return false; // if its not greater than zero it does nothing and the item is currently lost (NEED TO ADD A FUNCTION TO CHECK FOR THIS ON PICK UP!!!!!!!!)
	}
	
	//*************************************************************************************************
	// MOVING THE INVENTORY AROUND THE SCREEN WOULD GO HERE, BUT I'M NOT DOING THIS BECAUSE ITS STATIC
	//*************************************************************************************************
	
	private bool PlaceEmpty(Item item) 	// this function will go through all the slots and look for an empty item slot
	{
		if (emptySlots > 0) // if emptySlots is greater than zero, then there are slots left (i.e. it hasn't counted down to zero when being filled). If it's zero, all slots are taken
		{
			foreach (GameObject slot in allSlots)
			{
				Slot tmp = slot.GetComponent<Slot>();
				if (tmp.IsEmpty)
				{
					tmp.AddItem(item); // add the item into the slot
					emptySlots--; // remove an empty slot
					return true;
				}
			}
		}
		return false;
	}

	//*************

	public void MoveItem (GameObject clicked) // this will take the 'clicked' on object and do something with it.
	{
		Inventory.clicked = clicked;
		
		if (!movingSlot.IsEmpty)
		{
			Slot tmp = clicked.GetComponent<Slot>();
			if (tmp.IsEmpty)
			{
				tmp.AddItems(movingSlot.Items);
				movingSlot.Items.Clear();
				Destroy(GameObject.Find("Hover"));
			}
			else if (!tmp.IsEmpty && movingSlot.Items.Peek().type == tmp.CurrentItem.type && tmp.IsAvailable)
			{
				MergeStacks(movingSlot, tmp);
			}
		}
		else if (from == null && canvasGroup.alpha == 1 && !Input.GetKey(KeyCode.LeftShift)) // if we haven't picked up an item and the canvasGroup is visible.
		{
			if (!clicked.GetComponent<Slot>().IsEmpty && !GameObject.Find("Hover")) // if the slot we just clicked (!) isn't empty...
			{
				from = clicked.GetComponent<Slot>(); // a reference of the slot we are moving from.
				from.GetComponent<Image>().color = Color.grey; // sets the from slots color to grey, to visually indicate that its the slot we are moving from.
				CreateHoverIcon();
			}
		}
		else if(to == null && !Input.GetKey(KeyCode.LeftShift))
		{
			to = clicked.GetComponent<Slot>(); // sets the 'to' object
			Destroy (GameObject.Find("Hover")); // destroy the hover object when placing it in a slot
		}
		if (to != null && from != null) // if both 'to' and 'from' are null then we are done moving
		{
			if (!to.IsEmpty && from.CurrentItem.type == to.CurrentItem.type && to.IsAvailable)
			{
				MergeStacks(from, to);
			}
			else
			{
				Stack<Item> tmpTo = new Stack<Item>(to.Items);
				to.AddItems(from.Items);
			
				if (tmpTo.Count == 0)
				{
					from.ClearSlot();
				}
				else
				{
					from.AddItems(tmpTo);
				}
			}
			
			// Resets all values
			from.GetComponent<Image>().color = Color.white;
			to = null;
			from = null;
			Destroy (GameObject.Find("Hover")); // destroy the hover object when placing it in a slot
		}
	}
	
	//*************
	
	private void CreateHoverIcon () 
	{
		hoverObject = (GameObject)Instantiate(iconPrefab); // instantiates the hover object.
		hoverObject.GetComponent<Image>().sprite = clicked.GetComponent<Image>().sprite; // sets the sprite on the hover object so that it reflects the object we are moving.
		hoverObject.name = "Hover"; // sets the name of the hover object.
		
		// creates references to the transform.
		RectTransform hoverTransform = hoverObject.GetComponent<RectTransform>();
		RectTransform clickedTransform = clicked.GetComponent<RectTransform>();
		
		// sets the size of the hover object so that it has the same size as the clicked object.
		hoverTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, clickedTransform.sizeDelta.x);
		hoverTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, clickedTransform.sizeDelta.y);
		
		//sets the hover object's parent as the camvas, so that it is viosible in the game.
		hoverObject.transform.SetParent(GameObject.Find("Canvas").transform, true);
		
		// sets the local scale to make sure that it has the coirrect size.
		hoverObject.transform.localScale = clicked.gameObject.transform.localScale;
		
		hoverObject.transform.GetChild(0).GetComponent<Text>().text = movingSlot.Items.Count > 1 ? movingSlot.Items.Count.ToString() : string.Empty;
	}
	
	//*************
	
	private void PutItemBack ()
	{
		if (from != null)
		{
			Destroy(GameObject.Find("Hover"));
			from.GetComponent<Image>().color = Color.white;
			from = null;
		}
		else if (!movingSlot.IsEmpty)
		{
			Destroy(GameObject.Find("Hover"));
			foreach (Item item in movingSlot.Items)
			{
				clicked.GetComponent<Slot>().AddItem(item);
			}
			
			movingSlot.ClearSlot();
		}
		
		selectStackSize.SetActive(false);
	}
	
	//*************
	
	public void SetStackInfo(int maxStackCount)
	{
		selectStackSize.SetActive(true);
		splitAmount = 0;
		this.maxStackCount = maxStackCount;
		stackText.text = splitAmount.ToString();
	}
	
	//*************
	
	public void SplitStack()
	{
		selectStackSize.SetActive(false);
		if (splitAmount == maxStackCount)
		{
			MoveItem(clicked);
		}
		else if (splitAmount >0)
		{
			movingSlot.Items = clicked.GetComponent<Slot>().RemoveItems(splitAmount);
			CreateHoverIcon();
		}
	}
	
	//*************
	
	public void ChangestackText(int i)
	{
		splitAmount += i;
		
		if(splitAmount < 0)
		{
			splitAmount = 0;
		}
		if (splitAmount > maxStackCount)
		{
			splitAmount = maxStackCount;
		}
		
		stackText.text = splitAmount.ToString();
	}
	
	//*************
		
	public void MergeStacks(Slot source, Slot destination)
	{
		int max = destination.CurrentItem.maxSize - destination.Items.Count;
		int count = source.Items.Count < max ? source.Items.Count : max;
		
		for (int i =0; i < count; i++)
		{
			destination.AddItem(source.RemoveItem());
			hoverObject.transform.GetChild(0).GetComponent<Text>().text = movingSlot.Items.Count.ToString();
		}
		if (source.Items.Count == 0)
		{
			source.ClearSlot();
			Destroy(GameObject.Find ("Hover"));
		}
	}
		
	//*************
	
	private IEnumerator FadeOut()
	{
		if (!fadingOut)
		{
			fadingOut = true;
			fadingIn = false;
			StopCoroutine("fadeIn");
			
			float startAlpha = canvasGroup.alpha;
			float rate = 1.0f / fadeTime;
			float progress = 0.0f;
			
			while (progress < 1.0)
			{
				canvasGroup.alpha = Mathf.Lerp (startAlpha,0,progress);
				progress += rate * Time.deltaTime;
				yield return null;
			}
			
			canvasGroup.alpha = 0; // make sure it is zero for sure
			fadingOut = false;
		}
	}
	
	//*************
	
	private IEnumerator FadeIn()
	{
		if (!fadingIn)
		{
			fadingOut = false;
			fadingIn = true;
			StopCoroutine("fadeOut");
			
			float startAlpha = canvasGroup.alpha;
			float rate = 1.0f / fadeTime;
			float progress = 0.0f;
			
			while (progress < 1.0)
			{
				canvasGroup.alpha = Mathf.Lerp (startAlpha,1,progress);
				progress += rate * Time.deltaTime;
				yield return null;
			}
			
			canvasGroup.alpha = 1; // make sure it is zero for sure
			fadingIn = false;
		}
	}
}



