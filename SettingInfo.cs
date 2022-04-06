using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ForPHP {
    public class SettingInfo {
        public string PHPFilePath { get; set; }
        public string ServerAddr { get; set; }

        public SettingInfo() {
            PHPFilePath = "";
            ServerAddr = "";
        }
    }
}
