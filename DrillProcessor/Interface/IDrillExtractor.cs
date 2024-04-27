using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DrillProcessor.Interface
{
    public interface IDrillExtractor
    {
        List<RawPerformer> Extract(string file);
    }
}
