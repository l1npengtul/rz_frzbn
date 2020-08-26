using Godot;
using System;

namespace rz_frzbn.Singletons.InvItems.Items{
    public class ItemInformation{
        // ID to get from TR server. 
        protected string itemNameID;
        protected string itemDescriptionID;
        protected string itemRarityID;
        protected bool doProcessObfuscatedTextTag = false; //TODO: Implement obfuscated text tag

        // Value from TR server. Fallback to being the same as "ID" version if not found

        private string itemNameTR;
        private string itemDescriptionTR;
        protected string itemRarityTR;

        public ItemInformation(string nameID, string descID, string rareID, bool obfus){
            this.itemNameID = nameID;
            this.itemDescriptionID = descID;
            this.itemRarityID = rareID;
        }

        public ItemInformation(string nameID, string descID, string rareID){
            this.itemNameID = nameID;
            this.itemDescriptionID = descID;
            this.itemRarityID = rareID;
        }

        public void updateTranslations(){
            this.itemNameTR = TranslationServer.Translate(this.itemNameID);
            this.itemDescriptionTR = TranslationServer.Translate(this.itemDescriptionID);
            this.itemRarityTR = TranslationServer.Translate(this.itemRarityID);
        }

        public string getItemName(){
            return this.itemNameTR;
        }

        public string getItemDesc(){
            return this.itemDescriptionTR;
        }

        public string getItemRare(){
            return this.itemRarityTR;
        }
    }
}