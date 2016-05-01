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
        public static int Version = 3;
        public Text InfoText;

        public void ConstructOptionsScreen(RectTransform parent, ModBehaviour[] behaviours)
            {
            var versionLabel = WindowManager.SpawnLabel();
            versionLabel.text = "Version " + Version;
            versionLabel.fontSize += 4;
            WindowManager.AddElementToElement(versionLabel.gameObject, parent.gameObject, new Rect(2, 2, 256, 48), new Rect(0, 0, 0, 0));
            InfoText = WindowManager.SpawnLabel();
            var status = InfoText.gameObject.AddComponent<ModStatusText>();
            status.Loader = behaviours.OfType<FurnitureLoader>().First();
            WindowManager.AddElementToElement(InfoText.gameObject, parent.gameObject, new Rect(2, 48, 512, 512), new Rect(0, 0, 0, 0));
            }

        public string Name
            {
            get { return "Furniture Inc."; }
            }
        }
    }
