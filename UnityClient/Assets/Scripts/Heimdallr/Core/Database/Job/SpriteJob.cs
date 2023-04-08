using UnityEngine;

namespace Heimdallr.Core.Database.Job {
    
    [CreateAssetMenu(menuName = "Heimdallr/Database Entry/Sprite Job")]
    public class SpriteJob : Job {
        public SpriteData Male;
        public SpriteData Female;
    }
}