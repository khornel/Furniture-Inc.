using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace Furniture_Inc
    {
    public class ModStatusText : MonoBehaviour
        {
        public FurnitureLoader Loader;
        private Text Label;

        void Start()
            {
            Label = GetComponent<Text>();
            }

        void Update()
            {
            if (Loader != null)
                {
                Label.text = Loader.Status;
                }
            }
        }
    }
