using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using VSX.General;


namespace VSX.UniversalVehicleCombat
{
    /// <summary>
    /// This class manages the display of messages on the HUD, as well as sound effects that communicate important things
    /// to the player.
    /// </summary>
	public class HUDMessages : MonoBehaviour
    {
		
		[SerializeField]
		private Transform messageParent;    // The transform that new messages will be parented to

		[SerializeField]
		private GameObject messageInstancePrefab;
		
		[SerializeField]
		private float defaultMessageSustainPeriod;

		[SerializeField]
		private float defaultMessageFadePeriod;

		[SerializeField]
		private float defaultMessageFlashPeriod;
	
		HUDMessageController missileThreatWarning;
		
		private HUDManager manager;
		private bool hasManager;

		
		[SerializeField]
		private AudioSource lockingAudioSource;

		[SerializeField]
		private AudioSource lockedAudioSource;

		[SerializeField]
		private AudioSource missileThreatAudio;

		[SerializeField]
		private AudioSource newTargetAudio;
		private ITrackable lastTarget = null;


	
		/// <summary>
        /// Set the manager component for this HUD element.
        /// </summary>
        /// <param name="manager">The manager component for this HUD element.</param>
		public void SetManager(HUDManager manager)
		{
			this.manager = manager;
			this.hasManager = true;
		}


        /// <summary>
        /// Called when the HUD that this component is part of is activated.
        /// </summary>
		public void OnActivate()
		{
		}


        /// <summary>
        /// Called when the HUD that this component is part of is deactivated.
        /// </summary>
        public void OnDeactivate()
        {
            if (missileThreatWarning != null)
            {
                missileThreatWarning.CachedGameObject.SetActive(false);
                missileThreatWarning = null;
            }
        }


        /// <summary>
        /// Update the missile warning(s) (text and sfx)
        /// </summary>
        void UpdateMissileWarnings()
		{

			if (!manager.FocusedVehicle.HasRadar)
				return;
			
            // If there are missile threats, make sure the text is displayed and sfx is playing
			if (manager.FocusedVehicle.Radar.missileThreats.Count > 0)
			{
				if (missileThreatWarning == null)
				{
					GameObject newMessageGameObject = PoolManager.Instance.Get(messageInstancePrefab, Vector3.zero, Quaternion.identity, messageParent);
				
					missileThreatWarning = newMessageGameObject.GetComponent<HUDMessageController>();
					missileThreatWarning.CachedTransform.localRotation = Quaternion.identity;
					missileThreatWarning.CachedTransform.localPosition = Vector3.zero;
					missileThreatWarning.CachedTransform.localScale = new Vector3(1, 1, 1);
					missileThreatWarning.CachedTransform.SetAsLastSibling();
			
					missileThreatWarning.Initialize("MISSILE INCOMING", defaultMessageFlashPeriod);
				}
				if (missileThreatAudio != null && !missileThreatAudio.isPlaying) missileThreatAudio.Play();
			}
            // If there are no missile threats, make sure threat text is not showing and sfx is not playing
			else
			{
				if (missileThreatWarning != null)
				{
					missileThreatWarning.CachedGameObject.SetActive(false);
					missileThreatWarning = null;
				}
				if (missileThreatAudio != null && missileThreatAudio.isPlaying) missileThreatAudio.Stop();
			}
		}
        

		/// <summary>
        /// Display a new message on the HUD.
        /// </summary>
        /// <param name="message">Message content.</param>
        public void AddMessage(string message)
		{
			GameObject newMessageGameObject = PoolManager.Instance.Get(messageInstancePrefab, Vector3.zero, Quaternion.identity, messageParent);
			
			HUDMessageController messageController = newMessageGameObject.GetComponent<HUDMessageController>();
			messageController.CachedTransform.localPosition = Vector3.zero;
			messageController.CachedTransform.localRotation = Quaternion.identity;
			messageController.CachedTransform.localScale = new Vector3(1, 1, 1);
			messageController.CachedTransform.SetAsFirstSibling();
	
			messageController.Initialize(message, defaultMessageSustainPeriod, defaultMessageFadePeriod);

			// Make sure missile warning stays on top
			if (missileThreatWarning != null)
			{
				missileThreatWarning.CachedTransform.SetAsLastSibling();
			}
		}

	
		void Update()
		{
			if (!hasManager || !manager.HasFocusedVehicle)
				return;

			UpdateMissileWarnings();
			
            // Update the missile lock sound effects
			if (manager.FocusedVehicle.HasWeapons && manager.FocusedVehicle.Weapons.HasWeaponsComputer &&
			    manager.FocusedVehicle.Weapons.MountedWeaponsComputer.WeaponsComputer.IsLockingComputer)
			{
				for (int i = 0; i < manager.FocusedVehicle.Weapons.MountedWeaponsComputer.WeaponsComputer.LockingComputer.LockingDataList.Count; ++i)
				{
					if (manager.FocusedVehicle.Weapons.MountedWeaponsComputer.WeaponsComputer.LockingComputer.LockingDataList[i].MissileModule.LockState == LockState.Locking)
					{
						if (!lockingAudioSource.isPlaying)
							lockingAudioSource.Play();
					}
					else
					{
						lockingAudioSource.Stop();
					}

					if (manager.FocusedVehicle.Weapons.MountedWeaponsComputer.WeaponsComputer.LockingComputer.LockingDataList[i].IsChangedLockStateEvent)
					{
						if (manager.FocusedVehicle.Weapons.MountedWeaponsComputer.WeaponsComputer.LockingComputer.LockingDataList[i].LockState == LockState.Locked)
							lockedAudioSource.Play();
					}
				}
			}

            // Play 'new target' audio is new target is found.
			if (manager.FocusedVehicle.HasRadar)
			{
				if (manager.FocusedVehicle.Radar.HasSelectedTarget)
				{
					if (manager.FocusedVehicle.Radar.SelectedTarget != lastTarget) newTargetAudio.Play();
				}
				lastTarget = manager.FocusedVehicle.Radar.SelectedTarget;
			}		
		}
	}
}
