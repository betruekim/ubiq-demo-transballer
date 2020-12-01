using System;
using System.Collections.Generic;
using System.Text;

namespace Ubik.Networking
{
    namespace JmBucknall.Threading
    {
        interface ITask
        {
            void Execute();
            void Wait();
        }
    }
}
