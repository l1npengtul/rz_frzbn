#[macro_use]
extern crate gdnative;

mod character;

use self::{character::*};

fn init(handle: gdnative::init::InitHandle){
    handle.add_class::<>();
}

godot_gdnative_init!();
godot_nativescript_init!(init);
godot_gdnative_terminate!();