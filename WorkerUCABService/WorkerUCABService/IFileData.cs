using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WorkerUCABService
{
    public interface IFileData
    {

        public Task QueryDatabase();

    }
}
