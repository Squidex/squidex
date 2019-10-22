/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { of } from 'rxjs';
import { IMock, Mock } from 'typemoq';

import {
    CommentDto,
    CommentsDto,
    CommentsService,
    CommentsState,
    DialogService,
    Version
} from '@app/shared/internal';

import { TestValues } from './_test-helpers';

describe('CommentsState', () => {
    const {
        app,
        appsState,
        creator,
        modified
    } = TestValues;

    const commentsId = 'my-comments';

    const oldComments = new CommentsDto([
        new CommentDto('1', modified, 'text1', creator),
        new CommentDto('2', modified, 'text2', creator)
    ], [], [], new Version('1'));

    let dialogs: IMock<DialogService>;
    let commentsService: IMock<CommentsService>;
    let commentsState: CommentsState;

    beforeEach(() => {
        dialogs = Mock.ofType<DialogService>();

        commentsService = Mock.ofType<CommentsService>();
        commentsState = new CommentsState(appsState.object, commentsId, commentsService.object, dialogs.object);
    });

    beforeEach(() => {
        commentsService.verifyAll();
    });

    describe('Loading', () => {
        it('should load and merge comments', () => {
            const newComments = new CommentsDto([
                    new CommentDto('3', modified, 'text3', creator)
                ], [
                    new CommentDto('2', modified, 'text2_2', creator)
                ], ['1'], new Version('2'));

            commentsService.setup(x => x.getComments(app, commentsId, new Version('-1')))
                .returns(() => of(oldComments)).verifiable();

            commentsService.setup(x => x.getComments(app, commentsId, new Version('1')))
                .returns(() => of(newComments)).verifiable();

            commentsState.load().subscribe();
            commentsState.load().subscribe();

            expect(commentsState.snapshot.isLoaded).toBeTruthy();
            expect(commentsState.snapshot.comments).toEqual([
                new CommentDto('2', modified, 'text2_2', creator),
                new CommentDto('3', modified, 'text3', creator)
            ]);
        });
    });

    describe('Updates', () => {
        beforeEach(() => {
            commentsService.setup(x => x.getComments(app, commentsId, new Version('-1')))
                .returns(() => of(oldComments)).verifiable();

            commentsState.load().subscribe();
        });

        it('should add comment to snapshot when created', () => {
            const newComment = new CommentDto('3', modified, 'text3', creator);

            const request = { text: 'text3' };

            commentsService.setup(x => x.postComment(app, commentsId, request))
                .returns(() => of(newComment)).verifiable();

            commentsState.create('text3').subscribe();

            expect(commentsState.snapshot.comments).toEqual([
                new CommentDto('1', modified, 'text1', creator),
                new CommentDto('2', modified, 'text2', creator),
                new CommentDto('3', modified, 'text3', creator)
            ]);
        });

        it('should update properties when updated', () => {
            const request = { text: 'text2_2' };

            commentsService.setup(x => x.putComment(app, commentsId, '2', request))
                .returns(() => of({})).verifiable();

            commentsState.update(oldComments.createdComments[1], 'text2_2', modified).subscribe();

            expect(commentsState.snapshot.comments).toEqual([
                new CommentDto('1', modified, 'text1', creator),
                new CommentDto('2', modified, 'text2_2', creator)
            ]);
        });

        it('should remove comment from snapshot when deleted', () => {
            commentsService.setup(x => x.deleteComment(app, commentsId, '2'))
                .returns(() => of({})).verifiable();

            commentsState.delete(oldComments.createdComments[1]).subscribe();

            expect(commentsState.snapshot.comments).toEqual([
                new CommentDto('1', modified, 'text1', creator)
            ]);
        });
    });
});