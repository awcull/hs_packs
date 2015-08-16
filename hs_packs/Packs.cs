using System;

namespace hs_packs
{
	public class Packs
	{
		private float common;
		private float rare;
		private float epic;
		private float legendary;
		private float gold;

		// Stores number of cars
		private int commonCards;
		private int rareCards;
		private int epicCards;
		private int legCards;

		public Packs ()
		{
			// Probability of card [0,100] %
			common = 50.0;
			rare = 9;
			epic = 9;
			legendary = 1.01;
			gold = 5;

			// Set up number of cards as basic set
			commonCards = 300;
			rareCards = 150;
			epicCards = 100;
			legCards = 75;
		}


	}
}

