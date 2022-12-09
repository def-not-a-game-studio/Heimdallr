using Heimdallr.Core.Game;
using UnityEngine;

public class UtilsManager : MonoBehaviour {

    private void Start() {
        var male = new GameObject("novice").AddComponent<GameEntity>();
        male.Init(new GameEntityData {
            HairStyle = 0,
            Eye = 0,
            IsMale = true,
            HairColor = 3,
            Job = 0,
        });

        male.transform.SetPositionAndRotation(new Vector3(150,0,170), Quaternion.identity);
    }
}
