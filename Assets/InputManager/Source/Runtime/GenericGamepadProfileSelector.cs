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
using System;
using System.Collections.Generic;

namespace Luminosity.IO
{
    [RequireComponent(typeof(GenericGamepadStateAdapter))]
    public class GenericGamepadProfileSelector : GamepadProfileSelector
    {
        public enum QueryType
        {
            ExactMatch, StartsWith, EndsWith, Contains
        }

        [Serializable]
        public class Query
        {
            public QueryType Type = QueryType.Contains;
            public string Content = "";
        }

        [Serializable]
        public class Profile
        {
            public GenericGamepadProfile GamepadProfile = null;
            public List<Query> Constraints = null;
            public bool IsExpanded = true;
        }

        [SerializeField]
        private GenericGamepadProfile m_defaultProfile = null;
        [SerializeField]
        private List<Profile> m_profiles = null;

#if UNITY_EDITOR
        public Profile GetProfile(int index)
        {
            if(index >= 0 && index < m_profiles.Count)
                return m_profiles[index];

            return null;
        }
#endif
        protected override void OnAssignGamepadProfile(GamepadIndex gamepad)
        {
            if(!Adapter.IsConnected(gamepad))
                return;

            string gamepadName = Adapter.GetName(gamepad);
            for(int i = 0; i < m_profiles.Count; i++)
            {
                if(IsMatch(m_profiles[i], gamepadName))
                {
                    Adapter.SetProfile(gamepad, m_profiles[i].GamepadProfile);
                    Debug.LogFormat("Profile '{0}' assigned to '{1}'.", m_profiles[i].GamepadProfile.Name, gamepadName);
                    return;
                }
            }

            if(m_defaultProfile != null)
            {
                Adapter.SetProfile(gamepad, m_defaultProfile);
                Debug.LogFormat("No profile found for '{0}'. Assigning default profile '{1}'.", gamepadName, m_defaultProfile.Name);
            }
            else
            {
                Debug.LogFormat("No profile found for '{0}'.", gamepadName);
            }
        }

        private bool IsMatch(Profile entry, string gamepadName)
        {
            for(int i = 0; i < entry.Constraints.Count; i++)
            {
                if(!IsMatch(entry.Constraints[i], gamepadName))
                    return false;
            }

            return true;
        }

        private bool IsMatch(Query query, string gamepadName)
        {
            StringComparison comparison = StringComparison.InvariantCultureIgnoreCase;

            switch(query.Type)
            {
            case QueryType.ExactMatch:
                return string.Compare(gamepadName, query.Content, comparison) == 0;
            case QueryType.StartsWith:
                return gamepadName.StartsWith(query.Content, comparison);
            case QueryType.EndsWith:
                return gamepadName.EndsWith(query.Content, comparison);
            case QueryType.Contains:
                return gamepadName.ToLowerInvariant().Contains(query.Content.ToLowerInvariant());
            default:
                return false;
            }
        }
    }
}
