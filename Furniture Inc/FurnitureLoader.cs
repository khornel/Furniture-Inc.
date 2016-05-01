using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using DevConsole;
using UnityEngine;
using UnityEngine.UI;

namespace Furniture_Inc
    {
    public class FurnitureLoader : ModBehaviour
        {
        public bool ShouldLoad = false;
        public bool Loaded = false;
        public bool HasStarted = false;
        public string Status = "Not loaded";

        public override void OnDeactivate()
            {
            //Unfortunately we can't delete furniture after it's been added, so we can't unload mod at runtime
            try
                {
                DevConsole.Console.RemoveCommand("FORCE_RELOAD_FURNITURE");
                }
            catch (Exception)
                {
                //Alpha 8.10.12 might have a null reference for disabled console singleton
                }
            ShouldLoad = false;
            }

        //Load after all other mods to ensure their scripts are loaded in case we need them for furniture
        void Start()
            {
            if (!Loaded && ShouldLoad)
                {
                LoadFurniture();
                }
            HasStarted = true;
            }

        public override void OnActivate()
            {
            ShouldLoad = true;
            try
                {
                DevConsole.Console.AddCommand(new Command("FORCE_RELOAD_FURNITURE", ReLoadFurniture));
                }
            catch (Exception)
                {
                }
            
            //If mod wasn't activated on launch, load it now
            if (HasStarted && !Loaded && ShouldLoad)
                {
                LoadFurniture();
                }
            }

        private void ReLoadFurniture()
            {
            var furns = LoadFurniture();
            if (HUD.Instance != null)
                {
                foreach (var furn in furns.Select(x => x.GetComponent<Furniture>()))
                    {
                    var loc = Localization.GetFurniture(furn.name, furn.ButtonDescription);
                    var buyButton = Instantiate(HUD.Instance.BuyButtonPrefab) as GameObject;
                    buyButton.transform.SetParent(HUD.Instance.BuildButtonPanel.transform, false);
                    var desc = new BuildDescriptor(BuildDescriptor.BuildType.Furniture,
                        furn.Category, furn.Category.LocTry() + furn.Type.LocTry() + loc[0], furn);
                    var button = buyButton.GetComponent<Button>();
                    var localFurn = furn;
                    var gbutton = buyButton.GetComponent<BuildButton>();
                    gbutton.furn = furn;
                    gbutton.ButtonImage.sprite = furn.Thumbnail;
                    button.onClick.AddListener(() =>
                    {
                        BuildController.Instance.BeginBuildFurniture(localFurn.gameObject);
                    });
                    var bButton = typeof(HUD).GetField("BuildButtons", BindingFlags.NonPublic | BindingFlags.Instance);
                    var buttons = (Dictionary<Button, BuildDescriptor>)bButton.GetValue(HUD.Instance);
                    buttons.Add(button, desc);
                    }
                }
            }

        private List<GameObject> LoadFurniture()
            {
            var result = new List<GameObject>();
            Status = "Loaded\nLog:\n";
            //Should not load files twice in same session
            Loaded = true;
            if (!Directory.Exists("DLLMods/Furniture"))
                {
                Directory.CreateDirectory("DLLMods/Furniture");
                }
            else
                {
                var sb = new StringBuilder();
                var success = true;
                foreach (var dir in Directory.GetDirectories("DLLMods/Furniture"))
                    {
                    foreach (var file in Directory.GetFiles(dir, "*.xml"))
                        {
                        sb.AppendLine(Path.GetFileName(dir) + "/" + Path.GetFileName(file));
                        var failedLoading = false;
                        var content = "";
                        XMLParser.XMLNode root = null;
                        try
                            {
                            content = File.ReadAllText(file);
                            }
                        catch (Exception ex)
                            {
                            sb.AppendLine("\tFailed reading file");
                            sb.AppendLine("\t" + ex.ToString());
                            failedLoading = true;
                            }
                        if (!failedLoading)
                            {
                            try
                                {
                                root = XMLParser.ParseXML(content);
                                }
                            catch (Exception ex)
                                {
                                sb.AppendLine("\tFailed parsing xml");
                                sb.AppendLine("\t" + ex.ToString());
                                failedLoading = true;
                                }
                            }
                        GameObject newFurn = null;
                        if (!failedLoading)
                            {
                            newFurn = CreateFurnitureObject(Path.GetFileNameWithoutExtension(file), root, dir, sb, out success);
                            }
                        else
                            {
                            success = false;
                            }
                        if (success)
                            {
                            //Deactivate prefab and save it
                            newFurn.SetActive(false);
                            DontDestroyOnLoad(newFurn);
                            ObjectDatabase.Instance.AddFurniture(newFurn);
                            result.Add(newFurn);
                            sb.AppendLine("\tLoaded succesfuly");
                            }
                        else
                            {
                            if (newFurn != null)
                                {
                                Destroy(newFurn);
                                }
                            sb.AppendLine("\tFailed loading");
                            }
                        //Update status in options window
                        sb.AppendLine();
                        Status += sb.ToString();
                        sb.Clear();
                        }
                    }
                }
            return result;
            }

        public string FindUniqueFurnitureName(string input)
            {
            var current = 1;
            var result = input;
            while (ObjectDatabase.Instance.GetFurniture(result) != null)
                {
                result = input + " " + current;
                current++;
                }
            return result;
            }

        public GameObject CreateFurnitureObject(string furnName, XMLParser.XMLNode root, string rootFolder, StringBuilder output, out bool success)
            {
            GameObject go;
            var existing = root.TryGetAttribute("Base");
            Furniture furn;
            if (existing != null)
                {
                var xFurn = ObjectDatabase.Instance.GetFurniture(existing);
                if (xFurn != null)
                    {
                    go = Instantiate(xFurn);
                    //Remove existing renderers
                    foreach (var child in go.GetComponentsInChildren<Renderer>())
                        {
                        child.transform.SetParent(null);
                        Destroy(child.gameObject);
                        }
                    furn = go.GetComponent<Furniture>();
                    furn.Colorable.Clear();
                    }
                else
                    {
                    output.AppendLine("\tFailed loading furniture " + existing + " using default");
                    go = new GameObject();
                    furn = go.AddComponent<Furniture>();
                    }
                }
            else
                {
                go = new GameObject();
                furn = go.AddComponent<Furniture>();
                }
            //Since this mod only uses standard material, optimize using mesh combine
            furn.UseStandardMat = true;
            //Furniture are identified by their MonoBehavior names (Really dumb, I know)
            go.name = FindUniqueFurnitureName(furnName);
            var thumb = new Texture2D(128, 128);
            thumb.LoadImage(File.ReadAllBytes(Path.Combine(rootFolder, root.GetAttribute("Thumbnail"))));
            furn.Thumbnail = Sprite.Create(thumb, new Rect(0, 0, 128, 128), Vector2.zero);
            GameObject compTrans = null;
            GameObject[] models = null;
            foreach (var node in root.Children)
                {
                if (node.Name.Equals("Models"))
                    {
                    models = new GameObject[node.Children.Count];
                    var i = 0;
                    foreach (var model in node.Children)
                        {
                        try
                            {
                            //Load obj model using script from Unity Wiki
                            var m = ObjImporter.ImportFile(Path.Combine(rootFolder, model.GetNodeValue("File")));
                            var newMesh = new GameObject("SubMesh");
                            newMesh.AddComponent<MeshFilter>().sharedMesh = m;
                            models[i] = newMesh;
                            var rend = newMesh.AddComponent<MeshRenderer>();
                            rend.material = ObjectDatabase.Instance.DefaultFurnitureMaterial;
                            if (Convert.ToBoolean(GetNodeValue(model, "Computer", "False")))
                                {
                                compTrans = newMesh;
                                }
                            //Make sure the mesh has outline when highlighted
                            newMesh.tag = "Highlight";
                            var parent = Convert.ToInt32(GetNodeValue(model, "Parent", "-1"));
                            newMesh.transform.SetParent(parent >= 0 ? models[parent].transform : go.transform);
                            newMesh.transform.localPosition = SVector3.Deserialize(model.GetNodeValue("Position"));
                            newMesh.transform.localRotation = Quaternion.Euler(SVector3.Deserialize(model.GetNodeValue("Rotation")));
                            newMesh.transform.localScale = SVector3.Deserialize(model.GetNodeValue("Scale"));
                            //Make sure mesh gets colored when player changes colors
                            furn.Colorable.Add(rend);
                            }
                        catch (Exception ex)
                            {
                            var fName = GetNodeValue(model, "File", "Undefined");
                            output.AppendLine("\tFailed loading mesh " + fName + " with error:");
                            output.AppendLine("\t" + ex.Message);
                            success = false;
                            return go;
                            }
                        i++;
                        }
                    continue;
                    }
                if (node.Name.Equals("InteractionPoints"))
                    {
                    try
                        {
                        for (var i = 0; i < furn.InteractionPoints.Length; i++)
                            {
                            Destroy(furn.InteractionPoints[i].gameObject);
                            }
                        var newIps = new List<InteractionPoint>();
                        var links = new int[node.Children.Count];
                        var k = 0;
                        foreach (var ip in node.Children)
                            {
                            var ipGo = new GameObject("InteractionPoint");
                            ipGo.transform.SetParent(go.transform);
                            var ipC = ipGo.AddComponent<InteractionPoint>();
                            ipC.transform.localPosition = SVector3.Deserialize(ip.GetNodeValue("Position"));
                            ipC.transform.localRotation = Quaternion.Euler(SVector3.Deserialize(ip.GetNodeValue("Rotation")));
                            ipC.Name = ip.GetNodeValue("Name");
                            ipC.Animation = GetNodeValue(ip, "Animation", 0);
                            ipC.subAnimation = GetNodeValue(ip, "SubAnimation", 0);
                            ipC.MinimumNeeded = GetNodeValue(ip, "MinimumNeeded", 1);
                            ipC.NeedsReachCheck = GetNodeValue(ip, "ReachCheck", true);
                            ipC.Parent = furn;
                            links[k] = GetNodeValue(ip, "Child", -1);
                            newIps.Add(ipC);
                            k++;
                            }
                        furn.InteractionPoints = newIps.ToArray();
                        for (var i = 0; i < furn.InteractionPoints.Length; i++)
                            {
                            if (links[i] > -1)
                                {
                                furn.InteractionPoints[i].Child = furn.InteractionPoints[links[i]];
                                }
                            furn.InteractionPoints[i].Id = i;
                            }
                        }
                    catch (Exception ex)
                        {
                        output.AppendLine("\tFailed loading interaction points with error:");
                        output.AppendLine("\t" + ex.Message);
                        success = false;
                        return go;
                        }
                    continue;
                    }
                if (node.Name.Equals("SnapPoints"))
                    {
                    try
                        {
                        for (var i = 0; i < furn.SnapPoints.Length; i++)
                            {
                            Destroy(furn.SnapPoints[i].gameObject);
                            }
                        var newSnaps = new List<SnapPoint>();
                        var links = new int[node.Children.Count][];
                        var k = 0;
                        foreach (var snap in node.Children)
                            {
                            var snapGo = new GameObject("SnapPoint");
                            if (models != null)
                                {
                                var parent = Convert.ToInt32(GetNodeValue(snap, "MeshParent", "-1"));
                                snapGo.transform.SetParent(parent >= 0 ? models[parent].transform : go.transform);
                                }
                            else
                                {
                                snapGo.transform.SetParent(go.transform);
                                }
                            var snapC = snapGo.AddComponent<SnapPoint>();
                            snapC.transform.localPosition = SVector3.Deserialize(snap.GetNodeValue("Position"));
                            snapC.transform.localRotation = Quaternion.Euler(SVector3.Deserialize(snap.GetNodeValue("Rotation")));
                            snapC.Name = snap.GetNodeValue("Name");
                            snapC.CheckValid = GetNodeValue(snap, "CheckValid", true);
                            snapC.Parent = furn;
                            var n = snap.GetNode("Links", false);
                            links[k] = n != null ? n.Value.Split(',').Select(x => Convert.ToInt32(x)).ToArray() : new int[0];
                            newSnaps.Add(snapC);
                            k++;
                            }
                        furn.SnapPoints = newSnaps.ToArray();
                        for (var i = 0; i < furn.SnapPoints.Length; i++)
                            {
                            furn.SnapPoints[i].InitLinks = links[i].Select(x => furn.SnapPoints[x]).ToArray();
                            furn.SnapPoints[i].Id = i;
                            }
                        }
                    catch (Exception ex)
                        {
                        output.AppendLine("\tFailed loading snap points with error:");
                        output.AppendLine("\t" + ex.Message);
                        success = false;
                        return go;
                        }
                    continue;
                    }
                //Use reflection and string->type converters to create components and modify them

                //These need to be set if we're loading scripts from another mod, otherwise try defaults
                var nameSpace = node.TryGetAttribute("Namespace");
                var assembly = node.TryGetAttribute("Assembly");
                Type t;
                if (nameSpace != null && assembly != null)
                    {
                    t = Type.GetType(nameSpace + node.Name + ", " + assembly);
                    }
                else
                    {
                    t = Type.GetType(node.Name) ?? Type.GetType(node.Name + ", Assembly-CSharp") ?? Type.GetType("UnityEngine." + node.Name + ", UnityEngine");
                    }
                if (t != null)
                    {
                    if (node.TryGetAttribute("Destroy") != null)
                        {
                        var comp = go.GetComponent(t);
                        if (comp != null)
                            {
                            DestroyImmediate(comp);
                            }
                        }
                    else
                        {
                        var comp = go.GetComponent(t) ?? go.AddComponent(t);
                        if (comp != null)
                            {
                            foreach (var element in node.Children)
                                {
                                var field = t.GetField(element.Name);
                                if (field != null)
                                    {
                                    try
                                        {
                                        var val = ConvertValue(field.FieldType, element.Value);
                                        field.SetValue(comp, val);
                                        }
                                    catch (Exception ex)
                                        {
                                        output.AppendLine("\tFailed setting field " + element.Name + ":");
                                        output.AppendLine("\t" + ex.Message);
                                        }
                                    }
                                else
                                    {
                                    var prop = t.GetProperty(element.Name);
                                    if (prop != null)
                                        {
                                        try
                                            {
                                            var val = ConvertValue(prop.PropertyType, element.Value);
                                            prop.SetValue(comp, val, null);
                                            }
                                        catch (Exception ex)
                                            {
                                            output.AppendLine("\tFailed setting property " + element.Name + ":");
                                            output.AppendLine("\t" + ex.Message);
                                            }
                                        }
                                    else
                                        {
                                        output.AppendLine("\tUndefined variable " + element.Name);
                                        }
                                    }
                                }
                            }
                        else
                            {
                            output.AppendLine("\tCouldn't create type " + node.Name);
                            }
                        }
                    }
                else
                    {
                    output.AppendLine("\tUndefined type " + node.Name);
                    }
                }
            if (furn.Colorable.Count == 0)
                {
                output.AppendLine("\tFurniture needs at least one mesh");
                success = false;
                return go;
                }
            //If needed, project vertices to floor, convex hull and set as boundary to avoid wall clipping
            if (!furn.WallFurn && furn.BuildBoundary != null && furn.BuildBoundary.Length > 0)
                {
                furn.MeshBoundary = furn.CalculateBoundary().ToArray();
                }
            furn.ComputerTransform = compTrans == null ? null : compTrans.transform;
            furn.upg = go.GetComponent<Upgradable>();
            furn.Table = go.GetComponent<TableScript>();
            success = true;
            return go;
            }

        private T GetNodeValue<T>(XMLParser.XMLNode node, string name, T def)
            {
            var n = node.GetNode(name, false);
            if (n != null)
                {
                return (T)TypeDescriptor.GetConverter(typeof(T)).ConvertFrom(n.Value);
                }
            return def;
            }

        private object ConvertValue(Type type, string value)
            {
            if (type.IsArray)
                {
                var split = value.Split(new string[] { Environment.NewLine, "\r\n", "\n" },
                    StringSplitOptions.RemoveEmptyEntries);
                var t = type.GetElementType();
                var arr = Array.CreateInstance(t, split.Length);
                for (var i = 0; i < split.Length; i++)
                    {
                    arr.SetValue(ConvertValue(t, split[i]), i);
                    }
                return arr;
                }
            if (type == typeof(Color))
                {
                return SVector3.Deserialize(value).ToColor();
                }
            if (type == typeof(Vector3))
                {
                return SVector3.Deserialize(value).ToVector3();
                }
            if (type == typeof(Vector2))
                {
                return SVector3.Deserialize(value).ToVector2();
                }
            if (type == typeof(Quaternion))
                {
                return Quaternion.Euler(SVector3.Deserialize(value));
                }
            return TypeDescriptor.GetConverter(type).ConvertFrom(value);
            }
        }
    }
