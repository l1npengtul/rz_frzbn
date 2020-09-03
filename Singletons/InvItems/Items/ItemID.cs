namespace rz_frzbn.Singletons.InvItems.Items{
    // A List of every available item in the game.
    public enum ItemID {
        // catagory: Weapons

        // subcatagory: Mage
        ICE_BOLT,
        ICE_SHEILD,
        ICE_AOE_STORM,
        FIREBALL,

        // subcatagory: melee
        
        SPEAR,
        SHORTSWORD,
        KNIFE,
        RUST_KNIFE,

        // subcatagory: ranged

        CROSSBOW,

        // catagory: consumable

        // subcatagory: ammo

        ARROW,

        // subcatagory: food

        NUTS,
        ROASTED_NUTS,
        HEARTY_NUTS,
        
        BERRIES,
        DRIED_BERRIES,

        FISH,
        CARP,
        SHADY_FISH,


        // subcatagory: potions

        SWEET_POTION,
        STEALTH_POTION,
        POWER_POTION,


        // subcatagory: currency

        PYROXENE,
        
        //  catagory: placeable

        BERRY_FARM,

        // catagory: important item

        BOOK,
        MAP,

        // catagory: no

        NONE,
    }

    public enum ItemType {
        WEAPON_MELEE,
        WEAPON_RANGED,
        WEAPON_MAGE,
        CONSUMABLE_FOOD,
        CONSUMABLE_POTION,
        CONSUMABLE_CURRENCY,
        PLACEABLE,
        IMPORTANT_ITEM,
        NONE, // will throw runtime error
    }

    public enum ItemRarity { 
        COMMON,
        UNCOMMON,
        RARE,
        EPIC,
        LEGENDARY,
        
        NONE,
    }
}