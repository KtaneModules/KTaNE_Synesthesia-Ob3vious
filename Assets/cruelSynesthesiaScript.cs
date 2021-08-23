using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using KModkit;
using Rnd = UnityEngine.Random;

public class cruelSynesthesiaScript : MonoBehaviour {

	public KMAudio Audio;
	public KMSelectable[] Button;
	public KMSelectable[] Selection;
	public KMSelectable[] Utility;
	public KMBombModule Module;

	private bool solved = false;
	private bool playing = false;
	private int Color = 0;
	private int[] Value = { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
	private int[] Unmoved = { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
	private int[] Solution = { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };

	static int _moduleIdCounter = 1;
	int _moduleID = 0;

	private KMSelectable.OnInteractHandler ButtonPressed(int pos)
	{
		return delegate
		{
			Button[pos].AddInteractionPunch();
			if (!solved && !playing)
			{
				Button[pos].GetComponent<MeshRenderer>().material.color = new Color((Color / 16) / 3f, ((Color / 4) % 4) / 3f, (Color % 4) / 3f);
				Value[pos] = Color;
			}
			return false;
		};
	}

	private KMSelectable.OnInteractHandler SelectorPressed(int pos)
	{
		return delegate
		{
			Selection[pos].AddInteractionPunch();
			if (!solved && !playing)
			{
				if (pos / 4 == 2) 
					Color = (Color / 4) * 4 + pos % 4;
				else if (pos / 4 == 1)
					Color = (Color / 16) * 16 + Color % 4 + 4 * (pos % 4);
				else
					Color = (Color % 16) + 16 * (pos % 4);
				for (int j = 0; j < 3; j++)
				{
					if ((Color / (16 >> (j * 2))) % 4 != 0)
					{
						Audio.PlaySoundAtTransform(new string[] { "440", "550", "700" }[j] + "-" + ((Color / (16 >> (j * 2))) % 4), Module.transform);
					}
				}
				for (int j = 0; j < 12; j++)
				{
					if (j / 4 == 0)
						Selection[j].GetComponent<MeshRenderer>().material.color = new Color((j % 4) / 3f, ((Color / 4) % 4) / 3f, (Color % 4) / 3f);
					else if (j / 4 == 1)
						Selection[j].GetComponent<MeshRenderer>().material.color = new Color((Color / 16) / 3f, (j % 4) / 3f, (Color % 4) / 3f);
					else
						Selection[j].GetComponent<MeshRenderer>().material.color = new Color((Color / 16) / 3f, ((Color / 4) % 4) / 3f, (j % 4) / 3f);
				}
			}
			return false;
		};
	}

	private KMSelectable.OnInteractHandler PlayPressed()
	{
		return delegate
		{
			Utility[0].AddInteractionPunch();
			if (!solved && !playing)
			{
				StartCoroutine(Play());
			}
			else if (!solved && playing)
				playing = false;
			return false;
		};
	}

	private KMSelectable.OnInteractHandler SubmitPressed()
	{
		return delegate
		{
			Utility[1].AddInteractionPunch();
			if (!solved && !playing)
			{
				StartCoroutine(Finish());
			}
			return false;
		};
	}

	private void Highlight(KMSelectable button)
    {
        if (!solved && !playing)
        {
			button.GetComponent<MeshRenderer>().material.color = new Color(1, 1, 1);
        }
    }

	private void HighlightEnd(KMSelectable button, int type, int index)
	{
		if (!solved && !playing)
		{
			switch (type)
            {
				case 0:
					button.GetComponent<MeshRenderer>().material.color = new Color((Value[index] / 16) / 3f, ((Value[index] / 4) % 4) / 3f, (Value[index] % 4) / 3f);
					break;
				case 1:
					if (index / 4 == 0)
						Selection[index].GetComponent<MeshRenderer>().material.color = new Color((index % 4) / 3f, ((Color / 4) % 4) / 3f, (Color % 4) / 3f);
					else if (index / 4 == 1)
						Selection[index].GetComponent<MeshRenderer>().material.color = new Color((Color / 16) / 3f, (index % 4) / 3f, (Color % 4) / 3f);
					else
						Selection[index].GetComponent<MeshRenderer>().material.color = new Color((Color / 16) / 3f, ((Color / 4) % 4) / 3f, (index % 4) / 3f);
					break;
				case 2:
					button.GetComponent<MeshRenderer>().material.color = new Color(0, 0, 0);
					break;
            }
			
		}
	}

	//Initialise
	void Awake () {
		_moduleID = _moduleIdCounter++;
		for (int i = 0; i < Button.Length; i++)
		{
			Button[i].OnInteract += ButtonPressed(i);
			int x = i;
			Button[i].OnHighlight += delegate { Highlight(Button[x]); };
			Button[i].OnHighlightEnded += delegate { HighlightEnd(Button[x], 0, x); };
		}
		for (int i = 0; i < Selection.Length; i++)
		{
			Selection[i].OnInteract += SelectorPressed(i);
			int x = i;
			Selection[i].OnHighlight += delegate { Highlight(Selection[x]); };
			Selection[i].OnHighlightEnded += delegate { HighlightEnd(Selection[x], 1, x); };
		}
		Utility[0].OnInteract += PlayPressed();
		Utility[1].OnInteract += SubmitPressed();
        for (int i = 0; i < 2; i++)
        {
			int x = i;
			Utility[i].OnHighlight += delegate { Highlight(Utility[x]); };
			Utility[i].OnHighlightEnded += delegate { HighlightEnd(Utility[x], 2, 0); };
		}
		for (int j = 0; j < 12; j++)
		{
			if (j / 4 == 0)
				Selection[j].GetComponent<MeshRenderer>().material.color = new Color((j % 4) / 3f, 0f, 0f);
			else if (j / 4 == 1)
				Selection[j].GetComponent<MeshRenderer>().material.color = new Color(0f, (j % 4) / 3f, 0f);
			else
				Selection[j].GetComponent<MeshRenderer>().material.color = new Color(0f, 0f, (j % 4) / 3f);
		}
		for (int i = 0; i < 16; i++)
		{
			Unmoved[i] = Rnd.Range(0, 64);
			//Button[i].GetComponent<MeshRenderer>().material.color = new Color((Unmoved[i] / 16) / 3f, ((Unmoved[i] / 4) % 4) / 3f, (Unmoved[i] % 4) / 3f);
		}
		Debug.LogFormat("[Cruel Synesthesia #{0}] The sequence played is {1}.", _moduleID, Unmoved.Select(x => (x / 16).ToString() + (x / 4) % 4 + x % 4).Join(", "));
		Solution = Unmoved;
	}

	private IEnumerator Play()
	{
		if (!playing)
		{
			playing = true;
			Utility[1].GetComponent<MeshRenderer>().material.color = new Color(1f, 1f, 1f);
			for (int i = 0; i < 16 && playing; i++)
			{
				for (int j = 0; j < 3; j++)
				{
					if ((Unmoved[i] / (16 >> (j * 2))) % 4 != 0)
					{
						Audio.PlaySoundAtTransform(new string[] { "440", "550", "700" }[j] + "-" + ((Unmoved[i] / (16 >> (j * 2))) % 4), Module.transform);
					}
				}
				yield return new WaitForSeconds(0.5f);
			}
			playing = false;
			Utility[1].GetComponent<MeshRenderer>().material.color = new Color(0f, 0f, 0f);
		}
	}

	private IEnumerator Finish()
	{
		if (!playing)
		{
			playing = true;
			bool safe = true;
			bool solving = true;
			for (int i = 0; i < 16; i++)
			{
				int offby = 0;
                for (int j = 0; j < 3; j++)
                {
                    switch (Math.Abs((Value[i] / (16 >> (j * 2))) % 4 - (Solution[i] / (16 >> (j * 2))) % 4))
                    {
						case 0:
							break;
						case 1:
							offby++;
							break;
                        default:
							offby += 2;
                            break;
                    }
                }
				int col = 0;
                if (offby <= 3)
                {
					col = 12 + 16 * offby;
                }
                else
				{
					col = 72 - 4 * offby;
				}
				for (int j = 0; j < 3; j++)
				{
					if ((col / (16 >> (j * 2))) % 4 != 0)
					{
						Audio.PlaySoundAtTransform(new string[] { "440", "550", "700" }[j] + "-" + ((col / (16 >> (j * 2))) % 4), Module.transform);
					}
				}
				Button[i].GetComponent<MeshRenderer>().material.color = new Color((col / 16) / 3f, ((col / 4) % 4) / 3f, (col % 4) / 3f);
				if (offby == 1)
				{
					solving = false;
				}
				else if (offby >= 2)
				{
					solving = false;
					safe = false;
				}
				yield return new WaitForSeconds(0.0625f);
			}
			if (solving)
			{
				solved = true;
				int[] r = { 0, 0, 2, 2, 1, 0, 0, 0, 2, 0, 0, 2, 1, 0, 0, 0 };
				for (int i = 0; i < 16; i++)
				{
					Button[i].GetComponent<MeshRenderer>().material.color = new Color(r[i] / 3f + 1 / 3f, 0, 0);
					yield return new WaitForSeconds(0.0625f);
				}
				Audio.PlaySoundAtTransform("700-3", Module.transform);
				for (int i = 0; i < 12; i++)
				{
					Selection[i].GetComponent<MeshRenderer>().material.color = new Color(0f, 1f, 0f);
				}
				for (int i = 0; i < 2; i++)
				{
					Utility[i].GetComponent<MeshRenderer>().material.color = new Color(0f, 1f, 0f);
				}
				Module.HandlePass();
			}
			else
			{
				if (!safe)
				{
					Module.HandleStrike();
				}
				for (int i = 0; i < 16; i++)
				{
					Button[i].GetComponent<MeshRenderer>().material.color = new Color((Value[i] / 16) / 3f, ((Value[i] / 4) % 4) / 3f, (Value[i] % 4) / 3f);
					yield return new WaitForSeconds(0.0625f);
				}
				Debug.LogFormat("[Cruel Synesthesia #{0}] You submitted {1}, but I expected {2}.", _moduleID, Value.Select(x => (x / 16).ToString() + (x / 4) % 4 + x % 4).ToArray().Join(", "), Solution.Select(x => (x / 16).ToString() + (x / 4) % 4 + x % 4).ToArray().Join(", "));
			}
			playing = false;
		}
	}

#pragma warning disable 414
	private string TwitchHelpMessage = "'!{0} play' to play the sequence. '!{0} submit' to submit the image. '!{0} sample' to play the currently selected sound. '!{0} 201' to select RGB values R=2 G=0 B=1, '!{0} A3' to press A3. Commands can be chained. e.g. '!{0} 120 A2 201 B4 B3'";
#pragma warning restore 414
	IEnumerator ProcessTwitchCommand(string command)
	{
		yield return null;
		command = command.ToLowerInvariant();
		if (command == "play") { Utility[0].OnInteract(); }
		else if (command == "submit") { Utility[1].OnInteract(); yield return "strike"; yield return "solve"; }
		else if (command == "sample") { for (int j = 0; j < 3; j++) { if ((Color / (16 >> (j * 2))) % 4 != 0) { Audio.PlaySoundAtTransform(new string[] { "440", "550", "700" }[j] + "-" + ((Color / (16 >> (j * 2))) % 4), Module.transform); } } }
		else
		{
			string[] cmds = command.Split(' ');
			string[] coords = { "a4", "b4", "b3", "a3", "a2", "a1", "b1", "b2", "c2", "c1", "d1", "d2", "d3", "c3", "c4", "d4" };
			for (int i = 0; i < cmds.Length; i++)
			{
				if (!coords.Contains(cmds[i]) && (!(cmds[i].ToCharArray().All(x => "0123".Contains(x))) || cmds[i].Length != 3))
				{
					yield return "sendtochaterror Invalid command.";
					yield break;
				}
				else if (!coords.Contains(cmds[i]))
				{
					Selection[cmds[i][0] - '0'].OnInteract();
					yield return new WaitForSeconds(0.0625f);
					Selection[cmds[i][1] - '0' + 4].OnInteract();
					yield return new WaitForSeconds(0.0625f);
					Selection[cmds[i][2] - '0' + 8].OnInteract();
					yield return new WaitForSeconds(0.0625f);
				}
				else
				{
					for (int j = 0; j < 16; j++)
					{
						if (coords[j] == cmds[i])
						{
							Button[j].OnInteract();
							yield return new WaitForSeconds(0.0625f);
						}
					}
				}
			}
		}
		yield return null;
	}

	IEnumerator TwitchHandleForcedSolve()
	{
		yield return true;
		for (int i = 0; i < 16; i++)
		{
			Selection[Solution[i] / 16].OnInteract();
			yield return true;
			Selection[(Solution[i] / 4) % 4 + 4].OnInteract();
			yield return true;
			Selection[Solution[i] % 4 + 8].OnInteract();
			yield return true;
			Button[i].OnInteract();
			yield return true;
		}
		Utility[1].OnInteract();
	}
}