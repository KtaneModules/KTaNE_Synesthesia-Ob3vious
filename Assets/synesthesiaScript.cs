using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using KModkit;
using Rnd = UnityEngine.Random;

public class synesthesiaScript : MonoBehaviour {

	public KMAudio Audio;
	public AudioClip[] sounds;
	public KMBombInfo Bomb;
	public KMSelectable[] Button;
	public KMSelectable[] Selection;
	public KMSelectable Submit;
	public KMSelectable Playbutton;
	public KMBombModule Module;
	public KMColorblindMode CBM;

	private bool solved = false;
	private bool playing = false;
	private int Color = 0;
	private int[] Value = { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
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
				Button[pos].GetComponent<MeshRenderer>().material.color = new Color((Color / 9) / 2f, ((Color / 3) % 3) / 2f, (Color % 3) / 2f);
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
				if (pos / 3 == 2) 
					Color = (Color / 3) * 3 + pos % 3;
				else if (pos / 3 == 1)
					Color = (Color / 9) * 9 + Color % 3 + 3 * (pos % 3);
				else
					Color = (Color % 9) + 9 * (pos % 3);
				if (pos % 3 != 0)
				{
					string[] audio = { "a", "a2", "b", "b2", "c", "c2" };
					Audio.PlaySoundAtTransform(audio[(pos % 3) - 1 + (pos / 3) * 2], Module.transform);
				}
				Playbutton.GetComponent<MeshRenderer>().material.color = new Color((Color / 9) / 2f, ((Color / 3) % 3) / 2f, (Color % 3) / 2f);
				for (int j = 0; j < 9; j++)
				{
					if (j / 3 == 0)
						Selection[j].GetComponent<MeshRenderer>().material.color = new Color((j % 3) / 2f, ((Color / 3) % 3) / 2f, (Color % 3) / 2f);
					else if (j / 3 == 1)
						Selection[j].GetComponent<MeshRenderer>().material.color = new Color((Color / 9) / 2f, (j % 3) / 2f, (Color % 3) / 2f);
					else
						Selection[j].GetComponent<MeshRenderer>().material.color = new Color((Color / 9) / 2f, ((Color / 3) % 3) / 2f, (j % 3) / 2f);
				}
			}
			return false;
		};
	}
	private KMSelectable.OnInteractHandler PlayPressed()
	{
		return delegate
		{
			Playbutton.AddInteractionPunch();
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
			Submit.AddInteractionPunch();
			if (!solved && !playing)
			{
				bool safe = true;
				for (int i = 0; i < 16; i++)
				{
					if(Value[i] != Solution[i])
                    {
						safe = false;
                    }
				}
				if (safe)
				{
					StartCoroutine(Finish());
					solved = true;
				}
				else
					Module.HandleStrike();
			}
			return false;
		};
	}

	//Initialise
	void Awake () {
		_moduleID = _moduleIdCounter++;
		for (int i = 0; i < Button.Length; i++)
		{
			Button[i].OnInteract += ButtonPressed(i);
		}
		for (int i = 0; i < Selection.Length; i++)
		{
			Selection[i].OnInteract += SelectorPressed(i);
		}
		Playbutton.OnInteract += PlayPressed();
		Submit.OnInteract += SubmitPressed();
		for (int j = 0; j < 9; j++)
		{
			if (j / 3 == 0)
				Selection[j].GetComponent<MeshRenderer>().material.color = new Color((j % 3) / 2f, 0f, 0f);
			else if (j / 3 == 1)
				Selection[j].GetComponent<MeshRenderer>().material.color = new Color(0f, (j % 3) / 2f, 0f);
			else
				Selection[j].GetComponent<MeshRenderer>().material.color = new Color(0f, 0f, (j % 3) / 2f);
		}
		for (int i = 0; i < 16; i++)
		{
			Solution[i] = Rnd.Range(0, 27);
			//Button[i].GetComponent<MeshRenderer>().material.color = new Color((Solution[i] / 9) / 2f, ((Solution[i] / 3) % 3) / 2f, (Solution[i] % 3) / 2f);
		}
		Debug.LogFormat("[Synesthesia #{0}] The sequence played is {1}.", _moduleID, Solution.Join(", "));
	}

	private IEnumerator Play()
	{
		playing = true;
		Submit.GetComponent<MeshRenderer>().material.color = new Color(1f, 1f, 1f);
		for (int i = 0; i < 16 && playing; i++)
		{
			if ((Solution[i] / 9) == 1)
				Audio.PlaySoundAtTransform("a", Module.transform);
			if ((Solution[i] / 9) == 2)
				Audio.PlaySoundAtTransform("a2", Module.transform);
			yield return new WaitForSeconds(0.15f);
			if ((Solution[i] / 3) % 3 == 1)
				Audio.PlaySoundAtTransform("b", Module.transform);
			if ((Solution[i] / 3) % 3 == 2)
				Audio.PlaySoundAtTransform("b2", Module.transform);
			yield return new WaitForSeconds(0.15f);
			if ((Solution[i] % 3) == 1)
				Audio.PlaySoundAtTransform("c", Module.transform);
			if ((Solution[i] % 3) == 2)
				Audio.PlaySoundAtTransform("c2", Module.transform);
			yield return new WaitForSeconds(0.7f);
		}
		playing = false;
		Submit.GetComponent<MeshRenderer>().material.color = new Color(0f, 0f, 0f);
	}
	private IEnumerator Finish()
	{
		Submit.GetComponent<MeshRenderer>().material.color = new Color(0f, 1f, 0f);
		int[] r = { 1, 0, 1, 0, 1, 0, 1, 1, 1, 1, 0, 1, 0, 1, 0, 1 };
		for (int i = 0; i < 16; i++)
		{
			Button[i].GetComponent<MeshRenderer>().material.color = new Color(r[i], r[i], r[i]);
			yield return new WaitForSeconds(0.1f);
		}
		Playbutton.GetComponent<MeshRenderer>().material.color = new Color(0f, 1f, 0f);
		for (int i = 0; i < 9; i++)
		{
			Selection[i].GetComponent<MeshRenderer>().material.color = new Color(0f, 1f, 0f);
		}
		Module.HandlePass();
	}
#pragma warning disable 414
	private string TwitchHelpMessage = "'!{0} play' to play the sequence. '!{0} submit' to submit the image. '!{0} 201' to select RGB values R=2 G=0 B=1, '!{0} A3' to press A3. Commands can be chained. e.g. '!{0} 120 A2 201 B4 B3'";
#pragma warning restore 414
	IEnumerator ProcessTwitchCommand(string command)
	{
		yield return null;
		command = command.ToLowerInvariant();
		if (command == "play") { Playbutton.OnInteract(); }
		else if (command == "submit") { Submit.OnInteract(); yield return "solve"; }
		else
		{
			string[] cmds = command.Split(' ');
			string[] coords = { "a4", "b4", "b3", "a3", "a2", "a1", "b1", "b2", "c2", "c1", "d1", "d2", "d3", "c3", "c4", "d4" };
			string[] colours = { "000", "001", "002", "010", "011", "012", "020", "021", "022", "100", "101", "102", "110", "111", "112", "120", "121", "122", "200", "201", "202", "210", "211", "212", "220", "221", "222" };
			for (int i = 0; i < cmds.Length; i++)
			{
				if (!coords.Contains(cmds[i]) && !colours.Contains(cmds[i]))
				{
					yield return "sendtochaterror Invalid command.";
					yield break;
				}
                else if (!coords.Contains(cmds[i]))
                {
                    for (int j = 0; j < 27; j++)
                    {
						if(colours[j] == cmds[i])
                        {
							Selection[j / 9].OnInteract();
							yield return new WaitForSeconds(0.15f);
							Selection[(j / 3) % 3 + 3].OnInteract();
							yield return new WaitForSeconds(0.15f);
							Selection[j % 3 + 6].OnInteract();
							yield return new WaitForSeconds(0.15f);
						}
                    }
                }
                else
                {
					for (int j = 0; j < 16; j++)
					{
						if (coords[j] == cmds[i])
						{
							Button[j].OnInteract();
							yield return new WaitForSeconds(0.15f);
						}
					}
				}
			}
		}
		yield return null;
	}
	IEnumerator TwitchHandleForcedSolve()
	{
		yield return null;
		for (int i = 0; i < 16; i++)
		{
			Selection[Solution[i] / 9].OnInteract();
			yield return new WaitForSeconds(0.05f);
			Selection[(Solution[i] / 3) % 3 + 3].OnInteract();
			yield return new WaitForSeconds(0.05f);
			Selection[Solution[i] % 3 + 6].OnInteract();
			yield return new WaitForSeconds(0.05f);
			Button[i].OnInteract();
			yield return new WaitForSeconds(0.05f);
		}
		Submit.OnInteract();
	}
}
