// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.ClientLibrary.Management;
using TestSuite.Fixtures;

#pragma warning disable SA1300 // Element should begin with upper-case letter
#pragma warning disable SA1507 // Code should not contain multiple blank lines in a row

namespace TestSuite.ApiTests;

[UsesVerify]
public class CommentsTests : IClassFixture<CreatedAppFixture>
{
    private readonly string resource = Guid.NewGuid().ToString();

    public CreatedAppFixture _ { get; }

    public CommentsTests(CreatedAppFixture fixture)
    {
        _ = fixture;
    }

    [Fact]
    public async Task Should_make_watch_request()
    {
        var result = await _.Client.Comments.GetWatchingUsersAsync(resource);

        Assert.NotNull(result);
    }

    [Fact]
    public async Task Should_create_comment()
    {
        // STEP 1: Create the comment.
        var createRequest = new UpsertCommentDto
        {
            Text = resource
        };

        await _.Client.Comments.PostCommentAsync(resource, createRequest);


        // STEP 2: Get comments
        var comments = await _.Client.Comments.GetCommentsAsync(resource);

        Assert.Contains(comments.CreatedComments, x => x.Text == createRequest.Text);

        await Verify(comments)
            .IgnoreMember<CommentDto>(x => x.Text);
    }

    [Fact]
    public async Task Should_update_comment()
    {
        // STEP 1: Create the comment.
        var createRequest = new UpsertCommentDto
        {
            Text = resource
        };

        var comment = await _.Client.Comments.PostCommentAsync(resource, createRequest);


        // STEP 2: Update comment.
        var updateRequest = new UpsertCommentDto
        {
            Text = $"{resource}_Update"
        };

        await _.Client.Comments.PutCommentAsync(resource, comment.Id, updateRequest);


        // STEP 3: Get comments since create.
        var comments = await _.Client.Comments.GetCommentsAsync(resource, 0);

        Assert.Contains(comments.UpdatedComments, x => x.Text == updateRequest.Text);

        await Verify(comments)
            .IgnoreMember<CommentDto>(x => x.Text);
    }

    [Fact]
    public async Task Should_delete_comment()
    {
        // STEP 1: Create the comment.
        var createRequest = new UpsertCommentDto
        {
            Text = resource
        };

        var comment = await _.Client.Comments.PostCommentAsync(resource, createRequest);


        // STEP 2: Delete comment.
        await _.Client.Comments.DeleteCommentAsync(resource, comment.Id);


        // STEP 3: Get comments since create.
        var comments = await _.Client.Comments.GetCommentsAsync(resource, 0);

        Assert.Contains(comment.Id, comments.DeletedComments);

        await Verify(comments)
            .IgnoreMember<CommentDto>(x => x.Text);
    }
}
