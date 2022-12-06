using Heimdallr.Core.GameEntity;
using UnityEngine;

namespace Heimdallr.Core.Database.Job {

    [CreateAssetMenu]
    public class Job : ScriptableObject {
        public GameEntityViewer Female;
        public GameEntityViewer Male;
        public int JobId;
    }
}
