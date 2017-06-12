using System.Collections.Generic;

namespace TeamUtility.IO
{
    public class SaveLoadParameters
    {
        public List<InputConfiguration> inputConfigurations;
        public string playerOneDefault;
        public string playerTwoDefault;
        public string playerThreeDefault;
        public string playerFourDefault;

		public SaveLoadParameters()
		{
			inputConfigurations = new List<InputConfiguration>();
			playerOneDefault = null;
			playerTwoDefault = null;
			playerThreeDefault = null;
			playerFourDefault = null;
		}
    }
}
