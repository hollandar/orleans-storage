using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Webefinity.ContentRoot;
using Webefinity.ContentRoot.Abstractions;
using Webefinity.Frontmatter;
using Webefinity.Module.Blog.Data;
using Webefinity.Module.Blog.Models;

namespace Webefinity.Module.Blog.Services
{
    public partial class ArticleIndexHostedService : IHostedService
    {
        private readonly ILogger<ArticleIndexHostedService> logger;
        private readonly IServiceProvider serviceProvider;

        public ArticleIndexHostedService(ILogger<ArticleIndexHostedService> logger, IServiceProvider serviceProvider)
        {
            this.logger = logger;
            this.serviceProvider = serviceProvider;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            // Resolve a db context
            using var scope = this.serviceProvider.CreateScope();
            var contentRootLibrary = scope.ServiceProvider.GetRequiredService<IContentRootLibrary>();
            var dbContext = scope.ServiceProvider.GetRequiredService<BlogDbContext>();

            // Set up a database
            await dbContext.Database.MigrateAsync();

            // Index all articles
            HashSet<string> articleIds = new HashSet<string>();
            var enumerable = contentRootLibrary.EnumerateRecursiveAsync(Constants.BlogCollection, "*.md");
            await foreach (var file in enumerable)
            {
                using var article = contentRootLibrary.LoadReader(Constants.BlogCollection, file);
                var frontmatterResult = await FrontmatterLoader.LoadAsync<ArticleFrontmatter>(article);

                if (frontmatterResult.Frontmatter is null)
                {
                    logger.LogError("Failed to index article {ArticleFile}, frontmatter was not loaded.", file);
                    continue;
                }

                var articleEntry = await dbContext.Articles.FindAsync(frontmatterResult.Frontmatter.Id);
                if (articleEntry is null)
                {
                    articleEntry = new BlogArticle { Id = frontmatterResult.Frontmatter.Id };
                    dbContext.Articles.Add(articleEntry);
                }

                articleIds.Add(frontmatterResult.Frontmatter.Id);
                articleEntry.Title = frontmatterResult.Frontmatter.Title;
                articleEntry.Author = frontmatterResult.Frontmatter.Author;
                articleEntry.Summary = frontmatterResult.Frontmatter.Summary;
                articleEntry.Date = frontmatterResult.Frontmatter.Date;
                articleEntry.Image = frontmatterResult.Frontmatter.Image;
                
                // Populate the tag list and words list for this article
                await SynchronizeTagsAsync(dbContext, articleEntry, frontmatterResult.Frontmatter.Tags.ToHashSet());
                await SynchronizeWordsAsync(dbContext, articleEntry, frontmatterResult.Content);
            }

            // Clean up no longer existing articles
            var oldArticles = dbContext.Articles.Where(a => !articleIds.Contains(a.Id));
            dbContext.RemoveRange(oldArticles);

            await dbContext.SaveChangesAsync();
        }

        [GeneratedRegex("([a-zA-Z0-9]{3,})")]
        public static partial Regex WordRegex();

        private async Task SynchronizeWordsAsync(BlogDbContext blogDbContext, BlogArticle articleEntry, string content)
        {
            var words = WordRegex().Matches(content.ToLower());
            var wordCount = new Dictionary<string, int>();
            foreach (Match word in words)
            {
                if (wordCount.TryGetValue(word.Value, out var count))
                    wordCount[word.Value] = count + 1;
                else
                    wordCount[word.Value] = 1;
            }

            await blogDbContext.Entry(articleEntry).Collection(a => a.Words).LoadAsync();
            foreach (var wordDb in articleEntry.Words)
            {
                if (wordCount.TryGetValue(wordDb.Word, out var count))
                {
                    wordDb.Count = count;
                    wordCount.Remove(wordDb.Word);
                }
                else
                {
                    blogDbContext.Remove(wordDb);
                }
            }

            foreach (var word in wordCount)
            {
               blogDbContext.Add(new BlogWord { ArticleId = articleEntry.Id, Word = word.Key, Count = word.Value });
            }
        }

        private async Task SynchronizeTagsAsync(BlogDbContext blogDbContext, BlogArticle articleEntry, HashSet<string> tags)
        {
            await blogDbContext.Entry(articleEntry).Collection(a => a.Tags).LoadAsync();
            foreach (var tagDb in articleEntry.Tags)
            {
                if (tags.Contains(tagDb.Tag))
                {
                   tags.Remove(tagDb.Tag);
                }
                else
                {
                    blogDbContext.Remove(tagDb);
                }
            }

            foreach (var tag in tags)
            {
                blogDbContext.Add(new BlogTag { ArticleId = articleEntry.Id, Tag = tag });
            }
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}
