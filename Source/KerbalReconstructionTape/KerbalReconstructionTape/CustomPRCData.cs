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
