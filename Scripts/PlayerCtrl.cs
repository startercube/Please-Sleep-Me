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
    private AudioSource audioSource;    //배경음악
    public AudioClip audio1;    //카드 선택
    public AudioClip audio2;    //매칭실패
    public AudioClip audio3;    //매칭성공
    public AudioClip audio4;    //스테이지 클리어

    private bool isCardFlipped = false;
    private GameObject flippedCard;
    private int matchedPairs = 0;

    private bool isMatching = false; // 매칭이 진행 중인지 여부
    private float matchingDelay = 0.5f; // 매칭 시간 지연 (초)

    private int currentStage = 1;
    private int totalStages = 4; // 전체 스테이지 수

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

        isTriggered = Input.GetMouseButtonDown(0);//터치

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
                        // 카드 뒤집기
                        hit.transform.Rotate(180f, 0f, 0f);
                        isCardFlipped = true;
                        flippedCard = hit.transform.gameObject;
                    }
                    else
                    {
                        // 두 번째 카드 뒤집기
                        hit.transform.Rotate(180f, 0f, 0f);

                        // 카드 비교
                        if (hit.transform.gameObject.name == flippedCard.name)
                        {
                            // 같은 카드일 경우 사라지게 만들기
                            StartCoroutine(MatchCards(flippedCard, hit.transform.gameObject));
                        }
                        else
                        {
                            // 매칭 실패 처리
                            audioSource.clip = audio2;
                            audioSource.Play();

                            // 카드 다시 뒤집기
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
        // 매칭이 진행 중인 상태로 설정
        isMatching = true;

        // 카드 보여주기
        yield return new WaitForSeconds(matchingDelay);

        // 카드 사라지게 만들기
        Destroy(card1);
        Destroy(card2);
        matchedPairs++;

        if (matchedPairs == 15)
        {
            // 모든 스테이지 클리어
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
        Debug.Log("다음 스테이지로 이동: " + currentStage);
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
                // 장면이동 시키기
                SceneManager.LoadScene(2);
            }
        }
    }
}
