using System;
using System.Linq;

namespace hs_packs
{
	public class PackSet
	{
		// Probabilities 0-1 of opening a card
		private double cProb;
		private double rProb;
		private double eProb;
		private double lProb;
		// gold probabilities
		private double gcProb;
		private double grProb;
		private double geProb;
		private double glProb;
		// cummulative probability
		private double[] cumProb = new double[4];

		// Total number of cards
		private int commonCards;
		private int rareCards;
		private int epicCards;
		private int legCards;

		// Stores cards, if we get multiple then we know to disenchant
		private int[] cCardArray;
		private int[] rCardArray;
		private int[] eCardArray;
		private int[] lCardArray;

		// Total dust
		private int totalDust = 0;

		// number of packs 'opened'
		private int[] numPacks;

		// Disenchant dust value, reg and gold version
		// http://hearthstone.gamepedia.com/Crafting
		private int cDisDust = 5;
		private int gcDisDust = 50;
		private int rDisDust = 20;
		private int grDisDust = 100;
		private int eDisDust = 100;
		private int geDisDust = 400;
		private int lDisDust = 400;
		private int glDisDust = 1600;

		// Craft cost of cards in dust
		// http://hearthstone.gamepedia.com/Crafting
		private int cCraftDust = 40;
		private int rCraftDust = 100;
		private int eCraftDust = 400;
		private int lCraftDust = 1600;

		// Random number generator for opening packs
		Random ranNum = new Random ();

		// Set probabilities, data taken from gamepedia
		private void createProb ()
		{
			/* Probability of card [0,100] %
			 * http://hearthstone.gamepedia.com/Card_pack_statistics
 			 * Using total cards from both meta study, 12759 packs
 			 * http://hearthstone.gamepedia.com/Card_pack 
 			*/
			int totalPacks = 12759 * 5; // 5 cards per pack
			cProb = 45565.0 / totalPacks;
			rProb = 14545.0 / totalPacks;
			eProb = 2920.0 / totalPacks;
			lProb = 765.0 / totalPacks;

			// Produce cummulative probability
			cumProb [0] = cProb;
			cumProb [1] = cProb + rProb;
			cumProb [2] = cProb + rProb + eProb;
			cumProb [3] = cProb + rProb + eProb + lProb;
			//Console.WriteLine ("Common: {0}\nRare: {1}\nEpic: {2}\nLegendary: {3}", cumProb [0], cumProb [1], cumProb [2], cumProb [3]);
			/* Gold probabilities from Meta-study
			 * http://hearthstone.gamepedia.com/Card_pack_statistics
			*/
			gcProb = 1.47 / 71.42;
			grProb = 1.38 / 22.80;
			geProb = 0.31 / 4.58;
			glProb = 0.11 / 1.2;
			/*
			gcProb = 1.47 / 100;
			grProb = 1.38 / 100;
			geProb = 0.31 / 100;
			glProb = 0.11 / 100;
			*/
		}

		// Card count for Expert set
		private void expertSet ()
		{
			// Set up number of cards as expert set
			commonCards = 94;
			rareCards = 81;
			epicCards = 37;
			legCards = 33;
		}
		// end of expertSet()

		// Card count for Goblins vs Gnomes set
		private void gvgSet ()
		{
			// Set up number of cards in GvG set
			commonCards = 39;
			rareCards = 37;
			epicCards = 26;
			legCards = 21;
		}
		// end of gvgSet()

		// Card count for The Grand Tournament set
		private void tgtSet ()
		{
			// Set up number of cards in GvG set
			commonCards = (3*9)+22;
			rareCards = 9+(3*9);
			epicCards = 9+(2*9);
			legCards = 10+10;
		}
		// end of tgtSet()

		// Initial setup
		public PackSet ()
		{
			// Set cards
			expertSet ();
			// Set data, card probability and num of cards
			createProb ();
			// Create arrays for card rarities
			genSetArray ();
		}
		// end of PackSet()

		// Initial setup with picking set
		public PackSet (int whichSet)
		{

			/* Which set
			 * set 1 <- classic
			 * sec 2 <- GvG
			 * set 3 <- Grand Tournament
			*/
			if (whichSet == 0) {
				expertSet ();
			} else if (whichSet == 1) {
				gvgSet ();
			} else if (whichSet == 2) {
				tgtSet ();
			}
			// Set card probabilities
			createProb ();
			// Create arrays for card rarities
			genSetArray ();
		}
		// end of PackSet(int whichSet)

		/* getSetArray
		 * This generates a empy array to store unpacked cards
		*/
		private void genSetArray ()
		{
			cCardArray = new int[commonCards];
			rCardArray = new int[rareCards];
			eCardArray = new int[epicCards];
			lCardArray = new int[legCards];
		}

		private int getSum (int[] array)
		{
			int sum = 0;
			sum = array.Sum ();
			return(sum);
		}

		/* calcDustNeeded
		 * This calculates the dust remaining to complete the set
		*/
		private int calcDustNeeded()
		{
			int dust = -1; // If want to error catch
			// Calculate total dust needed
			dust = (2 * cCardArray.Length - getSum (cCardArray)) * cCraftDust +
			(2 * rCardArray.Length - getSum (rCardArray)) * rCraftDust +
			(2 * eCardArray.Length - getSum (eCardArray)) * eCraftDust +
			(1 * lCardArray.Length - getSum (lCardArray)) * lCraftDust;
			return(dust);
		}

		/* getRarity
		 * Checks for what rarity it should be
		*/
		private int getRarity (double ran)
		{
			int rarity = 0;
			bool isFound = false;
			while (rarity < cumProb.Length && !isFound) {
				if (!(cumProb[rarity] >= ran)) {
					rarity += 1;
				} else {
					isFound = true;
				}
			}
			return(rarity);
		}

		/* SimPacks
		 * Simulate opening packs until set is complete
		 * cards are disenchanted if there are more than required amount and all
		 * gold cards are disenchanted.
		*/
		public void Simulate(int numSims = 1000)
		{
			// Setup
			numPacks = new int[numSims]; // number of packs oppened
			double ran; // hold random number
			int ranIdx = 0; // random index into the specific array
			bool isRare = false; // Is a rare or better
			int currentDust = 0;
			int rarity; // rarity of card generated
			// Simulate 
			for (int k = 0; k < numSims; k++) {
				Console.WriteLine ("Current Sim: {0}", k);
				currentDust = 0;
				// Create empy array of cards
				genSetArray();
				// Continue to open packs until a complete set
				totalDust = calcDustNeeded();
				while (currentDust < totalDust) {
					/* Simulate opening a pack
					 * The method is not exactly clear, all that is known is
					 * that at least one rare or better per pack
					 */
					//Console.WriteLine ("{0}, {1}", totalDust, currentDust);
					isRare = false;
					for (int i = 0; i < 5; i++) {
						ran = ranNum.NextDouble();
						//rarity = getRarity(ranNum.NextDouble());
						rarity = getRarity(ran);
						/* Check card rarity, if its gold then disenchant or if we have already opened 2 of
					 	* common, rare, epic disenchant the extra or 1 of legendary (only allowed 1 leg per deck
					 	* Only checking if we went through 4 cards and dont get rare or better
						*/
						/*while (!isRare && i == 4) {
							if (rarity > 0) {
								isRare = true;
							} else {
								rarity = getRarity (ranNum.NextDouble());
							}
						}*/

						if (rarity == 0) {
							if (ranNum.NextDouble() < gcProb) {
								//disenchant
								currentDust += gcDisDust;								
							} else {
								ranIdx = ranNum.Next(0, commonCards);
								// Check to see if add card to collection or disenchant
								if (cCardArray [ranIdx] >= 2) {
									currentDust += cDisDust;
								} else {
									cCardArray [ranIdx] += 1;
								}
							}
						} else if (rarity == 1) {
							isRare = true;							
							if (ranNum.NextDouble() < grProb) {
								//disenchant
								currentDust += grDisDust;
							} else {
								ranIdx = ranNum.Next(0, rareCards);
								// Check to see if add card to collection or disenchant
								if (rCardArray [ranIdx] >= 2) {
									currentDust += rDisDust;
								} else {
									rCardArray [ranIdx] += 1;
								}
							}
						} else if (rarity == 2) {
							isRare = true;							
							if (ranNum.NextDouble() < geProb) {
								//disenchant
								currentDust += geDisDust;								
							} else {
								ranIdx = ranNum.Next(0, epicCards);
								// Check to see if add card to collection or disenchant
								if (eCardArray [ranIdx] >= 2) {
									currentDust += eDisDust;
								} else {
									eCardArray [ranIdx] += 1;
								}
							}
						} else if (rarity == 3) {
							isRare = true;							
							if (ranNum.NextDouble() < glProb) {
								//disenchant
								currentDust += glDisDust;
							} else {								
								ranIdx = ranNum.Next(0, legCards);
								// Check to see if add card to collection or disenchant
								if (lCardArray[ranIdx] >= 1) {
									currentDust += lDisDust;
								} else {
									lCardArray[ranIdx] += 1;
								}
							}
						} else {
						
						}
					} // end of i loop simulating one pack
					// Update current dust
					totalDust = calcDustNeeded();
					numPacks[k] += 1;
				} // end of while
			//Console.WriteLine(String.Join(",", lCardArray));
			} // end of number of sims
		} // end of Simulate

		/*getPackArray
		 * Returns packs array
		 */
		public int[] getPackArray() {
			return(numPacks);
		}

	}


}

