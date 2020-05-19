using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using VSX.UniversalVehicleCombat;


namespace VSX.UniversalVehicleCombat
{

    /// <summary>
    /// This class holds a reference to a module that has been created and stored, but not necessarily
    /// mounted on the module mount.
    /// </summary>
    public class MountableModule
	{

		public GameObject modulePrefab;

		public IModule createdModule;
	
		public MountableModule(GameObject modulePrefab, IModule createdModule)
		{
			this.modulePrefab = modulePrefab;
			this.createdModule = createdModule;
		}
	}

    /// <summary>
    /// This delegate is for any functions that need to be called when a new module is mounted onto a module mount
    /// </summary>
    /// <param name="moduleMount">The module mount that the new module was loaded at. </param>
    public delegate void OnNewModuleMountedEventHandler(ModuleMount moduleMount);
 
	/// <summary>
    /// This class provides a pathway between vehicle subsystems and the modules they manage, by providing a physical
    /// location as well as a reference point for where a module can be found, or swapped for another module.
    /// </summary>
	public class ModuleMount : MonoBehaviour 
	{
	
		[SerializeField]
		protected string label = "Module Mount";
		public string Label { get { return label; } }

		protected int moduleMountIndex;
		public int ModuleMountIndex 
		{ 
			get { return moduleMountIndex; }
			set { moduleMountIndex = value; }
		}

		// All the module types that can be mounted on this module mount
		[SerializeField]
		protected List<ModuleType> mountableTypes = new List<ModuleType>();
		public List<ModuleType> MountableTypes { get { return mountableTypes; } }

		// All the module prefabs that will be instantiated by default for this mount
		[SerializeField]
		protected List<GameObject> defaultModules = new List<GameObject>();	
		public List<GameObject> DefaultModules { get { return defaultModules; } }

		[SerializeField]
		protected bool createDefaultModulesAtStart;
		public bool CreateDefaultModulesAtStart
		{ 
			get { return createDefaultModulesAtStart; }
			set { createDefaultModulesAtStart = value; }
		}
		
		[SerializeField]
		protected bool defaultToFirstAvailable;
		public bool isDefaultToFirstAvailable { get { return defaultToFirstAvailable; } }
		
		// All the modules that are selectable for this mount
		[SerializeField]
		protected List<MountableModule> mountableModules = new List<MountableModule>();	
		public List<MountableModule> MountableModules { get { return mountableModules; } }

		// The index of the currently selected module
		protected int mountedModuleIndex = -1;
		public int MountedModuleIndex { get { return mountedModuleIndex; } }
	
		/// <summary>
        /// A list of all the attachment points for mounting weapons, for a multi-weapon module
        /// </summary>
		[SerializeField]
		protected List<Transform> attachmentPoints = new List<Transform>();
		public List<Transform> AttachmentPoints { get { return attachmentPoints; } }

        protected Transform cachedTransform;
        public Transform CachedTransform { get { return cachedTransform; } }

        /// <summary>
        /// Event called when a new module is mounted at this module mount.
        /// </summary>
        protected event OnNewModuleMountedEventHandler newModuleMountedEventHandler;
		public event OnNewModuleMountedEventHandler NewModuleMountedEventHandler
		{ 
			add { newModuleMountedEventHandler += value; }
			remove { newModuleMountedEventHandler -= value; }
		}
		
        /// <summary>
        /// The vehicle that this module mount belongs to.
        /// </summary>
		private Vehicle vehicle;
		public Vehicle Vehicle
		{
			get { return vehicle; } 
			set { vehicle = value; }
		}
	

		void Awake()
		{
			cachedTransform = transform;
		}


		protected virtual void Start()
		{

            // Get all of the modules already existing as children of this module mount
			IModule[] modules = transform.GetComponentsInChildren<IModule>();
			foreach (IModule module in modules)
			{

				if (!mountableTypes.Contains(module.ModuleType))
					continue;

				// Check if this module is already loaded
				bool found = false;
				for (int i = 0; i < mountableModules.Count; ++i)
				{		
					if (module.CachedGameObject == mountableModules[i].createdModule.CachedGameObject) found = true;
				}	
				if (found) continue;

				GameObject createdModuleObject = module.CachedGameObject;

				createdModuleObject.transform.localPosition = Vector3.zero;
				createdModuleObject.transform.localRotation = Quaternion.identity;

				AddMountableModule(module, null, (defaultToFirstAvailable && mountedModuleIndex == -1));
			}
			
			// Create all of the modules that can be mounted on this mount
			if (createDefaultModulesAtStart)
			{
				for (int i = 0; i < defaultModules.Count; ++i)
				{

					IModule module = defaultModules[i].GetComponent<IModule>();

					if (module == null)
						continue;

					if (!mountableTypes.Contains(module.ModuleType))
						continue;

					GameObject createdModuleObject = (GameObject)Instantiate(defaultModules[i], cachedTransform) as GameObject;

					createdModuleObject.transform.localPosition = Vector3.zero;
					createdModuleObject.transform.localRotation = Quaternion.identity;

					IModule createdModule = createdModuleObject.GetComponent<IModule>();

					AddMountableModule(createdModule, defaultModules[i], (defaultToFirstAvailable && mountedModuleIndex == -1));
					
				}
			}
		}



        /// <summary>
        /// Add a new mountable module to this module mount.
        /// </summary>
        /// <param name="createdModule">The module to be added (must be already created in the scene).</param>
        /// <param name="prefabReference">The prefab the module was made from.</param>
        /// <param name="mountImmediately">Whether the module should be mounted immediately. </param>
        /// <returns>The MountableModule class instance for the newly stored module.</returns>
        public MountableModule AddMountableModule(IModule createdModule, GameObject prefabReference = null, bool mountImmediately = false)
		{
			
			if (!mountableTypes.Contains(createdModule.ModuleType))
				return null;

			createdModule.CachedTransform.SetParent(transform);
			createdModule.CachedTransform.localPosition = Vector3.zero;
			createdModule.CachedTransform.localRotation = Quaternion.identity;
			
			MountableModule newMountableModule = new MountableModule(prefabReference, createdModule);
			mountableModules.Add(newMountableModule);			
			
			if (mountImmediately)
			{ 
				MountModule(mountableModules.Count - 1);
			}
			else
			{
				newMountableModule.createdModule.OnUnmount();
			}

			return newMountableModule;
		}
		

		/// <summary>
        /// Cycle the mounted module at this module mount.
        /// </summary>
        /// <param name="forward">Whether to cycle forward or backward.</param>
		public void Cycle(bool forward)
		{

			if (mountableModules.Count <= 1) return;

			// Increment or decrement the module index
			int newMountedModuleIndex = forward ? mountedModuleIndex + 1 : mountedModuleIndex - 1;
		
			// If exceeds highest index, return to zero index
			newMountedModuleIndex = newMountedModuleIndex >= mountableModules.Count ? 0 : newMountedModuleIndex;

			// If exceeds lowest index, return to last index
			newMountedModuleIndex = newMountedModuleIndex < 0 ? mountableModules.Count - 1 : newMountedModuleIndex;

			// Mount the new Module
			MountModule(newMountedModuleIndex);

		}
		
		
		/// <summary>
        /// Mount a new module at the module mount. Module must be already added as a MountableModule instance.
        /// </summary>
        /// <param name="newMountedModuleIndex">The new module's index within the list of Mountable Modules.</param>
        /// <returns>Whether the mounting of the module occurred.</returns>
		public virtual bool MountModule(int newMountedModuleIndex)
		{

			// If new is the same as old, return
			if (newMountedModuleIndex == mountedModuleIndex)
				return false;

			// Deactivate the last one
			if (mountedModuleIndex >= 0)
			{ 
				mountableModules[mountedModuleIndex].createdModule.OnUnmount();
			}

			// If new index is valid, activate the module
			if (newMountedModuleIndex >= 0 && newMountedModuleIndex < mountableModules.Count)
			{		
				mountableModules[newMountedModuleIndex].createdModule.OnMount(this);
				mountedModuleIndex = newMountedModuleIndex;
			}
			else
			{
				mountedModuleIndex = -1;
			}

			if (newModuleMountedEventHandler != null) newModuleMountedEventHandler(this);

			return true;

		}

	
		/// <summary>
        /// Clear all of the modules stored at this module mount
        /// </summary>
		public void RemoveAllMountableModules()
		{
			for (int i = 0; i < mountableModules.Count; ++i){
				mountableModules[i].createdModule.OnUnmount();
			}
			mountableModules.Clear();
			mountedModuleIndex = -1;
		}


		/// <summary>
        /// Get a reference to the IModule interface of the module currently mounted at this module mount.
        /// </summary>
        /// <returns>The interface for the mounted module.</returns>
		public IModule Module()
		{
			if (mountedModuleIndex == -1)
			{
				return null;
			}
			else
			{
				return mountableModules[mountedModuleIndex].createdModule;
			}
		}
	}
}
