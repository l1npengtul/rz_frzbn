#[macro_use]
extern crate gdnative;

// ima say it
// who's rem

use gdnative::{
    Vector2
};


// TODO: ADJUST VALUES
const BASE_SPEED : f32 = 200.0;

/* Player Directions
NorthWest  North  NorthEast
WestNorth \  |  / EastNorth
  West  --  EMT  -- East
WestSouth /  |  \ EastSouth
SouthWest  South  SouthEast
 */
pub enum PlayerDirection{
    North,
    NorthEast,
    EastNorth,
    East,
    EastSouth,
    SouthEast,
    South,
    SouthWest,
    WestSouth,
    West,
    WestNorth,
    NorthWest,
}

pub enum SimpleDirection{

}

pub enum AttackTypes{
    RANGED,
    MAGE,
    MELEE,
}

pub enum PlayerStates {
    WALKING {
        dir : PlayerDirection,
    },
    RUNNING {
        dir : PlayerDirection,
    },
    ROLLING {

    },
    ATTACKING {
        attack_type : AttackTypes,
    },


}

pub struct Player{
    moving : Vector2,
    velocity : f32,
    health_points : i16,
    mana_points : i16,

}