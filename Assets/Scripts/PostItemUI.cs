
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PostItemUI : MonoBehaviour
{
    public TextMeshProUGUI usernameText;
    public Image profilePicImage;
    public TextMeshProUGUI postContentText;
    public Button likeButton;
    public TextMeshProUGUI likeCountText;

    private PostData currentPostData;
    private bool isLiked = false;
    private int currentLikes = 0;

    public void SetPostData(PostData data)
    {
        currentPostData = data;
        usernameText.text = currentPostData.username;
        postContentText.text = currentPostData.content;
        currentLikes = currentPostData.likes;

        LoadProfilePicture(currentPostData.profilePic);

        likeButton.onClick.RemoveAllListeners();
        likeButton.onClick.AddListener(ToggleLike);

        UpdateLikeDisplay();
    }

    private void LoadProfilePicture(string fileName)
    {
        if (profilePicImage == null)
        {
            Debug.LogError("Profile Pic Image reference is missing on PostItemUI for post: " + currentPostData.username);
            return;
        }

        string spriteName = fileName.Replace(".png", "");
        Sprite loadedSprite = Resources.Load<Sprite>(spriteName);

        if (loadedSprite != null)
        {
            profilePicImage.sprite = loadedSprite;
        }
        else
        {
            Debug.LogWarning($"Profile picture '{fileName}' not found in any Resources folder for user: {currentPostData.username}. Using default/placeholder.");
        }
    }

    private void ToggleLike()
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

    private void UpdateLikeDisplay()
    {
        if (isLiked)
        {
            likeButton.image.color = Color.red;
            TextMeshProUGUI buttonText = likeButton.GetComponentInChildren<TextMeshProUGUI>();
            if (buttonText != null)
            {
                buttonText.text = "Liked";
            }
        }
        else
        {
            likeButton.image.color = Color.white;
            TextMeshProUGUI buttonText = likeButton.GetComponentInChildren<TextMeshProUGUI>();
            if (buttonText != null)
            {
                buttonText.text = "Like";
            }
        }

        if (likeCountText != null)
        {
            likeCountText.text = currentLikes + " Likes";
        }
    }
}