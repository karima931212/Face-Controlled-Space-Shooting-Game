using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using VSX.General;
using VSX.UniversalVehicleCombat;


namespace VSX.UniversalVehicleCombat
{
    /// <summary>
    /// This class manages the loadout menu scene.
    /// </summary>
    public class LoadoutManager : MonoBehaviour
    {

        [SerializeField]
        private PlayerItemManager itemManager;

        List<Vehicle> displayVehicles;

        public enum LoadoutMenuState
        {
            VehicleSelection,
            ModuleSelection
        }
        private LoadoutMenuState loadoutMenuState = LoadoutMenuState.VehicleSelection;


        [SerializeField]
        private GameObject moduleSelectionUI;

        [SerializeField]
        private GameObject shipSelectionUI;

        [SerializeField]
        private GameObject blackout;


        [Header("Menu Controllers")]

        [SerializeField]
        private LoadoutDisplayManager displayManager;

        [SerializeField]
        private LoadoutModuleMenuController moduleMenuController;
        int focusedModuleItemIndex;

        [SerializeField]
        private LoadoutMountMenuController mountMenuController;
        int focusedModuleMountIndex;



        [Header("Vehicles")]

        [SerializeField]
        private GameObject shipInfoParent;

        [SerializeField]
        private Text shipNameText;

        private int selectedVehicleIndex = -1;

        // for displaying ship attributes / max
        float maxVehicleThrust;
        float maxVehicleAgility;
        float maxVehicleArmor;
        float maxVehicleShields;

        // Vehicle attributes
        [SerializeField]
        private MenuBarController shipSpeedBar;

        [SerializeField]
        private MenuBarController shipAgilityBar;

        [SerializeField]
        private MenuBarController shipArmorBar;

        [SerializeField]
        private MenuBarController shipShieldsBar;


        [Header("Modules")]

        [SerializeField]
        private GameObject weaponInfoParent;

        [SerializeField]
        private Text moduleName;

        // For displaying gun attributes / max
        float maxGunSpeed;
        float maxGunArmorDamage;
        float maxGunShieldDamage;

        // For displaying missile attributes / max
        float maxMissileSpeed;
        float maxMissileArmorDamage;
        float maxMissileShieldDamage;
        float maxMissileAgility;
        float maxMissileLockingRange;

        // Weapon attribute visualization
        [SerializeField]
        private MenuBarController weaponSpeedBar;

        [SerializeField]
        private MenuBarController weaponArmorDamageBar;

        [SerializeField]
        private MenuBarController weaponShieldDamageBar;

        [SerializeField]
        private MenuBarController weaponAgilityBar;

        [SerializeField]
        private MenuBarController weaponRangeBar;


        [Header("Audio")]

        [SerializeField]
        private AudioSource audioSource;

        [SerializeField]
        private AudioClip buttonAudioClip;

        public static LoadoutManager Instance;



        void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
            }
        }


        void Start()
        {

            // Disable the blackout screen
            blackout.SetActive(false);

            // Disable module selection menu
            moduleSelectionUI.SetActive(false);

            // Enable the ship selection menu
            shipSelectionUI.SetActive(true);

            // Create the display vehicles
            displayVehicles = displayManager.AddDisplayVehicles(itemManager.Vehicles, itemManager);

            moduleMenuController.Initialize(itemManager);

            // Update weapon metrics reference values
            GetGunMetricsReferenceValues(itemManager.AllModulePrefabs, out maxGunSpeed, out maxGunArmorDamage, out maxGunShieldDamage);

            GetMissileMetricsReferenceValues(itemManager.AllModulePrefabs, out maxMissileSpeed, out maxMissileArmorDamage, out maxMissileShieldDamage,
                out maxMissileAgility, out maxMissileLockingRange);

            GetShipMetricsReferenceValues(displayVehicles, out maxVehicleThrust, out maxVehicleAgility, out maxVehicleArmor, out maxVehicleShields);

            // Select a ship
            if (displayVehicles.Count > 0)
            {
                int ind = PlayerData.GetSelectedVehicleIndex(itemManager);
                if (ind >= 0 && displayVehicles.Count > ind)
                {
                    OnSelectVehicle(ind);
                }
                else
                {
                    OnSelectVehicle(0);
                }
            }
        }



        /// <summary>
        /// Event called when a new vehicle is selected in the loadout menu.
        /// </summary>
        /// <param name="index">The index of the newly selected vehicle.</param>
        /// <param name="playAudio">Whether to play the vehicle selection sound effect.</param>
        void OnSelectVehicle(int index, bool playAudio = true)
        {

            int previousVehicleIndex = selectedVehicleIndex;
            selectedVehicleIndex = index;
            PlayerData.SaveSelectedVehicleIndex(selectedVehicleIndex);

            // Prepare the UI
            shipNameText.text = displayVehicles[selectedVehicleIndex].Label;
            UpdateVehicleInfo();

            displayManager.OnVehicleSelection(selectedVehicleIndex, previousVehicleIndex, itemManager);

            // Update the mount menu
            mountMenuController.UpdateMenu(displayVehicles[selectedVehicleIndex]);
            focusedModuleMountIndex = displayVehicles[selectedVehicleIndex].ModuleMounts.Count > 0 ? 0 : -1;
            SelectModuleMount(focusedModuleMountIndex, false);

            if (playAudio) PlayMenuAudio();

        }


        /// <summary>
        /// Called when a new vehicle is selected in the loadout menu, to update the vehicle stats UI.
        /// </summary>
        void UpdateVehicleInfo()
        {

            SpaceVehicleEngines vehicleController = displayVehicles[selectedVehicleIndex].CachedTransform.GetComponentInChildren<SpaceVehicleEngines>();
            if (vehicleController == null)
            {
                Debug.LogError("Attempting to calculate ship performance metrics for a vehicle which does not contain" +
                                " a SpaceVehicleEngines component.");
            }

            shipSpeedBar.SetValue(vehicleController.DefaultTranslationForces.z / maxVehicleThrust);

            float averageAxisTorque = (vehicleController.MaxRotationForces.x + vehicleController.MaxRotationForces.y + vehicleController.MaxRotationForces.z) / 3f;

            shipAgilityBar.SetValue(averageAxisTorque / maxVehicleAgility);

            shipShieldsBar.SetValue(displayVehicles[selectedVehicleIndex].Health.GetStartingHealthValue(HealthType.Shield) / maxVehicleShields);

            shipArmorBar.SetValue(displayVehicles[selectedVehicleIndex].Health.GetStartingHealthValue(HealthType.Armor) / maxVehicleArmor);

        }


        /// <summary>
        /// Toggle between the vehicle and module selection modes in the loadout menu.
        /// </summary>
        /// <param name="newStateIndex">The new menu state.</param>
        public void ChangeMenuState(int newStateIndex)
        {

            loadoutMenuState = (LoadoutMenuState)newStateIndex;

            if (loadoutMenuState == LoadoutMenuState.VehicleSelection)
            {
                shipSelectionUI.SetActive(true);
                moduleSelectionUI.SetActive(false);
                displayManager.FocusModuleMount(null);
            }
            else
            {
                shipSelectionUI.SetActive(false);
                moduleSelectionUI.SetActive(true);

                ModuleMount focusedMount = focusedModuleMountIndex == -1 ? null : displayVehicles[selectedVehicleIndex].ModuleMounts[focusedModuleMountIndex];
                displayManager.FocusModuleMount(focusedMount);
            }
        }


        /// <summary>
        /// Called when the player clicks on a button to focus on a different module mount in the loadout menu.
        /// </summary>
        /// <param name="newMountIndex">The index of the new module mount.</param>
        /// <param name="playAudio">Whether to play the module mount selection sound effect.</param>
        public void SelectModuleMount(int newMountIndex, bool playAudio = true)
        {

            // Update the module mount
            focusedModuleMountIndex = newMountIndex;
            mountMenuController.OnSelectMount(focusedModuleMountIndex);

            // Update the module options menu
            List<int> selectableIndexes = new List<int>();
            int mountedIndex = -1;
            for (int i = 0; i < displayVehicles[selectedVehicleIndex].ModuleMounts[focusedModuleMountIndex].MountableModules.Count; ++i)
            {

                int index = itemManager.AllModulePrefabs.IndexOf(displayVehicles[selectedVehicleIndex].ModuleMounts[focusedModuleMountIndex].MountableModules[i].modulePrefab);
                if (index != -1)
                {
                    selectableIndexes.Add(index);


                    if (displayVehicles[selectedVehicleIndex].ModuleMounts[focusedModuleMountIndex].MountedModuleIndex == i)
                    {
                        mountedIndex = index;
                        SelectModule(mountedIndex, false);
                    }
                }
            }

            moduleMenuController.UpdateModuleSelectionMenu(selectableIndexes, mountedIndex);

            if (loadoutMenuState == LoadoutMenuState.ModuleSelection)
            {
                ModuleMount focusedMount = focusedModuleMountIndex == -1 ? null : displayVehicles[selectedVehicleIndex].ModuleMounts[focusedModuleMountIndex];
                displayManager.FocusModuleMount(focusedMount);
            }

            // Play menu audio
            if (playAudio) PlayMenuAudio();

        }


        /// <summary>
        /// Called when the player clicks on a button to clear the current selection of a module at a module mount (mount nothing).
        /// </summary>
        public void ClearModuleMount()
        {
            SelectModule(-1);
        }


        /// <summary>
        /// Called when the player clicks on a module item in the loadout menu.
        /// </summary>
        /// <param name="newModuleIndex">The index of the newly selected module in the menu</param>
        /// <param name="playAudio">Whether to play the sound effect for module selection.</param>
        public void SelectModule(int newModuleIndex, bool playAudio = true)
        {
            if (newModuleIndex == -1)
            {
                displayVehicles[selectedVehicleIndex].ModuleMounts[focusedModuleMountIndex].MountModule(-1);
            }
            else
            {
                // Update the module being displayed at the mount
                for (int i = 0; i < displayVehicles[selectedVehicleIndex].ModuleMounts[focusedModuleMountIndex].MountableModules.Count; ++i)
                {
                    if (displayVehicles[selectedVehicleIndex].ModuleMounts[focusedModuleMountIndex].MountableModules[i].modulePrefab ==
                        itemManager.AllModulePrefabs[newModuleIndex])
                    {
                        displayVehicles[selectedVehicleIndex].ModuleMounts[focusedModuleMountIndex].MountModule(i);
                    }
                }
            }

            int mountedIndex = displayVehicles[selectedVehicleIndex].ModuleMounts[focusedModuleMountIndex].MountedModuleIndex;
            if (mountedIndex != -1)
            {
                GameObject prefab = displayVehicles[selectedVehicleIndex].ModuleMounts[focusedModuleMountIndex].MountableModules[mountedIndex].modulePrefab;
                focusedModuleItemIndex = itemManager.AllModulePrefabs.IndexOf(prefab);
            }
            else
            {
                focusedModuleItemIndex = -1;
            }

            // Update the module menu 
            moduleMenuController.OnSelectModule(focusedModuleItemIndex);

            weaponInfoParent.SetActive(focusedModuleItemIndex != -1);

            weaponArmorDamageBar.DisableBar();
            weaponShieldDamageBar.DisableBar();
            weaponSpeedBar.DisableBar();
            weaponAgilityBar.DisableBar();
            weaponRangeBar.DisableBar();

            // Update the module metrics
            if (focusedModuleItemIndex != -1)
            {

                IModule module = itemManager.AllModulePrefabs[focusedModuleItemIndex].GetComponent<IModule>();
                if (module == null) return;

                moduleName.text = module.Label;

                IGunModule gunModuleInterface = itemManager.AllModulePrefabs[focusedModuleItemIndex].GetComponent<IGunModule>();
                if (gunModuleInterface != null)
                {
                    weaponArmorDamageBar.SetValue(gunModuleInterface.DamageValueByHealthType[(int)HealthType.Armor] / maxGunArmorDamage);
                    weaponArmorDamageBar.EnableBar();

                    weaponShieldDamageBar.SetValue(gunModuleInterface.DamageValueByHealthType[(int)HealthType.Shield] / maxGunShieldDamage);
                    weaponShieldDamageBar.EnableBar();

                    weaponSpeedBar.SetValue(gunModuleInterface.ProjectileSpeed / maxGunSpeed);
                    weaponSpeedBar.EnableBar();

                }

                IMissileModule missileModuleInterface = itemManager.AllModulePrefabs[focusedModuleItemIndex].GetComponent<IMissileModule>();
                if (missileModuleInterface != null)
                {

                    weaponArmorDamageBar.SetValue(missileModuleInterface.DamageValueByHealthType[(int)HealthType.Armor] / maxMissileArmorDamage);
                    weaponArmorDamageBar.EnableBar();

                    weaponShieldDamageBar.SetValue(missileModuleInterface.DamageValueByHealthType[(int)HealthType.Shield] / maxMissileShieldDamage);
                    weaponShieldDamageBar.EnableBar();

                    weaponSpeedBar.SetValue(missileModuleInterface.MaxSpeed / maxMissileSpeed);
                    weaponSpeedBar.EnableBar();

                    weaponAgilityBar.SetValue(missileModuleInterface.Agility / maxMissileAgility);
                    weaponAgilityBar.EnableBar();

                    weaponRangeBar.SetValue(missileModuleInterface.MaxLockingRange / maxMissileLockingRange);
                    weaponRangeBar.EnableBar();
                }

                HealthGenerator healthModule = itemManager.AllModulePrefabs[focusedModuleItemIndex].GetComponent<HealthGenerator>();
                if (healthModule != null)
                {

                }
            }

            if (playAudio) PlayMenuAudio();

        }


        /// <summary>
        /// Play a button click.
        /// </summary>
        public void PlayMenuAudio()
        {

            audioSource.Stop();
            audioSource.clip = buttonAudioClip;
            audioSource.Play();

        }


        // Cycle the vehicle selection in the menu forward or backward.
        public void CycleVehicleSelection(bool cycleForward)
        {
            if (displayVehicles.Count == 0 || displayVehicles.Count == 1) return;

            int nextIndex = selectedVehicleIndex;

            // Cycle up or down
            if (cycleForward)
            {
                nextIndex += 1;
            }
            else
            {
                nextIndex -= 1;
            }

            // Make sure the new index corresponds to a valid index in the ship group list
            if (nextIndex < 0)
                nextIndex = displayVehicles.Count - 1;
            else if (nextIndex >= displayVehicles.Count)
                nextIndex = 0;

            if (nextIndex != selectedVehicleIndex)
            {
                OnSelectVehicle(nextIndex);
            }
        }


        /// <summary>
        /// Exit the loadout menu and begin the game.
        /// </summary>
        public void StartMission()
        {

            for (int i = 0; i < displayVehicles.Count; ++i)
            {
                PlayerData.SaveModuleLoadout(displayVehicles[i], i, itemManager);
            }

            blackout.SetActive(true);

            SceneManager.LoadScene("GameplayDemo");

        }

        /// <summary>
        /// Get the maximum values for all of the gun module metrics so that gun module performance can be displayed 
        /// as a relative value in a bar.
        /// </summary>
        /// <param name="modulePrefabs">A list of all the module prefabs available in the game.</param>
        /// <param name="maxSpeed">Return the max speed of the gun (relevant only for projectile weapons).</param>
        /// <param name="maxArmorDamage">Return the maximum armor damage that is available from a gun weapon.</param>
        /// <param name="maxShieldDamage">Return the maximum shield damage that is available from a gun weapon.</param>
        public static void GetGunMetricsReferenceValues(List<GameObject> modulePrefabs, out float maxSpeed, out float maxArmorDamage, out float maxShieldDamage)
        {

            maxSpeed = 0;
            maxArmorDamage = 0;
            maxShieldDamage = 0;

            for (int i = 0; i < modulePrefabs.Count; ++i)
            {

                IGunModule gunModule = modulePrefabs[i].GetComponent<IGunModule>();
                if (gunModule == null) continue;

                maxArmorDamage = gunModule.DamageValueByHealthType[(int)HealthType.Armor] > maxArmorDamage ? gunModule.DamageValueByHealthType[(int)HealthType.Armor] : maxArmorDamage;
                maxShieldDamage = gunModule.DamageValueByHealthType[(int)HealthType.Shield] > maxShieldDamage ? gunModule.DamageValueByHealthType[(int)HealthType.Shield] : maxShieldDamage;

                maxSpeed = gunModule.ProjectileSpeed > maxSpeed ? gunModule.ProjectileSpeed : maxSpeed;

            }
        }


        /// <summary>
        /// Get the maximum values for all of the missile module metrics so that missile module performance can be displayed 
        /// as a relative value in a bar.
        /// </summary>
        /// <param name="modulePrefabs">A list of all the module prefabs available in the game.</param>
        /// <param name="maxSpeed">Return the max speed of any of the missiles.
        /// <param name="maxArmorDamage">Return the maximum armor damage that is available from a missile.</param>
        /// <param name="maxShieldDamage">Return the maximum shield damage that is available from a missile.</param>
        /// <param name="maxAgility">Return the maximum agility found for any of the missiles.</param>
        /// <param name="maxRange">Return the maximum range found for any of the missiles.</param>
        public static void GetMissileMetricsReferenceValues(List<GameObject> modulePrefabs, out float maxSpeed, out float maxArmorDamage, out float maxShieldDamage,
                                                        out float maxAgility, out float maxRange)
        {

            maxSpeed = 0;
            maxArmorDamage = 0;
            maxShieldDamage = 0;
            maxAgility = 0;
            maxRange = 0;

            for (int i = 0; i < modulePrefabs.Count; ++i)
            {

                IMissileModule missileModule = modulePrefabs[i].GetComponent<IMissileModule>();
                if (missileModule == null) continue;


                maxArmorDamage = missileModule.DamageValueByHealthType[(int)HealthType.Armor] > maxArmorDamage ? missileModule.DamageValueByHealthType[(int)HealthType.Armor] : maxArmorDamage;
                maxShieldDamage = missileModule.DamageValueByHealthType[(int)HealthType.Shield] > maxShieldDamage ? missileModule.DamageValueByHealthType[(int)HealthType.Shield] : maxShieldDamage;

                maxAgility = missileModule.Agility > maxAgility ? missileModule.Agility : maxAgility;
                maxRange = missileModule.MaxLockingRange > maxRange ? missileModule.MaxLockingRange : maxRange;

            }
        }


        /// <summary>
        /// Get the maximum values for all of the vehicle metrics so that vehicle performance can be displayed 
        /// as a relative value in a bar.
        /// </summary>
        /// <param name="vehicles">A list of all the vehicles available in the loadout menu.</param>
        /// <param name="maxShipThrust">Return the maximum thrust found for any of the vehicles.</param>
        /// <param name="maxShipAgility">Return the maximum agility found for any of the vehicles.</param>
        /// <param name="maxShipArmor">Return the maximum armor health value for any of the vehicles.</param>
        /// <param name="maxShipShields">Return the maximum shield health value for any of the vehicles.</param>
	    public static void GetShipMetricsReferenceValues(List<Vehicle> vehicles, out float maxShipThrust, out float maxShipAgility, out float maxShipArmor, out float maxShipShields)
        {

            maxShipThrust = 0;
            maxShipAgility = 0;
            maxShipArmor = 0;
            maxShipShields = 0;

            for (int i = 0; i < vehicles.Count; ++i)
            {

                SpaceVehicleEngines vehicleController = vehicles[i].CachedTransform.GetComponentInChildren<SpaceVehicleEngines>();
                if (vehicleController == null)
                {
                    Debug.LogError("Attempting to calculate ship performance metrics for a vehicle which does not contain" +
                                    " a SpaceVehicleEngines component.");
                }

                maxShipThrust = vehicleController.DefaultTranslationForces.z > maxShipThrust ? vehicleController.DefaultTranslationForces.z : maxShipThrust;

                float averageAxisTorque = (vehicleController.MaxRotationForces.x + vehicleController.MaxRotationForces.y +
                                            vehicleController.MaxRotationForces.z) / 3f;

                maxShipAgility = averageAxisTorque > maxShipAgility ? averageAxisTorque : maxShipAgility;

                maxShipArmor = vehicles[i].Health.GetStartingHealthValue(HealthType.Armor) > maxShipArmor ?
                                    vehicles[i].Health.GetCurrentHealthValue(HealthType.Armor) : maxShipArmor;

                maxShipShields = vehicles[i].Health.GetStartingHealthValue(HealthType.Shield) > maxShipShields ?
                                    vehicles[i].Health.GetCurrentHealthValue(HealthType.Shield) : maxShipShields;
            }
        }
    }
}