using UnityEngine;
using System.Collections;
using VSX.UniversalVehicleCombat;
using System.Collections.Generic;


namespace VSX.UniversalVehicleCombat
{
    /// <summary>
    /// This class provides a floating origin for managing large-scale distances. This component is put on a transform 
    /// in the scene, and environment objects are parented to it. The player ship is not parented to this transform.
    /// </summary>
    public class SceneOriginManager : MonoBehaviour
    {

        [SerializeField]
        private float maxDistanceFromCenter = 5000;

        private Transform cachedTransform;
        public Transform CachedTransform { get { return cachedTransform; } }

        private Vector3 playerPosition;
        public Vector3 PlayerPosition { get { return playerPosition; } }

        [SerializeField]
        private VehicleCamera vehicleCamera;

        public static SceneOriginManager Instance;

        private GameAgent focusedGameAgent = null;
        private bool hasFocusedGameAgent = false;

        private List<SceneOriginChild> sceneOriginChildren = new List<SceneOriginChild>();


        // Called when scene starts
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

            cachedTransform = transform;

            UVCEventManager.Instance.StartListening(UVCEventType.OnFocusedGameAgentChanged, OnFocusedGameAgentChanged);
        }


        public void Register(SceneOriginChild newSceneOriginChild)
        {
            sceneOriginChildren.Add(newSceneOriginChild);
        }


        /// <summary>
        /// Event function called when the focused Game Agent is changed, to make sure that the Game Agent's vehicle
        /// is not parented to this transform.
        /// </summary>
        /// <param name="newGameAgent">The new focused game agent</param>
        void OnFocusedGameAgentChanged(GameAgent newGameAgent)
        {

            if (hasFocusedGameAgent)
            {
                focusedGameAgent.onAgentChangedVehicleEventHandler -= OnFocusedVehicleChanged;
            }

            focusedGameAgent = newGameAgent;
            hasFocusedGameAgent = focusedGameAgent != null;

            if (hasFocusedGameAgent)
            {
                focusedGameAgent.onAgentChangedVehicleEventHandler += OnFocusedVehicleChanged;
                OnFocusedVehicleChanged(focusedGameAgent, focusedGameAgent.Vehicle);
            }
        }


        /// <summary>
        /// Event called when the focused game agent changes vehicles, to make sure the focused Game Agent's
        /// vehicle is not parented to this transform (and that the old one is).
        /// </summary>
        /// <param name="gameAgent">The focused game agent.</param>
        /// <param name="previousVehicle">The game agent's previous vehicle.</param>
        void OnFocusedVehicleChanged(GameAgent gameAgent, Vehicle previousVehicle)
        {

            if (previousVehicle != null)
                previousVehicle.CachedTransform.SetParent(cachedTransform);

            if (gameAgent.IsInVehicle)
            {
                gameAgent.Vehicle.CachedTransform.SetParent(null);
            }
        }


        /// <summary>
        /// Shift the scene, placing the player ship back at (0,0,0).
        /// </summary>
        void ShiftScene()
        {

            // Call the pre-shift event on scene origin children
            for (int i = 0; i < sceneOriginChildren.Count; ++i)
            {
                sceneOriginChildren[i].OnPreOriginShift();
            }

            // Move the scene
            Vector3 playerRelativePosition = GameAgentManager.Instance.FocusedGameAgent.Vehicle.CachedTransform.position - transform.position;
            transform.position = -playerRelativePosition;


            // Store the relative camera position
            Vector3 cameraOffset = vehicleCamera.CachedTransform.position - GameAgentManager.Instance.FocusedGameAgent.Vehicle.CachedTransform.position;

            // Put the player in the center
            GameAgentManager.Instance.FocusedGameAgent.Vehicle.CachedTransform.position = Vector3.zero;

            vehicleCamera.CachedTransform.position = GameAgentManager.Instance.FocusedGameAgent.Vehicle.CachedTransform.position + cameraOffset;

            // Call the post-shift event on scene origin children
            for (int i = 0; i < sceneOriginChildren.Count; ++i)
            {
                sceneOriginChildren[i].OnPostOriginShift();
            }
        }


        // Called every frame
        void Update()
        {
            if (!GameAgentManager.Instance.FocusedGameAgent.IsInVehicle) return;

            Vector3 playerRealPos = GameAgentManager.Instance.FocusedGameAgent.Vehicle.CachedTransform.position;

            // If player is too far from center, shift the scene and place the player at (0,0,0)
            if (playerRealPos.magnitude > maxDistanceFromCenter)
            {
                ShiftScene();
            }

            playerPosition = playerRealPos - cachedTransform.position;

        }
    }
}