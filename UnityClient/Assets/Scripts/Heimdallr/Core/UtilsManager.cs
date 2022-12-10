using Heimdallr.Core.Game;
using UnityEngine;

public class UtilsManager : MonoBehaviour {

    [SerializeField]
    private GameEntity PlayerEntity;

    private void Start() {
        PlayerEntity.Init(new GameEntityData {
            HairStyle = 0,
            Eye = 0,
            IsMale = true,
            HairColor = 3,
            Job = 0,
        });

        PlayerEntity.transform.SetPositionAndRotation(new Vector3(150,0,170), Quaternion.identity);
    }
}
