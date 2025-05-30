using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SocialPostManager : MonoBehaviour
{
    // --- Public UI References (Drag & Drop in Inspector) ---
    public TextMeshProUGUI usernameText;
    public Image profilePicImage;
    public TextMeshProUGUI postContentText;
    public Button likeButton;
    public TextMeshProUGUI likeCountText;

    // --- Data Source (Assign your .json TextAsset here) ---
    [Header("Data Source")]
    [Tooltip("Drag your posts_data.json (or single_post.json) TextAsset here from the Project window.")]
    public TextAsset postJsonFile;

    // --- Private Internal State ---
    private bool isLiked = false;
    private int currentLikes = 0;

    private PostData loadedPostData; // Holds the currently displayed post's data.

    // --- Unity Lifecycle Method ---
    void Start()
    {
        // Attempt to load all post data from the assigned JSON TextAsset.
        LoadAllPostsDataFromTextAsset();

        // If data was successfully loaded and there's at least one post,
        // we'll display the first one.
        if (loadedPostData != null)
        {
            usernameText.text = loadedPostData.username;
            postContentText.text = loadedPostData.content; // --- TYPO FIXED HERE (AGAIN)! ---
            currentLikes = loadedPostData.likes;

            // --- Profile Picture (Manual Assignment or Advanced Loading) ---
            // For now, assign a default profile picture to the 'Profile Pic Image'
            // slot in the Inspector if you don't have dynamic image loading set up.
            // If you did have images in a 'Resources' folder, you might do:
            // profilePicImage.sprite = Resources.Load<Sprite>(loadedPostData.profilePic.Replace(".png", ""));
        }
        else
        {
            Debug.LogError("Failed to load post data! Ensure 'postJsonFile' is assigned and JSON is valid.");
            usernameText.text = "Error User";
            postContentText.text = "Failed to load content.";
            currentLikes = 0; // Default to 0 likes on error.
        }

        // Set up the listener for the like button click.
        likeButton.onClick.AddListener(ToggleLike);

        // Update the UI initially based on the loaded (or default) data.
        UpdateLikeDisplay();
    }

    // --- Custom Methods ---

    // Loads and parses the JSON data from the assigned TextAsset.
    // This method now expects an array of posts wrapped in a root object.
    void LoadAllPostsDataFromTextAsset()
    {
        if (postJsonFile == null)
        {
            Debug.LogError("No JSON TextAsset assigned to 'Post Json File' in the Inspector!");
            loadedPostData = null;
            return;
        }

        string jsonString = postJsonFile.text;

        try
        {
            // First, try to deserialize as an AllPostsData wrapper (for posts_data.json format).
            AllPostsData allPostsWrapper = JsonUtility.FromJson<AllPostsData>(jsonString);

            if (allPostsWrapper != null && allPostsWrapper.posts != null && allPostsWrapper.posts.Length > 0)
            {
                // If it successfully parsed as an array wrapper and has posts, display the first one.
                loadedPostData = allPostsWrapper.posts[0];
                Debug.Log($"Successfully loaded {allPostsWrapper.posts.Length} posts. Displaying the first post.");
            }
            else if (allPostsWrapper != null && allPostsWrapper.posts != null && allPostsWrapper.posts.Length == 0)
            {
                Debug.LogWarning("JSON file contains an empty 'posts' array. Using default values.");
                loadedPostData = null; // No posts to display.
            }
            else
            {
                // If it failed to parse as AllPostsData, it might be a single PostData object (like single_post.json).
                // Try to parse it as a single PostData object.
                PostData singlePost = JsonUtility.FromJson<PostData>(jsonString);
                if (singlePost != null)
                {
                    loadedPostData = singlePost;
                    Debug.LogWarning("Loaded JSON as a single post object (not an array wrapper).");
                }
                else
                {
                    // If neither worked, the JSON is likely malformed or in an unexpected format.
                    Debug.LogError("JSON file is malformed or not recognized as a single post or a 'posts' array wrapper.");
                    loadedPostData = null;
                }
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error parsing JSON from TextAsset '{postJsonFile.name}': {e.Message}");
            loadedPostData = null; // Indicate parsing failure.
        }
    }

    // Toggles the like state and updates the like count.
    void ToggleLike()
    {
        isLiked = !isLiked;

        if (isLiked)
        {
            currentLikes++;
        }
        else
        {
            currentLikes--;
        }

        UpdateLikeDisplay();
    }

    // Updates the visual appearance of the like button and count text.
    void UpdateLikeDisplay()
    {
        if (isLiked)
        {
            likeButton.image.color = Color.red;
            likeButton.GetComponentInChildren<TextMeshProUGUI>().text = "Liked";
        }
        else
        {
            likeButton.image.color = Color.white;
            likeButton.GetComponentInChildren<TextMeshProUGUI>().text = "Like";
        }

        if (likeCountText != null)
        {
            likeCountText.text = currentLikes + " Likes";
        }
    }
}

