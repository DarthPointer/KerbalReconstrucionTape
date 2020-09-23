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
        #region Static Assignment Lists
        public static List<RepairData> repairsCatchingAssignments = new List<RepairData>();
        public static List<IRepairParticipant> participantsCatchingAssignments = new List<IRepairParticipant>();
        #endregion

        List<RepairData> repairDatas = new List<RepairData>();
        List<IRepairable> repairables = new List<IRepairable>();

        #region Permanent KSPEvents
        [KSPEvent(guiActive = true, guiActiveEditor = false, guiName = "Maximum Selection", groupName = "KRTRepeirsSelection", groupDisplayName = "KRT Repairs Selection")]
        void MaxSelection()
        {
            List<RepairData> repairs = new List<RepairData>(repairDatas);
            repairs.ForEach((RepairData a) =>
            {
                if (a == null)
                {
                    return;
                }
                if (!a.IsSelected && a.UseForFullRepair)
                {
                    (a.customControllerData as CustomPRCData).PAWSelectionToggleButton.Invoke();        // We are literally clicking the button. 100% sure it will work right!
                }
            });
        }

        [KSPEvent(guiActive = true, guiActiveEditor = false, guiName = "Maximum Deselection", groupName = "KRTRepeirsSelection", groupDisplayName = "KRT Repairs Selection")]
        void FullDeselection()
        {
            List<RepairData> repairs = new List<RepairData>(repairDatas);
            repairs.ForEach((RepairData a) =>
            {
                if (a == null)
                {
                    return;
                }
                if (a.IsSelected)
                {
                    (a.customControllerData as CustomPRCData).PAWSelectionToggleButton.Invoke();        // We are literally clicking the button. 100% sure it will work right!
                }
            });
        }
        #endregion

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
                ToggleRepairSelection(repairData);
            }, attribHolder);

            Events.Add(PAWButton);
            (repairData.customControllerData as CustomPRCData).PAWSelectionToggleButton = PAWButton;
        }

        public void RemoveRepair(RepairData repairData)
        {
            Events.Remove((repairData.customControllerData as CustomPRCData).PAWSelectionToggleButton);
            part.Events.Remove((repairData.customControllerData as CustomPRCData).PAWSelectionToggleButton);
            part.PartActionWindow.displayDirty = true;

            repairDatas.Remove(repairData);
        }
        #endregion

        #region Internal Methods for Repair Selection
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

        void SelectRepair(RepairData repairData)        // Does not call repairData.Select, supposed to be used after/before calling it or Toggle somewhere else
        {
            string displayedResList = $"Chosen repair option is {repairData.RepairOptionDescription}.\nIt needs this set of rsources to be completed\n(Assuming repair efficiency is not lower than 1):";
            foreach (KeyValuePair<string, double> i in repairData.RequestedResources)
            {
                displayedResList += $"\n{PartResourceLibrary.Instance.GetDefinition(i.Key).displayName}: {i.Value}";
            }
            PopupDialog.SpawnPopupDialog(new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), "KRTRepairRequests", "Repair Option Chosen", displayedResList, "Prepare Duct Tape!", false, HighLogic.UISkin);
            (repairData.customControllerData as CustomPRCData).PAWSelectionToggleButton.guiName = $"Deselect: {repairData.RepairOptionDescription}";

            KSPEvent attribHolder = GenerateRepairAssignmentCatchingToggleAtribs(repairData);
            BaseEvent PAWButton = new BaseEvent(Events, repairData.RepairOptionDescription, () =>
            {
                AssignRepair(repairData);
            }, attribHolder);

            Events.Add(PAWButton);
            (repairData.customControllerData as CustomPRCData).PAWCatchingAssignmentButton = PAWButton;
        }

        void DeselectRepair(RepairData repairData)      // Does not call repairData.Deselect, supposed to be used after/before calling it or Toggle somewhere else
        {
            CustomPRCData cPRCD = (repairData.customControllerData as CustomPRCData);
            cPRCD.PAWSelectionToggleButton.guiName = $"Select: {repairData.RepairOptionDescription}";

            Events.Remove(cPRCD.PAWCatchingAssignmentButton);
            part.Events.Remove(cPRCD.PAWCatchingAssignmentButton);
            part.PartActionWindow.displayDirty = true;
            cPRCD.PAWCatchingAssignmentButton = null;
        }

        void ToggleRepairSelection(RepairData repairData)
        {
            if (repairData == null)
            {
                Debug.LogError($"[KRT] Repair toggle button pressed but relevant repairData is null");
                return;
            }
            repairData.ToggleSelection();

            if (repairData.IsSelected)
            {
                SelectRepair(repairData);
            }
            else
            {
                DeselectRepair(repairData);
            }
        }
        #endregion

        #region Internal Methods for Repairs Assignment
        static KSPEvent GenerateRepairAssignmentCatchingToggleAtribs(RepairData repairData)
        {
            KSPEvent attribHolder = new KSPEvent
            {
                guiActive = true,
                guiActiveUncommand = true,
                guiActiveUnfocused = true,
                requireFullControl = false,
                guiName = $"Start Assigning: {repairData.RepairOptionDescription}",
                groupName = "KRTRepeirsAssignment",
                groupDisplayName = "KRT Repairs Assignment",
                name = repairData.RepairOptionDescription
            };
            return attribHolder;
        }

        static void AssignRepair(RepairData repairData)
        {
            repairsCatchingAssignments.Add(repairData);

            CleanList(participantsCatchingAssignments);
            foreach (IRepairParticipant repairParticipant in participantsCatchingAssignments)
            {
                PerformAssignment(repairParticipant, repairData);
            }
        }

        static void StopAssigningRepair(RepairData repairData)
        {
        }

        static void CutRepairAssignments(RepairData repairData)
        { 
        }

        static void PerformAssignment(IRepairParticipant repairParticipant, RepairData repairData)
        {
        }

        static void CleanList<T>(List<T> list)
        {
            list.RemoveAll((T a) => a == null);
        }
        #endregion
    }
}
