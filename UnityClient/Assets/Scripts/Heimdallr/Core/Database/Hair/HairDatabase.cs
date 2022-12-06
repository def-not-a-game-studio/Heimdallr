using System.Collections.Generic;
using UnityEngine;

namespace Heimdallr.Core.Database.Hair {

    [CreateAssetMenu]
    public class HairDatabase : ScriptableObject {
        public List<Hair> Values;
    }
}
