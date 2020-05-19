using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System;
using VSX.UniversalVehicleCombat;


namespace VSX.UniversalVehicleCombat
{
    /// <summary>
    /// This class modifies the inspector of the HUDTargetTracking class so that the user can set style information for different teams in
    /// the Team enum.
    /// </summary>
	[CustomEditor(typeof(HUDTargetTracking))]
	public class HUDTargetTrackingEditor : Editor
    {
	
		HUDTargetTracking script;
		
		// General settings

		SerializedProperty hudCameraProperty;
		SerializedProperty uiViewportCoefficientsProperty;
		SerializedProperty useMeshBoundsCenterProperty;
		SerializedProperty enableAspectRatioProperty;
		SerializedProperty centerOffscreenArrowsProperty;
		SerializedProperty centerOffscreenArrowsRadiusProperty;
		SerializedProperty fadeMinAlphaProperty;
		SerializedProperty fadeMaxAlphaProperty;
		SerializedProperty maxNewTargetsEachFrameProperty;
		SerializedProperty expandingTargetBoxesProperty;

		// World space settings

		SerializedProperty useTargetWorldPositionsProperty;
		SerializedProperty worldSpaceVisorDistanceProperty;
		SerializedProperty worldSpaceScaleCoefficientProperty;


		void OnEnable()
		{

			script = (HUDTargetTracking)target;
	
			
			// General settings

			hudCameraProperty = serializedObject.FindProperty("UICamera");
			uiViewportCoefficientsProperty = serializedObject.FindProperty("UIViewportCoefficients");
			useMeshBoundsCenterProperty = serializedObject.FindProperty("useMeshBoundsCenter");
			enableAspectRatioProperty = serializedObject.FindProperty("enableAspectRatio");
			centerOffscreenArrowsProperty = serializedObject.FindProperty("centerOffscreenArrows");
			centerOffscreenArrowsRadiusProperty = serializedObject.FindProperty("centerOffscreenArrowsRadius");
			fadeMinAlphaProperty = serializedObject.FindProperty("fadeMinAlpha");
			fadeMaxAlphaProperty = serializedObject.FindProperty("fadeMaxAlpha");
			maxNewTargetsEachFrameProperty = serializedObject.FindProperty("maxNewTargetsEachFrame");
			expandingTargetBoxesProperty = serializedObject.FindProperty("expandingTargetBoxes");


			// World space settings
			
			useTargetWorldPositionsProperty = serializedObject.FindProperty("useTargetWorldPositions");
			worldSpaceVisorDistanceProperty = serializedObject.FindProperty("worldSpaceTargetTrackingDistance");
			worldSpaceScaleCoefficientProperty = serializedObject.FindProperty("worldSpaceScaleCoefficient");
			
		}

 		public override void OnInspectorGUI()
		{
	
			// Setup
			serializedObject.Update();

			string[] typeNames = Enum.GetNames(typeof(TrackableType));
			string[] teamNames = Enum.GetNames(typeof(Team));

			// Resize lists in the layout (not Repaint!) phase
			if (Event.current.type == EventType.Layout)
			{
				StaticFunctions.ResizeList(script.widgetSettingsByType, typeNames.Length);
				ResizeColorsList(script.colorByTeam, teamNames.Length, Color.green);
				serializedObject.ApplyModifiedProperties();
				serializedObject.Update();
			}


			// General settings

			EditorGUILayout.BeginVertical("box");
			EditorGUILayout.LabelField("General Settings", EditorStyles.boldLabel);

			EditorGUILayout.PropertyField(hudCameraProperty);

			EditorGUILayout.PropertyField(uiViewportCoefficientsProperty);

			EditorGUILayout.PropertyField(useMeshBoundsCenterProperty);

			EditorGUILayout.PropertyField(enableAspectRatioProperty);

			EditorGUILayout.PropertyField(centerOffscreenArrowsProperty);
			
			EditorGUILayout.PropertyField(centerOffscreenArrowsRadiusProperty);
			
			EditorGUILayout.PropertyField(fadeMinAlphaProperty);
			
			EditorGUILayout.PropertyField(fadeMaxAlphaProperty);
	
			EditorGUILayout.PropertyField(maxNewTargetsEachFrameProperty);
	
			EditorGUILayout.PropertyField(expandingTargetBoxesProperty);
			
			EditorGUILayout.EndVertical();
	
	
			// World space settings

			EditorGUILayout.BeginVertical("box");
			EditorGUILayout.LabelField("World Space Settings", EditorStyles.boldLabel);

			EditorGUILayout.PropertyField(useTargetWorldPositionsProperty);
		
			EditorGUILayout.PropertyField(worldSpaceVisorDistanceProperty);

			EditorGUILayout.PropertyField(worldSpaceScaleCoefficientProperty);

			EditorGUILayout.EndVertical();


			// Team colors
			
			EditorGUILayout.BeginVertical("box");
			EditorGUILayout.LabelField("Colors", EditorStyles.boldLabel);
			
			for (int i = 0; i < script.colorByTeam.Count; ++i)
			{

				script.colorByTeam[i] = EditorGUILayout.ColorField(teamNames[i] + " Color", script.colorByTeam[i]);
				
			}

			EditorGUILayout.EndVertical();


			// Per-type widget settings

			for (int i = 0; i < script.widgetSettingsByType.Count; ++i)
			{

				EditorGUILayout.BeginVertical("box");
				EditorGUILayout.LabelField("TrackableType " + typeNames[i] + " Visualization Settings", EditorStyles.boldLabel);

				// prefab
				script.widgetSettingsByType[i].widgetPrefab = EditorGUILayout.ObjectField("Widget Prefab", script.widgetSettingsByType[i].widgetPrefab, typeof(GameObject),false) as GameObject;


				// Visualization settings 

				script.widgetSettingsByType[i].showOffScreenTargets = EditorGUILayout.Toggle("Show Off Screen Targets", script.widgetSettingsByType[i].showOffScreenTargets);
	
				script.widgetSettingsByType[i].fadeUnselectedByDistance = EditorGUILayout.Toggle("Fade By Distance", script.widgetSettingsByType[i].fadeUnselectedByDistance);

				// Visible parameters
				script.widgetSettingsByType[i].showLabelField = EditorGUILayout.Toggle("Show Label Field", script.widgetSettingsByType[i].showLabelField);
				script.widgetSettingsByType[i].showValueField = EditorGUILayout.Toggle("Show Value Field", script.widgetSettingsByType[i].showValueField);
				script.widgetSettingsByType[i].showBarField = EditorGUILayout.Toggle("Show Bar Field", script.widgetSettingsByType[i].showBarField);
				
				EditorGUILayout.Space();
				EditorGUILayout.EndVertical();

			}

			// Serialize the updated values
			EditorUtility.SetDirty(script);
			serializedObject.ApplyModifiedProperties();
			
	    }

        // Resize the Team colors list
		public static void ResizeColorsList(List<Color> colorList, int newSize, Color defaultColor)
        {
            if (colorList.Count == newSize)
                return;

            if (colorList.Count < newSize)
            {
                for (int i = 0; i < newSize - colorList.Count; ++i)
                {
                    colorList.Add(defaultColor);
                }
            }
            else
            {
                for (int i = 0; i < colorList.Count - newSize; ++i)
                {
                    //Remove the last one in the list
                    colorList.RemoveAt(colorList.Count - 1);
                    --i;
                }
            }
        }
    }
}
