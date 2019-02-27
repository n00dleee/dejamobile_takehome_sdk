using System;
using System.Collections.Generic;
using System.Text;

namespace dejamobile_takehome_sdk
{
    public class Config
    {
        public bool autoReconnectOnTokenExpirtion = false;

        public Config(bool autoReconnectOnTokenExpirtion)
        {
            this.autoReconnectOnTokenExpirtion = autoReconnectOnTokenExpirtion;
        }
    }
}
