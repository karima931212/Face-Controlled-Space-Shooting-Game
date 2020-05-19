using UnityEngine;
using System.Collections;

namespace VSX.UniversalVehicleCombat
{

	/// <summary>
    /// This class provides a way to manage the lead target tracking for gun weapons.
    /// </summary>
	public class LeadTargetData 
	{
	
		private ModuleMount moduleMount; 
		public ModuleMount ModuleMount { get { return moduleMount; } }

		private IGunModule gunModule; 
		public IGunModule GunModule { get { return gunModule; } }

		private Vector3 leadTargetPosition;
		public Vector3 LeadTargetPosition { get { return leadTargetPosition; } }


        /// <summary>
        /// Create a new instance of the LeadTargetData class for a gun module.
        /// </summary>
        /// <param name="moduleMount">The module mount where the gun module is mounted.</param>
        /// <param name="gunModule">The gun module reference.</param>
		public LeadTargetData(ModuleMount moduleMount, IGunModule gunModule)
		{

			this.moduleMount = moduleMount;

			this.gunModule = gunModule;

		}


        /// <summary>
        /// Set the leas target position for this LeadTargetData instance.
        /// </summary>
        /// <param name="newLeadTargetPos"></param>
		public void SetLeadTargetPosition(Vector3 newLeadTargetPos)
        {
			this.leadTargetPosition = newLeadTargetPos;
		}

	}
}
