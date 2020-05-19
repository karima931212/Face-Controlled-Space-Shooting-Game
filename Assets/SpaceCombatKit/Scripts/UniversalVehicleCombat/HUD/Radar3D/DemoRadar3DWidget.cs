using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using VSX.UniversalVehicleCombat;
using VSX.General;


namespace VSX.UniversalVehicleCombat
{
    /// <summary>
    /// This class manages a widget displayed on a 3D radar.
    /// </summary>
	public class DemoRadar3DWidget : MonoBehaviour, IRadar3DWidget
    {


		private GameObject cachedGameObject;
		public GameObject CachedGameObject { get { return cachedGameObject; } }
		
		private Transform cachedTransform;
		public Transform CachedTransform { get { return cachedTransform; } }
	
		[SerializeField]
		private SpriteRenderer iconSprite;
		private HUDSpriteObject iconSpriteObject;
		
		[SerializeField]
		private LineRenderer legLineRenderer;
		private Material legMaterial;
		private HUDSpriteObject legSpriteObject; 
	    
		[SerializeField]
		private SpriteRenderer footSprite;
		private HUDSpriteObject footSpriteObject;
	
		[SerializeField]
		private SpriteRenderer targetSelectedSprite;
		private HUDSpriteObject targetSelectedSpriteObject;
	    
	
	    void Awake()
	    {
			
			cachedGameObject = gameObject;
			cachedTransform = transform;

            // Cache all of the parts and components of the 3D radar widget

			iconSpriteObject = new HUDSpriteObject(iconSprite);
	            
			legMaterial = legLineRenderer.material; 
	
			footSpriteObject = new HUDSpriteObject(footSprite);

            targetSelectedSpriteObject = new HUDSpriteObject(targetSelectedSprite);
			
	    }


        /// <summary>
        /// Anything that needs to be done before enabling
        /// </summary>
        public void Enable()
	    {
	        cachedGameObject.SetActive(true);
	    }


        /// <summary>
        /// Anything that needs to be done before disabling
        /// </summary>
        public void Disable()
	    {
	        cachedGameObject.SetActive(false);
	    }

	 
		/// <summary>
        /// Update the widget with the parameters passed from the 3D radar.
        /// </summary>
        /// <param name="widgetParameters">The parameters for thus widget.</param>
		public void Set(Radar3D_WidgetParameters widgetParameters){
			
			// Position
			iconSpriteObject.CachedTransform.localPosition = widgetParameters.widgetLocalPosition;
	
			// Show selected or not
			if (widgetParameters.isSelected)
			{
				targetSelectedSpriteObject.CachedGameObject.SetActive(true);
			}
			else
			{
                targetSelectedSpriteObject.CachedGameObject.SetActive(false);
			}
	
			// Position and size the leg of the 3D radar widget			
			legLineRenderer.SetPosition(0, widgetParameters.widgetLocalPosition);
			legLineRenderer.SetPosition(1, widgetParameters.widgetLocalPosition + new Vector3(0f, -widgetParameters.widgetLocalPosition.y, 0f));
	
	        // Foot
	        footSpriteObject.CachedTransform.localPosition = widgetParameters.widgetLocalPosition + new Vector3(0f, -widgetParameters.widgetLocalPosition.y, 0f);

			// Get the color
			Color col = widgetParameters.widgetColor;
			col.a = widgetParameters.alpha;

            // Set the colors
			iconSpriteObject.CachedSpriteRenderer.color = col;
	        footSpriteObject.CachedSpriteRenderer.color = col;
			targetSelectedSpriteObject.CachedSpriteRenderer.color = col;
			legMaterial.SetColor("_Color", col);
			
		}
	}
}
