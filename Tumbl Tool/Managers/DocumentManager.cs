﻿/* 01010011 01101000 01101001 01101110 01101111  01000001 01101101 01100001 01101011 01110101 01110011 01100001
 *
 *  Project: Tumblr Tools - Image parser and downloader from Tumblr blog system
 *
 *  Class: DocumentManager
 *
 *  Description: Class provides functionality for acquiring and processing JSON/XML documents from Tumblr API
 *
 *  Author: Shino Amakusa
 *
 *  Created: 2013
 *
 *  Last Updated: March, 2017
 *
 * 01010011 01101000 01101001 01101110 01101111  01000001 01101101 01100001 01101011 01110101 01110011 01100001 */

using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using Tumblr_Tool.Enums;
using Tumblr_Tool.Helpers;
using Tumblr_Tool.Objects.Tumblr_Objects;

namespace Tumblr_Tool.Managers
{
    public class DocumentManager
    {
        /// <summary>
        ///
        /// </summary>
        /// <param name="apiMode"></param>
        /// <param name="imageSize"></param>
        public DocumentManager(TumblrApiVersion apiMode = TumblrApiVersion.V2Json, ImageSize imageSize = ImageSize.Original)
        {
            ApiVersion = apiMode;
            ImageSize = imageSize;
            JsonDocument = null;
        }

        public TumblrApiVersion ApiVersion { get; set; }
        public ImageSize ImageSize { get; set; }
        public dynamic JsonDocument { get; set; }

        /// <summary>
        ///
        /// </summary>
        /// <param name="tumblrPostType"></param>
        /// <returns></returns>
        public HashSet<TumblrPost> GetPostListFromDoc(TumblrPostType tumblrPostType)
        {
            try
            {
                HashSet<TumblrPost> postList = new HashSet<TumblrPost>();

                if (JsonDocument != null && JsonDocument.response != null && JsonDocument.response.posts != null)
                {
                    JArray jPostArray = JsonDocument.response.posts;
                    HashSet<dynamic> jPostList = jPostArray.ToObject<HashSet<dynamic>>();

                    foreach (dynamic jPost in jPostList)
                    {
                        TumblrPost post = new TumblrPost();

                        switch (tumblrPostType)
                        {
                            case TumblrPostType.Photo:
                                PostHelper.GeneratePhotoPost(ref post, jPost, ImageSize);
                                break;

                            default:
                                PostHelper.IncludeCommonPostFields(ref post, jPost);
                                break;
                        }

                        postList.Add(post);
                    }
                }

                return postList;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="url"></param>
        /// <param name="blog"></param>
        /// <returns></returns>
        public bool GetRemoteBlogInfo(string url, TumblrBlog blog)
        {
            if (!string.IsNullOrEmpty(url))
            {
                try
                {
                    if (blog == null) blog = new TumblrBlog(url);
                    dynamic jsonDocument = JsonHelper.GetDynamicObjectFromString(WebHelper.GetRemoteDocumentAsString(url));

                    if (jsonDocument != null && jsonDocument.response != null && jsonDocument.response.blog != null)
                    {
                        blog.Title = jsonDocument.response.blog.title;
                        blog.Description = jsonDocument.response.blog.description;
                        blog.Name = jsonDocument.response.blog.name;
                        blog.Url = jsonDocument.response.blog.url;
                        blog.Nsfw = Convert.ToBoolean(jsonDocument.response.blog.is_nsfw);
                        blog.AskEnabled = Convert.ToBoolean(jsonDocument.response.blog.ask);
                        blog.AnonAskEnabled = Convert.ToBoolean(jsonDocument.response.blog.ask_anon);
                        blog.LastUpdated = DateTimeHelper.UnixTimeStampToDateTime(Convert.ToDouble(jsonDocument.response.blog.updated));

                        if (jsonDocument.response.total_posts != null)
                            blog.TotalPosts = Convert.ToInt32(jsonDocument.response.total_posts);
                        else if (jsonDocument.response.blog.posts != null)
                            blog.TotalPosts = Convert.ToInt32(jsonDocument.response.blog.posts);

                        if (jsonDocument.response.blog.posts != null)
                            blog.BlogTotalPosts = Convert.ToInt32(jsonDocument.response.blog.posts);

                        return true;
                    }
                }
                catch
                {
                    return false;
                }
            }

            return false;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="url"></param>
        public void GetRemoteDocument(string url)
        {
            try
            {
                JsonDocument = JsonHelper.GetDynamicObjectFromString(WebHelper.GetRemoteDocumentAsString(url));

                if ((JsonDocument != null && JsonDocument.meta != null && JsonDocument.meta.status == ((int)TumblrApiResponse.Ok).ToString()))
                {
                    // do nada
                }
                else
                {
                    JsonDocument = null;
                }
            }
            catch
            {
                JsonDocument = null;
            }
        }

        /// <summary>
        ///
        /// </summary>
        /// <returns></returns>
        public int GetTotalPostCount()
        {
            if (JsonDocument != null && JsonDocument.response != null && JsonDocument.response.blog != null)
            {
                if (JsonDocument.response.total_posts != null)
                    return Convert.ToInt32(JsonDocument.response.total_posts);
                if (JsonDocument.response.blog.posts != null)
                    return Convert.ToInt32(JsonDocument.response.blog.posts);
            }

            return 0;
        }
    }
}