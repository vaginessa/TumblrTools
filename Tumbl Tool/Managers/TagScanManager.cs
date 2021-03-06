﻿/* 01010011 01101000 01101001 01101110 01101111  01000001 01101101 01100001 01101011 01110101 01110011 01100001
 *
 *  Project: Tumblr Tools - Image parser and downloader from Tumblr blog system
 *
 *  Author: Shino Amakusa
 *
 *  Created: 2013
 *
 *  Last Updated: March, 2017
 *
 * 01010011 01101000 01101001 01101110 01101111  01000001 01101101 01100001 01101011 01110101 01110011 01100001 */

using System.Collections.Generic;
using Tumblr_Tool.Enums;
using Tumblr_Tool.Helpers;
using Tumblr_Tool.Objects.Tumblr_Objects;

namespace Tumblr_Tool.Managers
{
    public class TagScanManager
    {
        public TagScanManager(TumblrBlog blog = null, bool photoPostOnly = false)
        {
            Blog = blog;
            DocumentManager = new DocumentManager();
            TumblrUrl = WebHelper.RemoveTrailingBackslash(blog.Url);
            TumblrDomain = WebHelper.GetDomainName(blog.Url);
            TagList = new HashSet<string>();
            PhotoPostOnly = photoPostOnly;
        }

        public TumblrApiVersion ApiVersion { get; set; }
        public TumblrBlog Blog { get; set; }
        public bool IsCancelled { get; set; }
        public int NumberOfParsedPosts { get; set; }
        public int PercentComplete { get; private set; }
        public ProcessingCode ProcessingStatusCode { get; set; }
        public HashSet<string> TagList { get; set; }
        public int TotalNumberOfPosts { get; set; }
        public string TumblrDomain { get; set; }
        private int ApiQueryOffset { get; set; }
        private int ApiQueryPostLimit { get; set; }
        private DocumentManager DocumentManager { get; set; }
        private string TumblrUrl { get; set; }
        private bool PhotoPostOnly { get; set; }

        /// <summary>
        ///
        /// </summary>
        /// <returns></returns>
        public bool GetTumblrBlogInfo()
        {
            try
            {
                TumblrPostType postType = PhotoPostOnly == true ? TumblrPostType.Photo : TumblrPostType.All;
                return DocumentManager.GetRemoteBlogInfo(TumblrApiHelper.GeneratePostTypeQueryUrl(TumblrDomain, postType, 0, 1), Blog);
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="parseMode"></param>
        /// <returns></returns>
        public TumblrBlog ScanTags()
        {
            try
            {
                ProcessingStatusCode = ProcessingCode.Crawling;

                Blog.Posts = Blog.Posts ?? new HashSet<TumblrPost>();

                var numPostsPerDocument = (int)NumberOfPostsPerApiDocument.ApiV2;

                if (TotalNumberOfPosts == 0)
                    TotalNumberOfPosts = Blog.TotalPosts;

                PercentComplete = 0;

                while (ApiQueryOffset < TotalNumberOfPosts && !IsCancelled)
                {
                    HashSet<TumblrPost> posts = GetTumblrPostList(ApiQueryOffset);

                    foreach (TumblrPost post in posts)
                    {
                        lock (TagList)
                        {
                            if (post.Tags != null) TagList.UnionWith(post.Tags);
                        }
                    }

                    NumberOfParsedPosts += posts.Count;

                    NumberOfParsedPosts = (NumberOfParsedPosts > TotalNumberOfPosts) ? TotalNumberOfPosts : NumberOfParsedPosts;

                    PercentComplete = TotalNumberOfPosts > 0 ? (int)((NumberOfParsedPosts / (double)TotalNumberOfPosts) * 100.00) : 0;
                    ApiQueryOffset += numPostsPerDocument;

                    Blog.Posts = new HashSet<TumblrPost>();
                }

                return Blog;
            }
            catch
            {
                return Blog;
            }
        }

        /// <summary>
        ///
        /// </summary>
        /// <returns></returns>
        public bool TumblrExists()
        {
            try
            {
                var url = TumblrApiHelper.GeneratePostTypeQueryUrl(TumblrDomain, TumblrPostType.All, 0, 1);

                return url.TumblrExists();
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        ///
        /// 
        /// 
        /// 
        /// </summary>
        /// <param name="offset"></param>
        /// <returns></returns>
        private HashSet<TumblrPost> GetTumblrPostList(int offset = 0)
        {
            try
            {
                TumblrPostType postType = PhotoPostOnly == true ? TumblrPostType.Photo : TumblrPostType.All;
                var query = TumblrApiHelper.GeneratePostTypeQueryUrl(TumblrDomain, postType, offset);

                DocumentManager.GetRemoteDocument(query);

                if ((ApiVersion == TumblrApiVersion.V2Json && DocumentManager.JsonDocument != null))
                {
                    HashSet<TumblrPost> posts = DocumentManager.GetPostListFromDoc(TumblrPostType.All);
                    return posts;
                }
                ProcessingStatusCode = ProcessingCode.UnableDownload;
                return new HashSet<TumblrPost>();
            }
            catch
            {
                return new HashSet<TumblrPost>();
            }
        }
    }
}