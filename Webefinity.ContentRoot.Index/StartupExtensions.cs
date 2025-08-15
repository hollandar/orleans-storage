using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Runtime.CompilerServices;
using Webefinity.ContentRoot.Abstractions;
using Webefinity.ContentRoot.Index.Data;
using Webefinity.ContentRoot.Index.Services;
using Webefinity.ContentRoot.Index.Handlers;
using Webefinity.ContentRoot.IndexUI.Interfaces;
using Webefinity.ContentRoot.Index.Interfaces;
using Webefinity.ContentRoot.IndexUI.Services;
using Webefinity.ContentRoot.IndexUI;
using Webefinity.ContentRoot.IndexUI.Components;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Webefinity.ContentRoot.Index.Shared;
using HttpResults = Microsoft.AspNetCore.Http.Results;

namespace Webefinity.ContentRoot.Index;

public static class StartupExtensions
{
    public static void AddFileIndexModel(this ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<FileMetadata>(options =>
        {
            options.HasKey(r => r.Id);
            options.HasIndex(r => new { r.Key, r.Collection, r.FileName });
            options.HasMany(r => r.Metadata).WithOne(r => r.FileMetadata).HasForeignKey(r => r.FileMetadataId);
        });

        modelBuilder.Entity<Metadata>(options =>
        {
            options.HasKey(r => r.Id);
            options.HasIndex(r => r.Key);
        });
    }

    public static void AddFileMetadataDbContext(this IServiceCollection services, Action<DbContextOptionsBuilder> options)
    {
        services.AddDbContext<FileMetadataDbContext>(options);
        services.AddScoped<IFileMetadataDbContext>(sp => sp.GetRequiredService<FileMetadataDbContext>());
    }

    public static void AddFileMetadataDbContext<TDbContext>(this IServiceCollection services) where TDbContext : DbContext
    {
        services.AddScoped<IFileMetadataDbContext>((sp) => new FileMetadataDbContextChild<TDbContext>(sp.GetRequiredService<TDbContext>()));
    }

    public static void AddIndexedContentRoot(this IServiceCollection services, string? key = null)
    {
        if (key is null)
        {
            services.AddScoped<IIndexedContentRootLibrary>((sp) =>
            {
                var contentRoot = sp.GetRequiredService<IContentRootLibrary>();
                var dbContext = sp.GetRequiredService<IFileMetadataDbContext>();
                return new IndexedContentRootService(contentRoot, dbContext, "Default");
            });
        }
        else
        {
            services.AddKeyedScoped<IIndexedContentRootLibrary>(key, (sp, k) =>
            {
                var contentRoot = sp.GetRequiredKeyedService<IContentRootLibrary>(key);
                var dbContext = sp.GetRequiredService<IFileMetadataDbContext>();
                return new IndexedContentRootService(contentRoot, dbContext, key);
            });
        }
        services.AddScoped<IFileBrowserService, FileBrowserService>();

    }

    public static void AddFileBrowserService(this IServiceCollection services, string adminPolicy = "IC_Admin", string? browserPolicy = null, params KeyCollection[] keyCollections)
    {
        services.AddSingleton<IKeyCollectionsService, KeyCollectionService>(provider => new KeyCollectionService(keyCollections));
        services.AddSingleton<IFileBrowserPolicyService>(provider => new FileBrowserPolicyService(browserPolicy, adminPolicy));
    }

    public static void MapIndexedContentUI(this IEndpointRouteBuilder app)
    {

        var policyProvider = app.ServiceProvider.GetRequiredService<IFileBrowserPolicyService>();

        var getListUri = "/api/icr/{key}/{collection}/list/{search?}";
        app.MapGet(getListUri, (IServiceProvider sp, string key, string collection, string? search, [FromQuery] int skip = 0, [FromQuery] int take = 10) =>
            IndexedContentHandler.GetList(sp, collection, search, skip, take, key)
        ).RequireAuthorization(policyProvider.GetAdminPolicy());

        var postFileUri = "/api/icr/{key}/{collection}/upload";
        app.MapPost(postFileUri, (IServiceProvider sp, string key, string collection, [FromBody] Stream content, [FromHeader(Name = "Content-Type")] string contentType, [FromHeader(Name = "X-FileName")] string fileName) =>
            IndexedContentHandler.PostFile(sp, collection, content, contentType, fileName, key)
        ).RequireAuthorization(policyProvider.GetAdminPolicy());

        var getFileUri = "/icr/{key}/{collection}/{filename}";
        var getFileMap = app.MapGet(getFileUri, (IServiceProvider sp, HttpRequest request, HttpResponse response, string key, string collection, string filename) =>
            IndexedContentHandler.GetFile(sp, request, response, collection, filename, key)
        );

        if (policyProvider.GetBrowserPolicy() is not null)
            getFileMap.RequireAuthorization(policyProvider.GetBrowserPolicy()!);

        var getSizedFileUri = "/icr/{key}/{collection}/s/{imagesize}/{filename}";
        var getSizedFileMap = app.MapGet(getSizedFileUri, IndexedContentHandler.GetSizedFile);

        if (policyProvider.GetBrowserPolicy() is not null)
            getSizedFileMap.RequireAuthorization(policyProvider.GetBrowserPolicy()!);

        var getFileMetaUri = "/api/icr/{key}/{collection}/meta/{filenameorid}";
        app.MapGet(getFileMetaUri, (IServiceProvider sp, HttpResponse response, string key, string collection, string filenameorid) =>
            IndexedContentHandler.GetFileMeta(sp, response, collection, filenameorid, key)
        ).RequireAuthorization(policyProvider.GetAdminPolicy());

        var deleteFileUri = "/api/icr/{key}/{collection}/{filename}";
        app.MapDelete(deleteFileUri, (IServiceProvider sp, string key, string collection, string filename) =>
            IndexedContentHandler.DeleteFile(sp, collection, filename, key)
        ).RequireAuthorization(policyProvider.GetAdminPolicy());

        var getKeysAndPolicyUri = $"/api/icr/keysandpolicy";
        app.MapGet(getKeysAndPolicyUri, (IServiceProvider sp) =>
        {
            var fileBrowserService = sp.GetRequiredService<IFileBrowserService>();
            return fileBrowserService.GetKeysAndPolicyAsync();
        }).RequireAuthorization(policyProvider.GetAdminPolicy());
    }
}
