using System.Collections.Generic;
using UnityEngine;

/**
 * Controller script for the LootCrate.
 *
 * @author J.C. Wichman - Inner Drive Studios
 */
public class LootCrate : MonoBehaviour {

	//audio settings for open, close events
	[Header("Open close audio settings")]
	[SerializeField] private AudioSource openCloseAudioSource;
	[SerializeField] private AudioClip openClip;
	[SerializeField] private AudioClip closeClip;

	//audio settings for when steam is used
	[Header("Steam settings")]
	//specifies whether and when steam is played, see SteamMode, 
	//must be set before entering play mode
	[SerializeField] private SteamMode steamMode;
	//the prefab (if any) to use to create the steam prefabs
	[SerializeField] private ParticleSystem steamPrefab;
	//the list of spawn points to create the steam prefab at
	[SerializeField] private Transform[] steamSpawnPoints;
	[SerializeField] private AudioSource steamAudioSource;
	[SerializeField] private AudioClip oneShotSteamClip;
	[SerializeField] private AudioClip loopingSteamClip;
	[SerializeField] private float oneShotVolume = 1.0f;
	[SerializeField] private float loopVolume = 1.0f;

	public enum SteamMode { PLAY_ALWAYS, PLAY_NEVER, PLAY_WHILE_CLOSED, PLAY_WHEN_OPENED };

	private Animator _animator;
	private List<ParticleSystem> _particleSystems;

	private void Awake()
	{
		_animator = GetComponent<Animator>();
		if (steamMode != SteamMode.PLAY_NEVER) _setupParticleSystems();
	}

	private void Start()
	{
		if (
			steamMode == SteamMode.PLAY_ALWAYS || 
			(steamMode == SteamMode.PLAY_WHILE_CLOSED && IsClosed())
		)
		{
			PlayParticleSystems(true);
		}
	}

	private void _setupParticleSystems()
	{
		//some sanity checks
		if (steamPrefab == null) return;
		if (steamSpawnPoints == null || steamSpawnPoints.Length == 0) return;

		//attach a particle system on every spawnpoint
		_particleSystems = new List<ParticleSystem>();
		for (int i = 0; i < steamSpawnPoints.Length; i++)
		{
			_particleSystems.Add (Instantiate(steamPrefab, steamSpawnPoints[i], false));
		}
		if (_particleSystems.Count == 0) steamMode = SteamMode.PLAY_NEVER;
	}
	
	//can be called through script
	public void Open()
	{
		if (IsOpen() || IsOpening()) return;

		_animator.SetTrigger("Open");

		if (openClip != null && openCloseAudioSource != null)
		{
			openCloseAudioSource.clip = openClip;
			openCloseAudioSource.Play();
		}
	}

	public void Close()
	{
		if (IsClosed() || IsClosing()) return;

		_animator.SetTrigger("Close");

		if (closeClip != null && openCloseAudioSource != null)
		{
			openCloseAudioSource.clip = closeClip;
			openCloseAudioSource.Play();
		}
	}

	public bool IsOpen ()
	{
		return _animator.GetCurrentAnimatorStateInfo(0).IsName("Open");
	}

	public bool IsClosed()
	{
		return	_animator.GetCurrentAnimatorStateInfo(0).IsName("Idle") ||
				_animator.GetCurrentAnimatorStateInfo(0).IsName("Close");
	}

	public bool IsOpening()
	{
		return _animator.GetCurrentAnimatorStateInfo(0).IsName("Open") && IsOpeningOrClosing();
	}

	public bool IsClosing()
	{
		return _animator.GetCurrentAnimatorStateInfo(0).IsName("Close") && IsOpeningOrClosing();
	}

	public bool IsOpeningOrClosing()
	{
		AnimatorStateInfo asi = _animator.GetCurrentAnimatorStateInfo(0);
		return !_animator.GetCurrentAnimatorStateInfo(0).IsName("Idle") && asi.normalizedTime < 1;
	}

	public SteamMode GetSteamMode()
	{
		return steamMode;
	}

	//triggered from the animation clips, to decide whether to start or stop the steam
	private void CrateStateChangedEvent()
	{
		if (IsOpen()) { 
			if (steamMode == SteamMode.PLAY_WHILE_CLOSED)
			{
				StopParticleSystems();
			} else if (steamMode == SteamMode.PLAY_WHEN_OPENED)
			{
				PlayParticleSystems(false);
			}
		} else
		{
			if (steamMode == SteamMode.PLAY_WHILE_CLOSED)
			{
				PlayParticleSystems(true);
			}
		}
	}

	//internal handlers to play the particle systems (looped or not, based on the steam mode)
	private void PlayParticleSystems(bool pLoop)
	{
		foreach (ParticleSystem ps in _particleSystems)
		{
			var main = ps.main;
			main.duration = Random.Range(1, 1.5f);
			main.loop = pLoop;
			ps.Play();
		}

		if (steamAudioSource == null) return;

		if (pLoop)
		{
			steamAudioSource.clip = loopingSteamClip;
			steamAudioSource.loop = true;
			steamAudioSource.volume = loopVolume;
		} else
		{

			steamAudioSource.clip = oneShotSteamClip;
			steamAudioSource.loop = false;
			steamAudioSource.volume = oneShotVolume;
		}

		if (steamAudioSource.clip != null) steamAudioSource.Play();
	}

	//internal handlers to stop all particle systems
	private void StopParticleSystems()
	{
		foreach (ParticleSystem ps in _particleSystems)
		{
			ps.Stop();
		}

		if (steamAudioSource != null) steamAudioSource.Stop();
	}

	
}


