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
public static class DatabaseManager {
    private static List<Job> JobDb;
    private static List<SpriteHead> HeadDb;

    private static EyeDatabase EyeDb;
    private static HeadFaceDatabase HeadFaceDb;
    private static HairDatabase HairDb;

    static DatabaseManager() {
        JobDb = Resources.LoadAll<Job>("Database/Job").ToList();
        HeadDb = Resources.LoadAll<SpriteHead>("Database/Head").ToList();
        JobDb.AddRange(Resources.LoadAll<Job>("Database/Npc").ToList());

        EyeDb = Resources.Load<EyeDatabase>("Database/Eye");
        HeadFaceDb = Resources.Load<HeadFaceDatabase>("Database/HeadFace");
        HairDb = Resources.Load<HairDatabase>("Database/Hair");
    }

    public static Eye GetEyeById(int id) {
        return EyeDb.Human[id];
    }

    public static Job GetJobById(int id) {
        return JobDb.FirstOrDefault(it => it.JobId == id) ?? throw new Exception($"Job not found {id}");
    }

    public static SpriteHead GetHeadById(int id) {
        return HeadDb.FirstOrDefault(it => it.Id == id) ?? HeadDb.First();
    }

    public static HeadFace GetHeadFaceById(int id) {
        return HeadFaceDb.Human[id];
    }

    public static Hair GetHairById(int id) {
        return HairDb.Values[id % HairDb.Values.Count];
    }
}
//}