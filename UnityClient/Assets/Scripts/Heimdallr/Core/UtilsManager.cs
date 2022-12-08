using Heimdallr.Core.GameEntity;
using UnityEngine;

public class UtilsManager : MonoBehaviour {

    private void Start() {
        var job = DatabaseManager.GetJobById(0);
        var male = Instantiate<GameEntityViewer>(job.Male);

        male.SetGameEntityData(new GameEntityData {
            HairStyle = 0,
            Eye = 0,
            IsMale = true,
            HairColor = 3
        });

        male.transform.SetPositionAndRotation(new Vector3(150,0,170), Quaternion.identity);
    }
}
