using UnityEngine;
using System.Collections;

namespace VSX.UniversalVehicleCombat
{

    /// <summary>
    /// The different lock states that a missile weapon can be in
    /// </summary>
	public enum LockState
	{
		NoLock,
		Locking,
		Locked
	}

	/// <summary>
    /// This class provides an easy way to manage the lock state of a missile weapon by a weapon computer.
    /// </summary>
	public class LockingData 
	{
		
		private ModuleMount moduleMount; 
		public ModuleMount ModuleMount { get { return moduleMount; } }
	
		private IMissileModule missileModule; 
		public IMissileModule MissileModule { get { return missileModule; } }

		private bool isUnitConsumer;
		public bool IsUnitConsumer { get { return isUnitConsumer; } }

		private IUnitResourceConsumer unitResourceConsumer;
		public IUnitResourceConsumer UnitResourceConsumer { get { return unitResourceConsumer; } }
	
		private Coroutine lockingCoroutine;
		public Coroutine LockingCoroutine { get { return lockingCoroutine; } }

		public LockState LockState { get { return missileModule.LockState; } }
	
		private float lockEventTime = 0;
		public float LockEventTime { get { return lockEventTime; } }

		private bool isChangedLockStateEvent;
		public bool IsChangedLockStateEvent { get { return isChangedLockStateEvent; } }


        /// <summary>
        /// Initialize class instance with a locking coroutine.
        /// </summary>
        /// <param name="lockingCoroutine">The locking coroutine for this LockingData instance.</param>
		public void Initialize(Coroutine lockingCoroutine)
		{
			this.lockingCoroutine = lockingCoroutine;
		}


        /// <summary>
        /// Update the tag that shows if this weapon has changed its lock state
        /// </summary>
        /// <param name="isChangedLockStateEvent">Whether the missile weapon's lock state has changed.</param>
		public void SetIsChangedLockStateEvent(bool isChangedLockStateEvent)
		{
			this.isChangedLockStateEvent = isChangedLockStateEvent;
		}


        /// <summary>
        /// Set the lock state for this LockingData class instance.
        /// </summary>
        /// <param name="newLockState">The new lock state</param>
		public void SetLockState (LockState newLockState)
		{
			missileModule.SetLockState(newLockState);
		}

        /// <summary>
        /// Create a new instance of the LockingData class.
        /// </summary>
        /// <param name="moduleMount">The module mount where the missile weapon is mounted.</param>
        /// <param name="missileModule">A reference to the missile weapon module interface.</param>
        /// <param name="unitConsumer">A reference to the missile weapon's unit consumer interface.</param>
		public LockingData (ModuleMount moduleMount, IMissileModule missileModule, IUnitResourceConsumer unitConsumer)
		{
	
			this.moduleMount = moduleMount;
	
			this.missileModule = missileModule;

			this.unitResourceConsumer = unitConsumer;
			this.isUnitConsumer = unitConsumer != null;
	
		}
	}
}