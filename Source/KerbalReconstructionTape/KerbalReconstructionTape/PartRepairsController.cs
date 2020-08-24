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
        HashSet<RepairData> repairDatas = new HashSet<RepairData>();

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

            KSPEvent attribHolder = GenerateRepairOptionAttribs(repairData);
            repairData.customControllerData = new CustomPRCData();
            BaseEvent PAWButton = new BaseEvent(Events, repairData.RepairOptionDescription, () =>
            {
                repairData.Toggle();
                Events[repairData.RepairOptionDescription].guiName = repairData.InProgress ? $"Stop: {repairData.RepairOptionDescription}" : $"Start: {repairData.RepairOptionDescription}";
            }, attribHolder);

            Events.Add(PAWButton);
            (repairData.customControllerData as CustomPRCData).PAWButton = PAWButton;
        }

        public void RemoveRepair(RepairData repairData)
        {
            Events.Remove((repairData.customControllerData as CustomPRCData).PAWButton);
            part.Events.Remove((repairData.customControllerData as CustomPRCData).PAWButton);

            repairDatas.Remove(repairData);
        }
        #endregion

        static KSPEvent GenerateRepairOptionAttribs(RepairData repairData)
        {
            KSPEvent attribHolder = new KSPEvent();
            attribHolder.guiActive = true;
            attribHolder.guiActiveUncommand = true;
            attribHolder.guiActiveUnfocused = true;
            attribHolder.requireFullControl = false;
            attribHolder.guiName = $"Start: {repairData.RepairOptionDescription}";
            return attribHolder;
        }
    }
}
