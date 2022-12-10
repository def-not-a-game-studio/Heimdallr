using System.Collections.Generic;
using UnityEngine;

namespace Heimdallr.Core.Database.Job {

    [CreateAssetMenu(menuName = "Heimdallr/Database/Job")]
    public class JobDatabase : ScriptableObject {
        public List<Job> Values;
    }
}