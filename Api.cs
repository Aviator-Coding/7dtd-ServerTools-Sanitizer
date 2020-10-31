using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdminToolsSanitize
{
    public class API : IModApi
    {
        public void InitMod()
        {
            try
            {
                Log.Out("[MOD - AdminToolsSanitize] Try to apply our GamePatches");
                Patch.Game();
                Log.Out("[MOD - AdminToolsSanitize] Patching had sucess");
            }
            catch (Exception e)
            {
                Log.Error("[MOD - AdminToolsSanitize] Applying Patches where unsuccesfull Message - {0} - {1}", e.Message, e.ToString());
            }
        }
    }
}
