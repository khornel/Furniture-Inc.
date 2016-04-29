using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace Furniture_Inc
    {
    public class FurnMeta : ModMeta
        {
        public Text InfoText;

        public void ConstructOptionsScreen(RectTransform parent, ModBehaviour[] behaviours)
            {
            InfoText = WindowManager.SpawnLabel();
            var status = InfoText.gameObject.AddComponent<ModStatusText>();
            status.Loader = behaviours.OfType<FurnitureLoader>().First();
            WindowManager.AddElementToElement(InfoText.gameObject, parent.gameObject, new Rect(2, 2, 256, 512), new Rect(0, 0, 0, 0));
            }

        public string Name
            {
            get { return "Furniture Inc."; }
            }
        }
    }
