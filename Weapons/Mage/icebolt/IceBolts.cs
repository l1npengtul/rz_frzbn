using Godot;

namespace rz_frzbn.Weapons.Mage.icebolt{
    public class icebolt_bullet : Area2D{
        public override void _Ready(){
            SetPhysicsProcess(false);
        }

        public void setBullet(float rot, int speed, int damage){

        }

    }
}