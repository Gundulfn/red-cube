using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIInventory : MonoBehaviour
{
    public List<UIItem> uiItems = new List<UIItem>();
    public GameObject slotPrefab;
    public List<GameObject> slots = new List<GameObject>();
    public Transform slotPanel;

    private int currentSlot = 0;
    public int numberOfSlots = 10;

    private Color ACTIVE_SLOT_COLOR = Color.yellow;
    private Color INACTIVE_SLOT_COLOR;

    void Start()
    {
        INACTIVE_SLOT_COLOR = slotPrefab.GetComponent<Image>().color;

        for (int i = 0; i < numberOfSlots; i++)
        {
            GameObject instance = Instantiate(slotPrefab);
            slots.Add(instance);
            instance.transform.SetParent(slotPanel);
            uiItems.Add(instance.GetComponentInChildren<UIItem>());
        }

        SetActiveSlot(currentSlot);
    }

    public void SetActiveSlot(int slot)
    {
        if (slot < 0 || slot >= numberOfSlots)
            return;

        slots[currentSlot].GetComponent<Image>().color = INACTIVE_SLOT_COLOR;

        currentSlot = slot;

        slots[slot].GetComponent<Image>().color = ACTIVE_SLOT_COLOR;
    }

    void UpdateSlot(int slot, Item item)
    {
        uiItems[slot].UpdateItem(item);
    }

    public void AddItem(Item item, int slot = -1)
    {
        if (slot == -1)
        { 
            UpdateSlot(uiItems.FindIndex(i => i.item == null), item); 
        }
        else
        {
            UpdateSlot(slot, item); 
        }
    }

    public int GetSlotIndex(Item _item)
    {
        return uiItems.IndexOf(uiItems.Find(uiItem => uiItem.item == _item));
    }
}
