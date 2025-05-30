using UnityEngine;
using UnityEngine.UI; // For ScrollRect, LayoutRebuilder
using TMPro;
using System.Collections.Generic; // To use List<T>

public class SocialFeedManager : MonoBehaviour
{
    [Header("Feed UI References")]
    [Tooltip("Drag the 'Content' GameObject from your Scroll View here.")]
    public RectTransform contentParent; // <--- CHANGED FROM Transform TO RectTransform

    [Tooltip("Drag your Social Post Prefab here from the Project window.")]
    public GameObject socialPostPrefab;

    [Tooltip("Drag the ScrollRect component from your SocialFeedScrollView here.")]
    public ScrollRect scrollView;

    [Header("Data Source")]
    [Tooltip("Drag your posts_data.json TextAsset here from the Project window.")]
    public TextAsset postsJsonFile;

    private AllPostsData allLoadedPosts;
    private List<RectTransform> instantiatedPostRects = new List<RectTransform>();
    private int currentVisiblePostIndex = 0;
    private bool isSnapping = false;

    [Header("Scrolling & Snapping Behavior")]
    public float snapSpeed = 8f;

    [Tooltip("Approximate height of a single post prefab (including padding/margin within prefab).")]
    public float singlePostHeight = 1000f;

    [Tooltip("Spacing between posts as set in Vertical Layout Group.")]
    public float postSpacing = 50f;

    private float _scrollTargetNormalizedPosition;

    void Start()
    {
        // IMPORTANT: Check that contentParent and scrollView are assigned
        if (contentParent == null)
        {
            Debug.LogError("Content Parent (RectTransform) not assigned in Inspector!");
            return;
        }
        if (scrollView == null)
        {
            Debug.LogError("Scroll View (ScrollRect) not assigned in Inspector!");
            return;
        }

        LoadAllPostsDataFromTextAsset();

        if (allLoadedPosts != null && allLoadedPosts.posts != null)
        {
            InstantiateAllPosts();
        }

        currentVisiblePostIndex = 0;
        SnapToPost(currentVisiblePostIndex, true);

        scrollView.onValueChanged.RemoveAllListeners();
    }

    void Update()
    {
        float scrollDelta = Input.mouseScrollDelta.y;

        if (!isSnapping && scrollDelta != 0)
        {
            if (scrollDelta < 0)
            {
                MoveToNextPost();
            }
            else if (scrollDelta > 0)
            {
                MoveToPreviousPost();
            }
        }

        if (isSnapping)
        {
            scrollView.verticalNormalizedPosition = Mathf.Lerp(
                scrollView.verticalNormalizedPosition,
                _scrollTargetNormalizedPosition,
                snapSpeed * Time.deltaTime
            );

            if (Mathf.Abs(scrollView.verticalNormalizedPosition - _scrollTargetNormalizedPosition) < 0.001f)
            {
                scrollView.verticalNormalizedPosition = _scrollTargetNormalizedPosition;
                isSnapping = false;
            }
        }
    }

    private void MoveToNextPost()
    {
        if (allLoadedPosts == null || allLoadedPosts.posts == null) return;

        if (currentVisiblePostIndex < allLoadedPosts.posts.Length - 1)
        {
            currentVisiblePostIndex++;
            SnapToPost(currentVisiblePostIndex);
        }
        else
        {
            Debug.Log("Already at the last post.");
        }
    }

    private void MoveToPreviousPost()
    {
        if (currentVisiblePostIndex > 0)
        {
            currentVisiblePostIndex--;
            SnapToPost(currentVisiblePostIndex);
        }
        else
        {
            Debug.Log("Already at the first post.");
        }
    }

    private void SnapToPost(int targetIndex, bool instant = false)
    {
        if (targetIndex < 0 || targetIndex >= instantiatedPostRects.Count || contentParent == null || scrollView == null || scrollView.viewport == null)
        {
            Debug.LogWarning("Invalid post index or missing UI references for snapping.");
            return;
        }

        LayoutRebuilder.ForceRebuildLayoutImmediate(contentParent.GetComponent<RectTransform>());

        float contentTotalHeight = contentParent.rect.height;
        float viewportHeight = scrollView.viewport.rect.height;

        // Calculate the Y position the content needs to be at to center the target post.
        float targetContentYPosition = (targetIndex * (singlePostHeight + postSpacing));
        // We need to shift this to center in the viewport if viewportHeight is less than contentHeight
        targetContentYPosition -= (viewportHeight / 2f) - (singlePostHeight / 2f);


        float maxScrollY = contentTotalHeight - viewportHeight;
        if (maxScrollY <= 0)
        {
            // If content fits within viewport, stay at top (normalized 1)
            _scrollTargetNormalizedPosition = 1f;
        }
        else
        {
            // Calculate normalized position (0 at bottom, 1 at top)
            // targetContentYPosition is the desired Y of the top edge of the viewport's relative to content's top.
            _scrollTargetNormalizedPosition = 1f - (targetContentYPosition / maxScrollY);
            _scrollTargetNormalizedPosition = Mathf.Clamp01(_scrollTargetNormalizedPosition); // Clamp between 0 and 1
        }


        _scrollTargetNormalizedPosition = Mathf.Clamp01(_scrollTargetNormalizedPosition);
        isSnapping = true;

        if (instant)
        {
            scrollView.verticalNormalizedPosition = _scrollTargetNormalizedPosition;
            isSnapping = false;
        }
        Debug.Log($"Snapping to Post {targetIndex}. Target Normalized Y: {_scrollTargetNormalizedPosition}");
    }

    private void InstantiateAllPosts()
    {
        foreach (RectTransform rect in instantiatedPostRects)
        {
            Destroy(rect.gameObject);
        }
        instantiatedPostRects.Clear();

        if (allLoadedPosts != null && allLoadedPosts.posts != null && allLoadedPosts.posts.Length > 0)
        {
            foreach (PostData post in allLoadedPosts.posts)
            {
                GameObject postGO = Instantiate(socialPostPrefab, contentParent);
                RectTransform postRect = postGO.GetComponent<RectTransform>();
                instantiatedPostRects.Add(postRect);

                PostItemUI postItem = postGO.GetComponent<PostItemUI>();
                if (postItem != null)
                {
                    postItem.SetPostData(post);
                }
                else
                {
                    Debug.LogError("PostItemUI script not found on socialPostPrefab! Cannot set post data.");
                }
            }
        }
        else
        {
            Debug.LogWarning("No posts available to display for instantiation.");
        }
    }

    void LoadAllPostsDataFromTextAsset()
    {
        if (postsJsonFile == null)
        {
            Debug.LogError("No JSON TextAsset assigned to 'Posts Json File' in the Inspector!");
            allLoadedPosts = null;
            return;
        }

        string jsonString = postsJsonFile.text;

        try
        {
            AllPostsData allPostsWrapper = JsonUtility.FromJson<AllPostsData>(jsonString);

            if (allPostsWrapper != null && allPostsWrapper.posts != null && allPostsWrapper.posts.Length > 0)
            {
                allLoadedPosts = allPostsWrapper;
                Debug.Log($"Successfully parsed {allLoadedPosts.posts.Length} posts from JSON.");
            }
            else if (allPostsWrapper != null && allPostsWrapper.posts != null && allPostsWrapper.posts.Length == 0)
            {
                Debug.LogWarning("JSON file contains an empty 'posts' array. No posts available to display.");
                allLoadedPosts = null;
            }
            else
            {
                PostData singlePost = JsonUtility.FromJson<PostData>(jsonString);
                if (singlePost != null)
                {
                    allLoadedPosts = new AllPostsData { posts = new PostData[] { singlePost } };
                    Debug.LogWarning("Loaded JSON as a single post object. Wrapped it in an array for consistent loading.");
                }
                else
                {
                    Debug.LogError("JSON file is malformed or not recognized as a 'posts' array wrapper or single post object.");
                    allLoadedPosts = null;
                }
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error parsing JSON from TextAsset '{postsJsonFile.name}': {e.Message}");
            allLoadedPosts = null;
        }
    }
}