      ___           ___         ___                                   ___                                     
     /  /\         /  /\       /  /\        ___           ___        /  /\                                    
    /  /:/_       /  /::\     /  /::\      /  /\         /  /\      /  /:/_                                   
   /  /:/ /\     /  /:/\:\   /  /:/\:\    /  /:/        /  /:/     /  /:/ /\                                  
  /  /:/ /::\   /  /:/~/:/  /  /:/~/:/   /__/::\       /  /:/     /  /:/ /:/_                                 
 /__/:/ /:/\:\ /__/:/ /:/  /__/:/ /:/___ \__\/\:\__   /  /::\    /__/:/ /:/ /\                                
 \  \:\/:/~/:/ \  \:\/:/   \  \:\/:::::/    \  \:\/\ /__/:/\:\   \  \:\/:/ /:/                                
  \  \::/ /:/   \  \::/     \  \::/~~~~      \__\::/ \__\/  \:\   \  \::/ /:/                                 
   \__\/ /:/     \  \:\      \  \:\          /__/:/       \  \:\   \  \:\/:/                                  
     /__/:/       \  \:\      \  \:\         \__\/         \__\/    \  \::/                                   
     \__\/         \__\/       \__\/                                 \__\/                                    
      ___           ___           ___                                     ___           ___           ___     
     /  /\         /  /\         /__/\         _____        ___          /__/\         /  /\         /  /\    
    /  /:/        /  /::\       |  |::\       /  /::\      /  /\         \  \:\       /  /:/_       /  /::\   
   /  /:/        /  /:/\:\      |  |:|:\     /  /:/\:\    /  /:/          \  \:\     /  /:/ /\     /  /:/\:\  
  /  /:/  ___   /  /:/  \:\   __|__|:|\:\   /  /:/~/::\  /__/::\      _____\__\:\   /  /:/ /:/_   /  /:/~/:/  
 /__/:/  /  /\ /__/:/ \__\:\ /__/::::| \:\ /__/:/ /:/\:| \__\/\:\__  /__/::::::::\ /__/:/ /:/ /\ /__/:/ /:/___
 \  \:\ /  /:/ \  \:\ /  /:/ \  \:\~~\__\/ \  \:\/:/~/:/    \  \:\/\ \  \:\~~\~~\/ \  \:\/:/ /:/ \  \:\/:::::/
  \  \:\  /:/   \  \:\  /:/   \  \:\        \  \::/ /:/      \__\::/  \  \:\  ~~~   \  \::/ /:/   \  \::/~~~~ 
   \  \:\/:/     \  \:\/:/     \  \:\        \  \:\/:/       /__/:/    \  \:\        \  \:\/:/     \  \:\     
    \  \::/       \  \::/       \  \:\        \  \::/        \__\/      \  \:\        \  \::/       \  \:\    
     \__\/         \__\/         \__\/         \__\/                     \__\/         \__\/         \__\/    

Sprite Combiner is a simple, easy to use editor tool to combine multiple sprites together into a single texture.

It respects sorting orders, sorting layers and can even blend alpha values. It also supports scaling and rotation,
though this may lead to loss of quality if used. It will try to create as accurate of a combined texture as it can
taking into account all the given sprites and their transforms.

Sprite combination is useful for performance, as each individual sprite is another draw call sent to the GPU. By combining
them, the GPU can batch sprite drawing and save you valuable performance! The performance benefit gained is proportional to
the amount of sprite renderers combined.

==={HOW TO USE}===
    * Sprite Renderer Combiner
        - Attach a sprite renderer combiner component to any gameobject you wish to be the target for the final combined
        sprite product
        - Assign the sprite renderers you wish to combine
        - Adjust the sprite combiner and texture combiner settings
        - Either click the combine sprites button, and they will be combined on start!

    * Editor Window
        - Open the SpriteCombiner editor window at Window/SpriteCombiner
        - Add the sprites you wish to combine either through the "Add Sprite" button or drag & drop into the editor window
        - Click on a sprite to modify properties about it
        - Arrange the sprites as you wish in the sprite preview area
        - Adjust the texture combiner settings in the properties panel
        - Click "Reset Sprites" to reset sprite scale, position and rotation
            * As an added benefit, clicking reset will re-center sprites at the current zoom level
        - Click "Combine Sprites" to open a file prompt and save the combined texture to a location of your choosing!

==={NOTES}===
    - Within the sprite renderer combiner, the order of the sprite renderer in the renderers list will determine its sorting
    order. Feel free to re-arrange the list as needed.

    -There may be some distortion upon combination of the sprites. This is the result of sprite renderers being positioned
    oddly within the scene. It can be reduced to unnoticable levels but not eliminated completely. Sprite renderers are
    positioned with float values, whilst textures and pixels are written in integers. For example, in pixel space there's no
    such thing as 0.5.

I hope the tool proves useful to you in some way. Feel free to use any of the included demo sprites for any use as well.
If you have any feedback, questions, feature ideas, or comments feel free to contact me at awtdevcontact@gmail.com