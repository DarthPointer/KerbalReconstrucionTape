using System.Collections.Generic;
using KerbalRepairsInterface;

namespace KerbalReconstructionTape
{
    public interface IRepairParticipant
    {
        void AssignRepair(RepairData repair);
        void DeassignRepair(RepairData repair);
    }

    public class Bungler : PartModule, IRepairParticipant
    {
        List<RepairData> tasks = new List<RepairData>();
        bool inEditor = true;
        double handsCurvature;                               // aka repairs quality
        double swiftness;                                    // aka repairs speed

        public override void OnStart(StartState state)
        {
            if (state != StartState.Editor)
            {
                inEditor = false;
            }

            base.OnStart(state);
        }

        #region IRepairParticipant
        public void AssignRepair(RepairData repair)
        {
            tasks.Add(repair);
        }

        public void DeassignRepair(RepairData repair)
        {
            tasks.Remove(repair);
        }
        #endregion
    }
}
