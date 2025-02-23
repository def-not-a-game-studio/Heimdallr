using UnityEngine;
using UnityRO.Core;

namespace Heimdallr.UI
{
    public class CameraBillboard : ManagedMonoBehaviour
    {
        private Camera _camera;

        private void Awake()
        {
            _camera = Camera.main;
        }

        public override void ManagedUpdate()
        {
        }

        public override void ManagedLateUpdate()
        {
            transform.LookAt(transform.position + _camera.transform.rotation * Vector3.forward, _camera.transform.rotation * Vector3.up);
        }
    }
}