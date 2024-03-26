using System.Collections.Generic;
using UnityEngine;

public class Gameplay : MonoBehaviour
{
    // Static variable to track if the game is paused
    public static bool GameIsPaused { get; private set; } = false;
    public GameObject background; // The background object

    // References to game objects in the scene
    private GameObject creatureBody; // The player-controlled creature
    private GameObject enemyCreature; // The enemy creature
    private float moveSpeed = 4f; // Speed at which the player creature moves
    private float enemyFollowSpeed = 3f; // Speed at which the enemy creature follows the player

    // Reference to the MainMenuManager script
    private MainMenuManager mainMenuManager;

    // Reference to the main camera
    private Camera mainCamera;

    private void Start()
    {
        // Get references to MainMenuManager and the main camera
        mainMenuManager = GetComponent<MainMenuManager>();
        mainCamera = Camera.main;
    }

    private void Update()
    {
        // Toggle pause when the Escape key is pressed
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            TogglePause();
        }

        // Handle movement and following behaviors when the game is not paused and the player creature exists
        if (!GameIsPaused && creatureBody != null)
        {
            HandleMovement(); // Handle player creature movement
            FollowCreature(); // Adjust camera position to follow the player creature
        }

        // If the game is not paused and both player and enemy creatures exist, make the enemy creature follow the player
        if (!GameIsPaused && enemyCreature != null && creatureBody != null)
        {
            FollowPlayerCreature();

            // Check the distance between player and enemy creature
            float distance = Vector3.Distance(creatureBody.transform.position, enemyCreature.transform.position);

            // If the enemy creature is far enough from the player, despawn it and create a new one
            if (distance > despawnDistance)
            {
                Destroy(enemyCreature);
                CreateRandomEnemy();
            }
        }
    }

    // Define a variable for the despawn distance
    public float despawnDistance = 10f;


    // Initialize the game with a loaded world
    // Initialize the game with a loaded world
    public void InitGame(World loadedWorld)
    {
        // Get the selected features and create the player creature
        List<CreatureFeature> selectedFeatures = CreatureManager.GetFeaturesFromWorld(loadedWorld, "SelectedFeatures");
        creatureBody = CreatureManager.CreateSelectedFeatureModel(selectedFeatures);

        // Create random enemy creature
        CreateRandomEnemy();

        // Show gameplay UI
        mainMenuManager.ShowScreen(MenuScreen.Gameplay);

        // Unpause the game
        GameIsPaused = false;

        // Ensure time scale is set to normal
        Time.timeScale = 1f;

        // Adjust enemy creature speed based on world difficulty
        AdjustEnemyCreatureSpeed(loadedWorld.WorldDifficulty);

        // Calculate stats for the player creature
        (int totalHealth, int totalSpeed, int totalStrength) = CreatureManager.CalculateStats(loadedWorld);

        // Increase player creature's speed based on totalSpeed
        float speedIncreasePercentage = totalSpeed * 0.01f; // Convert totalSpeed to percentage
        moveSpeed *= (1f + speedIncreasePercentage); // Increase move speed by the percentage
    }


    // Method to adjust the speed of the enemy creature based on world difficulty
    private void AdjustEnemyCreatureSpeed(string worldDifficulty)
    {
        // Define speed modifiers based on difficulty levels
        float easyModifier = 0.1f;
        float mediumModifier = 0.05f;

        // Check the world difficulty and adjust enemy creature speed accordingly
        switch (worldDifficulty)
        {
            case "Easy":
                enemyFollowSpeed -= enemyFollowSpeed * easyModifier;
                break;
            case "Medium":
                enemyFollowSpeed -= enemyFollowSpeed * mediumModifier;
                break;
            case "Hard":
                // No change in enemy creature speed for "Hard" difficulty
                break;
            default:
                Debug.LogWarning("Unknown difficulty level. No adjustment made to enemy creature speed.");
                break;
        }
    }

    // Transition to creature editor
    public void EditCreature()
    {
        TogglePause(); // Pause the game
        Destroy(creatureBody); // Destroy player creature
        Destroy(enemyCreature); // Destroy enemy creature
        mainMenuManager.ShowScreen(MenuScreen.Creator); // Show creature editor UI
    }

    // Quit the game
    public void QuitGame()
    {
        Destroy(creatureBody); // Destroy player creature
        Destroy(enemyCreature); // Destroy enemy creature
        mainMenuManager.ShowScreen(MenuScreen.PlayScreen); // Show main menu UI
        GameIsPaused = false; // Unpause the game
    }

    // Toggle pause state
    public void TogglePause()
    {
        GameIsPaused = !GameIsPaused;
        Time.timeScale = GameIsPaused ? 0f : 1f; // Pause/unpause time
        mainMenuManager.ShowScreen(GameIsPaused ? MenuScreen.Pause : MenuScreen.Gameplay); // Show appropriate UI screen
    }

    // Handle player creature movement
    private void HandleMovement()
    {
        Vector2 input = GetInput(); // Get input from player
        MoveCreature(input); // Move player creature
        RotateCreature(input); // Rotate player creature
        RotateFin(input, creatureBody); // Rotate fin of player creature
    }

    // Get player input
    private Vector2 GetInput()
    {
        return new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
    }

    // Move the player creature
    private void MoveCreature(Vector2 input)
    {
        creatureBody.transform.Translate(moveSpeed * Time.deltaTime * (Vector3)input);
    }

    // Rotate the player creature based on input
    private void RotateCreature(Vector2 input)
    {
        if (input.x != 0)
        {
            creatureBody.transform.Rotate(Vector3.forward, -input.x * 180f * Time.deltaTime);
        }
    }

    // Rotate the fin of the creature based on input
    private void RotateFin(Vector2 input, GameObject target)
    {
        Transform finTransform = target.transform.Find("Fin");

        if (input.x != 0)
        {
            float rotationAngle = input.x < 0 ? -27f : 27f; // Determine rotation angle based on input
            finTransform.localRotation = Quaternion.Euler(0f, 0f, rotationAngle);
        }
        else if (input.y != 0)
        {
            float rotationAngle = Mathf.Sin(Time.time * 8f) * 27f; // Rotate fin sinusoidally if there's vertical input
            finTransform.localRotation = Quaternion.Euler(0f, 0f, rotationAngle);
        }
        else
        {
            finTransform.localRotation = Quaternion.identity; // Reset fin rotation if there's no input
        }
    }

    // Follow the player creature with the camera
    private void FollowCreature()
    {
        Vector3 desiredCameraPosition = creatureBody.transform.position + new Vector3(0f, 0f, -10f); // Calculate desired camera position
        mainCamera.transform.position = Vector3.Lerp(mainCamera.transform.position, desiredCameraPosition, Time.deltaTime * 5f); // Smoothly move camera

        //Keep background aligned with camera
        Vector3 backgroundPosition = mainCamera.transform.position;
        backgroundPosition.z = background.transform.position.z;
        background.transform.position = backgroundPosition;
    }
    // Create a random enemy creature near the player
    private void CreateRandomEnemy()
    {
        float spawnRadius = 5f; // Define a radius for the enemy spawn distance from the player

        // Calculate a random angle around the player creature
        float randomAngle = Random.Range(0f, Mathf.PI * 2f);

        // Calculate the spawn position based on the random angle and spawn radius
        Vector3 spawnPosition = creatureBody.transform.position + new Vector3(Mathf.Cos(randomAngle), Mathf.Sin(randomAngle)) * spawnRadius;

        // Instantiate the enemy creature at the calculated spawn position
        List<CreatureFeature> enemyFeatures = new()
        {
        CreatureManager.SelectRandomFeature("Fins"), // Select a random feature for enemy fins
        CreatureManager.SelectRandomFeature("Eyes"), // Select a random feature for enemy eyes
        CreatureManager.SelectRandomFeature("Body") // Select a random feature for enemy body
    };
        enemyCreature = CreatureManager.CreateSelectedFeatureModel(enemyFeatures); // Create enemy creature

        // Set the position of the enemy creature
        if (enemyCreature != null)
        {
            enemyCreature.transform.position = spawnPosition; // Set enemy position
        }
    }

    // Make the enemy creature follow the player creature
    private void FollowPlayerCreature()
    {
        Vector3 directionToPlayer = (creatureBody.transform.position - enemyCreature.transform.position).normalized;
        Quaternion targetRotation = Quaternion.LookRotation(Vector3.forward, directionToPlayer);
        enemyCreature.transform.rotation = Quaternion.Slerp(enemyCreature.transform.rotation, targetRotation, Time.deltaTime * enemyFollowSpeed);

        RotateFin(directionToPlayer, enemyCreature);

        Vector3 desiredEnemyPosition = creatureBody.transform.position - (creatureBody.transform.position - enemyCreature.transform.position) * 0.5f;

        // Calculate the speed ratio between the enemy and the player
        float speedRatio = enemyFollowSpeed / moveSpeed;

        // Move the enemy towards the desired position based on the speed ratio
        enemyCreature.transform.position = Vector3.MoveTowards(enemyCreature.transform.position, desiredEnemyPosition, moveSpeed * speedRatio * Time.deltaTime);
    }

    // Rotate the fin of the creature based on movement direction
    private void RotateFin(Vector3 moveDirection, GameObject target)
    {
        Transform finTransform = target.transform.Find("Fin");

        if (moveDirection.y != 0)
        {
            float rotationAngle = Mathf.Sin(Time.time * 8f) * 27f; // Rotate fin sinusoidally if there's vertical movement
            finTransform.localRotation = Quaternion.Euler(0f, 0f, rotationAngle);
        }
        else
        {
            finTransform.localRotation = Quaternion.identity; // Reset fin rotation if there's no vertical movement
        }
    }
}
