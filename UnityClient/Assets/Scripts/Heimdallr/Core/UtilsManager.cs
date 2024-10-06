using System.Threading.Tasks;
using Cinemachine;
using Core.Input;
using Core.Network;
using Core.Scene;
using Cysharp.Threading.Tasks;
using Heimdallr.Core.Game;
using Heimdallr.Core.Game.Sprite;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityRO.Core.GameEntity;
using UnityRO.Net;

[RequireComponent(typeof(NetworkClient))]
public class UtilsManager : MonoBehaviour {
    [Header(":: Offline settings")] [SerializeField]
    private bool UseMeshEntity = false;

    [SerializeField] private SpriteGameEntity SpritePlayerEntity;
    [SerializeField] private MeshGameEntity MeshPlayerEntity;

    private GameManager GameManager;
    private PlayerInputController InputController;
    private SessionManager SessionManager;

    [SerializeField] private TextMeshProUGUI DebugInfo;
    [SerializeField] private TMP_InputField InputField;
    private float _deltaTime;

    [Header(":: Test Server Only")] [SerializeField]
    private CinemachineVirtualCamera CinemachineVirtualCamera;

    // Whether to orchestrate the login journey
    [SerializeField] private bool OrchestrateConnect = true;

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
        ClothesColor = 2,
        MoveSpeed = 135,
        EntityType = EntityType.PC,
        Name = "Entity",
        GID = 0,
        AttackSpeed = 435,
    };

    private void Awake() {
        GameManager = FindObjectOfType<GameManager>();
        InputController = FindObjectOfType<PlayerInputController>();
        SessionManager = FindObjectOfType<SessionManager>();
    }

    public async void OnEnterPressed(string o) {
        var text = InputField.text;

        if (GameManager.IsOffline)
        {
            var command = text.Split(' ');
            if (command[0] == "@warp" && command.Length == 2)
            {
                await GameSceneManager.LoadScene(command[1], LoadSceneMode.Additive);
            }
            return;
        }
        
        if (text.Length > 0) {
            new CZ.REQUEST_CHAT(SessionManager.CurrentSession.Entity.GetEntityName(), text).Send();
            InputField.text = "";
        }
    }

    private async void Start() {
        GameManager.IsOffline = !OrchestrateConnect;
        CoreGameEntity entity = UseMeshEntity ? MeshPlayerEntity : SpritePlayerEntity;

        CinemachineVirtualCamera.Follow = entity.transform;
        CinemachineVirtualCamera.LookAt = entity.transform;
        
        if (OrchestrateConnect) {
            SpritePlayerEntity.gameObject.SetActive(false);
            MeshPlayerEntity.gameObject.SetActive(false);

            gameObject.AddComponent<BurstConnectionOrchestrator>()
                .Init(CharServerIndex, CharIndex, Username, Password, ServerHost, ForceMap, entity);
        } else {
            await GameSceneManager.LoadScene(ForceMap, LoadSceneMode.Additive);
            var gameMap = FindObjectOfType<GameMap>();
            if (UseMeshEntity) {
                SpritePlayerEntity.gameObject.SetActive(false);
                MeshPlayerEntity.Init(OfflineEntity);
                MeshPlayerEntity.transform.SetPositionAndRotation(new Vector3(157.5f, 0, 210.5f), Quaternion.identity);
            } else {
                MeshPlayerEntity.gameObject.SetActive(false);
                SpritePlayerEntity.Init(OfflineEntity);
                SpritePlayerEntity.transform.SetPositionAndRotation(new Vector3(160, 0, 160), Quaternion.identity);
            }
            SessionManager.OnSessionMapChanged.Invoke(gameMap);
        }
    }

    private void Update() {
        _deltaTime += (Time.deltaTime - _deltaTime) * 0.1f;
        float fps = 1.0f / _deltaTime;
        DebugInfo.text = $"{Mathf.Ceil(fps)} FPS";

        // var horizontal = Input.GetAxis("Horizontal");
        // var vertical = Input.GetAxis("Vertical");
        //
        // var x = horizontal != 0 ? Mathf.Max(horizontal, 2f) * Mathf.Sign(horizontal) : 0;
        // var y = vertical != 0 ? Mathf.Max(vertical, 2f) * Mathf.Sign(vertical) : 0;
        //
        // DebugInfo.text += $"\nDirection: {new Vector3Int((int)x, 0, (int)y)}";

        DebugInfo.text += $"\nTick: {GameManager.Tick}";
        DebugInfo.text += $"\nPing: {GameManager.Ping} ms";
    }
}