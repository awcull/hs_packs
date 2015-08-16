using System;

namespace hs_packs
{
	public class Packs
	{
		public float common;
		public float rare;
		public float epic;
		public float legendary;
		public float gold;
		

		public Packs ()
		{
			Prob cardProb = new Prob ();
			common = 50.0;
			rare = 9;
			epic = 9;
			legendary = 1.01;
			gold = 5;
		}


	}
}

