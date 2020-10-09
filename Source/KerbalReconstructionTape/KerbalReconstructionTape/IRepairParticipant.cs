using System.Collections.Generic;
using KerbalRepairsInterface;
using UnityEngine;

namespace KerbalReconstructionTape
{
    public interface IRepairParticipant
    {
        void AssignRepair(RepairData repair);
        void DeassignRepair(RepairData repair);

        double GetAssignedQuality(RepairData repair);
        double GetAssginedWorkPower(RepairData repair);
        bool IsHandlingThisRepairNow(RepairData repair);

        double ReserveResource(string resourceName, double desiredAmount);         // How much has been actually reserved, be CAREFUL with negative reserves
        bool UseReservedResource(string resourceName, double requestedAmount);       // True if reserved and available resources were in sufficient amounts and discarded successfully
    }

    public class Bungler : PartModule, IRepairParticipant
    {
        Dictionary<string, double> reservedResources = new Dictionary<string, double>();
        bool inEditor = true;

        List<RepairData> tasks = new List<RepairData>();
        
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

        public double GetAssignedQuality(RepairData repair)         // Making it repair-dependent for further possible features
        {
            if (part.protoModuleCrew.Count == 1)
            {
                ProtoCrewMember kerbal = part.protoModuleCrew[0];

                if (kerbal != null)
                {
                    return KerbalReconstructionTapeAddon.GetRepairQuality(kerbal.trait, kerbal.experienceLevel);
                }
                else
                {
                    Debug.LogError($"[KRT] Bungler module at part {part.name} has found a null ProtoCrewMember");
                    return 0;
                }
            }
            else
            {
                Debug.LogError($"[KRT] Bungler module has found out that his part {part.name} has not 1 crew member. It seems it is not an EVA Kerbal.");
                return 0;
            }
        }

        public double GetAssginedWorkPower(RepairData repair)
        {
            if (part.protoModuleCrew.Count == 1)
            {
                ProtoCrewMember kerbal = part.protoModuleCrew[0];

                if (kerbal != null)
                {
                    return KerbalReconstructionTapeAddon.GetRepairSpeed(kerbal.trait, kerbal.experienceLevel) * KerbalReconstructionTapeAddon.GetRepairQuality(kerbal.trait, kerbal.experienceLevel);
                }
                else
                {
                    Debug.LogError($"[KRT] Bungler module at part {part.name} has found a null ProtoCrewMember");
                    return 0;
                }
            }
            else
            {
                Debug.LogError($"[KRT] Bungler module has found out that his part {part.name} has not 1 crew member. It seems it is not an EVA Kerbal.");
                return 0;
            }
        }

        public bool IsHandlingThisRepairNow(RepairData repair)
        {
            return tasks[0] == repair;
        }

        public double ReserveResource(string resourceName, double desiredAmount)
        {
            if (part.Resources.Contains(resourceName))
            {
                if (!reservedResources.ContainsKey(resourceName))
                {
                    reservedResources.Add(resourceName, 0);
                }

                double reservingNow = (reservedResources[resourceName] + desiredAmount < part.Resources[resourceName].amount) ? desiredAmount : part.Resources[resourceName].amount;
                reservedResources[resourceName] += reservingNow;
                return reservingNow;
            }
            else
            {
                return 0;
            }
        }

        public bool UseReservedResource(string resourceName, double requestedAmount)
        {
            if (reservedResources.ContainsKey(resourceName))
            {
                if (reservedResources[resourceName] >= requestedAmount)
                {
                    if (part.Resources[resourceName].amount >= requestedAmount)
                    {
                        part.RequestResource(resourceName, requestedAmount);
                        return true;
                    }
                    Debug.LogWarning($"[KRT] Resource {resourceName} was requested from an IRepairable, reserved amount is OK but there is not enough resource in a part");
                    return false;
                }
                else
                {
                    Debug.LogWarning($"[KRT] Resource {resourceName} was requested from an IRepairable but reserved amount is less");
                    return false;
                }
            }
            else
            {
                Debug.LogWarning($"[KRT] Resource {resourceName} was requested from an IRepairable but it has not been reserved");
                return false;
            }
        }
        #endregion
    }
}
