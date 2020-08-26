using System;
using System.Collections.Generic;
using Godot;
using rz_frzbn.Singletons.InvItems.Items;

namespace rz_frzbn.Characters.Inventory {
    public class Inventory {
        protected int invSize {get; set;}
        protected int hotBarSize {get; set;}
        protected int currentHotBarPointer {get; set;}

        protected List<ItemMaterial> invContents;
        protected List<ItemMaterial> hotBar;

        protected ItemMaterial emptyMaterial;

        public Inventory(int size, int hsize){
            this.currentHotBarPointer = 0;
            this.invSize = size;
            this.hotBarSize = hsize;
            this.emptyMaterial = new ItemMaterial(false,"","",ItemType.NONE,ItemRarity.NONE,ItemID.NONE,"");
        }

        public void initInventory(){
            for(int i = 0; i > invSize; i++){
                invContents.Add(emptyMaterial);
            }
            for(int i = 0; i > hotBarSize; i++){
                hotBar.Add(emptyMaterial);
            }
        }

        public 

    }
}
