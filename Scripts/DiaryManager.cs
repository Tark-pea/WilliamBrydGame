using UnityEngine;
using TMPro;
using System.Collections;
using Unity.VisualScripting;

[System.Serializable]
public class EffectSet {
    public int wealthDelta;
    public int moraleDelta;
    public int reflectionDelta;
}
[System.Serializable]
public class DiaryDay {
    public string prompt;
    public string[] choices; // e.g. length 2 or 3
    public EffectSet[] effects; // effects[i] = new int[] {wealthDelta, moraleDelta, reflectionDelta}
    public string[] feedbacks; // Feedback for each choice
}

public class DiaryManager : MonoBehaviour
{
    // Arrays for diary entries, choices, effects, ending texts
    // e.g., List<DiaryDay> days; struct DiaryDay { string entry; string[] choices; int[][] effects; string[] feedback; }

    [Header("Diary Entries")]
    public DiaryDay[] days = new DiaryDay[] {
    new DiaryDay {
        prompt = "Today, I had to decide whether to clear more land for tobacco, I thought it would require more labor though.",
        choices = new string[] {"Clear more land", "Do not clear land"},
        effects = new EffectSet[] {
            new EffectSet {wealthDelta = 2, moraleDelta = -1, reflectionDelta = 0},
            new EffectSet {wealthDelta = 0, moraleDelta = 0, reflectionDelta = 1}
        },
        feedbacks = new string[] {"I ended up clearing more land, but the workers grumbled a lot.", "I kept the land how it was, which I guess was nice for the slaves."}
    },
    new DiaryDay {
        prompt = "I awoke early and read Greek; I was debating whether production would increase if I gave stricter discipline or if I gave them a break, as whenever I want to do my best I always take a break to dance my dance.",
        choices = new string[] {"Be stricter to have the slaves work faster", "Give them a little rest today"},
        effects = new EffectSet[] {
            new EffectSet() {wealthDelta = 2, moraleDelta = -3, reflectionDelta = -1},
            new EffectSet() {wealthDelta = -2, moraleDelta = +2, reflectionDelta = 1}
        },
        feedbacks = new string[] {"I decided to give them some stricter deadlines and rules to increase our profits. The slaves had some resentment though.", "I personally told the slaves about the break. They seemed confused and worried at first, but they eventually seemed happy about it, which made me feel somewhat good."}
    },
    new DiaryDay {
        prompt = "This evening I had supper with a neighboring farmer, he was discussing that the soil has been terrible for farming, specifically on his tobacco fields. He was suggesting I should plant corn.",
        choices = new string[] {"Plant corn", "Keep the tobacco fields", "Tell the neighbor that his workers may not be working hard enough"},
        effects = new EffectSet[] {
            new EffectSet() {wealthDelta = -2, moraleDelta = -1, reflectionDelta = 1},
            new EffectSet() {wealthDelta = 1, moraleDelta = 0, reflectionDelta = 0},
            new EffectSet() {wealthDelta = 0, moraleDelta = 0, reflectionDelta = -2}
        },
        feedbacks = new string[] {"I took his advice and I told the workers to change out the fields for corn. I am hopeful that this will pay off.", "Though I did not like his idea. So I kept the tobacco.", "I proceeded to suggest to him that his workers might not have planted it right. He did not take my words lightly. I am unsure of was he is going to do."}
    }

    };

    [Header("Stats")]
    public int wealth;
    public int morale, reflection, dayIndex;


    [Header("Objects")]
    public GameObject diaryPanel;
    public TextMeshProUGUI diaryEntryText1;

    public GameObject twoButtonsHolder;
    public TextMeshProUGUI[] twoButtonsText;
    public GameObject threeButtonsHolder;
    public TextMeshProUGUI[] threeButtonsText;


    public TextMeshProUGUI dayIndicator;
    private DiaryDay currentDay;

    void Awake()
    {
        diaryEntryText1 = diaryPanel.transform.Find("Diary Text").GetComponent<TextMeshProUGUI>();
        twoButtonsHolder = diaryPanel.transform.Find("2ButtonHolder").gameObject;
        threeButtonsHolder = diaryPanel.transform.Find("3ButtonHolder").gameObject;

        for (int i = 0; i < 2; i++)
        {
            twoButtonsText[i] = twoButtonsHolder.transform.GetChild(i).GetChild(0).GetComponent<TextMeshProUGUI>();
        }
        for (int i = 0; i < 3; i++)
        {
            threeButtonsText[i] = threeButtonsHolder.transform.GetChild(i).GetChild(0).GetComponent<TextMeshProUGUI>();

        }
    }

    void Start()
    { /* Reset variables, load first entry */
        dayIndex = 1;
        StartCoroutine(OpeningScene());

    }
    IEnumerator OpeningScene()
    {
        diaryPanel.SetActive(false);
        dayIndicator.gameObject.SetActive(true);
        dayIndicator.text = "April " + (dayIndex+7).ToString()+", 1709"; // Animate later
        yield return new WaitForSeconds(2);
        dayIndicator.gameObject.SetActive(false);
        diaryPanel.SetActive(true);
        currentDay = days[dayIndex-1];
        StartCoroutine(PlayScene(currentDay.prompt, currentDay.choices));
    }
    IEnumerator PlayScene(string prompt, string[] choices)
    {
        diaryEntryText1.text = prompt;
        if (choices.Length == 3)
        {
            threeButtonsHolder.SetActive(true);
            threeButtonsText[0].text = choices[0];
            threeButtonsText[1].text = choices[1];
            threeButtonsText[2].text = choices[2];
        }
        else
        {
            twoButtonsHolder.SetActive(true);
            twoButtonsText[0].text = choices[0];
            twoButtonsText[1].text = choices[1];
        }
        
        yield return new WaitForSeconds(1);

    }

    public void Choose(int choice)
    {
        choice--;
        // Test
        // print("Button " + choice + " was pressed. " + currentDay.effects);

        // Appling effects to variables
        wealth += currentDay.effects[choice].wealthDelta;
        morale += currentDay.effects[choice].moraleDelta;
        reflection += currentDay.effects[choice].reflectionDelta;

        // Set feedback text
        twoButtonsHolder.SetActive(false);
        threeButtonsHolder.SetActive(false);

        diaryEntryText1.text += "\n" + currentDay.feedbacks[choice];

        // Enable Continue life button
        StartCoroutine(Continue());
    }

    public IEnumerator Continue()
    {
        yield return new WaitForSeconds(5f);
        dayIndex++;
        diaryPanel.SetActive(false);
        dayIndicator.gameObject.SetActive(true);
        dayIndicator.text = "April " + (dayIndex+7).ToString()+", 1709"; // Animate later
        yield return new WaitForSeconds(2);
        dayIndicator.gameObject.SetActive(false);
        diaryPanel.SetActive(true);
        
        if (dayIndex <= 5)
        {
            currentDay = days[dayIndex - 1];
            StartCoroutine(PlayScene(currentDay.prompt, currentDay.choices));
        }
        else
        {
            ShowEnding();
        }
    }

    void ShowEnding()
    {
        // Simple logic: if wealth > x, ending 1, else if reflection > y, ending 2, etc.
        // Will probably change all numbers later
        if (wealth > 2)
        {

        }
        else if (reflection > 2)
        {

        }
        else if (morale > 2)
        {

        }
        else
        {

        }
        
    }
}
