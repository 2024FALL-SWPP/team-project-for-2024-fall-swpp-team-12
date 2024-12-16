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
            "���� �������� �ô�.\n�ӻ��� �ٴ��� \'��� �ö�\', ���� �ذ��� �Ѵ�.\n< press 'm', to read the monologue Again >",
            "�� ��� ������ �ǵ����� ����ߴ�.\n�� \'������\'�� �о��糢�� ����� �����������.",
            "���ſ��� ã�ƿ� �������̴� �� �̷��� ����,\n������ ����, �ѹ��ѹ� �ɾ��.\n�ƹ��� ���� ��ġ����,\n�� \'�ܻ�\'�� ���� ������ ���ƿ� ���̴�.",
            "������ ������ \'�� �Ʒ�\' ���� �ڵ��� �ٶ󺻴�.\n��ü�� ������ Ȧ�� ��Ƴ��� �ڰ�.",
            "ũ�γ뽺�� �������̴�.\n\'���� ����迡 �׸� �÷�\' �¿��� ���´�.",
            "���� �� ���� ������ ���� ã�� ��������.\n\'��\'�� ���θ�����, �ʽ� ���� ���� ���̴�.",
            "�� ũ�γ뽺���� �� �ٸ� �̸��� �ٿ�����.\n���...\n���ο��� \'���ٰ�\' �ᱹ ���� ���ȴ°�.",
            "���, \'����ݱ�\'�� �ؼ��� ���� ���� ã�ư���.",
            "�� ��谡 �մ� \'�߰ſ� ���ٱ�\'�� �����͵� ���Ҵ�.\n�ұԸ� ���� ��ġ�� ������ ������ �� �翡 �� �� ���� �ʾҴ�.",
            "������ ���� ���� �Ʒ��� ���� ���̴�.\n\'��Ȥ�� ������ ������\'�� �� ���̸� �����ش�.",
            "\"���� ������ �� ���� ������.\n���� ��ü �� ������, �̵��ݾ� �ǻ�Ƴ��� ���̿�?\"",
            "\"�� ��� ���ô�. \'��\'�̼� �̾߱⳪ ��������.\"\n\"��ħ �а� �ƴ��ϴ�,\n�׷��� \'��\'�̶�, ���� �� �ۿ� ���� �ʼ�?\"",
            "\'������ ���\'�� ã�ƺ���.\n���� ���� �ð��� �˹��ϴ�.\nũ�γ뽺...",
            "�ϸ��� �Ⱦ�� �;��⿡, �̸� �վ� ����������.\n�׸���� �Ҿ���� �ٶ��� �۾����� ���Ҵ�.",
            "������ \'�̷�\'�͵� ����.\n������ �ǹ̽����ϳ�, ���� �㹫�� ���̴�.",
            "�� �ü��� �״�� �߰��� ���̾�.\n\'�� ������ ��ȣ\'���� �״�δ�.\n���⼭ ���� �ǹ� �ִ� ���� ã�� �� ������."
        };
        testMonologue = "����� �� ���۵� �� ������ �� ������ ��������.\n" +
            "�� �����ڴ� �ð� ���ุ�� ����̶� �ϰ� ������ ����������,\n" +
            "�ܻ��� ���� �ҿ����� ��踦 ����� �� ���ƴ�.\n" +
            "�ҽ� �ִ� �䳢 ����, \'�� ũ�γ뽺\'��.\n" +
            "�׸��� �䳢�� �Ҿ����� ���� ������, ������ ã�� ������.";
        monologue = testMonologue;
        baseText = "";
        baseText1 = "[ ��� �ص� ]" + "\n";
        baseText2 = "[ ��ȭ ���� }" + "\n";

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
        for (int i=0; i< monologue.Length; i++)
        {
            monologuePaper.text = baseText + monologue.Substring(0, i+1);
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
        while (progress < typeSpeedStartOffset/6)
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
