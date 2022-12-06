using System.Collections.Generic;
using UnityEngine;

namespace Heimdallr.Core.Database.HeadFace {

    [CreateAssetMenu]
    public class HeadFaceDatabase : ScriptableObject {
        public List<HeadFace> Human;
        public List<HeadFace> Doram;
    }
}