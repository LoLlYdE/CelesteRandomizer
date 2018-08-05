using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Celeste.Mod.CelesteRandomizer {
    public class RandomizerModuleSettings : EverestModuleSettings {

        public bool Enabled { get; set; } = false;

        public bool EndScreenAsLast { get; set; } = false;

    }
}
