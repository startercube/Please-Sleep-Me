using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PlayerCtrl : MonoBehaviour
{
    public Image CursorGaugeImage;
    private Vector3 ScreenCenter;
    private float GaugeTimer;

    private bool isTriggered = false;
    public Text TextUI;
    private AudioSource audioSource;    //�������
    public AudioClip audio1;    //ī�� ����
    public AudioClip audio2;    //��Ī����
    public AudioClip audio3;    //��Ī����
    public AudioClip audio4;    //�������� Ŭ����

    private bool isCardFlipped = false;
    private GameObject flippedCard;
    private int matchedPairs = 0;

    private bool isMatching = false; // ��Ī�� ���� ������ ����
    private float matchingDelay = 0.5f; // ��Ī �ð� ���� (��)

    private int currentStage = 1;
    private int totalStages = 4; // ��ü �������� ��

    public enum MoveType
    {
        WAYPOINT,
        LOOK_AT
    }
    public MoveType moveType = MoveType.WAYPOINT;
    public float speed = 3.0f;
    public float damping = 3.0f;

    private Transform playerTransform;
    private Transform[] waypoints;
    private int nextIndex = 1;

    private bool isGoNext = false;


    // Start is called before the first frame update
    void Start()
    {

        ScreenCenter = new Vector3(Camera.main.pixelWidth / 2, Camera.main.pixelHeight / 2);
        audioSource = GetComponent<AudioSource>();

        playerTransform = this.GetComponent<Transform>();
        waypoints = GameObject.Find("Waypoint Group").GetComponentsInChildren<Transform>();

    }

    // Update is called once per frame
    void Update()
    {

        Ray ray = Camera.main.ScreenPointToRay(ScreenCenter);
        RaycastHit hit;
        CursorGaugeImage.fillAmount = GaugeTimer;

        isTriggered = Input.GetMouseButtonDown(0);//��ġ

        if (Physics.Raycast(ray, out hit, 10.0f))
        {
            if (hit.collider.CompareTag("Start"))
            {
                GaugeTimer += (1.0f / 3.0f) * Time.deltaTime;
                if (GaugeTimer >= 1.0f || isTriggered)
                {
                    hit.transform.gameObject.SetActive(false);

                    SceneManager.LoadScene(1);
                    GaugeTimer = 0.0f;
                    isTriggered = false;

                    GaugeTimer = 0.0f;
                    isTriggered = false;
                }

            }
            else if (hit.collider.CompareTag("Card"))
            {

                if (isTriggered)
                {
                    audioSource.clip = audio1;
                    audioSource.Play();

                    if (!isCardFlipped)
                    {
                        // ī�� ������
                        hit.transform.Rotate(180f, 0f, 0f);
                        isCardFlipped = true;
                        flippedCard = hit.transform.gameObject;
                    }
                    else
                    {
                        // �� ��° ī�� ������
                        hit.transform.Rotate(180f, 0f, 0f);

                        // ī�� ��
                        if (hit.transform.gameObject.name == flippedCard.name)
                        {
                            // ���� ī���� ��� ������� �����
                            StartCoroutine(MatchCards(flippedCard, hit.transform.gameObject));
                        }
                        else
                        {
                            // ��Ī ���� ó��
                            audioSource.clip = audio2;
                            audioSource.Play();

                            // ī�� �ٽ� ������
                            StartCoroutine(FlipCardsBack(flippedCard, hit.transform.gameObject));
                        }

                        isCardFlipped = false;
                        flippedCard = null;
                    }

                    GaugeTimer = 0.0f;
                    isTriggered = false;
                }
            }
            else
            {
                //TextUI.text = "";
                GaugeTimer = 0.0f;
            }

        }
        else
        {
            GaugeTimer = 0.0f;
        }

        switch (moveType)
        {
            case MoveType.WAYPOINT:
                MoveWaypoint();
                break;
            case MoveType.LOOK_AT:
                break;
        }

    }

    IEnumerator MatchCards(GameObject card1, GameObject card2)
    {
        // ��Ī�� ���� ���� ���·� ����
        isMatching = true;

        // ī�� �����ֱ�
        yield return new WaitForSeconds(matchingDelay);

        // ī�� ������� �����
        Destroy(card1);
        Destroy(card2);
        matchedPairs++;

        if (matchedPairs == 15)
        {
            // ��� �������� Ŭ����
            audioSource.clip = audio4;
            audioSource.Play();

            NextStage();


        }
        else if (matchedPairs == 9)
        {
            if (currentStage < totalStages)
            {
                audioSource.clip = audio3;
                audioSource.Play();
                currentStage++;
                NextStage();

            }
        }
        else if (matchedPairs == 5)
        {
            if (currentStage < totalStages)
            {
                audioSource.clip = audio3;
                audioSource.Play();
                currentStage++;
                NextStage();

            }
        }
        else if (matchedPairs == 2)
        {
            if (currentStage < totalStages)
            {
                audioSource.clip = audio3;
                audioSource.Play();
                currentStage++;
                NextStage();

            }
        }

        isMatching = false;
    }

    IEnumerator FlipCardsBack(GameObject card1, GameObject card2)
    {
        yield return new WaitForSeconds(0.7f);

        if (!isMatching)
        {
            card1.transform.Rotate(180f, 0f, 0f);
            card2.transform.Rotate(180f, 0f, 0f);
        }
    }

    void NextStage()
    {

        isGoNext = true;
        Debug.Log("���� ���������� �̵�: " + currentStage);
        if (matchedPairs == 15)
        {
            TextUI.text = "Game clear!";
        }
        else
        {
            TextUI.text = "stage " + currentStage;
        }
        nextIndex = (++nextIndex >= waypoints.Length) ? waypoints.Length : nextIndex;


    }


    void MoveWaypoint()
    {
        if (isGoNext == true)
        {
            Vector3 direction = waypoints[nextIndex].position - playerTransform.position;

            Quaternion goalRotation = Quaternion.LookRotation(direction);

            playerTransform.rotation = Quaternion.Slerp(playerTransform.rotation, goalRotation, Time.deltaTime * damping);

            playerTransform.Translate(Vector3.forward * Time.deltaTime * speed);

        }

    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Waypoint"))
        {
            isGoNext = false;
            if (matchedPairs == 15)
            {
                // ����̵� ��Ű��
                SceneManager.LoadScene(2);
            }
        }
    }
}
