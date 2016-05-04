using UnityEngine;
using UnityEngine.UI;

namespace Furniture_Inc
    {
    public class ModStatusText : MonoBehaviour
        {
        public FurnitureLoader Loader;
        private Text _label;

        void Start()
            {
            _label = GetComponent<Text>();
            }

        void Update()
            {
            if (Loader != null)
                {
                _label.text = Loader.Status;
                }
            }
        }
    }
