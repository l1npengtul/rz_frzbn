using Godot;
using System;
using rz_frzbn.Singletons.utils;

namespace rz_frzbn.Singletons.InvItems.Items{
    public class ItemMaterial : Godot.Node2D{ // FIXME: Change to a more sensible option for a inventory item e.g. child of canvasitem
        public bool isMoveable;
        public string gamePath; // In-game representation of the item.
        public string iconPath; // String to in game node to spawn.
        public ItemType itemType;
        public ItemRarity itemRarity;
        public ItemID itemID;
        public string itemDescID;


        
        private Sprite icon;
        private Texture icoTexture;
        private ItemInformation itemMeta;

        
        public ItemMaterial(bool moveable, string nodePath, string iconPath, ItemType itemType, ItemRarity itemRarity, ItemID itemID, string itemDescID){
            this.isMoveable = moveable;
            this.gamePath = nodePath;
            if (iconPath.BeginsWith("res://")){
                this.iconPath = iconPath;
            }
            else {
                throw new System.ArgumentException("Icon path must start with res://");
            }
            this.itemID = itemID;
            this.itemType = itemType;
            this.itemRarity = itemRarity;
            this.itemDescID = itemDescID;
        }

        public ItemMaterial(){
            
        }
        

        public override void _Ready() {
            icoTexture = GD.Load<Texture>(this.iconPath);
            icon = GetNode<Sprite>("Icon2D");
            icon.Texture = icoTexture;
            this.itemMeta = new ItemInformation(
                EnumToString.makeString(this.itemID), 
                this.itemDescID,
                EnumToString.makeString(this.itemRarity)
            );
        }
    }
}