using System;
[Serializable]
public class PostData
{
    public string username;
    public string profilePic;
    public string content;
    public int likes;
}

[Serializable]
public class AllPostsData
{
    public PostData[] posts;
}
