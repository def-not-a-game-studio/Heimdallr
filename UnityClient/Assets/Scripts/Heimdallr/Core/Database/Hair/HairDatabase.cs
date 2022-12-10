using System.Collections.Generic;
using UnityEngine;

namespace Heimdallr.Core.Database.Hair {

    [CreateAssetMenu(menuName = "Heimdallr/Database/Hair")]
    public class HairDatabase : ScriptableObject {
        public List<Hair> Values;
    }
}
