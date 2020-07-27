using Godot;
using ItemClass;

namespace BaseItemClass{
    public class BaseItemClass : Node2D{
        [Export]
        protected string itemModifierID;
        [Export]
        protected string itemNameID;
        [Export]
        protected string itemIcon;
        [Export]
        protected string itemInGameDisplay;
        [Export]
        protected string itemDescriptionID;
        [Export]
        protected ItemClass.ItemTypes itemTypes;

        public string getItemModifier(){
            return Tr(itemModifierID);
        }
        public void setItemModifier(string mod){
            this.itemModifierID = mod;
        }

        public string getItemName(){
            return Tr(itemNameID);
        }
        public string getFullItemName(){
            return Tr(itemModifierID) + Tr(itemNameID);
        }
        public void setItemName(string mod){
            this.itemNameID = mod;
        }

        public string getItemDesc(){
            return Tr(itemDescriptionID);
        }
        public void setItemDesc(string mod){
            this.itemDescriptionID = mod;
        }
    }
}