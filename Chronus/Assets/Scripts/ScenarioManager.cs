using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ScenarioManager : MonoBehaviour
{
    public static ScenarioManager scenarioManager;
    public GameObject monologueText;
    private TextMeshProUGUI monologuePaper;
    private string[] monologues;
    private string monologue;
    private string baseText;
    private string baseText1;
    private string baseText2;
    private string testMonologue;
    private float typeSpeedStartOffset = 2.4f;
    private float typeSpeedEndOffset = 2.3f;
    private float typeSpeedDefault = 0.1f;
    private float typeSpeedSingleQuote = 0.03f;
    private float typeSpeedDoubleQuote = 0.4f;
    private float typeSpeedRest = 0.8f;
    private float typeSpeedComma = 1.0f;
    private float typeSpeedNewLine = 0.2f;

    public GameObject directionalLight;
    private Light light;
    private float defaultLightIntensity;

    public bool isReadingMonologue = false;
    public bool isLockedToRead = false;

    private void Awake()
    {
        if (scenarioManager == null) { scenarioManager = this; }
        monologuePaper = monologueText.GetComponent<TextMeshProUGUI>();
        monologuePaper.text = "";
        monologueText.SetActive(false);
        monologues = new string[]
        {
            "땅이 꺼져가는 시대.\n앙상한 바닥을 \'밟고 올라가\', 빛을 쬐고자 한다.\n<press 'm', to read the monologue again>",
            "이 모든 과오를 되돌리려 노력했다.\n그 \'손잡이\'를 밀어젖히던 기억이 희미해져 간다.",
            "과거에서 찾아온 아지랑이는 또 미래를 향해,\n죽음을 향해, 뚜벅뚜벅 걸어간다.\n아무리 세게 밀치더라도,\n그 \'잔상\'은 남아 어김없이 돌아올 것이다.",
            "죽음에 붙잡혀 \'땅 아래\' 꺼진 자들을 바라본다.\n육체를 버리고 홀로 살아남은 자가.",
            "크로노스는 실패작이다.\n\'작은 나룻배에 그를 올려\' 태워서 보냈다.",
            "내가 서 있을 발판을 새로 찾아 떠나리라.\n\'벽\'에 가로막혀도, 필시 길은 있을 것이다.",
            "모델 크로노스에겐 또 다른 이름을 붙였었다.\n루시...\n서로에게 \'기대다가\' 결국 길이 갈렸는가.",
            "루시, \'도움닫기\'를 해서라도 너의 길을 찾아가렴.",
            "그 기계가 뿜던 \'뜨거운 빛줄기\'는 투지와도 같았다.\n소규모 구동 장치가 답임을 깨달을 땐 곁에 몇 명 남지 않았다.",
            "이제는 고개를 돌려 아래로 향할 때이다.\n\'가혹한 선택의 갈림길\'은 그 깊이를 더해준다.",
            "\"도통 원리를 알 수가 없구려.\n땅은 대체 왜 꺼지며, 이따금씩 되살아나는 것이오?\"",
            "\"거 잠시 쉽시다. \'셋\'이서 이야기나 나눠보죠.\"\n\"마침 넓고 아늑하니,\n그런데 \'셋\'이라, 지금 둘 밖에 없지 않소?\"",
            "\'숨겨진 통로\'를 찾아보자.\n지금 내겐 시간이 촉박하다.\n크로노스...",
            "암막을 걷어내고 싶었기에, 이를 뚫어 열어젖혔다.\n그리고는 불어오는 바람에 휩쓸리고 말았다.",
            "과학은 \'미로\'와도 같다.\n과정은 의미심장하나, 끝은 허무할 것이다.",
            "옛 시설을 그대로 발견할 줄이야.\n\'세 구간의 암호\'또한 그대로다.\n여기서 과연 의미 있는 것을 찾을 수 있을까."
        };
        testMonologue = "수백년 전 시작된 땅 꺼짐에 온 세상이 무너졌다.\n" +
            "한 과학자는 시간 역행만이 방법이라 믿고선 연구에 몰두했으나,\n" +
            "잔상이 남는 불완전한 기계를 만드는 데 그쳤다.\n" +
            "뚝심 있는 토끼 인형, \'모델 크로노스\'를.\n" +
            "그리고 토끼는 불안정한 땅을 딛으며, 주인을 찾아 나선다.";
        monologue = testMonologue;
        baseText = "";
        baseText1 = "[ 기록 해독 ]" + "\n";
        baseText2 = "[ 대화 녹음 }" + "\n";

        light = directionalLight.GetComponent<Light>();
        defaultLightIntensity = light.intensity;
    }

    private void Update()
    {
        if (Input.GetKeyDown("m") &&
            !TurnManager.turnManager.CLOCK &&
            !PlayerController.playerController.isTimeRewinding &&
            !PlayerController.playerController.isBlinking) TestTyping();
    }

    public void TestTyping()
    {
        PrepareMonologue();
        //GetTestMonologueText();
        StartCoroutine(Typing());
    }

    public void StartMonologue(int index)
    {
        PrepareMonologue();
        GetMonologueText(index);
        StartCoroutine(Typing());
    }

    private void PrepareMonologue()
    {
        isReadingMonologue = true;
        isLockedToRead = true;

        monologuePaper.text = "";
        monologueText.SetActive(true);
        StartCoroutine(MakeSceneDark());
    }
    private void GetMonologueText(int index)
    {
        monologue = monologues[index];
        if (index == 10 || index == 11) baseText = baseText2;
        else baseText = baseText1;
    }
    private void GetTestMonologueText()
    {
        monologue = testMonologue;
        baseText = "[ ��� ���� ]" + "\n";
    }

    IEnumerator Typing()
    {
        yield return new WaitForSeconds(typeSpeedStartOffset / 3);
        monologuePaper.text = baseText;
        yield return new WaitForSeconds(typeSpeedStartOffset * 2 / 3);
        for (int i = 0; i < monologue.Length; i++)
        {
            monologuePaper.text = baseText + monologue.Substring(0, i + 1);
            if (i == monologue.Length - 1) yield return null;
            else
            {
                float typeSpeed;
                switch (monologue[i])
                {
                    case '\'':
                        typeSpeed = typeSpeedSingleQuote;
                        break;
                    case '\"':
                        typeSpeed = typeSpeedDoubleQuote;
                        break;
                    case ',':
                        typeSpeed = typeSpeedRest;
                        break;
                    case '.':
                        typeSpeed = typeSpeedComma;
                        break;
                    case '\n':
                        typeSpeed = typeSpeedNewLine;
                        break;
                    default:
                        typeSpeed = typeSpeedDefault;
                        break;
                }
                yield return new WaitForSeconds(typeSpeed);
            }
        }
        isLockedToRead = false;
        StartCoroutine(MakeSceneBrightAgain());
        float autoSkip = 0.0f;
        while ((autoSkip < typeSpeedEndOffset) && isReadingMonologue)
        {
            autoSkip += Time.deltaTime;
            yield return null;
        }
        monologueText.SetActive(false);
        if (isReadingMonologue) isReadingMonologue = false;
    }

    IEnumerator MakeSceneDark()
    {
        float progress = 0.0f;
        while (progress < typeSpeedStartOffset / 6)
        {
            progress += Time.deltaTime;
            light.intensity -= defaultLightIntensity / typeSpeedStartOffset * 6 * Time.deltaTime;
            yield return null;
        }
        light.intensity = 0.0f; //dark-change
        //need sound
    }
    IEnumerator MakeSceneBrightAgain()
    {
        float progress = 0.0f;
        while (progress < typeSpeedEndOffset)
        {
            float delta;
            if (!isReadingMonologue) delta = 2 * Time.deltaTime;
            else delta = Time.deltaTime;

            progress += delta;
            light.intensity += defaultLightIntensity / typeSpeedEndOffset * delta;
            yield return null;
        }
        light.intensity = defaultLightIntensity;
    }
}