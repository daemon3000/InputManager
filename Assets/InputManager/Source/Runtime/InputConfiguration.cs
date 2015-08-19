#region [Copyright (c) 2015 Cristian Alexandru Geambasu]
//	Distributed under the terms of an MIT-style license:
//
//	The MIT License
//
//	Copyright (c) 2015 Cristian Alexandru Geambasu
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
using System.Collections;
using System.Collections.Generic;

namespace TeamUtility.IO
{
	[Serializable]
	public sealed class InputConfiguration
	{
		/// <summary>
		/// Do not change the name of an input configuration at runtime because it will invalidate the lookup tables.
		/// </summary>
		public string name;
		public List<AxisConfiguration> axes;
		public bool isExpanded;
		
		public InputConfiguration() :
			this("New Configuration") { }
		
		public InputConfiguration(string name)
		{
			axes = new List<AxisConfiguration>();
			this.name = name;
			isExpanded = false;
		}
		
		public static InputConfiguration Duplicate(InputConfiguration source)
		{
			InputConfiguration inputConfig = new InputConfiguration();
			inputConfig.name = source.name;
			
			inputConfig.axes = new List<AxisConfiguration>(source.axes.Count);
			for(int i = 0; i < source.axes.Count; i++)
			{
				inputConfig.axes.Add(AxisConfiguration.Duplicate(source.axes[i]));
			}
			
			return inputConfig;
		}
	}
}