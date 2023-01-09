using Heimdallr.Core.Database.Eye;
using Heimdallr.Core.Database.Hair;
using Heimdallr.Core.Database.HeadFace;
using Heimdallr.Core.Database.Job;
using System.Linq;
using UnityEngine;

// Commenting for now, otherwise eye/job etc will error
//namespace Heimdallr.Core.Database {
    public static class DatabaseManager {

        private static EyeDatabase EyeDb;
        private static JobDatabase JobDb;
        private static HeadFaceDatabase HeadFaceDb;
        private static HairDatabase HairDb;

        static DatabaseManager() {
            EyeDb = Resources.Load<EyeDatabase>("Database/Eye");
            JobDb = Resources.Load<JobDatabase>("Database/Job");
            HeadFaceDb = Resources.Load<HeadFaceDatabase>("Database/HeadFace");
            HairDb = Resources.Load<HairDatabase>("Database/Hair");
        }

        public static Eye GetEyeById(int id) {
            return EyeDb.Human[id];
        }

        public static Job GetJobById(int id) {
            return JobDb.Values.First(it => it.JobId == id);
        }

        public static HeadFace GetHeadFaceById(int id) {
            return HeadFaceDb.Human[id];
        }

        public static Hair GetHairById(int id) {
            return HairDb.Values[id % HairDb.Values.Count];
        }
    }
//}
