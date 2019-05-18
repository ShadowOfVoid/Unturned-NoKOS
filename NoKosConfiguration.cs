using Rocket.API;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace Wth.NoKos
{
    public class NoKosConfiguration : IRocketPluginConfiguration
    {
        public int noKOSprotectionTime;
        public string messageColor;

        public void LoadDefaults()
        {
            messageColor = "cyan";

            noKOSprotectionTime = 10;
        }
    }
}
