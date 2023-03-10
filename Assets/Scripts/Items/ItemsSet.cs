using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Items
{
    [CreateAssetMenu(fileName = "ItemsSet", menuName = "Custom/ItemsSet")]
    public class ItemsSet: ScriptableObject
    {
        [SerializeField] private List<ItemSetItem> items;
        [field: SerializeField] public ItemBehaviour ItemPref { get; private set; }

        public BaseItem GetItem()
        {
            var possibleItems = items.Where(x => x.Item == null || (!x.Item.WasDropped && x.Item.opened) || x.Item.FallowPlayer).ToArray();

            if (possibleItems.Length == 0) return null;
            var allChances = possibleItems.Select(x => x.Chance).Sum();
            var chance = Random.value * allChances;
            var i = 0;
            while (chance > possibleItems[i].Chance)
            {
                chance -= possibleItems[i].Chance;
                i++;
            }

            if (possibleItems[i].Item != null)
            {
                possibleItems[i].Item.WasDropped = true;
            }
            return possibleItems[i].Item;
        }
    }

    [Serializable]
    public class ItemSetItem
    {
        [field: SerializeField] public int Chance { get; private set; } = 1;
        [field: SerializeField] public BaseItem Item { get; private set; }
    }
}