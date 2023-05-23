using System;
using System.Collections.Generic;
using System.Linq;
using Heimdallr.Core.Database.Eye;
using Heimdallr.Core.Database.Hair;
using Heimdallr.Core.Database.HeadFace;
using UnityEngine;
using UnityRO.Core.Database;

// Commenting for now, otherwise eye/job etc will error
//namespace Heimdallr.Core.Database {
public class DatabaseManager : MonoBehaviour {
    [SerializeField] private JobDatabase JobDb;
    [SerializeField] private SpriteHeadDatabase HeadDb;

    private static EyeDatabase EyeDb;
    private static HeadFaceDatabase HeadFaceDb;
    private static HairDatabase HairDb;

    private void Start() {
        EyeDb = Resources.Load<EyeDatabase>("Database/Eye");
        HeadFaceDb = Resources.Load<HeadFaceDatabase>("Database/HeadFace");
        HairDb = Resources.Load<HairDatabase>("Database/Hair");
    }

    public static Eye GetEyeById(int id) {
        return EyeDb.Human[id];
    }

    public Job GetJobById(int id) {
        return JobDb.Values.FirstOrDefault(it => it.JobId == id) ?? throw new Exception($"Job not found {id}");
    }

    public SpriteHead GetHeadById(int id) {
        return HeadDb.Values.FirstOrDefault(it => it.Id == id) ?? HeadDb.Values.First();
    }

    public static HeadFace GetHeadFaceById(int id) {
        return HeadFaceDb.Human[id];
    }

    public static Hair GetHairById(int id) {
        return HairDb.Values[id % HairDb.Values.Count];
    }
}
//}