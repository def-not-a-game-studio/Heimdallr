﻿using System.Collections.Generic;
using UnityEngine;

namespace Heimdallr.Core.Database.Hair {

    [CreateAssetMenu(menuName = "Heimdallr/Database Entry/Hair")]
    public class Hair : ScriptableObject {
        public GameObject HairMale;
        public GameObject HairFemale;
        public List<Material> ColorsMale;
        public List<Material> ColorsFemale;
    }
}
