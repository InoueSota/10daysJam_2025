using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class CreateStage : MonoBehaviour
{
    [Header("CSV")]
    [SerializeField] private TextAsset csvFile;
    private List<string[]> csvDatas = new List<string[]>();
    private List<int> ints = new List<int>();

    [Header("�����Ώ�")]
    [SerializeField] private GameObject verticalLaserPrefab;
    [SerializeField] private GameObject horizontalLaserPrefab;

    [Header("�����ɕK�v�ȏ��Q")]
    [SerializeField] private float tileSize;

    // �����C���^�[�o��
    private float createIntervalTimer;

    // �����J�n�t���O
    private bool canSpawn;


    void Start()
    {
        canSpawn = false;

        LoadEnemyData();
    }

    void Update()
    {
        if (canSpawn)
        {
            CreateBullet();
        }
    }
    void CreateBullet()
    {
        if (createIntervalTimer <= 0.0f)
        {
            for (int i = 0; i < csvDatas.Count; i++)
            {
                if (ints[i] == 1)
                {
                    // �c���[�U�[
                    if (0 < int.Parse(csvDatas[i][0]) && int.Parse(csvDatas[i][0]) < 4)
                    {
                        // �e�̈ꎞ����
                        GameObject laser = Instantiate(verticalLaserPrefab);

                        if (int.Parse(csvDatas[i][0]) == 1) { laser.transform.position = new(-tileSize, 0f, 0f); }
                        if (int.Parse(csvDatas[i][0]) == 2) { laser.transform.position = new(0f, 0f, 0f); }
                        if (int.Parse(csvDatas[i][0]) == 3) { laser.transform.position = new(tileSize, 0f, 0f); }
                    }
                    // �c���[�U�[
                    else if (3 < int.Parse(csvDatas[i][0]))
                    {
                        // �e�̈ꎞ����
                        GameObject laser = Instantiate(horizontalLaserPrefab);

                        if (int.Parse(csvDatas[i][0]) == 4) { laser.transform.position = new(0f, tileSize, 0f); }
                        if (int.Parse(csvDatas[i][0]) == 5) { laser.transform.position = new(0f, 0f, 0f); }
                        if (int.Parse(csvDatas[i][0]) == 6) { laser.transform.position = new(0f, -tileSize, 0f); }
                    }

                    // �C���^�[�o���̍Đݒ�
                    createIntervalTimer = float.Parse(csvDatas[i][1]);

                    // �������t���O�̏���
                    ints[i] = 0;
                    break;
                }
            }
        }
        else
        {
            createIntervalTimer -= Time.deltaTime;
        }
    }

    void LoadEnemyData()
    {
        StringReader reader = new StringReader(csvFile.text);
        csvDatas.Clear();
        ints.Clear();

        while (reader.Peek() != -1)
        {
            string line = reader.ReadLine();
            csvDatas.Add(line.Split(','));
            ints.Add(1);
        }
    }

    // Setter
    public void SetCanSpawn()
    {
        canSpawn = true;
    }

    // Getter
    public bool IsAllFinish()
    {
        for (int i = 0; i < csvDatas.Count; i++)
        {
            if (ints[i] == 1)
            {
                return false;
            }
        }
        return true;
    }
}
