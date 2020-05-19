using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using VSX.General;


namespace VSX.UniversalVehicleCombat
{

	/// <summary>
    /// This class manages a single message that is showing on the UI.
    /// </summary>
	public class HUDMessageController : MonoBehaviour 
	{
	
        /// <summary>
        /// The different types of message animation.
        /// </summary>
		private enum MessageAnimationType
		{
			Flashing,
			Fading
		}
	
		private MessageAnimationType messageAnimationType = MessageAnimationType.Fading;
	
		private float flashPeriod = 1;
		private float sustainPeriod = 0.5f;
		private float fadePeriod = 0.5f;
	
		private float animationStartTime = 0;
	
		[SerializeField]
		private Text messageText;
	
		private GameObject cachedGameObject;
		public GameObject CachedGameObject { get { return cachedGameObject; } }
		
		private Transform cachedTransform;
		public Transform CachedTransform { get { return cachedTransform; } }
	
	
	
		void Awake()
		{
			cachedTransform = transform;
			cachedGameObject = gameObject;
		}
	

        /// <summary>
        /// Initialize this HUD message as a flashing message.
        /// </summary>
        /// <param name="message">The message content.</param>
        /// <param name="flashPeriod">The flash period.</param>
		public void Initialize(string message, float flashPeriod)
		{
	
			this.messageAnimationType = MessageAnimationType.Flashing;
	
			this.flashPeriod = flashPeriod;
			
			messageText.text = message;
			messageText.enabled = true;
	
			animationStartTime = Time.time;
	
			Color c = messageText.color;	
			c.a = 1;
			messageText.color = c;
			
		}


        /// <summary>
        /// Initialize this HUD message as a fading message.
        /// </summary>
        /// <param name="message">The message content.</param>
        /// <param name="sustainPeriod">The period that the message is shown at full opacity.</param>
        /// <param name="fadePeriod">The time it takes the message to fade.</param>
        public void Initialize(string message, float sustainPeriod, float fadePeriod)
		{
	
			this.messageAnimationType = MessageAnimationType.Fading;
	
			this.sustainPeriod = sustainPeriod;
			this.fadePeriod = fadePeriod;
	
			messageText.text = message;
			messageText.enabled = true;
	
			animationStartTime = Time.time;
	
            // Begin at full opacity
			Color c = messageText.color;	
			c.a = 1;
			messageText.color = c;
			
		}
	

        /// <summary>
        /// Force stop the message.
        /// </summary>
		public void Stop()
		{
			cachedGameObject.SetActive(false);
		}
	


		void Update()
		{
			
            // Animate the message
			float animationAmount;
			switch (messageAnimationType)
			{
				case MessageAnimationType.Flashing:
	
					animationAmount = ((Time.time - animationStartTime) % flashPeriod) / flashPeriod;
					messageText.enabled = animationAmount < 0.5f;
					break;
	
				case MessageAnimationType.Fading:
	
					float amount = (Time.time - (animationStartTime + sustainPeriod)) / fadePeriod;
					if (amount > 1)
					{
						cachedGameObject.SetActive(false);
					}
					else
					{
						Color c = messageText.color;	
						c.a = 1 - amount;
						messageText.color = c;
					}
					break;
	
				default:
	
					break;
			}
	
			
		}
	}
}
