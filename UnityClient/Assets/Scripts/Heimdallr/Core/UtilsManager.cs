using Heimdallr.Core.Game;
using TMPro;
using UnityEngine;

public class UtilsManager : MonoBehaviour {

    [SerializeField]
    private GameEntity PlayerEntity;

    [SerializeField]
    private TextMeshProUGUI FpsLabel;
    private float _deltaTime;

    private void Start() {
        PlayerEntity.Init(new GameEntityData {
            HairStyle = 0,
            Eye = 0,
            IsMale = true,
            HairColor = 3,
            Job = 4073,
            ClothesColor = 1,
            MoveSpeed = 135
        });

        PlayerEntity.transform.SetPositionAndRotation(new Vector3(150,0,170), Quaternion.identity);
    }

    private void Update() {
        _deltaTime += (Time.deltaTime - _deltaTime) * 0.1f;
        float fps = 1.0f / _deltaTime;
        FpsLabel.text = $"{Mathf.Ceil(fps)} FPS";
    }
}
