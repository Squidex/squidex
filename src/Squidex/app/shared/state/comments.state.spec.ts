/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { of } from 'rxjs';
import { IMock, It, Mock, Times } from 'typemoq';

import {
    AppsState,
    CommentDto,
    CommentsDto,
    CommentsService,
    CommentsState,
    DateTime,
    DialogService,
    ImmutableArray,
    UpsertCommentDto,
    Version
 } from '@app/shared';

describe('CommentsState', () => {
    const app = 'my-app';
    const commentsId = 'my-comments';
    const now = DateTime.today();
    const user = 'not-me';

    const oldComments = new CommentsDto([
        new CommentDto('1', now, 'text1', user),
        new CommentDto('2', now, 'text2', user)
    ], [], [], new Version('1'));

    let dialogs: IMock<DialogService>;
    let appsState: IMock<AppsState>;
    let commentsService: IMock<CommentsService>;
    let commentsState: CommentsState;

    beforeEach(() => {
        dialogs = Mock.ofType<DialogService>();

        appsState = Mock.ofType<AppsState>();

        appsState.setup(x => x.appName)
            .returns(() => app);

        commentsService = Mock.ofType<CommentsService>();

        commentsService.setup(x => x.getComments(app, commentsId, new Version('-1')))
            .returns(() => of(oldComments));

        commentsState = new CommentsState(appsState.object, commentsId, commentsService.object, dialogs.object);
        commentsState.load().subscribe();
    });

    it('should load and merge comments', () => {
        const newComments = new CommentsDto([
            new CommentDto('3', now, 'text3', user)
        ], [
            new CommentDto('2', now, 'text2_2', user)
        ], ['1'], new Version('2'));

        commentsService.setup(x => x.getComments(app, commentsId, new Version('1')))
            .returns(() => of(newComments));

        commentsState.load().subscribe();

        expect(commentsState.snapshot.isLoaded).toBeTruthy();
        expect(commentsState.snapshot.comments).toEqual(ImmutableArray.of([
            new CommentDto('2', now, 'text2_2', user),
            new CommentDto('3', now, 'text3', user)
        ]));

        commentsService.verify(x => x.getComments(app, commentsId, It.isAny()), Times.exactly(2));
    });

    it('should add comment to snapshot when created', () => {
        const newComment = new CommentDto('3', now, 'text3', user);

        commentsService.setup(x => x.postComment(app, commentsId, new UpsertCommentDto('text3')))
            .returns(() => of(newComment));

        commentsState.create('text3').subscribe();

        expect(commentsState.snapshot.comments).toEqual(ImmutableArray.of([
            new CommentDto('1', now, 'text1', user),
            new CommentDto('2', now, 'text2', user),
            new CommentDto('3', now, 'text3', user)
        ]));
    });

    it('should update properties when updated', () => {
        commentsService.setup(x => x.putComment(app, commentsId, '2', new UpsertCommentDto('text2_2')))
            .returns(() => of({}));

        commentsState.update('2', 'text2_2', now).subscribe();

        expect(commentsState.snapshot.comments).toEqual(ImmutableArray.of([
            new CommentDto('1', now, 'text1', user),
            new CommentDto('2', now, 'text2_2', user)
        ]));

        commentsService.verify(x => x.putComment(app, commentsId, '2', new UpsertCommentDto('text2_2')), Times.once());
    });

    it('should remove comment from snapshot when deleted', () => {
        commentsService.setup(x => x.deleteComment(app, commentsId, '2'))
            .returns(() => of({}));

        commentsState.delete('2').subscribe();

        expect(commentsState.snapshot.comments).toEqual(ImmutableArray.of([
            new CommentDto('1', now, 'text1', user)
        ]));

        commentsService.verify(x => x.deleteComment(app, commentsId, '2'), Times.once());
    });
});