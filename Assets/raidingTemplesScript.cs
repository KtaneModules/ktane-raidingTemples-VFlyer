using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using KModkit;
using rnd = UnityEngine.Random;

public class raidingTemplesScript : MonoBehaviour 
{
	public KMBombInfo bomb;
	public KMAudio Audio;

	readonly int SPIDERS = 0;
	readonly int ROCKS = 1;
	readonly int SNAKES = 2;
	readonly int QUICKSAND = 3;

	//Logging
	static int moduleIdCounter = 1;
    int moduleId;
    private bool moduleSolved;

	public TextMesh commonPoolText;
	public GameObject[] buttonSets;
	public KMSelectable[] explorerBtns;
	public KMSelectable skullBtn;

	int commonPool;
	int[] treasures;
	int[] hazards;
	int nExplorers;
	String[] explorers;
	int[] explorerTreasure;
	bool[] explorerInTemple;
	int[] leaveRound;
	List<int> hazardHistory;
	List<int> solution;
	List<int> pressed;
	int nextPress;

	void Awake()
	{
		moduleId = moduleIdCounter++;
		explorerBtns[0].OnInteract += delegate () { HandleExplorer(0); return false; };
        explorerBtns[1].OnInteract += delegate () { HandleExplorer(1); return false; };
        explorerBtns[2].OnInteract += delegate () { HandleExplorer(2); return false; };
		explorerBtns[3].OnInteract += delegate () { HandleExplorer(0); return false; };
        explorerBtns[4].OnInteract += delegate () { HandleExplorer(1); return false; };
        explorerBtns[5].OnInteract += delegate () { HandleExplorer(2); return false; };
        explorerBtns[6].OnInteract += delegate () { HandleExplorer(3); return false; };
        explorerBtns[7].OnInteract += delegate () { HandleExplorer(0); return false; };
        explorerBtns[8].OnInteract += delegate () { HandleExplorer(1); return false; };
        explorerBtns[9].OnInteract += delegate () { HandleExplorer(2); return false; };
        explorerBtns[10].OnInteract += delegate () { HandleExplorer(3); return false; };
        explorerBtns[11].OnInteract += delegate () { HandleExplorer(4); return false; };
        skullBtn.OnInteract += delegate () { HandleSkull(); return false; };
	}

	void HandleExplorer(int n)
	{
		GetComponent<KMAudio>().PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, transform);
        explorerBtns[n].AddInteractionPunch(.5f);

		if(moduleSolved || pressed.Exists(x => x == n))
			return;

		if(solution.ElementAt(nextPress) == n)
		{
			pressed.Add(n);
			nextPress++;

			if(nextPress == solution.Count())
			{
				Debug.LogFormat("[Raiding Temples #{0}] Successfully pressed button \"{1}\". Module solved.", moduleId, GetButton(n));
				moduleSolved = true;
            	GetComponent<KMBombModule>().HandlePass();
			}
			else
				Debug.LogFormat("[Raiding Temples #{0}] Successfully pressed button \"{1}\".", moduleId, GetButton(n));
		}
		else
		{
			Debug.LogFormat("[Raiding Temples #{0}] Strike! Pressed button \"{1}\" when button \"{2}\" was expected.", moduleId, GetButton(n), GetButton(solution.ElementAt(nextPress)));
            GetComponent<KMBombModule>().HandleStrike();
		}
	}

	void HandleSkull()
	{
		GetComponent<KMAudio>().PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, transform);
        skullBtn.AddInteractionPunch(.5f);

		if(moduleSolved)
			return;

		if(solution.ElementAt(nextPress) == -1)
		{
			Debug.LogFormat("[Raiding Temples #{0}] Successfully pressed button \"{1}\". Module solved.", moduleId, GetButton(-1));
			moduleSolved = true;
			GetComponent<KMBombModule>().HandlePass();
		}
		else
		{
			Debug.LogFormat("[Raiding Temples #{0}] Strike! Pressed button \"{1}\" when button \"{2}\" was expected.", moduleId, GetButton(-1), GetButton(solution.ElementAt(nextPress)));
            GetComponent<KMBombModule>().HandleStrike();
		}
	}

	void Start () 
	{
		foreach(GameObject set in buttonSets) set.SetActive(false);
		CalcStartCommonPool();
		CalcExplorers();
		CalcRounds();
		CalcExploration();
		CalcButtonPresses();
	}
	
	void CalcStartCommonPool()
	{
		commonPool = rnd.Range(0, 6) + rnd.Range(0, 6);
        commonPoolText.text = commonPool + "";
		Debug.LogFormat("[Raiding Temples #{0}] Starting common pool is {1} treasure.", moduleId, commonPool);
	}

	void CalcExplorers()
	{
		nExplorers = rnd.Range(0, 3) + 3;
		explorers = new String[nExplorers];

		buttonSets[nExplorers - 3].SetActive(true);

		List<String> explorerOrder = new String[] {"Indiana", "Francis", "Robert", "Clara", "Sandy", "Nate", "Allan", "Carlos", "Shelley", "Michael", "Trini"}.ToList();
		List<Indicator> indicatorOrder = new Indicator[] {Indicator.BOB, Indicator.CAR, Indicator.CLR, Indicator.FRK, Indicator.FRQ, Indicator.IND, Indicator.MSA, Indicator.NSA, Indicator.SIG, Indicator.SND, Indicator.TRN}.ToList();

		for(int i = 0; i < nExplorers; i++)
		{
			while(indicatorOrder.Count() != 0 && !bomb.IsIndicatorPresent(indicatorOrder[0]))
				indicatorOrder.Remove(indicatorOrder[0]);

			if(indicatorOrder.Count() != 0)
			{
				switch(indicatorOrder[0])
				{
					case Indicator.BOB:	explorers[i] = "Robert"; break;
					case Indicator.CAR:	explorers[i] = "Carlos"; break;
					case Indicator.CLR:	explorers[i] = "Clara"; break;
					case Indicator.FRK:	explorers[i] = "Francis"; break;
					case Indicator.FRQ:	explorers[i] = "Allan"; break;
					case Indicator.IND:	explorers[i] = "Indiana"; break;
					case Indicator.MSA:	explorers[i] = "Michael"; break;
					case Indicator.NSA:	explorers[i] = "Nate"; break;
					case Indicator.SIG:	explorers[i] = "Shelley"; break;
					case Indicator.SND:	explorers[i] = "Sandy"; break;
					case Indicator.TRN:	explorers[i] = "Trini"; break;
				}

				indicatorOrder.Remove(indicatorOrder[0]);
				explorerOrder.Remove(explorers[i]);

			}
			else
			{	
				explorers[i] = explorerOrder[0];
				explorerOrder.Remove(explorers[i]);
			}

			Debug.LogFormat("[Raiding Temples #{0}] Explorer #{1} is {2}.", moduleId, i+1, explorers[i]);
		}
	}

	void CalcRounds()
	{
		treasures = new int[6];
		hazards = new int[6];

		String sn = bomb.GetSerialNumber();

		for(int i = 0; i < sn.Length; i++)
		{
			switch(sn[i])
			{
				case 'a':
				case 'A':
				{
					treasures[i] = 1;
					hazards[i] = SPIDERS;
					break;
				}
				case 'b':
				case 'B':
				{
					treasures[i] = 13;
					hazards[i] = ROCKS;
					break;
				}
				case 'c':
				case 'C':
				{
					treasures[i] = 6;
					hazards[i] = SPIDERS;
					break;
				}
				case 'd':
				case 'D':
				{
					treasures[i] = 1;
					hazards[i] = SNAKES;
					break;
				}
				case 'e':
				case 'E':
				{
					treasures[i] = 1;
					hazards[i] = SPIDERS;
					break;
				}
				case 'f':
				case 'F':
				{
					treasures[i] = 13;
					hazards[i] = SPIDERS;
					break;
				}
				case 'g':
				case 'G':
				{
					treasures[i] = 5;
					hazards[i] = SNAKES;
					break;
				}
				case 'h':
				case 'H':
				{
					treasures[i] = 5;
					hazards[i] = SPIDERS;
					break;
				}
				case 'i':
				case 'I':
				{
					treasures[i] = 15;
					hazards[i] = SNAKES;
					break;
				}
				case 'j':
				case 'J':
				{
					treasures[i] = 2;
					hazards[i] = QUICKSAND;
					break;
				}
				case 'k':
				case 'K':
				{
					treasures[i] = 14;
					hazards[i] = SNAKES;
					break;
				}
				case 'l':
				case 'L':
				{
					treasures[i] = 3;
					hazards[i] = SNAKES;
					break;
				}
				case 'm':
				case 'M':
				{
					treasures[i] = 3;
					hazards[i] = ROCKS;
					break;
				}
				case 'n':
				case 'N':
				{
					treasures[i] = 3;
					hazards[i] = SPIDERS;
					break;
				}
				case 'o':
				case 'O':
				{
					treasures[i] = 7;
					hazards[i] = QUICKSAND;
					break;
				}
				case 'p':
				case 'P':
				{
					treasures[i] = 8;
					hazards[i] = ROCKS;
					break;
				}
				case 'q':
				case 'Q':
				{
					treasures[i] = 7;
					hazards[i] = QUICKSAND;
					break;
				}
				case 'r':
				case 'R':
				{
					treasures[i] = 16;
					hazards[i] = QUICKSAND;
					break;
				}
				case 's':
				case 'S':
				{
					treasures[i] = 8;
					hazards[i] = QUICKSAND;
					break;
				}
				case 't':
				case 'T':
				{
					treasures[i] = 11;
					hazards[i] = QUICKSAND;
					break;
				}
				case 'u':
				case 'U':
				{
					treasures[i] = 10;
					hazards[i] = SNAKES;
					break;
				}
				case 'v':
				case 'V':
				{
					treasures[i] = 4;
					hazards[i] = SPIDERS;
					break;
				}
				case 'w':
				case 'W':
				{
					treasures[i] = 6;
					hazards[i] = ROCKS;
					break;
				}
				case 'x':
				case 'X':
				{
					treasures[i] = 2;
					hazards[i] = SNAKES;
					break;
				}
				case 'y':
				case 'Y':
				{
					treasures[i] = 9;
					hazards[i] = ROCKS;
					break;
				}
				case 'z':
				case 'Z':
				{
					treasures[i] = 3;
					hazards[i] = QUICKSAND;
					break;
				}
				case '0':
				{
					treasures[i] = 5;
					hazards[i] = ROCKS;
					break;
				}
				case '1':
				{
					treasures[i] = 1;
					hazards[i] = SNAKES;
					break;
				}
				case '2':
				{
					treasures[i] = 10;
					hazards[i] = QUICKSAND;
					break;
				}
				case '3':
				{
					treasures[i] = 12;
					hazards[i] = ROCKS;
					break;
				}
				case '4':
				{
					treasures[i] = 11;
					hazards[i] = SPIDERS;
					break;
				}
				case '5':
				{
					treasures[i] = 17;
					hazards[i] = ROCKS;
					break;
				}
				case '6':
				{
					treasures[i] = 15;
					hazards[i] = SNAKES;
					break;
				}
				case '7':
				{
					treasures[i] = 13;
					hazards[i] = QUICKSAND;
					break;
				}
				case '8':
				{
					treasures[i] = 2;
					hazards[i] = ROCKS;
					break;
				}
				case '9':
				{
					treasures[i] = 17;
					hazards[i] = SPIDERS;
					break;
				}
			}

        	Debug.LogFormat("[Raiding Temples #{0}] Round {1}: {2} treasure with {3}.", moduleId, i+1, treasures[i], GetHazardName(hazards[i]));
		}
	}

	void CalcExploration()
	{
        Debug.LogFormat("[Raiding Temples #{0}] ===============Exploration===============", moduleId);

		explorerTreasure = Enumerable.Repeat(0, nExplorers).ToArray();
		explorerInTemple = Enumerable.Repeat(true, nExplorers).ToArray();
		leaveRound = Enumerable.Repeat(-1, nExplorers).ToArray();
		hazardHistory = new List<int>();

		for(int i = 0; i < treasures.Length; i++)
		{
        	Debug.LogFormat("[Raiding Temples #{0}] ----------Round {1}----------", moduleId, i+1);
		
			DivideTreasure(i);
			CalcLeaves(i);
			if(EvalDeath(i)) break;

			if(!Array.Exists(explorerInTemple, x => x))
			{
        		Debug.LogFormat("[Raiding Temples #{0}] No explorers remain on the temple.", moduleId);
				return;
			}
		}
	}

	bool EvalDeath(int n)
	{
		if(hazardHistory.Exists(x => x == hazards[n]))
		{
			for(int i = 0; i < explorerInTemple.Length; i++)
			{
				if(explorerInTemple[i])
				{
					explorerTreasure[i] = -1;
				}
			}

        	Debug.LogFormat("[Raiding Temples #{0}] Second occurence of {1}. Explorers [ {2}] died.", moduleId, GetHazardName(hazards[n]), GetDeadExplorers());
			return true;
		}

        Debug.LogFormat("[Raiding Temples #{0}] {1} caused no deaths.", moduleId, GetHazardName(hazards[n]));
		hazardHistory.Add(hazards[n]);
		return false;
	}

	void DivideTreasure(int n)
	{
		int explorerCount = 0;
		for(int i = 0; i < explorerInTemple.Length; i++)
			if(explorerInTemple[i]) explorerCount++;

		int share = treasures[n] / explorerCount;
		commonPool += treasures[n] % explorerCount;

		for(int i = 0; i < explorerTreasure.Length; i++)
			if(explorerInTemple[i]) explorerTreasure[i] += share;

        Debug.LogFormat("[Raiding Temples #{0}] {1} treasure divided by {2} explorer(s). Each explorer got {3} treasure. Common pool is now {4}.", moduleId, treasures[n], explorerCount, share, commonPool);
	}

	void CalcLeaves(int n)
	{
		List<int> leaves = new List<int>();
		int shelley = -1;

		for(int i = 0; i < explorerInTemple.Length; i++)
		{
			if(!explorerInTemple[i]) continue;

			switch(explorers[i])
			{
				case "Indiana":
				{
					if(hazards[n] == SNAKES) leaves.Add(i);
					break;
				}
				case "Francis":
				{
					int explorerCount = 0;
					for(int j = 0; j < explorerInTemple.Length; j++)
						if(explorerInTemple[j]) explorerCount++;
					if(explorerCount != 1) break;

					int maxTreasure = 0;
					for(int j = 0; j < explorerTreasure.Length; j++)
						if(explorerTreasure[j] > maxTreasure && i != j) maxTreasure = explorerTreasure[j];

					if(explorerTreasure[i] + commonPool > maxTreasure)
						leaves.Add(i);
					break;
				}
				case "Robert":
				{
					if(explorerTreasure[i] != 0)
						leaves.Add(i);
					break;
				}
				case "Clara":
				{
					if(n == 1)
						leaves.Add(i);
					break;
				}
				case "Sandy":
				{
					if(n <= 1) break;

					if(hazardHistory.Exists(x => x == QUICKSAND) || hazards[n] == QUICKSAND)
						leaves.Add(i);
					break;
				}
				case "Nate":
				{
					if(explorerTreasure[i] + commonPool >= 10)
						leaves.Add(i);
					break;
				}
				case "Allan":
				{
					if(n + 1 >= 2 * commonPool)
						leaves.Add(i);
					break;
				}
				case "Carlos":
				{
					int explorerCount = 0;
					for(int j = 0; j < explorerInTemple.Length; j++)
						if(explorerInTemple[j]) explorerCount++;
					if(explorerTreasure[i] + (commonPool / explorerCount) >= 7)
						leaves.Add(i);
					break;
				}
				case "Shelley":
				{
					shelley = i;
					break;
				}
				case "Michael":
				{
					int explorerCount = 0;
					for(int j = 0; j < explorerInTemple.Length; j++)
						if(explorerInTemple[j]) explorerCount++;
					if(explorerCount != nExplorers)
						leaves.Add(i);
					break;
				}
				case "Trini":
				{
					if(commonPool >= 5)
						leaves.Add(i);
					break;
				}
				default:
				{
        			Debug.LogFormat("[Raiding Temples #{0}] Error calculating leaves.", moduleId);
					break;
				}
			}
		}

		if(shelley != -1 && leaves.Count() != 0)
			leaves.Add(shelley);

		foreach(int expl in leaves)
			explorerInTemple[expl] = false;

		if(leaves.Count() == 0)
		{
        	Debug.LogFormat("[Raiding Temples #{0}] No explorers leave the temple.", moduleId);
			return;
		}

		int share = commonPool / leaves.Count();
		commonPool = commonPool % leaves.Count();

		foreach(int expl in leaves)
		{
			explorerInTemple[expl] = false;
			explorerTreasure[expl] += share;
			leaveRound[expl] = n;

			Debug.LogFormat("[Raiding Temples #{0}] Explorer #{1} ({2}) leaves with {3} treasure.", moduleId, expl + 1, explorers[expl], explorerTreasure[expl]);
		}
		
		Debug.LogFormat("[Raiding Temples #{0}] Common pool is now {1}.", moduleId, commonPool);
	}

	void CalcButtonPresses()
	{
		solution = new List<int>();
		pressed = new List<int>();
		nextPress = 0;

		List<int> toPress = new List<int>();
		for(int i = 0; i < explorerTreasure.Length; i++)
		{
			if(explorerTreasure[i] != -1)
				toPress.Add(i);
		}

		while(toPress.Count() != 0)
		{
			List<int> maxTreasure = new List<int>();
			int max = -1;
			for(int i = 0; i < toPress.Count(); i++)
			{
				if(explorerTreasure[toPress[i]] > max)
				{
					max = explorerTreasure[toPress[i]];
					maxTreasure = new List<int>();
					maxTreasure.Add(toPress[i]);
				}
				else if(explorerTreasure[toPress[i]] == max)
				{
					maxTreasure.Add(toPress[i]);
				}
			}

			toPress.RemoveAll(x => explorerTreasure[x] == max);

			while(maxTreasure.Count() != 0)
			{
				List<int> maxRound = new List<int>();
				int round = 100;
				for(int i = 0; i < maxTreasure.Count(); i++)
				{
					if(leaveRound[maxTreasure[i]] < round)
					{
						round = leaveRound[maxTreasure[i]];
						maxRound = new List<int>();
						maxRound.Add(maxTreasure[i]);
					}
					else if(leaveRound[maxTreasure[i]] == round)
					{
						maxRound.Add(maxTreasure[i]);
					}
				}

				maxTreasure.RemoveAll(x => leaveRound[x] == round);
				
				while(maxRound.Count() != 0)
				{
					int alpha = -1;
					String comp = "ZZZ";
					
					for(int i = 0; i < maxRound.Count(); i++)
					{
						if(explorers[maxRound[i]].CompareTo(comp) < 0)
						{
							alpha = maxRound[i];
							comp = explorers[maxRound[i]];
						}
					}

					solution.Add(alpha);

					maxRound.RemoveAll(x => x == alpha);
				}
			}
		}

		if(solution.Count() < nExplorers)
			solution.Add(-1);

		Debug.LogFormat("[Raiding Temples #{0}] Button press order is [ {1}].", moduleId, GetSolution());
		
	}

	String GetSolution()
	{
		String ret = "";

		for(int i = 0; i < solution.Count(); i++)
		{
			if(solution.ElementAt(i) == -1)
				ret += "Skull ";
			else
				ret += (solution.ElementAt(i) + 1) + " ";
		}

		return ret;
	}

	String GetHazardName(int i)
	{
		switch(i)
		{
			case 0: return "Spiders";
			case 1: return "Rocks";
			case 2: return "Snakes";
			case 3: return "Quicksand";
		}

		return "";
	}

	String GetDeadExplorers()
	{
		String ret = "";

		for(int i = 0; i < explorerTreasure.Length; i++)
		{
			if(explorerTreasure[i] == -1)
				ret += (i + 1) + " ";
		}

		return ret;
	}

	String GetButton(int btn)
	{
		if(btn == -1)
			return "Skull";
		
		return (btn + 1) + "";
	}
}
