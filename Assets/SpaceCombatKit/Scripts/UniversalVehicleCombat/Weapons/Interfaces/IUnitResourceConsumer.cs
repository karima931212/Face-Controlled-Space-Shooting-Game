using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace VSX.UniversalVehicleCombat
{
    /// <summary>
    /// This interface provides a way to interact with modules that consume units of a particular resource,
    /// such as missiles or bullets.
    /// </summary>
    public interface IUnitResourceConsumer
    {

        bool InfiniteResourceUnits { get; set; }

        int StartingResourceUnits { get; }
        int CurrentResourceUnits { get; }

        void AddResourceUnits(int numAdditionalUnits);
        void SetResourceUnits(int numResourceUnits);

    }
}
