using System.Collections.Generic;
using UnityEngine;

namespace Heimdallr.Core.Database.Eye {

    [CreateAssetMenu(menuName = "Heimdallr/Database/Eye")]
    public class EyeDatabase : ScriptableObject {
        public List<Eye> Human;
        public List<Eye> Doram;
    }
}