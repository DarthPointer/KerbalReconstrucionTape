using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KerbalRepairsInterface;

namespace KerbalReconstructionTape
{
    class CustomPRCData : IKRISerializedCustomData
    {
        public BaseEvent PAWSelectionToggleButton;
        public BaseEvent PAWCatchingAssignmentButton;
        public BaseEvent PAWCutAssignmentsButton;

        public bool isBeingAssigned = false;
        public bool isRunning = false;
        public bool autorunWhenMaxQualityAvailable = false;

        public List<IRepairParticipant> assignedParticipants = new List<IRepairParticipant>();
        public double maxAssignedQuality = 0;
        public double currentlyAvailableQuality = 0;
        public double workingAtQuality = 0;

        public Dictionary<string, double> reservedResourcesBuffer;
        public double workUnitsBuffer = 0;

        public CustomPRCData(IEnumerable<string> resourceNames)
        {
            reservedResourcesBuffer = new Dictionary<string, double>();
            foreach (string resourceName in resourceNames)
            {
                reservedResourcesBuffer[resourceName] = 0;
            }
        }

        public void AssignRepairParticipant(IRepairParticipant repairParticipant)
        {
        }

        public void DeassignRepairParticipant(IRepairParticipant repairParticipant)
        {
        }

        #region IKRISerializedCustomData
        public string Serialize()
        {
            return "";
        }

        public void Deserialize(string serialized)
        {
        }
        #endregion
    }
}
