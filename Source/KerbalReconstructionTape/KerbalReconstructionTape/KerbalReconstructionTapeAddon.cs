using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;


namespace KerbalReconstructionTape
{
    [KSPAddon(KSPAddon.Startup.AllGameScenes, false)]
    class KerbalReconstructionTapeAddon
    {
        #region Static
        static KerbalReconstructionTapeAddon instance;
        public static double GetRepairQuality(string traitName, int level)
        {
            if (instance != null)
            {
                if (instance.traitRepairQualities.TryGetValue(traitName, out List<double> tRQ))
                {
                    if (level < tRQ.Count && level >= 0)
                    {
                        return tRQ[level];
                    }
                    else
                    {
                        Debug.LogError($"[KRT] Requested repair quality for trait {traitName} for level {level}, that is not present in the config");
                        return 0;
                    }
                }
                else
                {
                    Debug.LogError($"[KRT] Requested repair quality for trait {traitName} which is not present in the config");
                    return 0;
                }
            }
            else
            {
                Debug.LogError("[KRT] KRTAddon.GetRepairQuality is called, but KRTAddon is not instantaniated");
                return 0;
            }
        }

        public static double GetRepairSpeed(string traitName, int level)
        {
            if (instance != null)
            {
                if (instance.traitRepairSpeeds.TryGetValue(traitName, out List<double> tRS))
                {
                    if (level < tRS.Count && level >= 0)
                    {
                        return tRS[level];
                    }
                    else
                    {
                        Debug.LogError($"[KRT] Requested repair speed for trait {traitName} for level {level}, that is not present in the config");
                        return 0;
                    }
                }
                else
                {
                    Debug.LogError($"[KRT] Requested repair speed for trait {traitName} which is not present in the config");
                    return 0;
                }
            }
            else
            {
                Debug.LogError("[KRT] KRTAddon.GetRepairSpeed is called, but KRTAddon is not instantaniated");
                return 0;
            }
        }
        #endregion

        Dictionary<string, List<double>> traitRepairQualities = new Dictionary<string, List<double>>();
        Dictionary<string, List<double>> traitRepairSpeeds = new Dictionary<string, List<double>>();

        #region KSPAddon
        public void Start()
        {
            instance = this;

            ConfigNode configNode = GameDatabase.Instance.GetConfigNodes("REPAIR_TRAITS")?[0];
            if (configNode == null)
            {
                Debug.LogWarning("[KRT] Could not find REPAIR_TRAITS node in the game database");
                return;
            }

            foreach (ConfigNode traitNode in configNode.GetNodes("TRAIT"))
            {
                if (!traitNode.HasValue("name"))
                {
                    Debug.LogError("[KRT] Found a TRAIT node without \"name\" field");
                }
                else
                {
                    string traitName = traitNode.GetValue("name");

                    if (traitRepairQualities.ContainsKey(traitName))
                    {
                        Debug.LogWarning($"[KRT] Found multiple repair configs for {traitName} trait, ignoring all but the first one");
                    }
                    else
                    {
                        traitRepairQualities[traitName] = new List<double> { };
                        traitRepairSpeeds[traitName] = new List<double> { };

                        int i = 0;

                        while (traitNode.HasNode($"LEVEL{i}"))
                        {
                            ConfigNode levelNode = traitNode.GetNode($"LEVEL{i}");

                            if (levelNode.HasValue("quality") && levelNode.HasValue("speed"))
                            {
                                string qualString = levelNode.GetValue("quality");
                                string speedString = levelNode.GetValue("speed");

                                double qual, speed;                 // Not embedding defs for readability

                                if (double.TryParse(qualString, out qual) && double.TryParse(speedString, out speed))
                                {
                                    traitRepairQualities[traitName].Add(qual);
                                    traitRepairSpeeds[traitName].Add(speed);
                                }
                                else
                                {
                                    Debug.LogError($"[KRT] Level node LEVEL{i} for trait {traitName} has misformatted values \"quality\" \"speed\"");
                                    break;
                                }
                            }
                            else
                            {
                                Debug.LogError($"[KRT] Level node LEVEL{i} for trait {traitName} misses needed values (\"quality\" and \"speed\")");
                                break;
                            }

                            i++;
                        }
                    }
                }
            }
        }

        public void OnDestroy()
        {
            instance = null;
        }
        #endregion
    }
}
