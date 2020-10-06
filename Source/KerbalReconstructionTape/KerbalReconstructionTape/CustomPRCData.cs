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

        public bool isBeingAssigned = false;
        public List<IRepairParticipant> assignedParticipants = new List<IRepairParticipant>();
        public double maxAssignedQuality = 0;
        public double currentlyAvailableQuality = 0;

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
