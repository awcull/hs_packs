using System;
using System.Linq;
using System.IO;

namespace hs_packs
{
	class MainClass
	{
		public static void runSim(int val, int numSims, string fileName) {
			PackSet packSim = new PackSet(val);
			packSim.Simulate(numSims);
			int[] numPacks = packSim.getPackArray();
			File.WriteAllLines(@fileName, numPacks.Select(d => d.ToString()).ToArray());
		}


		public static void Main (string[] args)
		{
			int numSims = 100000;
			runSim (0, numSims, "/Users/polar/expert.txt");
			runSim (1, numSims, "/Users/polar/gvg.txt");
			runSim (2, numSims, "/Users/polar/tgt.txt");

		}
	}
}
