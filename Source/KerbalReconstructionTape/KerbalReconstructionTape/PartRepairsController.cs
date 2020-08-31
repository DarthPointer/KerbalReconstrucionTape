using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KerbalRepairsInterface;
using UnityEngine;

namespace KerbalReconstructionTape
{
    public class PartRepairsController : PartModule, IRepairsController
    {
        List<RepairData> repairDatas = new List<RepairData>();
        List<IRepairable> repairables = new List<IRepairable>();

        [KSPEvent(guiName = "Request repairs", groupName = "KRT", groupDisplayName = "Kerbal Reconstruction Tape", guiActive = true, guiActiveUncommand = true, guiActiveUnfocused = true, requireFullControl = false)]
        void RequestRepairs()
        {
            foreach (IRepairable repairable in repairables)
            {
                repairable.RequestRepairs();
            }
        }

        #region PartModule

        public override void OnStart(StartState state)
        {
            if (state != StartState.Editor)
            {
                Debug.Log("[KRT] OnStarting not in editor, looking for IRepairable modules");
                int c = 0;
                foreach (IRepairable repairable in part.FindModulesImplementing<IRepairable>())
                {
                    repairable.AcceptRepairsController(this);
                    repairables.Add(repairable);
                    c++;
                }
                Debug.Log($"[KRT] found {c} IRepairable modules");
            }
        }

        #endregion

        #region IReparisController
        public void AddRepair(RepairData repairData)
        {
            repairDatas.Add(repairData);

            KSPEvent attribHolder = GenerateRepairOptionSelectionAttribs(repairData);
            repairData.customControllerData = new CustomPRCData();
            BaseEvent PAWButton = new BaseEvent(Events, repairData.RepairOptionDescription, () =>
            {
                if (repairData == null)
                {
                    Debug.LogError($"[KRT] Repair toggle button pressed but relevant repairData is null");
                    return;
                }
                repairData.Toggle();

                if (repairData.IsSelected)
                {
                    string displayedResList = $"Chosen repair option is {repairData.RepairOptionDescription}.\nIt needs this set of rsources to be completed\n(Assuming repair efficiency is not lower than 1):";
                    foreach (KeyValuePair<string, double> i in repairData.RequestedResources)
                    {
                        displayedResList += $"\n{PartResourceLibrary.Instance.GetDefinition(i.Key).displayName}: {i.Value}";
                    }
                    PopupDialog.SpawnPopupDialog(new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), "KRTRepairRequests", "Repair Option Chosen", displayedResList, "Prepare Duct Tape!", false, HighLogic.UISkin);
                    (repairData.customControllerData as CustomPRCData).PAWButton.guiName = $"Deselect: {repairData.RepairOptionDescription}";
                }
                else
                {
                    (repairData.customControllerData as CustomPRCData).PAWButton.guiName = $"Select: {repairData.RepairOptionDescription}";
                }
            }, attribHolder);

            Events.Add(PAWButton);
            (repairData.customControllerData as CustomPRCData).PAWButton = PAWButton;
        }

        public void RemoveRepair(RepairData repairData)
        {
            Events.Remove((repairData.customControllerData as CustomPRCData).PAWButton);
            part.Events.Remove((repairData.customControllerData as CustomPRCData).PAWButton);
            part.PartActionWindow.displayDirty = true;

            repairDatas.Remove(repairData);
        }
        #endregion

        static KSPEvent GenerateRepairOptionSelectionAttribs(RepairData repairData)
        {
            KSPEvent attribHolder = new KSPEvent
            {
                guiActive = true,
                guiActiveUncommand = true,
                guiActiveUnfocused = true,
                requireFullControl = false,
                guiName = $"Select: {repairData.RepairOptionDescription}",
                groupName = "KRTRepeirsSelection",
                groupDisplayName = "KRT Repairs Selection",
                name = repairData.RepairOptionDescription
            };
            return attribHolder;
        }
    }
}
