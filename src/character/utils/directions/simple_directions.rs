pub enum SimpleFourDirections{
    North,
    East,
    West,
    South,
}

pub fn get_fdir_from_angle(angle : i32) -> Option<SimpleFourDirections>{
    match angle {
        // We need 2 cases for north since 315..0 will go 315,314,313...
        // North
        315..359 => {
            return Some(SimpleFourDirections::North);
        },
        0..44 => {
            return Some(SimpleFourDirections::North);
        },
        // East
        45..134 => {
            return Some(SimpleFourDirections::East);
        },
        // South
        135..224 => {
            return Some(SimpleFourDirections::South);
        },
        // West
        225..314 => {
            return Some(SimpleFourDirections::West);
        }
        // If some BS value is fed through
        _ => {
            return None;
        }
    };
}


pub enum SimpleEightDirections{
    North,
    NorthEast,
    East,
    SouthEast,
    South,
    SouthWest,
    West,
    NorthWest,
}

pub fn get_edir_from_angle(angle : i32) -> Option<SimpleEightDirections>{
    match angle{

    }
}
