using Heimdallr.Core.Game;
using System.Collections.Generic;
using UnityEngine;

namespace Heimdallr.Core.Database.Job {

    [CreateAssetMenu(menuName = "Heimdallr/Database Entry/Job")]
    public class Job : ScriptableObject {
        public GameEntityViewer Female;
        public GameEntityViewer Male;
        public int JobId;
        public List<Material> ColorsMale;
        public List<Material> ColorsFemale;
    }
}
