using Heimdallr.Core.Game;
using Heimdallr.Core.Network;
using TMPro;
using UnityEngine;

[RequireComponent(typeof(NetworkClient), typeof(ThreadManager))]
public class UtilsManager : MonoBehaviour {
    [SerializeField] private MeshGameEntity PlayerEntity;

    [SerializeField] private TextMeshProUGUI FpsLabel;
    private float _deltaTime;

    [Header(":: Test Server Only")]
    // Whether to orchestrate the login journey
    [SerializeField]
    private bool OrchestrateConnect = true;

    [SerializeField] private int CharServerIndex = 0;
    [SerializeField] private int CharIndex = 0;
    [SerializeField] private string Username = "danilo";
    [SerializeField] private string Password = "123456";
    [SerializeField] private string ServerHost = "127.0.0.1";
    [SerializeField] private string ForceMap = "prt_fild08";

    [SerializeField] private GameEntityBaseStatus OfflineEntity = new() {
        HairStyle = 0,
        IsMale = false,
        HairColor = 3,
        Job = 3,
        ClothesColor = 0,
        MoveSpeed = 135,
        EntityType = EntityType.PC,
        Name = "Entity",
        GID = 0,
        AttackSpeed = 435,
    };

    private void Start() {
        if (OrchestrateConnect) {
            gameObject.AddComponent<BurstConnectionOrchestrator>()
                      .Init(CharServerIndex, CharIndex, Username, Password, ServerHost, ForceMap, PlayerEntity);
        } else {
            PlayerEntity.Init(OfflineEntity);

            PlayerEntity.transform.SetPositionAndRotation(new Vector3(150, 0, 170), Quaternion.identity);
        }
    }

    private void Update() {
        _deltaTime += (Time.deltaTime - _deltaTime) * 0.1f;
        float fps = 1.0f / _deltaTime;
        FpsLabel.text = $"{Mathf.Ceil(fps)} FPS";
    }
}