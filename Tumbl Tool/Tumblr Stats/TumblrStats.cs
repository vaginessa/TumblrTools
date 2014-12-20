﻿/* 01010011 01101000 01101001 01101110 01101111  01000001 01101101 01100001 01101011 01110101 01110011 01100001
 *
 *  Project: Tumblr Tools - Image parser and downloader from Tumblr blog system
 *
 *  Author: Shino Amakusa
 *
 *  Created: 2013
 *
 *  Last Updated: December, 2014
 *
 * 01010011 01101000 01101001 01101110 01101111  01000001 01101101 01100001 01101011 01110101 01110011 01100001 */

using System.Collections.Generic;
using System.Linq;
using Tumblr_Tool.Common_Helpers;
using Tumblr_Tool.Enums;
using Tumblr_Tool.Managers;
using Tumblr_Tool.Tumblr_Objects;

namespace Tumblr_Tool.Tumblr_Stats
{
    public class TumblrStats
    {
        public TumblrStats()
        {
        }

        public TumblrStats(TumblrBlog blog, string url, string apiMode, int startNum = 0, int endNum = 0)
        {
            this.crawlManager = new CrawlManager();
            setAPIMode(apiMode);
            step = (int)postStepEnum.JSON;

            if (blog == null)
            {
                this.blog = new TumblrBlog();
            }
            else
            {
                this.blog = blog;
            }

            this.blog.posts = new HashSet<TumblrPost>();
            this.blog.url = url;
            this.tumblrDomain = CommonHelper.getDomainName(url);
            setBlogInfo();
            this.url = FileHelper.fixURL(this.blog.url);
            this.tumblrDomain = CommonHelper.getDomainName(this.url);

            this.maxNumPosts = endNum;
            this.start = startNum;

            this.step = (int)postStepEnum.JSON; //20 for JSON, 50 for XML
        }

        public int answerPosts { get; set; }

        public int audioPosts { get; set; }

        public TumblrBlog blog { get; set; }

        public int chatPosts { get; set; }

        public int linkPosts { get; set; }

        public int maxNumPosts { get; set; }

        public int parsed { get; set; }

        public int photoPosts { get; set; }

        public int quotePosts { get; set; }

        public processingCodes statusCode { get; set; }

        public int textPosts { get; set; }

        public int totalPosts { get; set; }

        public int videoPosts { get; set; }

        private string apiMode { get; set; }

        private CrawlManager crawlManager { get; set; }

        private int found { get; set; }

        private int start { get; set; }

        private int step { get; set; }

        private string tumblrDomain { get; set; }

        private string url { get; set; }

        public TumblrBlog parsePosts()
        {
            try
            {
                string url = XMLHelper.getQueryString(this.url, tumblrPostTypes.empty.ToString(), 0);
                this.step = (int)postStepEnum.XML;

                if (this.crawlManager.mode == apiModeEnum.JSON.ToString())
                {
                    url = JSONHelper.getQueryString(this.tumblrDomain, tumblrPostTypes.empty.ToString(), 0);
                    this.step = (int)postStepEnum.JSON;
                }

                if (this.crawlManager.isValidTumblr(url))
                {

                    this.totalPosts = this.blog.totalPosts;

                    int endCount;

                    if (maxNumPosts != 0)
                        endCount = maxNumPosts;
                    else
                        endCount = totalPosts;

                    int i = start;

                    this.parsed = 0;

                    while (i < this.totalPosts)
                    {
                        if (this.crawlManager.mode == apiModeEnum.JSON.ToString())
                        {
                            url = JSONHelper.getQueryString(this.tumblrDomain, tumblrPostTypes.empty.ToString(), i);
                        }

                        this.crawlManager.getDocument(url);
                        this.blog.posts.UnionWith(this.crawlManager.getPostList(tumblrPostTypes.empty.ToString(), this.crawlManager.mode));

                        this.photoPosts += (from p in this.blog.posts where p.type == tumblrPostTypes.photo.ToString() select p).ToList().Count;
                        this.textPosts += (from p in this.blog.posts where p.type == tumblrPostTypes.regular.ToString() || p.type == tumblrPostTypes.text.ToString() select p).ToList().Count;
                        this.videoPosts += (from p in this.blog.posts where p.type == tumblrPostTypes.video.ToString() select p).ToList().Count;
                        this.linkPosts += (from p in this.blog.posts where p.type == tumblrPostTypes.link.ToString() select p).ToList().Count;
                        this.audioPosts += (from p in this.blog.posts where p.type == tumblrPostTypes.audio.ToString() select p).ToList().Count;
                        this.quotePosts += (from p in this.blog.posts where p.type == tumblrPostTypes.quote.ToString() select p).ToList().Count;
                        this.chatPosts += (from p in this.blog.posts where p.type == tumblrPostTypes.chat.ToString() || p.type == tumblrPostTypes.conversation.ToString() select p).ToList().Count;
                        this.answerPosts += (from p in this.blog.posts where p.type == tumblrPostTypes.answer.ToString() select p).ToList().Count;
                        this.parsed += this.blog.posts.Count;
                        this.blog.posts.Clear();
                        i += step;
                    }

                    this.found = blog.posts.Count;
                }
                else
                {
                    this.statusCode = processingCodes.invalidURL;
                }
                return this.blog;
            }
            catch
            {
                return this.blog;
            }
        }

        public void setAPIMode(string mode)
        {
            try
            {
                this.apiMode = mode; // XML or JSON
                this.crawlManager.mode = mode;
            }
            catch
            {
                return;
            }
        }

        private void setBlogInfo()
        {
            try
            {
                if (this.crawlManager.mode == apiModeEnum.XML.ToString())
                {
                    this.crawlManager.setBlogInfo(XMLHelper.getQueryString(this.url, tumblrPostTypes.empty.ToString(), 0, 1), this.blog);
                }
                else if (this.crawlManager.mode == apiModeEnum.JSON.ToString())
                {
                    this.crawlManager.setBlogInfo(JSONHelper.getQueryString(this.tumblrDomain, tumblrPostTypes.empty.ToString(), 0, 1), this.blog);
                }
            }
            catch
            {
                return;
            }
        }
    }
}