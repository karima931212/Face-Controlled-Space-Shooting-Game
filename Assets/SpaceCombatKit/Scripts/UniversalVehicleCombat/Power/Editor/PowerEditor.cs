using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System;
using VSX.UniversalVehicleCombat;

namespace VSX.UniversalVehicleCombat 
{

	/// <summary>
    /// This editor script modifies the inspector of the Power component to display a set of configurable parameters for each
    /// of the values defined in the PoweredSubsystem enum.
    /// </summary>
	[CustomEditor(typeof(Power))]
	public class PowerEditor : Editor 
	{
		
		Power script;

		
		void OnEnable()
		{
			script = (Power)target;	
		}

        
 		public override void OnInspectorGUI()
		{
	
			// Setup
			serializedObject.Update();

			// Resize the list of PoweredSubsystem instances depending on the PoweredSubsystemType
			string[] subsystemTypeNames = Enum.GetNames(typeof(SubsystemType));
            SubsystemType[] subsystemValues = (SubsystemType[])Enum.GetValues(typeof(SubsystemType));
			
			// Resize lists in the layout (not Repaint!) phase
			if (Event.current.type == EventType.Layout)
			{
				StaticFunctions.ResizeList(script.poweredSubsystems, subsystemTypeNames.Length);
				serializedObject.ApplyModifiedProperties();
				serializedObject.Update();
			}
			
			// General settings
			float totalFixedPower = 0;
			float totalDistributablePowerFraction = 0;

			for (int i = 0; i < script.poweredSubsystems.Count; ++i)
			{
				EditorGUILayout.Space();

				EditorGUILayout.BeginVertical("box");
			
				script.poweredSubsystems[i].type = subsystemValues[i];
				EditorGUILayout.LabelField(script.poweredSubsystems[i].type.ToString(), EditorStyles.boldLabel);

				script.poweredSubsystems[i].powerConfiguration = (SubsystemPowerConfiguration)EditorGUILayout.EnumPopup("Power Configuration", script.poweredSubsystems[i].powerConfiguration);

				if (script.poweredSubsystems[i].powerConfiguration == SubsystemPowerConfiguration.Collective ||
				    script.poweredSubsystems[i].powerConfiguration == SubsystemPowerConfiguration.Independent)
				{
					if (script.poweredSubsystems[i].powerConfiguration == SubsystemPowerConfiguration.Collective)
					{
	
						script.poweredSubsystems[i].fixedPowerFraction = EditorGUILayout.FloatField("Fixed Power Fraction", script.poweredSubsystems[i].fixedPowerFraction);
						script.poweredSubsystems[i].fixedPowerFraction = Mathf.Clamp(script.poweredSubsystems[i].fixedPowerFraction, 0f, 1 - totalFixedPower);
						totalFixedPower += script.poweredSubsystems[i].fixedPowerFraction;

						script.poweredSubsystems[i].defaultDistributablePowerFraction = EditorGUILayout.FloatField("Default Distributable Power Fraction", script.poweredSubsystems[i].defaultDistributablePowerFraction);
						script.poweredSubsystems[i].defaultDistributablePowerFraction = Mathf.Clamp(script.poweredSubsystems[i].defaultDistributablePowerFraction, 0f, 1 - totalDistributablePowerFraction);
						totalDistributablePowerFraction += script.poweredSubsystems[i].defaultDistributablePowerFraction;
						
							
					}
					else if (script.poweredSubsystems[i].powerConfiguration == SubsystemPowerConfiguration.Independent)
					{
						script.poweredSubsystems[i].independentPowerOutput = EditorGUILayout.FloatField("Independent Power Output", script.poweredSubsystems[i].independentPowerOutput);
						script.poweredSubsystems[i].fixedPowerFraction = script.poweredSubsystems[i].independentPowerOutput;
					}

					


					EditorGUILayout.BeginVertical("box");
					script.poweredSubsystems[i].rechargePowerFraction = EditorGUILayout.Slider("", script.poweredSubsystems[i].rechargePowerFraction, 0f, 1f);

					EditorGUILayout.BeginHorizontal();
					GUILayout.Label("Direct");
					float val = (1 - script.poweredSubsystems[i].rechargePowerFraction) * script.poweredSubsystems[i].fixedPowerFraction;
					GUILayout.Label(val.ToString("F1"));
					TextAnchor defaultAlignment = GUI.skin.label.alignment;
					GUI.skin.label.alignment = TextAnchor.UpperRight;
					GUILayout.Label("Recharge");
					val = (script.poweredSubsystems[i].rechargePowerFraction) * script.poweredSubsystems[i].fixedPowerFraction;
					GUILayout.Label(val.ToString("F1"));
					GUI.skin.label.alignment = defaultAlignment;
					EditorGUILayout.EndHorizontal();
					EditorGUILayout.EndVertical();

					script.poweredSubsystems[i].maxRechargeRate = EditorGUILayout.FloatField("Max Recharge Rate", script.poweredSubsystems[i].maxRechargeRate);

					script.poweredSubsystems[i].storageCapacity = EditorGUILayout.FloatField("Storage Capacity", script.poweredSubsystems[i].storageCapacity);
				}

				if (script.poweredSubsystems[i].powerConfiguration != SubsystemPowerConfiguration.Collective)
				{
					script.poweredSubsystems[i].distributablePowerFraction = 0;
				}

				EditorGUILayout.EndVertical();
	
			}

			// Serialize the updated values
			EditorUtility.SetDirty(script);
			serializedObject.ApplyModifiedProperties();

			
		}
	}
}
