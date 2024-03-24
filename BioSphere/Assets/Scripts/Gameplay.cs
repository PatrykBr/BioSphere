using System.Collections.Generic;
using UnityEngine;

public class Gameplay : MonoBehaviour
{
    public static GameObject creatureBody;
    public static GameObject enemyCreature;
    public Camera mainCamera;
    public float moveSpeed = 5f;
    public GameObject background;
    public float enemyFollowSpeed = 2f;

    private void Start()
    {
        mainCamera = Camera.main;
        //InitializeGame();
    }

    private void Update()
    {
        if (creatureBody != null)
        {
            HandleMovement();
            FollowCreature();
        }

        if (enemyCreature != null && creatureBody != null)
        {
            FollowPlayerCreature();
        }
    }

    public static void InitGame(World loadedWorld)
    {
        List<CreatureFeature> selectedFeatures = CreatureManager.GetFeaturesFromWorld(loadedWorld, "SelectedFeatures");
        creatureBody = CreatureManager.CreateSelectedFeatureModel(selectedFeatures);
        CreateRandomEnemy();
    }

    private void HandleMovement()
    {
        Vector2 input = GetInput();
        MoveCreature(input);
        RotateCreature(input);
        RotateFin(input, creatureBody);
    }

    private Vector2 GetInput()
    {
        return new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
    }

    private void MoveCreature(Vector2 input)
    {
        creatureBody.transform.Translate(new Vector3(input.x, input.y, 0f) * moveSpeed * Time.deltaTime);
    }

    private void RotateCreature(Vector2 input)
    {
        if (input.x != 0)
        {
            creatureBody.transform.Rotate(Vector3.forward, -input.x * 180f * Time.deltaTime);
        }
    }

    private void RotateFin(Vector2 input, GameObject target)
    {
        Transform finTransform = target.transform.Find("Fin");

        if (input.x != 0)
        {
            float rotationAngle = input.x < 0 ? -27f : 27f;
            finTransform.localRotation = Quaternion.Euler(0f, 0f, rotationAngle);
        }
        else if (input.y != 0)
        {
            float rotationAngle = Mathf.Sin(Time.time * 8f) * 27f;
            finTransform.localRotation = Quaternion.Euler(0f, 0f, rotationAngle);
        }
        else
        {
            finTransform.localRotation = Quaternion.identity;
        }
    }

    private void FollowCreature()
    {
        Vector3 desiredCameraPosition = creatureBody.transform.position + new Vector3(0f, 0f, -10f);
        mainCamera.transform.position = Vector3.Lerp(mainCamera.transform.position, desiredCameraPosition, Time.deltaTime * 5f);

        Vector3 backgroundPosition = mainCamera.transform.position;
        backgroundPosition.z = background.transform.position.z;
        background.transform.position = backgroundPosition;
    }

    private static void CreateRandomEnemy()
    {
        List<CreatureFeature> enemyFeatures = new List<CreatureFeature>();

        enemyFeatures.Add(CreatureManager.SelectRandomFeature("Fins"));
        enemyFeatures.Add(CreatureManager.SelectRandomFeature("Eyes"));
        enemyFeatures.Add(CreatureManager.SelectRandomFeature("Body"));

        enemyCreature = CreatureManager.CreateSelectedFeatureModel(enemyFeatures);
    }

    private void FollowPlayerCreature()
    {
        Vector3 directionToPlayer = (creatureBody.transform.position - enemyCreature.transform.position).normalized;
        Quaternion targetRotation = Quaternion.LookRotation(Vector3.forward, directionToPlayer);
        enemyCreature.transform.rotation = Quaternion.Slerp(enemyCreature.transform.rotation, targetRotation, Time.deltaTime * enemyFollowSpeed);

        RotateFin(directionToPlayer, enemyCreature);

        Vector3 desiredEnemyPosition = creatureBody.transform.position - (creatureBody.transform.position - enemyCreature.transform.position) * 0.5f;
        enemyCreature.transform.position = Vector3.Lerp(enemyCreature.transform.position, desiredEnemyPosition, Time.deltaTime * enemyFollowSpeed);
    }

    private void RotateFin(Vector3 moveDirection, GameObject target)
    {
        Transform finTransform = target.transform.Find("Fin");

        if (moveDirection.y != 0)
        {
            float rotationAngle = Mathf.Sin(Time.time * 8f) * 27f;
            finTransform.localRotation = Quaternion.Euler(0f, 0f, rotationAngle);
        }
        else
        {
            finTransform.localRotation = Quaternion.identity;
        }
    }
}
