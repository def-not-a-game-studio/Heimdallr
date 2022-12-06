using System.Collections.Generic;
using UnityEngine;

namespace Heimdallr.Core.Database.Job {

    [CreateAssetMenu]
    public class JobDatabase : ScriptableObject {
        public List<Job> Values;
    }
}