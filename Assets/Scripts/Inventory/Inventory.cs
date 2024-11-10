using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Inventory : MonoBehaviour
{
    public int maxSize = 20;
    public List<InventorySlot> slots = new List<InventorySlot>();
    public UnityEvent onInventoryChanged;

    public bool AddItem(Item item, int amount = 1)
    {
        // Check for existing stack
        if (item.isStackable)
        {
            InventorySlot existingSlot = slots.Find(slot => 
                slot.item == item && slot.amount < item.maxStackSize);
            
            if (existingSlot != null)
            {
                existingSlot.amount += amount;
                onInventoryChanged?.Invoke();
                return true;
            }
        }

        // Add to new slot
        if (slots.Count < maxSize)
        {
            slots.Add(new InventorySlot(item, amount));
            onInventoryChanged?.Invoke();
            return true;
        }

        return false;
    }
}

[System.Serializable]
public class InventorySlot
{
    public Item item;
    public int amount;

    public InventorySlot(Item item, int amount)
    {
        this.item = item;
        this.amount = amount;
    }
} 