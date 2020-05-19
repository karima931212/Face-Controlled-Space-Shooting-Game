using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System;
using VSX.UniversalVehicleCombat;


namespace VSX.UniversalVehicleCombat
{
    /// <summary>
    /// This class modifies the inspector of the HUDHologram class so that the user can set style information for different teams in
    /// the Team enum.
    /// </summary>
	[CustomEditor(typeof(HUDHologram))]
	public class HUDHologramEditor : Editor
    {

        HUDHologram script;

        SerializedProperty targetHologramProperty;


        private void OnEnable()
        {
            script = (HUDHologram)target;

            targetHologramProperty = serializedObject.FindProperty("targetHologram");
        }

        public override void OnInspectorGUI()
		{

			// Setup
			serializedObject.Update();
			
			string[] teamNames = Enum.GetNames(typeof(Team));
	                
			StaticFunctions.ResizeList(script.colorByTeam, teamNames.Length);
			
			EditorGUILayout.BeginVertical("box");
			EditorGUILayout.LabelField("Settings", EditorStyles.boldLabel);

            EditorGUILayout.PropertyField(targetHologramProperty);
			
			EditorGUILayout.EndVertical();


			EditorGUILayout.BeginVertical("box");
			EditorGUILayout.LabelField("Colors", EditorStyles.boldLabel);
			
			for (int i = 0; i < script.colorByTeam.Count; ++i)
			{
				script.colorByTeam[i] = EditorGUILayout.ColorField(teamNames[i] + " Color", script.colorByTeam[i]);			
			}

			EditorGUILayout.EndVertical();

			EditorUtility.SetDirty(script);
			
			serializedObject.ApplyModifiedProperties();
	    }
	}
}
