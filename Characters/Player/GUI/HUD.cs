using Godot;
using rz_frzbn.Singletons.InvItems.Items;
using System;

public class HUD : MarginContainer {
    private Label HP;
    private Label Mana;
    private Label CurrentWeapon;
    public override void _Ready() {
        HP = GetNode<Label>("VBoxContainer2/VBoxContainer/HBoxContainer2/Bars/HP");
        Mana = GetNode<Label>("VBoxContainer2/VBoxContainer/HBoxContainer2/Bars/MANA");
    }

    public void _on_Player_HPChangedSignal(float NewHP){
        HP.Text = "HP: " + NewHP;
    }

    public void _on_Player_AmmoChangedSignal(float NewAmmo){

    }

    public void _on_Player_MPChangedSignal(float NewMana){
        Mana.Text = "Mana: " + NewMana;
    }

    public void _on_Player_WeaponChangedSignal(HotbarItems hotbarItems){

    }

}
