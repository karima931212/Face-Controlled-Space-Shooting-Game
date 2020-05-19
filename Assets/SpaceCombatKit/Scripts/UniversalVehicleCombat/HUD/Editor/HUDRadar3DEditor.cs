using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System;
using VSX.UniversalVehicleCombat;

// This script is to modify the inspector of the HUDRadar3D so that the user can set different parameters
// for displaying targets of different types

namespace VSX.UniversalVehicleCombat
{
    /// <summary>
    /// This class modifies the inspector of the Radar3D class so that the user can set style information for different teams in
    /// the Team enum.
    /// </summary>
	[CustomEditor(typeof(HUDRadar3D))]
	public class HUDRadar3DEditor : Editor
    {
		
		HUDRadar3D script;

        SerializedProperty equatorRadiusProperty;
        SerializedProperty scaleExponentProperty;
        SerializedProperty zoomSpeedProperty;
        SerializedProperty fadeMinAlphaProperty;
        SerializedProperty fadeMaxAlphaProperty;
        SerializedProperty maxNewTargetsEachFrameProperty;


		void OnEnable()
		{
			script = (HUDRadar3D)target;

            equatorRadiusProperty = serializedObject.FindProperty("equatorRadius");
            scaleExponentProperty = serializedObject.FindProperty("scaleExponent");
            zoomSpeedProperty = serializedObject.FindProperty("zoomSpeed");
            fadeMinAlphaProperty = serializedObject.FindProperty("fadeMinAlpha");
            fadeMaxAlphaProperty = serializedObject.FindProperty("fadeMaxAlpha");
            maxNewTargetsEachFrameProperty = serializedObject.FindProperty("maxNewTargetsEachFrame");

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
				StaticFunctions.ResizeList(script.colorByTeam, teamNames.Length);
				
				serializedObject.ApplyModifiedProperties();
				serializedObject.Update();
			}


			// Settings

			EditorGUILayout.BeginVertical("box");
			EditorGUILayout.LabelField("Settings", EditorStyles.boldLabel);

            EditorGUILayout.PropertyField(equatorRadiusProperty);

            EditorGUILayout.PropertyField(scaleExponentProperty);

            EditorGUILayout.PropertyField(zoomSpeedProperty);

            EditorGUILayout.PropertyField(fadeMinAlphaProperty);

            EditorGUILayout.PropertyField(fadeMaxAlphaProperty);

            EditorGUILayout.PropertyField(maxNewTargetsEachFrameProperty);

			EditorGUILayout.EndVertical();


			// Per-team colors
			EditorGUILayout.BeginVertical("box");
			EditorGUILayout.LabelField("Colors", EditorStyles.boldLabel);
			
			for (int i = 0; i < script.colorByTeam.Count; ++i)
			{				
				script.colorByTeam[i] = EditorGUILayout.ColorField(teamNames[i] + " Color", script.colorByTeam[i]);				
			}

			EditorGUILayout.EndVertical();
			

			// Per-type settings

			for (int i = 0; i < script.widgetSettingsByType.Count; ++i)
			{
				EditorGUILayout.BeginVertical("box");
				EditorGUILayout.LabelField("TrackableType " + typeNames[i] + " Visualization Settings", EditorStyles.boldLabel);

				script.widgetSettingsByType[i].ignore = EditorGUILayout.Toggle("Ignore", script.widgetSettingsByType[i].ignore);

				script.widgetSettingsByType[i].fadeUnselectedByDistance = EditorGUILayout.Toggle("Fade Unselected By Distance", script.widgetSettingsByType[i].fadeUnselectedByDistance);
		
				script.widgetSettingsByType[i].widgetPrefab = EditorGUILayout.ObjectField("Widget Prefab", script.widgetSettingsByType[i].widgetPrefab, typeof(GameObject), false) as GameObject;
            	
				EditorGUILayout.Space();
				EditorGUILayout.EndVertical();
			}
			
			// serialize the updated values
			EditorUtility.SetDirty(script);
			serializedObject.ApplyModifiedProperties();
	    }
	}
}
