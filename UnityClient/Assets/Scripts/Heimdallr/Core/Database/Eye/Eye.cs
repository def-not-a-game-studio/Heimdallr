using System.Collections.Generic;
using UnityEngine;

namespace Heimdallr.Core.Database.Eye {

    [CreateAssetMenu(menuName = "Heimdallr/Database Entry/Eye")]
    public class Eye : ScriptableObject {
        public GameObject EyeMale;
        public GameObject EyeFemale;
        public List<Material> ColorsMale;
        public List<Material> ColorsFemale;
    }
}