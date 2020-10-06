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
        Dictionary<string, List<double>> traitRepairQualities = new Dictionary<string, List<double>>();
        Dictionary<string, List<double>> traitRepairSpeeds = new Dictionary<string, List<double>>();

        #region KSPAddon
        public void Start()
        {
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

                                double qual, speed;

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
        #endregion
    }
}
