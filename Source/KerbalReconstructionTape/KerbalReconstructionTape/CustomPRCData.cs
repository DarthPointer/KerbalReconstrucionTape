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
        public BaseEvent PAWButton;

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
