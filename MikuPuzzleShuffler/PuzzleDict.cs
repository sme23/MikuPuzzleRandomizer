using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MikuPuzzleShuffler
{
    class PuzzleDict
    {
        // dict of puzzle ID mapped to puzzle string
        public static Dictionary<String, String> puzzleDict = new Dictionary<String, String>();
        
        PuzzleDict()
        {
            //puzzleDict.Add("%KEY%", new Tuple<Int32, Int32>(%NAMEOFFSET%,%PUZZLEOFFSET%));

            // the very first puzzle is at 0x63641C in the resources.assets file
            // they're all more or less adjacent, though there's some stuff intermixed we'll deal with when we get to
            // we can just go through linearly, reading the name string, skipping ahead exactly 15 bytes from the start of the name string, and inserting the CSV
            // then at the position at the end of inserting the CSV is some padding and then the next name string, where we repeat
            // we can write to this dict as String,String pairs as we go along randomizing things
            // no need to hardcode the address of each puzzle within the file
        }

    }

}
