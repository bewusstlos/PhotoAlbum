using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cloud
{
    class CloudManage
    {
        public CloudManage()
        {
            using (var redis = new Redis("127.0.0.1"))
            {

            }
        }
    }
}
