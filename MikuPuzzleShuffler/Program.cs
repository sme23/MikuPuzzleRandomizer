using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Collections;

namespace MikuPuzzleShuffler
{
    class Program
    {
        static void Main(string[] args)
        {
            //starting off, the seed string we get passed as an arg
            string seed = "0";
            if (args.Length > 0) seed = args[0];

            //this rng object is going to be used for everything from here on out, initialized with the seed
            Random rng = new Random(Int32.Parse(seed));

            //here our goal is to parse CSVs and insert them overtop existing CSVs embedded in a binary file
            //for one, we'll need a dict of file offsets and the associated puzzles
            //by internal name, so N1-N3_XXX and S1-S4_XXX
            //then we replace each of them with a different puzzle of the same size pulled from a CSV file

            //for now we're limited to X puzzles of each size and just normal mode puzzles, so that's
            // 5 5x5 puzzles
            // 40 10x10 puzzles
            // 150 15x15 puzzles
            // 150 20x20 puzzles

            //we want to get a list of each size from the files in the folder for each size at random with no repeats
            //these are always located relative to this program at Puzzles/5x5, Puzzles/10x10, etc.

            string baseFilepath = Directory.GetCurrentDirectory() + "\\Puzzles\\";

            //get all puzzle filenames of each size
            ArrayList puzzles5x5 = GetCSVsInDirectory(baseFilepath+"5x5\\");
            ArrayList puzzles10x10 = GetCSVsInDirectory(baseFilepath + "10x10\\");
            ArrayList puzzles15x15 = GetCSVsInDirectory(baseFilepath + "15x15\\");
            ArrayList puzzles20x20 = GetCSVsInDirectory(baseFilepath + "20x20\\");

            //with how this is currently implemented, these values are constants
            const int num5x5Puzzles = 5;
            const int num10x10Puzzles = 40;
            const int num15x15Puzzles = 150;
            const int num20x20Puzzles = 150;

            //get ArrayLists of the same size as each puzzle list of random numbers 
            ArrayList puzzles5x5rand   = GenerateNUniqueRandomIntegersInRange(num5x5Puzzles,   0, puzzles5x5.Count   - 1, rng);
            ArrayList puzzles10x10rand = GenerateNUniqueRandomIntegersInRange(num10x10Puzzles, 0, puzzles10x10.Count - 1, rng);
            ArrayList puzzles15x15rand = GenerateNUniqueRandomIntegersInRange(num15x15Puzzles, 0, puzzles15x15.Count - 1, rng);
            ArrayList puzzles20x20rand = GenerateNUniqueRandomIntegersInRange(num20x20Puzzles, 0, puzzles20x20.Count - 1, rng);

            //parse the resources.assets file to note all of the locations of puzzle names and CSVs
            Dictionary<string, Dictionary<string, long>> puzzleDicts = ParseAssetsFileForCSVs(Directory.GetCurrentDirectory() + "\\resources.assets");
            // puzzleDicts is a collection of 5 dicts with specific names
            
            // we should actually have 300 15x15 puzzles in that dict, half of them are special mode puzzles
            // it's not possible to change their dimensions, so we aren't going to randomize them
            puzzleDicts["dict15x15"] = StripSpecialPuzzlesFromDict(puzzleDicts["dict15x15"]);

            // we now have all of the information we need to randomize puzzles: puzzle locations, indices of CSVs in the filename array to put in those locations
            FileStream file = File.OpenWrite(Directory.GetCurrentDirectory() + "\\resources.assets");
            // we write to the file by using a byte array as a buffer then doing file.Write(bufferArray, arrayStartIndex, arrayEndIndex)
            // so we want to open each CSV, read it into a string array, use a GetBytes method to convert it to a byte array, then write it at designated offset + 15

            Console.WriteLine("starting 5x5 puzzle shuffling, " + puzzleDicts["dict5x5"].Values.Count + " puzzles located");

            // starting with 5x5 puzzles
            int i = 0;
            foreach (long offset in puzzleDicts["dict5x5"].Values)
            {
                Console.WriteLine("shuffling puzzle at " + $"0x{offset:X}");
                // get the i-th value in puzzles5x5
                string puzzleCSV = (string)puzzles5x5[(int)puzzles5x5rand[i]];

                // open that file
                FileStream csv = File.OpenRead(puzzleCSV);

                Console.WriteLine("using CSV " + puzzleCSV.Substring(puzzleCSV.Length-10) + " of length " + csv.Length);

                // read it into a byte array
                byte[] csvContents = new byte[csv.Length];
                csv.Read(csvContents, 0, (int)csv.Length);

                // close that file
                csv.Close();

                // write the contents of the file to the location to the current offset + 15
                file.Position = offset + 15;
                file.Write(csvContents, 0, csvContents.Length);

                // increment i
                i++;
            }

            // close resources.assets
            file.Close();

            // and now we're done!

        }

        static ArrayList GetCSVsInDirectory(string filepath)
        {
            //given filepath, get all files in that directory
            //store them in an ArrayList, then return that
            ArrayList ret = new ArrayList();
            ret.AddRange(Directory.GetFiles(filepath, "*.csv"));
            return ret;
        }

        static ArrayList GenerateNUniqueRandomIntegersInRange(int n, int min, int max, Random rand)
        {
            //generates n unique random integers and returns them in an ArrayList
            ArrayList ret = new ArrayList();

            if (max - min + 1 == n)
            {
                //we want every number from min to max exactly once in an arraylist; don't think this is ever not true actually
                for (int i = 0; i < n; i++)
                {
                    ret.Add(i);
                }

                //now we just want to shuffle the order of this list randomly
                for (int i = ret.Count - 1; i >= 0; i--)
                {
                    int r = rand.Next(i);
                    int t = (int)ret[i];
                    ret[i] = ret[r];
                    ret[r] = t;
                }
            }
            else
            {
                for (int i = 0; i < n; i++)
                {
                    int j = rand.Next(min, max);
                    if (!ret.Contains(j))
                    {
                        ret.Add(j);
                    }
                    else
                    {
                        i--;
                    }
                }
            }
            return ret;
        }

        static Dictionary<string,Dictionary<string, long>> ParseAssetsFileForCSVs(string assetsFilepath)
        {
            //the location of the first CSV in the file, found manually and used to make sure we're doing this properly
            const int baseOffset = 0x63641C;

            FileStream file = File.OpenRead(assetsFilepath);
            file.Position = baseOffset;

            string puzzleName;
            long puzzleOffset;

            Dictionary<string, long> dict5x5 = new Dictionary<string, long>();
            Dictionary<string, long> dict10x10 = new Dictionary<string, long>();
            Dictionary<string, long> dict15x15 = new Dictionary<string, long>();
            Dictionary<string, long> dict20x20 = new Dictionary<string, long>();
            Dictionary<string, long> dictMetadata = new Dictionary<string, long>();

            // the structure of each CSV in this file is as follows:
            // - 6-byte string, filename
            // - 9 bytes of something that's the same for every file and we don't want to touch 
            // - CSV contents, dependent on size of puzzle but since they're all square we can figure it out by the number of entries before the first line break
            // - 10 bytes of something that's the same size for every file and we don't want to touch
            // At some point, we will encounter non-puzzle CSVs. We still want to note the locations of these, since they're things like puzzle sizes
            // No CSV has a width above 20 (40 characters), so we know we're done parsing when we try to get the dimensions and it's bigger than that

            while (true) //we'll have an exit condition within this loop rather than one here
            {

                puzzleOffset = file.Position;

                byte[] readBuffer = new byte[6]; 
                
                //6-byte string
                file.Read(readBuffer, 0, 6);
                puzzleName = Encoding.Default.GetString(readBuffer);
                Console.WriteLine("currently parsing " + puzzleName + " at " + $"0x{puzzleOffset:X}");

                //usually 9 bytes padding, but we can't be certain on that for dumb reasons
                //so let's do it this way
                long curLoc = file.Position;
                int length = 0;

                file.Position += 1;
                int[] b = new int[2];
                b[0] = file.ReadByte();
                b[1] = file.ReadByte();
                file.Position = curLoc;
                length = b[0] << 8 | b[1];


                int k = 0;
                int j = 0;
                while (k != 0x30 && k != 0x31)
                {
                    //Console.WriteLine("reading byte at " + $"0x{file.Position:X}");
                    k = file.ReadByte();
                    //Console.WriteLine("just read byte " + $"0x{k:X}");
                    j++;
                }
                file.Position--;
                if (j > 9) file.Position = curLoc + 9; //this should handle when it's a metadata CSV

                //this also doesn't work sometimes because the part we're skipping over has 0x30 or 0x31 in it
                //notably misses all the 5x5 puzzles
                // luckily for us, length is actually stored somewhere in this section of the file so we can consistently find it; the rest of this is fine for getting us to the next puzzle
                

                //CSV contents, read a byte at a time until we find 0xD
                int cur = 0;
                
                while (cur != 0xD)  //note that this implementation assumes that there is another byte 0xD somewhere in the file after this point
                {
                    cur = file.ReadByte();
                    
                }

                Console.WriteLine("length is " + length);

                if (length > 1000 || length == 0) break; //we're not reading a CSV, exit the loop

                switch (length) 
                {
                    case 55:
                        //this is a 5x5 puzzle
                        dict5x5.Add(puzzleName, puzzleOffset);
                        break;
                    
                    case 211:
                        //this is a 10x10 puzzle
                        dict10x10.Add(puzzleName, puzzleOffset);
                        break;
                    
                    case 210:
                        //this is a 15x15 puzzle
                        dict15x15.Add(puzzleName, puzzleOffset);
                        break;
                    
                    case 53:
                        //this is a 20x20 puzzle
                        dict20x20.Add(puzzleName, puzzleOffset);
                        break;
                    
                    default:
                        //this is metadata
                        dictMetadata.Add(puzzleName, puzzleOffset);
                        break;
                }

                //skip over the rest of the puzzle by reading until we find a 0
                //we could try to do some math to figure out the amount to skip but metadata CSVs make that inconsistent
                cur = 1;
                while (cur != 0)
                {
                    cur = file.ReadByte();
                }

                //skip over the rest of the padding at end
                int c = 0;
                while (c != 6)
                {
                    c = file.ReadByte();
                }
                file.Position += 3;

            }

            //close the file
            file.Close();

            // put all our dictionaries into another dictionary and return that
            Dictionary<string, Dictionary<string, long>> retDict = new Dictionary<string, Dictionary<string, long>>();
            retDict.Add("dict5x5", dict5x5);
            retDict.Add("dict10x10", dict10x10);
            retDict.Add("dict15x15", dict15x15);
            retDict.Add("dict20x20", dict20x20);
            retDict.Add("dictMetadata", dictMetadata);

            return retDict;

        }

        static Dictionary<string, long> StripSpecialPuzzlesFromDict(Dictionary<string,long> dict)
        {
            //any puzzle whose name begins with S gets removed
            Dictionary<string, long> otherDictWeNeedBecauseFunctionIsStatic = new Dictionary<string, long>();
            
            foreach (KeyValuePair<string,long> pair in dict) {
                otherDictWeNeedBecauseFunctionIsStatic.Add(pair.Key, pair.Value);
            }

            foreach (string key in dict.Keys)
            {
                if (key.StartsWith("S")) otherDictWeNeedBecauseFunctionIsStatic.Remove(key);
            }

            return otherDictWeNeedBecauseFunctionIsStatic;

        }

    }
}
