﻿using System;
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

        public override void OnFixedUpdate()
        {
            foreach (RepairData repairData in repairDatas)
            {
                CustomPRCData cPRCD = repairData.customControllerData as CustomPRCData;

                bool foundNulls = false;
                foreach (IRepairParticipant repairParticipant in cPRCD.assignedParticipants)
                {
                    if (repairParticipant == null)
                    {
                        foundNulls = true;
                    }
                    else
                    {
                        // TODO: repair progress handling
                    }
                }

                if (foundNulls)
                {
                    cPRCD.assignedParticipants.RemoveAll((IRepairParticipant a) => a == null);
                }
            }
        }
        #endregion

        #region IReparisController
        public void AddRepair(RepairData repairData)
        {
            repairDatas.Add(repairData);

            KSPEvent attribHolder = GenerateRepairOptionSelectionAttribs(repairData);
            repairData.customControllerData = new CustomPRCData(repairData.RequestedResources.Keys.AsEnumerable());
            BaseEvent PAWButton = new BaseEvent(Events, $"SelectionToggle {repairData.RepairOptionDescription}", () =>
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
            BaseEvent PAWButton = new BaseEvent(Events, $"AssigningToggle: {repairData.RepairOptionDescription}", () =>
            {
                ToggleRepairAssigning(repairData);
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

            StopRepairAssignment(repairData, cPRCD);
            CutRepairAssignments(repairData);
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
            return new KSPEvent
            {
                guiActive = true,
                guiActiveUncommand = true,
                guiActiveUnfocused = true,
                requireFullControl = false,
                guiName = $"Start Assigning: {repairData.RepairOptionDescription}",
                groupName = "KRTRepeirsAssignment",
                groupDisplayName = "KRT Repairs Assignment"
            };
        }

        static KSPEvent GenerateCutAssignmentsAttribs(RepairData repairData)
        {
            return new KSPEvent
            {
                guiActive = true,
                guiActiveUncommand = true,
                guiActiveUnfocused = true,
                requireFullControl = false,
                guiName = $"Cut Assignments For {repairData.RepairOptionDescription}",
                groupName = "KRTRepeirsAssignment",
                groupDisplayName = "KRT Repairs Assignment"
            };
        }

        void StartRepairAssignment(RepairData repairData, CustomPRCData customPRCData)
        {
            customPRCData.PAWCatchingAssignmentButton.guiName = $"Stop Assigning: {repairData.RepairOptionDescription}";
            repairsCatchingAssignments.Add(repairData);

            customPRCData.PAWCutAssignmentsButton = new BaseEvent(Events, $"CutAssignmentsFor {repairData.RepairOptionDescription}",
                () => CutRepairAssignments(repairData), GenerateCutAssignmentsAttribs(repairData));

            participantsCatchingAssignments.RemoveAll((IRepairParticipant a) => a == null);
            foreach (IRepairParticipant assigningRepairParticipant in participantsCatchingAssignments)
            {
                PerformAssignment(assigningRepairParticipant, repairData);
            }
        }

        void StopRepairAssignment(RepairData repairData, CustomPRCData customPRCData)
        {
            customPRCData.PAWCatchingAssignmentButton.guiName = $"Start Assigning: {repairData.RepairOptionDescription}";
            repairsCatchingAssignments.RemoveAll((RepairData a) => a == repairData);
        }

        void ToggleRepairAssigning(RepairData repairData)
        {
            CustomPRCData customPRCData = repairData.customControllerData as CustomPRCData;
            customPRCData.isBeingAssigned = !customPRCData.isBeingAssigned;

            if (customPRCData.isBeingAssigned)
            {
                StartRepairAssignment(repairData, customPRCData);
            }
            else
            {
                StopRepairAssignment(repairData, customPRCData);
            }
        }

        void CutRepairAssignments(RepairData repairData)
        {
            CustomPRCData cPRCD = (repairData.customControllerData as CustomPRCData);
            if (cPRCD.PAWCutAssignmentsButton != null)
            {
                Events.Remove(cPRCD.PAWCutAssignmentsButton);
                part.Events.Remove(cPRCD.PAWCutAssignmentsButton);
                cPRCD.PAWCutAssignmentsButton = null;
            }

            cPRCD.assignedParticipants.RemoveAll((IRepairParticipant a) => a == null);
            foreach (IRepairParticipant repairParticipant in cPRCD.assignedParticipants)
            {
                repairParticipant.DeassignRepair(repairData);
            }
            cPRCD.assignedParticipants.Clear();
        }

        static void PerformAssignment(IRepairParticipant repairParticipant, RepairData repairData)
        {
            CustomPRCData cPRCD = repairData.customControllerData as CustomPRCData;

            cPRCD.assignedParticipants.Add(repairParticipant);
            repairParticipant.AssignRepair(repairData);

            double newlyAssignedQuality = repairParticipant.GetAssignedQuality(repairData);

            if (newlyAssignedQuality > cPRCD.maxAssignedQuality)
            {
                cPRCD.maxAssignedQuality = newlyAssignedQuality;
            }

            if (repairParticipant.IsHandlingThisRepairNow(repairData) && newlyAssignedQuality > cPRCD.currentlyAvailableQuality)
            {
                cPRCD.currentlyAvailableQuality = newlyAssignedQuality;
            }

            if (newlyAssignedQuality > cPRCD.workingAtQuality && (repairData.progressRatio != 0 || cPRCD.isRunning))
            {
                // TODO: notify about ability to restart repair for better quality
            }
        }
        #endregion
    }
}
