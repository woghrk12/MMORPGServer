namespace Server.Game
{
    public class Inventory
    {
        #region Variables

        private Dictionary<int, Item> itemDictionary = new();

        #endregion Variables

        #region Methods

        public void AddItem(Item item)
        {
            if (item.IsStackable == true)
            {
                if (itemDictionary.ContainsKey(item.ID) == false)
                {

                }
            }
            else
            {

            }
        }

        public Item GetItem(int id)
        {
            if (itemDictionary.TryGetValue(id, out Item item) == false) return null;

            return item;
        }

        public List<Item> GetAllItems() => [.. itemDictionary.Values];

        public Item FindItem(Func<Item, bool> condition)
        {
            foreach (Item item in itemDictionary.Values)
            {
                if (condition.Invoke(item) == false) continue;

                return item;
            }

            return null;
        }

        #endregion Methods
    }
}