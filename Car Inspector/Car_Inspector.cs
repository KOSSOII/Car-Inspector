using HutongGames.PlayMaker;
using MSCLoader;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using static System.Net.Mime.MediaTypeNames;

namespace Car_Inspector
{
    public class Car_Inspector : Mod
    {
        public override string ID => "Car_Inspector"; // Your (unique) mod ID 
        public override string Name => "Car Inspector"; // Your mod name
        public override string Author => "Izuko"; // Name of the Author (your name)
        public override string Version => "1.4"; // Version
        public override string Description => "Show Corris parts wear and adjustments"; // Short description of your mod 
        public override Game SupportedGames => Game.MyWinterCar;
        public class PartInfoFloat
        {
            public string Path;
            public string Name;
            public FsmFloat PartValue;
        }
        public class ValvesAdjs:PartInfoFloat
        {
            public string OriginalName;
        }
        public class PartInfoBool
        {
            public string Path;
            public string Name;
            public FsmBool PartValue;
        }
        private List<PartInfoFloat> runningParams = new List<PartInfoFloat>();
        Dictionary<string,string> wearPartsDictEngine = new Dictionary<string,string>();
        Dictionary<string, string> wearPartsDictEngineAttach = new Dictionary<string, string>();
        Dictionary<string, string> wearPartsDictIgnSystem = new Dictionary<string, string>();
        Dictionary<string, string> wearPartsDictSusp = new Dictionary<string, string>();
        Dictionary<string, string> wearPartsDictTransminion = new Dictionary<string, string>();

        Dictionary<string, string> wearPartsDictMisc = new Dictionary<string, string>();
        Dictionary<string, string> beltsPartsDictMisc = new Dictionary<string, string>();
        Dictionary<string, string> wheelsPartsDict = new Dictionary<string, string>();
        Dictionary<string, string> damagedDict = new Dictionary<string, string>();

        Dictionary<string, string> carTunesDict = new Dictionary<string, string>();

        private List<PartInfoFloat> wearPartsEngine = new List<PartInfoFloat>();
        private List<PartInfoFloat> wearPartsAttach = new List<PartInfoFloat>();
        private List<PartInfoFloat> wearPartsIgnition = new List<PartInfoFloat>();
        private List<PartInfoFloat> wearPartsSusp= new List<PartInfoFloat>();
        private List<PartInfoFloat> wearPartsTransition = new List<PartInfoFloat>();
        private List<PartInfoFloat> wearMisc = new List<PartInfoFloat>();
        private List<PartInfoFloat> wearBelts = new List<PartInfoFloat>();
        private List<PartInfoFloat> wheelsHeal = new List<PartInfoFloat>();
        private List<PartInfoFloat> carTunesValues = new List<PartInfoFloat>();
        private List<PartInfoBool> damagedParts = new List<PartInfoBool>();
        private List<PartInfoFloat> wheelsTune = new List<PartInfoFloat>();
        private List<ValvesAdjs> valvesList = new List<ValvesAdjs>();
        GUIStyle percentStyle;
        GUIStyle headerStyle;
        Transform crankPosition;
        Transform camPosition;
        FsmFloat oilCurrent;
        FsmFloat oilContamination;
        float contaminationWarning = 70;
        FsmFloat oilMax;
        FsmFloat fuelCurrent;
        FsmFloat fuelMax;
        FsmFloat BrakeCurrent;
        FsmFloat BatteryCurrent;
        FsmFloat BatteryMax;
        FsmFloat oilfilterDirt;
        FsmVector3 WheelFLRotation;
        FsmVector3 WheelFRRotation;
        FsmVector3 WheelRLRotation;
        FsmVector3 WheelRRRotation;
        FsmFloat ATFLevel;
        FsmFloat AFTMax;
        FsmFloat coolantMax;
        FsmFloat coolantCurrent;
        FsmFloat OilPressure;
        FsmFloat CoolantTemp;
        FsmFloat EngineTemp;

        SettingsSlider _windowScale;
        SettingsCheckBox _manulScale;
        SettingsCheckBox _hideValues;
        SettingsCheckBox _showDebugMSG;
        SettingsKeybind keybind;
        bool show;

        Rect windowRect = new Rect(200, 200, 950, 900);
        static readonly Vector2 REFERENCE_RES = new Vector2(1920f, 1080f);
        public override void ModSetup()
        {
            SetupFunction(Setup.OnLoad, Mod_OnLoad);
            SetupFunction(Setup.Update, Mod_Update);
            SetupFunction(Setup.FixedUpdate, Mod_FixedUpdate);
            SetupFunction(Setup.ModSettings, Mod_Settings);
            SetupFunction(Setup.OnGUI, OnGUI);
            SetupFunction(Setup.ModSettingsLoaded, Mod_SettingsLoaded);
        }
        private void Mod_SettingsLoaded()
        {
            if (_manulScale.GetValue())
            {
                _windowScale.SetVisibility(true);
            }
        }
        private void InitEntities()
        {
            //Rockers
            wearPartsDictEngine.Add("VINP_Rocker1Exh", "Rocker #1 Exh");
            wearPartsDictEngine.Add("VINP_Rocker2In", "Rocker #2 In");
            wearPartsDictEngine.Add("VINP_Rocker3Exh", "Rocker #3 Exh");
            wearPartsDictEngine.Add("VINP_Rocker4In", "Rocker #4 In");
            wearPartsDictEngine.Add("VINP_Rocker5Exh", "Rocker #5 Exh");
            wearPartsDictEngine.Add("VINP_Rocker6In", "Rocker #6 In");
            wearPartsDictEngine.Add("VINP_Rocker7Exh", "Rocker #7 Exh");
            wearPartsDictEngine.Add("VINP_Rocker8In", "Rocker #8 In");

            wearPartsDictEngine.Add("VINP_CamShaft", "Camshaft");
            wearPartsDictEngine.Add("VINP_Headgasket", "Head Gasket");
            //Pistons
            wearPartsDictEngine.Add("VINP_Piston1", "Piston #1");
            wearPartsDictEngine.Add("VINP_Piston2", "Piston #2");
            wearPartsDictEngine.Add("VINP_Piston3", "Piston #3");
            wearPartsDictEngine.Add("VINP_Piston4", "Piston #4");

            wearPartsDictEngine.Add("VINP_Crankshaft", "Crankshaft");
            //Main brngs
            wearPartsDictEngine.Add("VINP_Mainbearing1", "Main Bearing #1");
            wearPartsDictEngine.Add("VINP_Mainbearing2", "Main Bearing #2");
            wearPartsDictEngine.Add("VINP_Mainbearing3", "Main Bearing #3");
            wearPartsDictEngine.Add("VINP_Mainbearing4", "Main Bearing #4");
            wearPartsDictEngine.Add("VINP_Mainbearing5", "Main Bearing #5");

            wearPartsDictEngine.Add("VINP_AuxShaft", "Aux Shaft");

            wearPartsDictEngine.Add("VINP_Oilpump", "Oil Pump");

            wearPartsDictEngine.Add("VINP_Fuelpump", "Fuel Pump");

            wearPartsDictEngine.Add("VINP_Waterpump", "Water Pump");

            wearPartsDictEngine.Add("VINP_Thermostat", "Thermostat");

            wearPartsDictEngineAttach.Add("VINP_Alternator", "Alternator");
            wearPartsDictEngineAttach.Add("VINP_Starter", "Starter");

            wearPartsDictIgnSystem.Add("VINP_Distributor", "Distributor");
            wearPartsDictIgnSystem.Add("VINP_IgnitionCoil", "Ignition Coil");
            //Spark
            wearPartsDictIgnSystem.Add("VINP_Sparkplug1", "Spark plug #1");
            wearPartsDictIgnSystem.Add("VINP_Sparkplug2", "Spark plug #2");
            wearPartsDictIgnSystem.Add("VINP_Sparkplug3", "Spark plug #3");
            wearPartsDictIgnSystem.Add("VINP_Sparkplug4", "Spark plug #4");

            //Springs
            wearPartsDictSusp.Add("VINP_CoilspringFL", "Coil Spring - FL");
            wearPartsDictSusp.Add("VINP_CoilspringFR", "Coil Spring - FR");
            wearPartsDictSusp.Add("VINP_CoilspringRL", "Coil Spring - RL");
            wearPartsDictSusp.Add("VINP_CoilspringRR", "Coil Spring - RR");
            //Shocks
            wearPartsDictSusp.Add("VINP_ShockFL", "Shock Absorber - FL");
            wearPartsDictSusp.Add("VINP_ShockFR", "Shock Absorber - FR");
            wearPartsDictSusp.Add("VINP_ShockRL", "Shock Absorber - RL");
            wearPartsDictSusp.Add("VINP_ShockRR", "Shock Absorber - RR");

            //Transmision
            wearPartsDictTransminion.Add("VINP_Gearbox", "Gearbox");
            wearPartsDictTransminion.Add("VINP_ClutchCable", "Clutch Cable");
            wearPartsDictTransminion.Add("VINP_Clutchdisc", "Clutch Disc");
            wearPartsDictTransminion.Add("VINP_Driveshaft", "Driveshaft");
            wearPartsDictTransminion.Add("VINP_RearAxle", "Rear Axle");

            //Misc
            wearPartsDictMisc.Add("VINP_SteeringRack", "Steering Rack");
            wearPartsDictMisc.Add("VINP_BrakeMasterCylinder", "Brake Master Cylinder");
            wearPartsDictMisc.Add("VINP_Heaterbox", "Heater Box");
            wearPartsDictMisc.Add("VINP_Wipermotor", "Wiper Motor");
            wearPartsDictMisc.Add("VINP_Radiator", "Radiator");

            beltsPartsDictMisc.Add("VINP_FanBelt", "Fan Belt");
            beltsPartsDictMisc.Add("VINP_TimingBelt", "Timing Belt");

            //Wheels
            wheelsPartsDict.Add("VINP_WheelFL", "Wheel Front Left");
            wheelsPartsDict.Add("VINP_WheelFR", "Wheel Front Right");
            wheelsPartsDict.Add("VINP_WheelRL", "Wheel Rear Left");
            wheelsPartsDict.Add("VINP_WheelRR", "Wheel Rear Right");

            damagedDict.Add("VINP_Block", "Engine Block");
            damagedDict.Add("VINP_Cylinderhead", "Cylinder Head");
            damagedDict.Add("VINP_Oilpan", "Oil Pan");
            damagedDict.Add("VINP_RockerCover", "Rocker Cover");
        }

        private void Mod_Settings()
        {
            Keybind.AddHeader("Keybind");
            keybind = Keybind.Add("KB1", "Show Inspector Window", KeyCode.F10);
            Settings.AddHeader("Debug");
            _showDebugMSG = Settings.AddCheckBox("_showDebugMSG", "Show debug messages", false);
            _manulScale = Settings.AddCheckBox("_manulScale", "Scale Inspector Window", false, SettingChanged);
            _windowScale = Settings.AddSlider("Window Scale Sl", "Window Scale", 0.1f, 10f, 1f,visibleByDefault:false);
            _hideValues = Settings.AddCheckBox("_hideValues", "Hide values under spoiler",false);

        }
        private void SettingChanged()
        {
            if(_manulScale.GetValue())
            {
                _windowScale.SetVisibility(true);
            }
            else
            {
                _windowScale.SetVisibility(false);
            }
        }
        private void UpdatePartsEntities()
        {
            //Rockers

            wearPartsEngine.Clear();
            foreach (var part in wearPartsDictEngine)
            {
                wearPartsEngine.Add(InitPartValue(part.Value, part.Key));
            }
            wearPartsAttach.Clear();
            foreach (var part in wearPartsDictEngineAttach)
            {
                wearPartsAttach.Add(InitPartValue(part.Value, part.Key));
            }
            wearPartsIgnition.Clear();
            foreach (var part in wearPartsDictIgnSystem)
            {
                wearPartsIgnition.Add(InitPartValue(part.Value, part.Key));
            }
            wearPartsSusp.Clear();
            foreach (var part in wearPartsDictSusp)
            {
                wearPartsSusp.Add(InitPartValue(part.Value, part.Key));
            }
            wearPartsTransition.Clear();
            foreach (var part in wearPartsDictTransminion)
            {
                wearPartsTransition.Add(InitPartValue(part.Value, part.Key));
            }
            wearMisc.Clear();
            foreach (var part in wearPartsDictMisc)
            {
                wearMisc.Add(InitPartValue(part.Value, part.Key));
            }
            wearBelts.Clear();
            foreach (var part in beltsPartsDictMisc)
            {
                wearBelts.Add(InitPartValue(part.Value, part.Key));
            }
            wheelsHeal.Clear();
            foreach (var part in wheelsPartsDict)
            {
                wheelsHeal.Add(InitPartValue(part.Value, part.Key, "TireHealth"));
            }
            damagedParts.Clear();
            foreach (var part in damagedDict)
            {
                damagedParts.Add(InitDamaged(part.Value, part.Key));
            }

            var wheelFR = GameObject.Find("WHEELc_FR");
            if (wheelFR != null)
            {
                var wheelFRDataComp = wheelFR.GetComponents<PlayMakerFSM>().Where(x => x.FsmName == "Data").First();
                if (wheelFRDataComp != null)
                {
                    WheelFRRotation = wheelFRDataComp.GetVariable<FsmVector3>("Rotation");
                }
            }


            var wheelFL = GameObject.Find("WHEELc_FL");
            if (wheelFL != null)
            {
                var wheelFLDataComp = wheelFL.GetComponents<PlayMakerFSM>().Where(x => x.FsmName == "Data").First();
                if (wheelFLDataComp != null)
                {
                    WheelFLRotation = wheelFLDataComp.GetVariable<FsmVector3>("Rotation");
                }
            }

            //Bend for Read removed from game? 01.02.2026

            //var wheelRR = GameObject.Find("WHEELc_RL");
            //if (wheelRR != null)
            //{
            //    var wheelRRDataComp = wheelRR.GetComponents<PlayMakerFSM>().Where(x => x.FsmName == "Data").First();
            //    if (wheelRRDataComp != null)
            //    {
            //        WheelRRRotation = wheelRRDataComp.GetVariable<FsmVector3>("Rotation");
            //    }
            //}

            //var wheelRL = GameObject.Find("WHEELc_RL");
            //if (wheelRL != null)
            //{
            //    var wheelRLDataComp = wheelRL.GetComponents<PlayMakerFSM>().Where(x => x.FsmName == "Data").First();
            //    if (wheelRLDataComp != null)
            //    {
            //        WheelRLRotation = wheelRLDataComp.GetVariable<FsmVector3>("Rotation");
            //    }
            //}

            InitCarTunes();
            InitLiq();
            initRunningVals();
        }
        private void Mod_OnLoad()
        {
            InitEntities();
            UpdatePartsEntities();
        }
        void OnGUI()
        {
            if (!show) return;

            percentStyle = new GUIStyle(GUI.skin.label);
            percentStyle.alignment = TextAnchor.MiddleRight;
            percentStyle.fontSize = 14;
            headerStyle = new GUIStyle(GUI.skin.label);
            headerStyle.fontSize = 18;
            headerStyle.alignment = TextAnchor.MiddleCenter;

            float scaleX = Screen.width / REFERENCE_RES.x;
            float scaleY = Screen.height / REFERENCE_RES.y;


            float scale = Mathf.Min(scaleX, scaleY);
            if(_manulScale.GetValue())
            {
                scale = _windowScale.GetValue();
            }
            Matrix4x4 oldMatrix = GUI.matrix;

            GUI.matrix = Matrix4x4.TRS(
                Vector3.zero,
                Quaternion.identity,
                new Vector3(scale, scale, 1f)
            );


            windowRect = GUI.Window(1234, windowRect, DisplayReport, "Car Inspector 1.4");

            GUI.matrix = oldMatrix;

        }
        private void ToggleGui(bool value)
        {
            show = value;
        }
        private void Mod_Update()
        {
            if (keybind.GetKeybindDown())
            {
                ToggleGui(!show);
            }               
        }
        int times = 0;
        private void Mod_FixedUpdate()
        {
            if (!show) return;

            if (times >=240)
            {
                UpdatePartsEntities();
                times = 0;
            }
            else
            {
                times++;
            }
        }
        private void initValveTunes()
        {
            valvesList.Clear();
            var AdjustmentsGameObj = GameObject.Find("CORRIS").transform.Find("MotorPivot/MassCenter/Block/VINP_Block/Engine Block(VINX0)/VINP_Cylinderhead/Cylinder Head(VINX0)/ValveAdjustment/Masked");
            if (AdjustmentsGameObj != null) {
                var valves = AdjustmentsGameObj.GetComponentsInChildren<PlayMakerFSM>(true).Where(x => x.FsmName == "Screw").ToList();
                if (valves.Count > 0)
                {
                    foreach (var adjComp in valves)
                    {
                        var value = adjComp.GetVariable<FsmFloat>("AdjustmentF");
                        var name = adjComp.GetVariable<FsmString>("Name");
                        switch (name.Value)
                        {
                            case "bolt0":
                                valvesList.Add(new ValvesAdjs
                                {
                                    PartValue = value,
                                    OriginalName = name.Value,
                                    Name = "Valve #1 In C1"
                                });
                                break;
                            case "bolt1":
                                valvesList.Add(new ValvesAdjs
                                {
                                    PartValue = value,
                                    OriginalName = name.Value,
                                    Name = "Valve #2 Exh C1"
                                });
                                break;
                            case "bolt2":
                                valvesList.Add(new ValvesAdjs
                                {
                                    PartValue = value,
                                    OriginalName = name.Value,
                                    Name = "Valve #3 In C2"
                                });
                                break;
                            case "bolt3":
                                valvesList.Add(new ValvesAdjs
                                {
                                    PartValue = value,
                                    OriginalName = name.Value,
                                    Name = "Valve #4 Exh C2"
                                });
                                break;
                            case "bolt4":
                                valvesList.Add(new ValvesAdjs
                                {
                                    PartValue = value,
                                    OriginalName = name.Value,
                                    Name = "Valve #5 In C3"
                                });
                                break;
                            case "bolt5":
                                valvesList.Add(new ValvesAdjs
                                {
                                    PartValue = value,
                                    OriginalName = name.Value,
                                    Name = "Valve #6 Exh C3"
                                });
                                break;
                            case "bolt6":
                                valvesList.Add(new ValvesAdjs
                                {
                                    PartValue = value,
                                    OriginalName = name.Value,
                                    Name = "Valve #7 In C4"
                                });
                                break;
                            case "bolt7":
                                valvesList.Add(new ValvesAdjs
                                {
                                    PartValue = value,
                                    OriginalName = name.Value,
                                    Name = "Valve #8 Exh C4"
                                });
                                break;
                        }
                    }
                }
            } 
            

        }
        private void InitCarTunes()
        {
            initValveTunes();
            carTunesValues.Clear();
            carTunesValues.Add(InitPartValue("Carb. AF", "VINP_Carburettor", "SettingMixture"));
            carTunesValues.Add(InitPartValue("Dist. Angle", "VINP_Distributor", "SparkAngle"));
            carTunesValues.Add(InitPartValue("Timing Cam", "CORRIS/Simulation/Systems/EngineTiming", "RotationCam", "TimingData"));
            carTunesValues.Add(InitPartValue("Timing Crank", "CORRIS/Simulation/Systems/EngineTiming", "RotationCrank", "TimingData"));

            wheelsTune.Clear();
            wheelsTune.Add(InitPartValue("Wheel Left", "VINP_SteeringRack", VaribleName:"TieRodLeftSetting"));
            wheelsTune.Add(InitPartValue("Wheel Right", "VINP_SteeringRack", VaribleName: "TieRodRightSetting"));

            var crankObject = GameObject.Find("CrankshaftParent/VINP_CrankPulley");
            if (crankObject != null)
            {
                var crankTransform = crankObject.transform;
                if(crankTransform != null)
                {
                    if(crankTransform.localRotation != null)
                    {

                        crankPosition = crankTransform;


                    }
                    
                }
            }
            var camObject = GameObject.Find("CamParent/VINP_CamshaftSprocket");
            if (camObject != null)
            {
                var camTransform = camObject.transform;
                if (camTransform != null)
                {
                    if (camTransform.localRotation != null)
                    {
                        camPosition = camTransform;
                    }

                }
            }
        }
        float Clamp(float value, float min, float max)
        {
            if (value < min) return min;
            if (value > max) return max;
            return value;
        }
        float GetPercentLiquid(float value,float min, float max)
        {
            if (max == 0)
                max = 10;
            value = Clamp(value, min, max);
            return (value - min) / (max - min) * 100f;
        }
        private void InitLiq()
        {

            var radiator = GameObject.Find("VINP_Radiator");
            if (radiator != null)
            {
                var radiatprComp = radiator.GetComponents<PlayMakerFSM>().Where(x => x.FsmName == "Data").First();
                if (radiatprComp != null)
                {
                    coolantCurrent = radiatprComp.GetVariable<FsmFloat>("Coolant");
                    coolantMax = radiatprComp.GetVariable<FsmFloat>("CoolantMax");
                }
            }

            var oilpan = GameObject.Find("VINP_Oilpan");
            if(oilpan != null)
            {
                var dataComp = oilpan.GetComponents<PlayMakerFSM>().Where(x => x.FsmName == "Data").First();
                if(dataComp != null)
                {
                    oilCurrent = dataComp.GetVariable<FsmFloat>("Oil");
                    oilMax = dataComp.GetVariable<FsmFloat>("OilMax");
                    oilContamination = dataComp.GetVariable<FsmFloat>("OilContamination");
                }
            }
            var oilFilter = GameObject.Find("VINP_Oilfilter");
            if(oilFilter != null)
            {
                var filterComp = oilFilter.GetComponents<PlayMakerFSM>().Where(x => x.FsmName == "Data").First();
                if(filterComp != null)
                {
                    oilfilterDirt = filterComp.GetVariable<FsmFloat>("Dirt");
                }
            }
            var brakeCyl = GameObject.Find("VINP_BrakeMasterCylinder");
            if(brakeCyl != null)
            {
                var brakedataComp = brakeCyl.GetComponents<PlayMakerFSM>().Where(x => x.FsmName == "Data").First();
                if(brakedataComp != null)
                {
                    BrakeCurrent = brakedataComp.GetVariable<FsmFloat>("BrakeFluidF");
                }
            }
            var battery = GameObject.Find("VINP_Battery");
            if (battery != null)
            {
                var batteryComp = battery.GetComponents<PlayMakerFSM>().Where(x => x.FsmName == "Data").First();
                if(batteryComp != null)
                {
                    BatteryCurrent = batteryComp.GetVariable<FsmFloat>("Charge");
                    BatteryMax = batteryComp.GetVariable<FsmFloat>("ChargeMax");
                }
            }
            var fueltank = GameObject.Find("VINP_Fueltank");
            if (fueltank != null)
            {
                var fueldataComp = fueltank.GetComponents<PlayMakerFSM>().Where(x => x.FsmName == "Data").First();
                if (fueldataComp != null)
                {
                    fuelCurrent= fueldataComp.GetVariable<FsmFloat>("FuelLevel");
                    fuelMax = fueldataComp.GetVariable<FsmFloat>("MaxCapacity");
                }
            }
            var automaticGearbox = GameObject.Find("VINP_Gearbox");
            if(automaticGearbox != null)
            {
                var gearboxDataComp = automaticGearbox.GetComponents<PlayMakerFSM>().Where(x => x.FsmName == "Data").First();
                if(gearboxDataComp != null)
                {
                    var Current = gearboxDataComp.GetVariable<FsmFloat>("OilLevel");
                    if(Current != null)
                    {
                        ATFLevel = Current;
                    }
                    var maxlevel = gearboxDataComp.GetVariable<FsmFloat>("OilMax");
                    if(maxlevel != null)
                    {
                        AFTMax = maxlevel;
                    }
                }
            }
        }
        private PartInfoFloat InitPartValue(string Name, string Path,string VaribleName = "Wear",string FSMName = "Data")
        {
            var gameObj = GameObject.Find(Path);
            if(gameObj == null)
            {
                if (_showDebugMSG.GetValue())
                    ModConsole.Log($"Part {Name}\n On Path {Path}\n Not found. Skip");
                return null;
            }
                

            var dataFSM = gameObj.GetComponents<PlayMakerFSM>().Where(x=> x.FsmName == FSMName).First();
            if(dataFSM == null)
            {
                if (_showDebugMSG.GetValue())
                    ModConsole.Log($"For Part {Name} Data FSM Not found. Skip");
                return null;
            }
            var value = dataFSM.GetVariable<FsmFloat>(VaribleName);
            if(value == null)
            {
                if (_showDebugMSG.GetValue())
                    ModConsole.Log($"Value with {VaribleName} Not found. Skip");
                return null;
            }
            return new PartInfoFloat
            {
                Name = Name,
                Path = Path,
                PartValue = value
            };
        }
        private PartInfoBool InitDamaged(string Name, string Path, string VaribleName = "Damaged")
        {
            var gameObj = GameObject.Find(Path);
            if (gameObj == null)
            {
                if (_showDebugMSG.GetValue())
                    ModConsole.Log($"Part {Name}\n On Path {Path}\n Not found. Skip");
                return null;

            }
                
            var dataFSM = gameObj.GetComponents<PlayMakerFSM>().Where(x => x.FsmName == "Data").First();
            if (dataFSM == null)
            {
                if (_showDebugMSG.GetValue())
                    ModConsole.Log($"For Part {Name} Data FSM Not found. Skip");
                return null;
            }
                

            return new PartInfoBool
            {
                Name = Name,
                Path = Path,
                PartValue = dataFSM.GetVariable<FsmBool>(VaribleName)
            };
        }
        public static Color GetColorByPercent(float percent,bool invert = false)
        {
            if (invert)
                percent = 100 - percent;
            // Clamp percent to 0..100
            percent = Mathf.Clamp(percent, 0f, 100f);

            if (percent >= 70f)
            {
                return Color.green;       // 96–70
            }
            else if (percent >= 40f)
            {
                return Color.yellow;      // 70–40
            }
            else if (percent >= 20f)
            {
                return new Color(1f, 0.5f, 0f); // Orange 40–20
            }
            else
            {
                return Color.red;         // 20–0
            }
        }
        private void initRunningVals()
        {
            var oilsim = GameObject.Find("CORRIS/Simulation/Engine/Oil");
            if (oilsim != null)
            {
                var oilDataComp = oilsim.GetComponents<PlayMakerFSM>().Where(x => x.FsmName == "Oil").First();
                if (oilDataComp != null)
                {
                    var pressure = oilDataComp.GetVariable<FsmFloat>("OilPressure");
                    OilPressure = pressure;
                }
            }
            var coolingSim = GameObject.Find("CORRIS/Simulation/Systems/Cooling");
            if(coolingSim != null)
            {
                var coolingDataComp = coolingSim.GetComponents<PlayMakerFSM>().Where(x => x.FsmName == "Cooling").First();
                if(coolingDataComp != null)
                {
                    var coolatnTemp = coolingDataComp.GetVariable<FsmFloat>("CoolantTemp");
                    CoolantTemp = coolatnTemp;

                }
            }

            EngineTemp = FsmVariables.GlobalVariables.FindFsmFloat("EngineTemp");


        }
        Dictionary<string, bool> revealValue = new Dictionary<string, bool>();
        struct GuiColorScope : IDisposable
        {
            Color old;
            public GuiColorScope(Color color)
            {
                old = GUI.color;
                GUI.color = color;
            }

            public void Dispose()
            {
                GUI.color = old;
            }
        }
        void DrawRevealValue(string key, string value, GUIStyle style, float width, bool hidden, Color color)
        {
  
            if (hidden)
            {
                if (!revealValue.ContainsKey(key)) 
                    revealValue[key] = false;

                
                if(revealValue[key])
                    GUI.color = color;
                if (GUILayout.Button(revealValue[key] ? value : "•SHOW•", style, GUILayout.Width(width)))
                {
                    revealValue[key] = !revealValue[key];
                }
                if (revealValue[key])
                    GUI.color = original;
            }
            else
            {
                GUI.color = color;
                GUILayout.Label(value, style, GUILayout.Width(width));
                GUI.color = original;
            }

        }
        Color original = GUI.color;
        private void DisplayReport(int id)
        {
            var isValuesHidden = _hideValues.GetValue();
            GUILayout.BeginVertical();
            GUILayout.Space(10);
            GUILayout.BeginHorizontal();
            // Column 1
            GUILayout.BeginVertical(GUILayout.Width(235));
            GUILayout.Label("Engine main parts condition", headerStyle);
            GUILayout.Space(5);
            foreach (var part in wearPartsEngine)
            {
                GUILayout.BeginHorizontal();
                try
                {
                    GUILayout.Label(" " + part.Name, GUILayout.Width(140));
                    var value = part.PartValue.Value;
                    var valueFormated = value.ToString("00.00") + "%";
                    float valueWidth = 60f;
                    DrawRevealValue(part.Name, valueFormated, percentStyle, valueWidth, isValuesHidden, GetColorByPercent(value));
                }
                catch {
                    if (_showDebugMSG.GetValue())
                        ModConsole.Log($"Build GUI Error. {part.Name}. Skip this line");
                }
                GUILayout.EndHorizontal();

            }
            //Part 2
            GUILayout.Label("Damaged?", headerStyle);
            GUILayout.Space(5);
            foreach (var part in damagedParts)
            {
                GUILayout.BeginHorizontal();
                try
                {

                    GUILayout.Label(" " + part.Name, GUILayout.Width(140));
                    var value = part.PartValue.Value;
                    if (value)
                    {
                        var valueWidht = 60f;
                        DrawRevealValue(part.Name, "Damaged!", percentStyle, valueWidht, isValuesHidden, Color.red);

                    }
                    else
                    {
                        var valueWidht = 60f;
                        DrawRevealValue(part.Name, "OK!", percentStyle, valueWidht,isValuesHidden,Color.green);

                    }

                }
                catch {
                    if (_showDebugMSG.GetValue())
                        ModConsole.Log($"Build GUI Error. {part.Name}. Skip this line");
                }
                GUILayout.EndHorizontal();

            }
            GUILayout.EndVertical();

            // Column 2
            GUILayout.BeginVertical(GUILayout.Width(235));
            GUILayout.Label("Engine attachments condition", headerStyle);
            GUILayout.Space(5);
            foreach (var part in wearPartsAttach)
            {
                GUILayout.BeginHorizontal();
                try
                {

                    GUILayout.Label(" " + part.Name, GUILayout.Width(140));
                    var value = part.PartValue.Value;
                    var valueFormated = value.ToString("00.00") + "%";
                    float valueWidth = 60f;
                    DrawRevealValue(part.Name, valueFormated, percentStyle, valueWidth, isValuesHidden, GetColorByPercent(value));
                    
                }
                catch {
                    if (_showDebugMSG.GetValue())
                        ModConsole.Log($"Build GUI Error. {part.Name}. Skip this line");
                }
                GUILayout.EndHorizontal();
            }

            //Part 2 
            GUILayout.Label("Ignition system condition", headerStyle);
            GUILayout.Space(5);
            foreach (var part in wearPartsIgnition)
            {
                GUILayout.BeginHorizontal();
                try
                {

                    GUILayout.Label(" " + part.Name, GUILayout.Width(140));

                    var value = part.PartValue.Value;
                    var valueFormated = value.ToString("00.00") + "%";
                    float valueWidth = 60f;
                    DrawRevealValue(part.Name, valueFormated, percentStyle, valueWidth, isValuesHidden, GetColorByPercent(value));

                }
                catch {
                    if (_showDebugMSG.GetValue())
                        ModConsole.Log($"Build GUI Error. {part.Name}. Skip this line");
                }
                GUILayout.EndHorizontal();
            }
            GUILayout.Label("Transmission condition", headerStyle);
            GUILayout.Space(5);
            foreach (var part in wearPartsTransition)
            {
                GUILayout.BeginHorizontal();
                try
                {

                    GUILayout.Label(" " + part.Name, GUILayout.Width(140));
                    var value = part.PartValue.Value;
                    var valueFormated = value.ToString("00.00") + "%";
                    float valueWidth = 60f;
                    DrawRevealValue(part.Name, valueFormated, percentStyle, valueWidth, isValuesHidden, GetColorByPercent(value));

                }
                catch
                {
                    if (_showDebugMSG.GetValue())
                        ModConsole.Log($"Build GUI Error. {part.Name}. Skip this line");
                }
                GUILayout.EndHorizontal();
            }
            //Part 3 
            GUILayout.Label("MISC condition", headerStyle);
            GUILayout.Space(5);
            foreach (var part in wearMisc)
            {
                GUILayout.BeginHorizontal();
                try
                {
                   
                    GUILayout.Label(" " + part.Name, GUILayout.Width(140));

                    var value = part.PartValue.Value;
                    var valueFormated = value.ToString("00.00") + "%";
                    float valueWidth = 60f;
                    DrawRevealValue(part.Name, valueFormated, percentStyle, valueWidth, isValuesHidden, GetColorByPercent(value));

                }
                catch {
                    if (_showDebugMSG.GetValue())
                        ModConsole.Log($"Build GUI Error. {part.Name}. Skip this line");
                }
                GUILayout.EndHorizontal();

            }
            //Part 4
            GUILayout.Label("Belts condition", headerStyle);
            GUILayout.Space(5);
            foreach (var part in wearBelts)
            {
                GUILayout.BeginHorizontal();
                try
                {
                    
                    GUILayout.Label(" " + part.Name, GUILayout.Width(140));
                    var value = part.PartValue.Value;
                    var valueFormated = value.ToString("00.00") + "%";
                    float valueWidth = 60f;
                    DrawRevealValue(part.Name, valueFormated, percentStyle, valueWidth, isValuesHidden, GetColorByPercent(value));

                }
                catch
                {
                    if (_showDebugMSG.GetValue())
                        ModConsole.Log($"Build GUI Error. {part.Name}. Skip this line");
                }
                GUILayout.EndHorizontal();
            }
            GUILayout.EndVertical();

            // Column 3
            GUILayout.BeginVertical(GUILayout.Width(235));
            GUILayout.Label("Suspension condition", headerStyle);
            GUILayout.Space(5);
            foreach (var part in wearPartsSusp)
            {
                GUILayout.BeginHorizontal();
                try
                {
                    
                    GUILayout.Label(" " + part.Name, GUILayout.Width(140));
                    var value = part.PartValue.Value;
                    var valueFormated = value.ToString("00.00") + "%";
                    float valueWidth = 60f;
                    DrawRevealValue(part.Name, valueFormated, percentStyle, valueWidth, isValuesHidden, GetColorByPercent(value));

                }
                catch
                {
                    if (_showDebugMSG.GetValue())
                        ModConsole.Log($"Build GUI Error. {part.Name}. Skip this line");
                }
                GUILayout.EndHorizontal();
            }
            //Part 2
            GUILayout.Label("Suspension Bend", headerStyle);
            GUILayout.Space(5);
            GUILayout.BeginHorizontal();
            var partName = "Bend Wheel FL";
            try
            {
                GUILayout.Label($" {partName}", GUILayout.Width(100));
                var valueFormated = $"{WheelFLRotation.Value.x},{WheelFLRotation.Value.y},{WheelFLRotation.Value.z}";
                float valueWidth = 100f;
                DrawRevealValue(partName, valueFormated, percentStyle, valueWidth, isValuesHidden, original);
            }
            catch
            {
                if (_showDebugMSG.GetValue())
                    ModConsole.Log($"Build GUI Error. {partName}. Skip this line");
            }
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            partName = "Bend Wheel FR";
            try
            {
                GUILayout.Label($" {partName}", GUILayout.Width(100));
                var valueFormated = $"{WheelFRRotation.Value.x},{WheelFRRotation.Value.y},{WheelFRRotation.Value.z}";
                float valueWidth = 100f;
                DrawRevealValue(partName, valueFormated, percentStyle, valueWidth, isValuesHidden, original);
            }
            catch
            {
                if (_showDebugMSG.GetValue())
                    ModConsole.Log($"Build GUI Error. {partName}. Skip this line");
            }
            GUILayout.EndHorizontal();

            //Bend for Read removed from game? 01.02.2026

            //GUILayout.BeginHorizontal();
            //partName = "Bend Wheel RL";
            //try
            //{
            //    GUILayout.Label($" {partName}", GUILayout.Width(100));
            //    var valueFormated = $"{WheelRLRotation.Value.x},{WheelRLRotation.Value.y},{WheelRLRotation.Value.z}";
            //    float valueWidth = 100f;
            //    DrawRevealValue($"{partName}", valueFormated, percentStyle, valueWidth, isValuesHidden, original);
            //}
            //catch
            //{
            //    if (_showDebugMSG.GetValue())
            //        ModConsole.Log($"Build GUI Error. {partName}. Skip this line");
            //}
            //GUILayout.EndHorizontal();
            //GUILayout.BeginHorizontal();
            //partName = "Bend Wheel RR";
            //try
            //{
            //    GUILayout.Label($" {partName}", GUILayout.Width(100));
            //    var valueFormated = $"{WheelRRRotation.Value.x},{WheelRRRotation.Value.y},{WheelRRRotation.Value.z}";
            //    float valueWidth = 100f;
            //    DrawRevealValue(partName, valueFormated, percentStyle, valueWidth, isValuesHidden, original);
            //}
            //catch
            //{
            //    if (_showDebugMSG.GetValue())
            //        ModConsole.Log($"Build GUI Error. {partName}. Skip this line");
            //}
            //GUILayout.EndHorizontal();

            GUILayout.Label("Wheels Alignment", headerStyle);
            GUILayout.Space(5);
            foreach (var part in wheelsTune)
            {
                GUILayout.BeginHorizontal();
                try
                {

                    GUILayout.Label(" " + part.Name, GUILayout.Width(90));
                    var value = part.PartValue.Value;
                    var valueFormated = value.ToString("0.0000000000");
                    float valueWidth = 110f;
                    DrawRevealValue(part.Name, valueFormated, percentStyle, valueWidth, isValuesHidden, original);

                }
                catch
                {
                    if (_showDebugMSG.GetValue())
                        ModConsole.Log($"Build GUI Error. {part.Name}. Skip this line");
                }
                GUILayout.EndHorizontal();

            }

            GUILayout.Label("Wheels condition", headerStyle);
            GUILayout.Space(5);
            foreach (var part in wheelsHeal)
            {
                GUILayout.BeginHorizontal();
                try
                {
                    
                    GUILayout.Label(" " + part.Name, GUILayout.Width(140));
                    var value = part.PartValue.Value;
                    var valueFormated = value.ToString("0.00") + "%";
                    float valueWidth = 60f;
                    DrawRevealValue(part.Name, valueFormated, percentStyle, valueWidth, isValuesHidden, GetColorByPercent(value));

                }
                catch
                {
                    if (_showDebugMSG.GetValue())
                        ModConsole.Log($"Build GUI Error. {part.Name}. Skip this line");
                }
                GUILayout.EndHorizontal();

            }
            GUILayout.EndVertical();

            // Column 4
            GUILayout.BeginVertical(GUILayout.Width(235));
            GUILayout.Label("Car tune", headerStyle);
            GUILayout.Space(5);
            foreach (var part in carTunesValues)
            {
                GUILayout.BeginHorizontal();
                try
                {
                    
                    GUILayout.Label(" " + part.Name, GUILayout.Width(90));
                    var value = part.PartValue.Value;
                    var valueFormated = value.ToString("0.000");
                    float valueWidth = 110f;
                    DrawRevealValue(part.Name, valueFormated, percentStyle, valueWidth, isValuesHidden, original);

                }
                catch
                {
                    if (_showDebugMSG.GetValue())
                        ModConsole.Log($"Build GUI Error. {part.Name}. Skip this line");
                }
                GUILayout.EndHorizontal();

            }
            foreach (var part in valvesList)
            {
                GUILayout.BeginHorizontal();
                try
                {

                    GUILayout.Label(" " + part.Name, GUILayout.Width(115));
                    var value = part.PartValue.Value;
                    var valueFormated = value.ToString("0.000000");
                    float valueWidth = 85f;
                    DrawRevealValue(part.Name, valueFormated, percentStyle, valueWidth, isValuesHidden, original);

                }
                catch
                {
                    if (_showDebugMSG.GetValue())
                        ModConsole.Log($"Build GUI Error. {part.Name}. Skip this line");
                }
                GUILayout.EndHorizontal();

            }
            GUILayout.Label("Timing Live Data", headerStyle);
            GUILayout.Space(5);
            if (crankPosition != null)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label(" Сrank Timing", GUILayout.Width(90));
                var valueFormated = crankPosition.localRotation.y.ToString("0.0000000000");
                float valueWidth = 110f;
                DrawRevealValue("Сrank Timing", valueFormated, percentStyle, valueWidth, isValuesHidden, original);
                GUILayout.EndHorizontal();
            }
            if (camPosition != null) {
                GUILayout.BeginHorizontal();
                GUILayout.Label(" Cam Timing", GUILayout.Width(90));
                var valueFormated = camPosition.localRotation.y.ToString("0.0000000000");
                float valueWidth = 110f;
                DrawRevealValue("Cam Timing", valueFormated, percentStyle, valueWidth, isValuesHidden, original);
                GUILayout.EndHorizontal();
            }
            GUILayout.Label("Consumables", headerStyle);
            GUILayout.Space(5);
            //ATF LVL
            GUILayout.BeginHorizontal();
            try
            {
                if (ATFLevel.Value >=0)
                {
                    GUILayout.Label(" ATF Level", GUILayout.Width(80));
                    var ATFPercent = GetPercentLiquid(ATFLevel.Value, 0f, AFTMax.Value);
                    var valueFormated = $"{ATFLevel.Value.ToString("0.00")}L - {ATFPercent.ToString("0.0")}%";
                    float valueWidth = 120f;
                    DrawRevealValue("ATF Level", valueFormated, percentStyle, valueWidth, isValuesHidden, GetColorByPercent(ATFPercent));
                }

            }
            catch
            {
                if (_showDebugMSG.GetValue())
                    ModConsole.Log($"Build GUI Error. ATF Level. Skip this line");
            }
            GUILayout.EndHorizontal();
            //Coolant LVL
            GUILayout.BeginHorizontal();
            try
            {
                GUILayout.Label(" Coolant", GUILayout.Width(80));
                var coolantPercent = GetPercentLiquid(coolantCurrent.Value, 0f, coolantMax.Value);
                var valueFormated = $"{coolantCurrent.Value.ToString("0.00")}L - {coolantPercent.ToString("0.0")}%";
                float valueWidth = 120f;
                DrawRevealValue("Coolant", valueFormated, percentStyle, valueWidth, isValuesHidden, GetColorByPercent(coolantPercent));
            }
            catch
            {
                if (_showDebugMSG.GetValue())
                    ModConsole.Log($"Build GUI Error. Coolant. Skip this line");
            }
            GUILayout.EndHorizontal();
            //Coolant Temp
            GUILayout.BeginHorizontal();
            try
            {
                GUILayout.Label(" Coolant Temp", GUILayout.Width(100));
                var valueFormated = $"{CoolantTemp.Value.ToString("0.0")}C";
                float valueWidth = 100f;
                DrawRevealValue("Coolant Temp", valueFormated, percentStyle, valueWidth, isValuesHidden, original);
            }
            catch
            {
                if (_showDebugMSG.GetValue())
                    ModConsole.Log($"Build GUI Error. Coolant Temp. Skip this line");
            }
            GUILayout.EndHorizontal();

            //Engine Temp
            GUILayout.BeginHorizontal();
            try
            {
                GUILayout.Label(" Engine Temp", GUILayout.Width(100));
                var valueFormated = $"{EngineTemp.Value.ToString("0.0")}C";
                float valueWidth = 100f;
                DrawRevealValue("Engine Temp", valueFormated, percentStyle, valueWidth, isValuesHidden, original);
            }
            catch
            {
                if (_showDebugMSG.GetValue())
                    ModConsole.Log($"Build GUI Error. Engine Temp. Skip this line");
            }
            GUILayout.EndHorizontal();
            //Oil LVL
            GUILayout.BeginHorizontal();
            try
            {
                GUILayout.Label(" Oil Level", GUILayout.Width(80));
                var oilpercent = GetPercentLiquid(oilCurrent.Value, 0f, oilMax.Value);
                var valueFormated = $"{oilCurrent.Value.ToString("0.00")}L - {oilpercent.ToString("0.0")}%";
                float valueWidth = 120f;
                DrawRevealValue("Oil Level", valueFormated, percentStyle, valueWidth, isValuesHidden,GetColorByPercent(oilpercent));
            }
            catch {
                if (_showDebugMSG.GetValue())
                    ModConsole.Log($"Build GUI Error. Oil Level. Skip this line");
            }
            GUILayout.EndHorizontal();
            //Oil Pressure
            GUILayout.BeginHorizontal();
            try
            {
                GUILayout.Label(" Oil Pressure", GUILayout.Width(90));
                var valueFormated = $"{OilPressure.Value.ToString("0.00")}PSI";
                float valueWidth = 110f;
                DrawRevealValue("Oil Pressure", valueFormated, percentStyle, valueWidth, isValuesHidden, original);
            }
            catch
            {
                if (_showDebugMSG.GetValue())
                    ModConsole.Log($"Build GUI Error. Oil Pressure. Skip this line");
            }
            GUILayout.EndHorizontal();
            //Oil Cont
            GUILayout.BeginHorizontal();
            try
            {
                GUILayout.Label(" Oil Dirtiness", GUILayout.Width(120));
                var oilpercent = GetPercentLiquid(oilContamination.Value, 0f, contaminationWarning);
                var valueFormated = $"{oilContamination.Value.ToString("0.00")}%";
                float valueWidth = 80f;
                DrawRevealValue("Oil Dirtness", valueFormated, percentStyle, valueWidth, isValuesHidden, GetColorByPercent(oilpercent,true));
            }
            catch
            {
                if (_showDebugMSG.GetValue())
                    ModConsole.Log($"Build GUI Error. Oil Dirtness. Skip this line");
            }
            GUILayout.EndHorizontal();
            //Oil Filter
            GUILayout.BeginHorizontal();
            try
            {

                GUILayout.Label(" Filter Dirtiness", GUILayout.Width(120));
                var filterDirt = oilfilterDirt.Value;
                var valueFormated = $"{filterDirt.ToString("0.00")}%";
                float valueWidth = 80f;
                DrawRevealValue("Filter Dirtness", valueFormated, percentStyle, valueWidth, isValuesHidden, GetColorByPercent(filterDirt, true));
            }
            catch
            {
                if (_showDebugMSG.GetValue())
                    ModConsole.Log($"Build GUI Error. Filter Dirtiness. Skip this line");
            }
            GUILayout.EndHorizontal();
            //Brake Fluid
            GUILayout.BeginHorizontal();
            try
            {
                GUILayout.Label(" Brake Fluid", GUILayout.Width(80));
                var brakePercent = GetPercentLiquid(BrakeCurrent.Value, 0f, 1);
                var valueFormated = $"{BrakeCurrent.Value.ToString("0.00")}L - {brakePercent.ToString("0.0")}%";
                float valueWidth = 120f;
                DrawRevealValue("Brake Fluid", valueFormated, percentStyle, valueWidth, isValuesHidden, GetColorByPercent(brakePercent));
            }
            catch
            {
                if (_showDebugMSG.GetValue())
                    ModConsole.Log($"Build GUI Error. Brake Fluid. Skip this line");
            }
            GUILayout.EndHorizontal();
            //Fuel LVL
            GUILayout.BeginHorizontal();
            try
            {
                GUILayout.Label(" Fuel Level", GUILayout.Width(80));
                var fuelpercent = GetPercentLiquid(fuelCurrent.Value, 0f, fuelMax.Value);
                var valueFormated = $"{fuelCurrent.Value.ToString("0.00")}L - {fuelpercent.ToString("0.0")}%";
                float valueWidth = 120f;
                DrawRevealValue("Fuel Level", valueFormated, percentStyle, valueWidth, isValuesHidden, GetColorByPercent(fuelpercent));
            }
            catch
            {
                if (_showDebugMSG.GetValue())
                    ModConsole.Log($"Build GUI Error. Build GUI Error. Skip this line");
            }
            GUILayout.EndHorizontal();
            //Battery
            GUILayout.BeginHorizontal();
            try
            {
                GUILayout.Label(" Battery", GUILayout.Width(75));
                var batpercent = GetPercentLiquid(BatteryCurrent.Value, 0f, BatteryMax.Value);
                var valueFormated = $"{BatteryCurrent.Value.ToString("0.00")}Ah - {batpercent.ToString("0.0")}%";
                float valueWidth = 125f;
                DrawRevealValue("Battery", valueFormated, percentStyle, valueWidth, isValuesHidden, GetColorByPercent(batpercent));
            }
            catch
            {
                if (_showDebugMSG.GetValue())
                    ModConsole.Log($"Build GUI Error. Build GUI Error. Skip this line");
            }
            GUILayout.EndHorizontal();

            GUILayout.EndVertical();
            GUILayout.EndHorizontal();

            GUILayout.EndVertical();

            GUI.DragWindow(new Rect(0, 0, 10000, 20));
        }
    }
    
}
