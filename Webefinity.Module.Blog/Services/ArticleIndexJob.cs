using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Text.RegularExpressions;
using Webefinity.Module.Blog.Data;
using Webefinity.Module.Scheduler.Interfaces;

namespace Webefinity.Module.Blog.Services
{
    public partial class ArticleIndexJob : IJob
    {
        private readonly ILogger<ArticleIndexJob> logger;
        private readonly IServiceProvider serviceProvider;
        private readonly string? key;

        public ArticleIndexJob(IServiceProvider serviceProvider, string? key = null)
        {
            this.logger = serviceProvider.GetRequiredService<ILogger<ArticleIndexJob>>();
            this.serviceProvider = serviceProvider;
            this.key = key;
        }

        public async Task ExecuteAsync(IJobExecutionContext jobExecutionContext)
        {
            // Resolve a db context
            using var scope = this.serviceProvider.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<IBlogDbContext>();

            // Index all articles
            HashSet<string> articleIds = new HashSet<string>();
            var enumerable = dbContext.Articles.ToList();
            foreach (var file in enumerable)
            {
                // Populate the tag list and words list for this article
                await SynchronizeTagsAsync(dbContext, file);
                await SynchronizeWordsAsync(dbContext, file);
            }

            await dbContext.SaveChangesAsync();
        }

        [GeneratedRegex("([a-zA-Z0-9]{3,})")]
        public static partial Regex WordRegex();

        private async Task SynchronizeWordsAsync(IBlogDbContext blogDbContext, BlogArticle articleEntry)
        {
            var content = articleEntry.Content ?? string.Empty;
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
                    blogDbContext.Words.Remove(wordDb);
                }
            }

            foreach (var word in wordCount)
            {
                blogDbContext.Words.Add(new BlogWord { ArticleId = articleEntry.Id, Word = word.Key, Count = word.Value });
            }
        }

        private async Task SynchronizeTagsAsync(IBlogDbContext blogDbContext, BlogArticle articleEntry)
        {
            var tags = (articleEntry.TagList ?? string.Empty).Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToHashSet();

            await blogDbContext.Entry(articleEntry).Collection(a => a.Tags).LoadAsync();
            foreach (var tagDb in articleEntry.Tags)
            {
                if (tags.Contains(tagDb.Tag))
                {
                    tags.Remove(tagDb.Tag);
                }
                else
                {
                    blogDbContext.Tags.Remove(tagDb);
                }
            }

            foreach (var tag in tags)
            {
                blogDbContext.Tags.Add(new BlogTag { ArticleId = articleEntry.Id, Tag = tag });
            }
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}
