using DrillProcessor.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace DrillProcessor
{
    public class PywareDrillExtractor : IDrillExtractor
    {
        public List<RawPerformer> Extract(string file)
        {
            bool doubled = HasTwoPerPage(file);
            string text = Helpers.ExtractText(file);
            Console.WriteLine(text);

            List<RawPerformer> performers = new List<RawPerformer>();
            return performers;
        }

        //unsure if Pyware can export one per page, but this check doesn't hurt anyone!
        private bool HasTwoPerPage(string file) 
        {
            string firstPage = Helpers.ExtractText(file, 1);
            return Regex.Matches(firstPage, "Performer:").Count > 1;
        }
    }
}
