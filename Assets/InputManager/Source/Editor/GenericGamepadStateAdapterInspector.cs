#region [Copyright (c) 2018 Cristian Alexandru Geambasu]
//	Distributed under the terms of an MIT-style license:
//
//	The MIT License
//
//	Copyright (c) 2018 Cristian Alexandru Geambasu
//
//	Permission is hereby granted, free of charge, to any person obtaining a copy of this software 
//	and associated documentation files (the "Software"), to deal in the Software without restriction, 
//	including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, 
//	and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, 
//	subject to the following conditions:
//
//	The above copyright notice and this permission notice shall be included in all copies or substantial 
//	portions of the Software.
//
//	THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, 
//	INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR 
//	PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE
//	FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, 
//	ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
#endregion
using UnityEngine;
using UnityEditor;
using Luminosity.IO;

namespace LuminosityEditor.IO
{
    [CustomEditor(typeof(GenericGamepadStateAdapter))]
    public class GenericGamepadStateAdapterInspector : Editor
    {
        private SerializedProperty m_gamepadOne;
        private SerializedProperty m_gamepadTwo;
        private SerializedProperty m_gamepadThree;
        private SerializedProperty m_gamepadFour;
        private SerializedProperty m_joystickCheckFrequency;
        private SerializedProperty m_triggerGravity;
        private SerializedProperty m_triggerSensitivity;
        private SerializedProperty m_dpadGravity;
        private SerializedProperty m_dpadSensitivity;
        private SerializedProperty m_dpadSnap;
        private SerializedProperty m_ignoreTimescale;
        private SerializedProperty m_useSharedProfile;
        private static string[] m_profileModeOptions = new string[] { "Shared", "Independent" };

        private void OnEnable()
        {
            m_gamepadOne = serializedObject.FindProperty("m_gamepadOne");
            m_gamepadTwo = serializedObject.FindProperty("m_gamepadTwo");
            m_gamepadThree = serializedObject.FindProperty("m_gamepadThree");
            m_gamepadFour = serializedObject.FindProperty("m_gamepadFour");
            m_joystickCheckFrequency = serializedObject.FindProperty("m_joystickCheckFrequency");
            m_triggerGravity = serializedObject.FindProperty("m_triggerGravity");
            m_triggerSensitivity = serializedObject.FindProperty("m_triggerSensitivity");
            m_dpadGravity = serializedObject.FindProperty("m_dpadGravity");
            m_dpadSensitivity = serializedObject.FindProperty("m_dpadSensitivity");
            m_dpadSnap = serializedObject.FindProperty("m_dpadSnap");
            m_ignoreTimescale = serializedObject.FindProperty("m_ignoreTimescale");
            m_useSharedProfile = serializedObject.FindProperty("m_useSharedProfile");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            if(!HasCustomSelector())
            {
                EditorGUILayout.Space();
                EditorGUILayout.LabelField("PROFILES", EditorStyles.boldLabel);
                m_useSharedProfile.boolValue = EditorGUILayout.Popup("Asignment", m_useSharedProfile.boolValue ? 0 : 1, m_profileModeOptions) == 0;
                if(m_useSharedProfile.boolValue)
                {
                    DrawSharedGamepadField();
                }
                else
                {
                    EditorGUILayout.PropertyField(m_gamepadOne);
                    EditorGUILayout.PropertyField(m_gamepadTwo);
                    EditorGUILayout.PropertyField(m_gamepadThree);
                    EditorGUILayout.PropertyField(m_gamepadFour);
                }
            }

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("PROPERTIES", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(m_joystickCheckFrequency);
            EditorGUILayout.PropertyField(m_triggerGravity);
            EditorGUILayout.PropertyField(m_triggerSensitivity);
            EditorGUILayout.PropertyField(m_dpadGravity);
            EditorGUILayout.PropertyField(m_dpadSensitivity);
            EditorGUILayout.PropertyField(m_dpadSnap);
            EditorGUILayout.PropertyField(m_ignoreTimescale);

            serializedObject.ApplyModifiedProperties();
        }

        private void DrawSharedGamepadField()
        {
            EditorGUILayout.PropertyField(m_gamepadOne);
            CopyProfile(m_gamepadOne, m_gamepadTwo);
            CopyProfile(m_gamepadOne, m_gamepadThree);
            CopyProfile(m_gamepadOne, m_gamepadFour);
        }

        private void CopyProfile(SerializedProperty source, SerializedProperty destination)
        {
            destination.objectReferenceValue = source.objectReferenceValue;
            destination.objectReferenceInstanceIDValue = source.objectReferenceInstanceIDValue;
        }

        private bool HasCustomSelector()
        {
            return ((Component)target).gameObject.GetComponent<GamepadProfileSelector>() != null;
        }
    }
}
