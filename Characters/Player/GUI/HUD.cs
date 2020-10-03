using Godot;
using rz_frzbn.Singletons.InvItems.Items;
using System;

public class HUD : MarginContainer {
    private TextureRect[] HP = new TextureRect[10];
    private TextureRect[] Mana = new TextureRect[10];
    private TextureRect ItemIcon1;
    private TextureRect ItemIcon2;
    private TextureRect ItemIcon3;


    private Texture HeartEmpty;
    private Texture HeartHalf;
    private Texture HeartFull;

    private Texture ManaEmpty;
    private Texture ManaHalf;
    private Texture ManaFull;

    private Texture Crossbow;
    private Texture None;
    private Texture Icebolt;
    private Texture Sheild;



    private Label CurrentWeapon;
    public override void _Ready() {
        for (int i = 0; i < 10; i++){
            HP[i] = GetNode<TextureRect>("VBoxContainer2/VBoxContainer/HBoxContainer2/Bars/HP/HEART"+(i+1));
        }
        for (int i = 0; i < 10; i++){
            Mana[i] = GetNode<TextureRect>("VBoxContainer2/VBoxContainer/HBoxContainer2/Bars/MANA/MANACRYSTAL"+(i+1));
        }

        HeartEmpty = GD.Load<Texture>("res://Singletons/HUD/ICONS/HEART/heart_icon_empty-64.png");
        HeartHalf = GD.Load<Texture>("res://Singletons/HUD/ICONS/HEART/heart_icon_half-64.png");
        HeartFull = GD.Load<Texture>("res://Singletons/HUD/ICONS/HEART/heart_icon_full-64.png");

        //HeartEmpty.Flags = 0;
        //HeartHalf.Flags = 0;
        //HeartFull.Flags = 0;

        ManaEmpty = GD.Load<Texture>("res://Singletons/HUD/ICONS/MANA_CRYSTAL/mana_crystal_empty-64.png");
        ManaHalf = GD.Load<Texture>("res://Singletons/HUD/ICONS/MANA_CRYSTAL/mana_crystal_half-64.png");
        ManaFull = GD.Load<Texture>("res://Singletons/HUD/ICONS/MANA_CRYSTAL/mana_crystal_full-64.png");

        //ManaEmpty.Flags = 0;
        //ManaHalf.Flags = 0;
        //.Flags = 0;

        Crossbow = GD.Load<Texture>("res://Singletons/HUD/ICONS/ITEMS/CROSSBOW/crossbow-64.png");
        Icebolt = GD.Load<Texture>("res://Singletons/HUD/ICONS/ITEMS/ICEBOLT/icebolt-64.png");
        Sheild = GD.Load<Texture>("res://Singletons/HUD/ICONS/ITEMS/sheild/sheild-64.png");
        None = GD.Load<Texture>("res://Singletons/HUD/ICONS/ITEMS/NONE/none-64.png");

        //Crossbow.Flags = 0;
        //Icebolt.Flags = 0;
        //Sheild.Flags = 0;
        //None.Flags = 0;

        ItemIcon1 = GetNode<TextureRect>("VBoxContainer2/VBoxContainer/HOTBAR/ICO1");
        ItemIcon2 = GetNode<TextureRect>("VBoxContainer2/VBoxContainer/HOTBAR/ICO2");
        ItemIcon3 = GetNode<TextureRect>("VBoxContainer2/VBoxContainer/HOTBAR/ICO3");
        ItemIcon1.Texture = Icebolt;
        ItemIcon2.Texture = Sheild;
        ItemIcon3.Texture = Crossbow; // TODO: Change it so that you can only see crossbow if you picked one up
        _on_Player_WeaponChangedSignal(HotbarItems.NONE);

    }

    public void _on_Player_HPChangedSignal(float NewHP){
        //HP.Text = "HP: " + (int)NewHP;
        if ((int)NewHP > 0){
            int HalfHearts = (int)((NewHP*2)/10);
            int a = 0;
            while (a < 10) {
                if (HalfHearts >= 2){
                    HP[a].Texture = HeartFull;
                    HalfHearts -= 2;
                }
                else if (HalfHearts == 1){
                    HP[a].Texture = HeartHalf;
                    HalfHearts -= 1;
                }
                else {
                    HP[a].Texture = HeartEmpty;
                }
                a++;
            }
        }
        else {
            foreach (TextureRect heart in HP){
                heart.Texture = HeartEmpty;
            }
        }

    }

    public void _on_Player_AmmoChangedSignal(float NewAmmo){

    }

    public void _on_Player_MPChangedSignal(float NewMana){
        //GD.Print(NewMana);
        //Mana.Text = "Mana: " + (int)NewMana;
        if ((int)NewMana > 0){
            int HalfManas = (int)((NewMana)/10);
            int a = 0;
            while (a < 10) {
                if (HalfManas >= 2){
                    Mana[a].Texture = ManaFull;
                    HalfManas -= 2;
                }
                else if (HalfManas == 1){
                    Mana[a].Texture = ManaHalf;
                    HalfManas -= 1;
                }
                else {
                    Mana[a].Texture = ManaEmpty;
                }
                a++;
            }
        }
        else {
            foreach (TextureRect mana in Mana){
                mana.Texture = ManaEmpty;
            }
        }
    }

    public void _on_Player_WeaponChangedSignal(HotbarItems hotbarItems){
        switch (hotbarItems){
            case HotbarItems.CROSSBOW: 
                ItemIcon1.Modulate = new Color(0.75F,0.75F,0.75F,1F);
                ItemIcon2.Modulate = new Color(0.75F,0.75F,0.75F,1F);
                ItemIcon3.Modulate = new Color(1F,1F,1F,1F);

                break;
            case HotbarItems.ICEBOLT: 
                ItemIcon1.Modulate = new Color(1F,1F,1F,1F);
                ItemIcon2.Modulate = new Color(0.75F,0.75F,0.75F,1F);
                ItemIcon3.Modulate = new Color(0.75F,0.75F,0.75F,1F);
                break;
            case HotbarItems.NONE: 
                ItemIcon1.Modulate = new Color(0.75F,0.75F,0.75F,1F);
                ItemIcon2.Modulate = new Color(0.75F,0.75F,0.75F,1F);
                ItemIcon3.Modulate = new Color(0.75F,0.75F,0.75F,1F);
                break;
            case HotbarItems.SHIELD: 
                ItemIcon1.Modulate = new Color(0.75F,0.75F,0.75F,1F);
                ItemIcon2.Modulate = new Color(1F,1F,1F,1F);
                ItemIcon3.Modulate = new Color(0.75F,0.75F,0.75F,1F);
                break;
            
        }
    }

}
