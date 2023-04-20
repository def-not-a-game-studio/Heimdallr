using Heimdallr.Core.Game;
using Heimdallr.Core.Game.Sprite;
using Heimdallr.Core.Network;
using TMPro;
using UnityEngine;

[RequireComponent(typeof(NetworkClient), typeof(ThreadManager))]
public class UtilsManager : MonoBehaviour {
    [Header(":: Offline settings")] [SerializeField]
    private bool UseMeshEntity = false;

    [SerializeField] private SpriteGameEntity SpritePlayerEntity;
    [SerializeField] private MeshGameEntity MeshPlayerEntity;


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
            SpritePlayerEntity.gameObject.SetActive(false);
            MeshPlayerEntity.gameObject.SetActive(false);
            
            gameObject.AddComponent<BurstConnectionOrchestrator>()
                .Init(CharServerIndex, CharIndex, Username, Password, ServerHost, ForceMap, UseMeshEntity ? MeshPlayerEntity : SpritePlayerEntity);
        } else {
            if (UseMeshEntity) {
                SpritePlayerEntity.gameObject.SetActive(false);
                MeshPlayerEntity.Init(OfflineEntity);
                MeshPlayerEntity.transform.SetPositionAndRotation(new Vector3(157, 0, 210), Quaternion.identity);
            } else {
                MeshPlayerEntity.gameObject.SetActive(false);
                SpritePlayerEntity.Init(OfflineEntity);
                SpritePlayerEntity.transform.SetPositionAndRotation(new Vector3(157, 0, 210), Quaternion.identity);
            }
        }
    }

    private void Update() {
        _deltaTime += (Time.deltaTime - _deltaTime) * 0.1f;
        float fps = 1.0f / _deltaTime;
        FpsLabel.text = $"{Mathf.Ceil(fps)} FPS";
        
        var horizontal = Input.GetAxis("Horizontal");
        var vertical = Input.GetAxis("Vertical");

        var x = horizontal != 0 ? Mathf.Max(horizontal, 2f) * Mathf.Sign(horizontal) : 0;
        var y = vertical != 0 ? Mathf.Max(vertical, 2f) * Mathf.Sign(vertical) : 0;

        FpsLabel.text += $"\nDirection: {new Vector3Int((int)x, 0, (int)y)}";
    }
}