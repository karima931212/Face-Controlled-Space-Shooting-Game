using UnityEngine;
using System.Collections;

namespace VSX.UniversalVehicleCombat
{

	/// <summary>
    /// This class represents a powerplant module that can be loaded onto a vehicle.
    /// </summary>
	public class PowerPlantModule : MonoBehaviour, IModule, IPowerPlant 
	{
	
		[SerializeField]
		private float output;
		public float Output { get { return output; } }

		[Header("Module")]

		[SerializeField]
		private string label;
		public string Label { get { return label; } }
		
		public ModuleType ModuleType { get { return ModuleType.Powerplant; }  }
		
		[SerializeField]
		private Sprite menuSprite;
		public Sprite MenuSprite { get { return menuSprite; } }
		
		private GameObject cachedGameObject;
		public GameObject CachedGameObject { get { return cachedGameObject; } }
		public GameObject GameObject { get { return gameObject; } }
	
		private Transform cachedTransform;
		public Transform CachedTransform { get { return cachedTransform; } }
		
		private ModuleState moduleState;
		public ModuleState ModuleState { get { return moduleState; } }
	
		private ModuleMount moduleMount;
		


		protected virtual void Awake()
		{
			cachedTransform = transform;
			cachedGameObject = gameObject;

			// To suppress warning
			if (moduleMount == null)
			{
				moduleMount = null;
			}
		}

		
        /// <summary>
        /// Set a new module state.
        /// </summary>
        /// <param name="newState"></param>
		public void SetModuleState(ModuleState newState)
		{
			switch (newState)
			{
				case ModuleState.Activated:
	
					moduleState = ModuleState.Activated;
					break;
	
				case ModuleState.Deactivated:
		
					moduleState = ModuleState.Deactivated;
					break;

				case ModuleState.Destroyed:
					
					moduleState = ModuleState.Destroyed;
					break;
			}
		}

		
        /// <summary>
        /// Event called when this module is mounted on one of the vehicle's module mounts.
        /// </summary>
        /// <param name="moduleMount">The module mount where the new module was loaded.</param>
		public void OnMount(ModuleMount moduleMount)
		{
			
			this.moduleMount = moduleMount;
			
			cachedTransform.SetParent(moduleMount.CachedTransform);
			cachedTransform.localPosition = Vector3.zero;
			cachedTransform.localRotation = Quaternion.identity;
			
			cachedGameObject.SetActive(true);

		}
			
		
        /// <summary>
        /// Event called when this module is unmounted from a vehicle's module mount.
        /// </summary>
		public void OnUnmount()
		{
			this.moduleMount = null;
			cachedGameObject.SetActive(false);
			
		}


        /// <summary>
        /// Reset the module to starting conditions.
        /// </summary>
		public void ResetModule()
		{
		}

	}
}
