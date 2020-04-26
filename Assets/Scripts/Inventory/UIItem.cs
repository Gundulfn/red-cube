using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIItem : MonoBehaviour
{
    public Item item;
    public Image spriteImage;

    public void UpdateItem(Item item)
    {
        this.item = item;

        if (this.item != null) {
            spriteImage.sprite = this.item.sprite;
        } else {
            spriteImage.sprite = null;
        }
    }
}
