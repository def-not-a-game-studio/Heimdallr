using System.Collections.Generic;
using Heimdallr.Core.Game;
using UnityEngine;

namespace Heimdallr.Core.Database.Job {
    
    [CreateAssetMenu(menuName = "Heimdallr/Database Entry/Mesh Job")]
    public class MeshJob : Job {
        public MeshGameEntityViewer Female;
        public MeshGameEntityViewer Male;
        public List<Material> ColorsMale;
        public List<Material> ColorsFemale;
    }
}