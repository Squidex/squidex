/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { of } from 'rxjs';
import { IMock, It, Mock, Times } from 'typemoq';

import {
    CommentDto,
    CommentsDto,
    CommentsService,
    CommentsState,
    DialogService,
    ImmutableArray,
    Version
} from './../';

import { TestValues } from './_test-helpers';

describe('CommentsState', () => {
    const {
        app,
        appsState,
        creator,
        now
    } = TestValues;

    const commentsId = 'my-comments';

    const oldComments = new CommentsDto([
        new CommentDto('1', now, 'text1', creator),
        new CommentDto('2', now, 'text2', creator)
    ], [], [], new Version('1'));

    let dialogs: IMock<DialogService>;
    let commentsService: IMock<CommentsService>;
    let commentsState: CommentsState;

    beforeEach(() => {
        dialogs = Mock.ofType<DialogService>();

        commentsService = Mock.ofType<CommentsService>();

        commentsService.setup(x => x.getComments(app, commentsId, new Version('-1')))
            .returns(() => of(oldComments)).verifiable(Times.atLeastOnce());

        commentsState = new CommentsState(appsState.object, commentsId, commentsService.object, dialogs.object);
        commentsState.load().subscribe();
    });

    beforeEach(() => {
        commentsService.verifyAll();
    });

    it('should load and merge comments', () => {
        const newComments = new CommentsDto([
                new CommentDto('3', now, 'text3', creator)
            ], [
                new CommentDto('2', now, 'text2_2', creator)
            ], ['1'], new Version('2'));

        commentsService.setup(x => x.getComments(app, commentsId, new Version('1')))
            .returns(() => of(newComments)).verifiable();

        commentsState.load().subscribe();

        expect(commentsState.snapshot.isLoaded).toBeTruthy();
        expect(commentsState.snapshot.comments).toEqual(ImmutableArray.of([
            new CommentDto('2', now, 'text2_2', creator),
            new CommentDto('3', now, 'text3', creator)
        ]));

        commentsService.verify(x => x.getComments(app, commentsId, It.isAny()), Times.exactly(2));
    });

    it('should add comment to snapshot when created', () => {
        const newComment = new CommentDto('3', now, 'text3', creator);

        const request = { text: 'text3' };

        commentsService.setup(x => x.postComment(app, commentsId, request))
            .returns(() => of(newComment)).verifiable();

        commentsState.create('text3').subscribe();

        expect(commentsState.snapshot.comments).toEqual(ImmutableArray.of([
            new CommentDto('1', now, 'text1', creator),
            new CommentDto('2', now, 'text2', creator),
            new CommentDto('3', now, 'text3', creator)
        ]));
    });

    it('should update properties when updated', () => {
        const request = { text: 'text2_2' };

        commentsService.setup(x => x.putComment(app, commentsId, '2', request))
            .returns(() => of({})).verifiable();

        commentsState.update(oldComments.createdComments[1], 'text2_2', now).subscribe();

        expect(commentsState.snapshot.comments).toEqual(ImmutableArray.of([
            new CommentDto('1', now, 'text1', creator),
            new CommentDto('2', now, 'text2_2', creator)
        ]));
    });

    it('should remove comment from snapshot when deleted', () => {
        commentsService.setup(x => x.deleteComment(app, commentsId, '2'))
            .returns(() => of({})).verifiable();

        commentsState.delete(oldComments.createdComments[1]).subscribe();

        expect(commentsState.snapshot.comments).toEqual(ImmutableArray.of([
            new CommentDto('1', now, 'text1', creator)
        ]));
    });
});