using System;
using NUnit.Framework;
using Game.Code;
using DataFiles;

namespace wlo.pserver.tests
{
    [TestFixture]
    public class InventoryTests
    {
        private Inventory CreateInventory()
        {
            return new Inventory(null, null);
        }

        private Item CreateItem(ushort id, byte amount = 1, byte width = 1, byte height = 1, byte itemType = 10)
        {
            // create item info via reflection since type is in external assembly
            var infoType = Type.GetType("DataFiles.PhxItemInfo, PhoenixData");
            if (infoType == null)
                throw new InvalidOperationException("PhxItemInfo type not found");
            var info = Activator.CreateInstance(infoType);
            var propId = infoType.GetProperty("ItemID") ?? infoType.GetProperty("itemID");
            if (propId != null) propId.SetValue(info, id, null);
            var propWidth = infoType.GetProperty("cellwidth") ?? infoType.GetProperty("CellWidth");
            if (propWidth != null) propWidth.SetValue(info, width, null);
            var propHeight = infoType.GetProperty("cellheight") ?? infoType.GetProperty("CellHeight");
            if (propHeight != null) propHeight.SetValue(info, height, null);
            var propType = infoType.GetProperty("ItemType") ?? infoType.GetProperty("itemType");
            if (propType != null) propType.SetValue(info, itemType, null);

            var item = new InvItem();
            typeof(Item).GetMethod("CopyFrom", new[] { infoType }).Invoke(item, new[] { info });
            item.Ammt = amount;
            return item;
        }

        [Test]
        public void AddItem_DefaultSlot_PlacesItemInFirstAvailable()
        {
            var inv = CreateInventory();
            var item = CreateItem(1);
            var added = inv.AddItem(item, 0, false);
            Assert.AreEqual(1, added);
            Assert.AreEqual(1, inv[1].ItemID);
        }

        [Test]
        public void AddItem_SpecificSlot_NoInfiniteLoop()
        {
            var inv = CreateInventory();
            var item = CreateItem(2);
            var added = inv.AddItem(item, 5, false);
            Assert.AreEqual(1, added);
            Assert.AreEqual(2, inv[5].ItemID);
        }

        [Test]
        public void AddItem_StackableItem_IncreasesQuantity()
        {
            var inv = CreateInventory();
            var item1 = CreateItem(3);
            var item2 = CreateItem(3);
            inv.AddItem(item1, 1, false);
            var added = inv.AddItem(item2, 1, false);
            Assert.AreEqual(1, added);
            Assert.AreEqual(2, inv[1].Ammt);
        }
    }
}
