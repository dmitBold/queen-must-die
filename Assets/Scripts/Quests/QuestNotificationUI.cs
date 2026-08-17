using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;
using NightCycle;

// Типы уведомлений
public enum QuestNotificationType
{
    NewQuest,
    StageAdvanced,
    QuestCompleted
}

// Контейнер данных для очереди
public struct QuestNotificationData
{
    public QuestNotificationType Type;
    public string QuestName;
    public string StageName;
    public string PreviousStageName; // Нужно только для StageAdvanced
}

public class QuestNotificationUI : MonoBehaviour
{
    [Header("Ссылки на UI")]
    [SerializeField] private QuestTypewriter tpQuestName;
    [SerializeField] private QuestTypewriter tpQuestStage;

    [Header("Настройки времени")]
    [SerializeField] private float slowFadeDuration = 1.5f;
    [SerializeField] private float fastFadeDuration = 0.5f;
    [SerializeField] private float readDelay = 2.0f; // Время, чтобы игрок успел прочитать
    [SerializeField] private float strikethroughDelay = 0.5f; // Пауза после зачёркивания

    private Queue<QuestNotificationData> notificationQueue = new Queue<QuestNotificationData>();
    private bool isProcessingQueue = false;
    private QuestManager questManager;

    [Inject]
    public void Construct(QuestManager questManager)
    {
        this.questManager = questManager;
        questManager.OnQuestAdded += HandleQuestAdded;
        questManager.OnQuestStageFinished += HandleStageAdvanced;
        questManager.OnQuestFinished += HandleQuestCompleted;
    }

    private void OnDestroy()
    {
        if (questManager != null)
        {
            questManager.OnQuestAdded -= HandleQuestAdded;
            questManager.OnQuestStageFinished -= HandleStageAdvanced;
            questManager.OnQuestFinished -= HandleQuestCompleted;
        }
    }

    private void Start()
    {
        // Прячем текст при старте
        tpQuestName.SetAlphaInstant(0f);
        tpQuestStage.SetAlphaInstant(0f);
        tpQuestName.ClearText();
        tpQuestStage.ClearText();
    }

    // --- ОБРАБОТЧИКИ СОБЫТИЙ (ДОБАВЛЕНИЕ В ОЧЕРЕДЬ) ---

    private void HandleQuestAdded(Quest quest)
    {
        string firstStageText = quest.QuestStages != null && quest.QuestStages.Count > 0 ? quest.QuestStages[0].StageDescription : "";

        EnqueueNotification(new QuestNotificationData
        {
            Type = QuestNotificationType.NewQuest,
            QuestName = quest.QuestName,
            StageName = firstStageText
        });
    }

    private void HandleStageAdvanced(Quest quest, int finishedStageIndex)
    {
        string prevStage = quest.QuestStages[finishedStageIndex].StageDescription;
        string newStage = (finishedStageIndex + 1 < quest.QuestStages.Count) ? quest.QuestStages[finishedStageIndex + 1].StageDescription : "";

        EnqueueNotification(new QuestNotificationData
        {
            Type = QuestNotificationType.StageAdvanced,
            QuestName = quest.QuestName,
            PreviousStageName = prevStage,
            StageName = newStage
        });
    }

    private void HandleQuestCompleted(Quest quest)
    {
        string lastStageText = quest.QuestStages[quest.QuestStages.Count - 1].StageDescription;

        EnqueueNotification(new QuestNotificationData
        {
            Type = QuestNotificationType.QuestCompleted,
            QuestName = quest.QuestName,
            StageName = lastStageText
        });
    }

    // --- ЛОГИКА ОЧЕРЕДИ ---

    private void EnqueueNotification(QuestNotificationData data)
    {
        notificationQueue.Enqueue(data);
        if (!isProcessingQueue)
        {
            StartCoroutine(ProcessQueue());
        }
    }

    private IEnumerator ProcessQueue()
    {
        isProcessingQueue = true;

        while (notificationQueue.Count > 0)
        {
            QuestNotificationData currentNotification = notificationQueue.Dequeue();

            // Сбрасываем альфу перед каждой новой анимацией
            tpQuestName.SetAlphaInstant(0f);
            tpQuestStage.SetAlphaInstant(0f);

            switch (currentNotification.Type)
            {
                case QuestNotificationType.NewQuest:
                    yield return StartCoroutine(PlayNewQuestSequence(currentNotification));
                    break;
                case QuestNotificationType.StageAdvanced:
                    yield return StartCoroutine(PlayStageAdvancedSequence(currentNotification));
                    break;
                case QuestNotificationType.QuestCompleted:
                    yield return StartCoroutine(PlayQuestCompletedSequence(currentNotification));
                    break;
            }

            // Небольшая пауза между уведомлениями, если их скопилось несколько
            yield return new WaitForSeconds(0.5f);

            tpQuestName.ClearText();
            tpQuestStage.ClearText();
        }

        isProcessingQueue = false;
    }

    // --- ПОСЛЕДОВАТЕЛЬНОСТИ АНИМАЦИЙ ---

    private IEnumerator PlayNewQuestSequence(QuestNotificationData data)
    {
        // 1. Печатается название квеста
        tpQuestName.TypeText(data.QuestName);
        yield return new WaitWhile(() => tpQuestName.IsTyping);

        // 2. Печатается название стадии
        tpQuestStage.TypeText(data.StageName);
        yield return new WaitWhile(() => tpQuestStage.IsTyping);

        // Пауза на чтение
        yield return new WaitForSeconds(readDelay);

        // 3. Медленный fade out обоих
        Coroutine fade1 = StartCoroutine(tpQuestName.FadeTo(0f, slowFadeDuration));
        Coroutine fade2 = StartCoroutine(tpQuestStage.FadeTo(0f, slowFadeDuration));
        yield return fade1;
        yield return fade2;
    }

    private IEnumerator PlayStageAdvancedSequence(QuestNotificationData data)
    {
        // 1. Устанавливаем текст без печати (с альфой 0)
        tpQuestName.SetTextInstant(data.QuestName);
        tpQuestStage.SetTextInstant(data.PreviousStageName);

        // 2. Fade in квеста и предыдущей стадии
        Coroutine fade1 = StartCoroutine(tpQuestName.FadeTo(1f, fastFadeDuration));
        Coroutine fade2 = StartCoroutine(tpQuestStage.FadeTo(1f, fastFadeDuration));
        yield return fade1;
        yield return fade2;

        // Пауза, чтобы игрок увидел, что выполнил
        yield return new WaitForSeconds(readDelay / 2);

        // 3. Название стадии зачёркивается
        tpQuestStage.StrikethroughText();
        yield return new WaitForSeconds(strikethroughDelay);

        // 4. Быстрый fade out предыдущей стадии
        yield return StartCoroutine(tpQuestStage.FadeTo(0f, fastFadeDuration));

        // 5. Устанавливаем новую стадию и делаем быстрый fade in
        tpQuestStage.SetTextInstant(data.StageName);
        yield return StartCoroutine(tpQuestStage.FadeTo(1f, fastFadeDuration));

        // Пауза на чтение новой стадии
        yield return new WaitForSeconds(readDelay);

        // 6. Медленный fade out
        fade1 = StartCoroutine(tpQuestName.FadeTo(0f, slowFadeDuration));
        fade2 = StartCoroutine(tpQuestStage.FadeTo(0f, slowFadeDuration));
        yield return fade1;
        yield return fade2;
    }

    private IEnumerator PlayQuestCompletedSequence(QuestNotificationData data)
    {
        // 1. Устанавливаем текст без печати (с альфой 0)
        tpQuestName.SetTextInstant(data.QuestName);
        tpQuestStage.SetTextInstant(data.StageName);

        // 2. Быстрый fade in названия и последней стадии
        Coroutine fade1 = StartCoroutine(tpQuestName.FadeTo(1f, fastFadeDuration));
        Coroutine fade2 = StartCoroutine(tpQuestStage.FadeTo(1f, fastFadeDuration));
        yield return fade1;
        yield return fade2;

        yield return new WaitForSeconds(readDelay / 2);

        // 3. Зачёркивается название стадии
        tpQuestStage.StrikethroughText();
        yield return new WaitForSeconds(strikethroughDelay);

        // 4. Зачёркивается название квеста
        tpQuestName.StrikethroughText();
        yield return new WaitForSeconds(strikethroughDelay * 2);

        // 5. Медленный fade out
        fade1 = StartCoroutine(tpQuestName.FadeTo(0f, slowFadeDuration));
        fade2 = StartCoroutine(tpQuestStage.FadeTo(0f, slowFadeDuration));
        yield return fade1;
        yield return fade2;
    }
}