using Heimdallr.Core.GameEntity;
using UnityEngine;

public class UtilsManager : MonoBehaviour {

    private void Start() {
        var job = DatabaseManager.GetJobById(0);
        var female = Instantiate<GameEntityViewer>(job.Female);
        var male = Instantiate<GameEntityViewer>(job.Male);

        female.SetGameEntityData(new GameEntityData {
            HairStyle = 0,
            Eye = 0,
            IsMale = false,
            HairColor = 1
        });

        male.SetGameEntityData(new GameEntityData {
            HairStyle = 0,
            Eye = 0,
            IsMale = true,
            HairColor = 3
        });

        female.transform.SetPositionAndRotation(new Vector3(2,0,0), Quaternion.identity);
    }

}
