using UnityEngine;
using UnityEngine.UIElements;

namespace Heimdallr.UI
{
    public class RootLayoutController : MonoBehaviour
    {
        private UIDocument _rootDocument;
        private VisualElement _root;

        private VisualElement _basicInterfaceMenu;
        private VisualElement _statusWindow;

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            _rootDocument = GetComponent<UIDocument>();
            _root = _rootDocument.rootVisualElement;

            _basicInterfaceMenu = _root.Q<VisualElement>("BasicInterfaceMenu");
            _statusWindow = _root.Q<VisualElement>("StatusWindow");

            _basicInterfaceMenu.Q<Button>("BasicInterfaceButton_Status").clicked += () =>
            {
                _statusWindow.style.visibility = _statusWindow.style.visibility == Visibility.Visible ? Visibility.Hidden : Visibility.Visible;
            };
        }
    }
}