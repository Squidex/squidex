/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { CommentDto, CommentsDto, CommentsService, CommentsState, DialogService, Version } from '@app/shared/internal';
import { of } from 'rxjs';
import { IMock, Mock } from 'typemoq';
import { TestValues } from './_test-helpers';

describe('CommentsState', () => {
    const {
        creator,
        modified,
    } = TestValues;

    const commentsUrl = 'my-comments';

    const oldComments = new CommentsDto([
        new CommentDto('1', modified, 'text1', undefined, creator),
        new CommentDto('2', modified, 'text2', undefined, creator),
    ], [], [], new Version('1'));

    let dialogs: IMock<DialogService>;
    let commentsService: IMock<CommentsService>;
    let commentsState: CommentsState;

    beforeEach(() => {
        dialogs = Mock.ofType<DialogService>();

        commentsService = Mock.ofType<CommentsService>();
        commentsState = new CommentsState(commentsUrl, commentsService.object, dialogs.object);
    });

    beforeEach(() => {
        commentsService.verifyAll();
    });

    describe('Loading', () => {
        it('should load and merge comments', () => {
            const newComments = new CommentsDto([
                new CommentDto('3', modified, 'text3', undefined, creator),
            ], [
                new CommentDto('2', modified, 'text2_2', undefined, creator),
            ], ['1'], new Version('2'));

            commentsService.setup(x => x.getComments(commentsUrl, new Version('-1')))
                .returns(() => of(oldComments)).verifiable();

            commentsService.setup(x => x.getComments(commentsUrl, new Version('1')))
                .returns(() => of(newComments)).verifiable();

            commentsState.load().subscribe();
            commentsState.load().subscribe();

            expect(commentsState.snapshot.isLoaded).toBeTruthy();
            expect(commentsState.snapshot.comments).toEqual([
                new CommentDto('2', modified, 'text2_2', undefined, creator),
                new CommentDto('3', modified, 'text3', undefined, creator),
            ]);
        });
    });

    describe('Updates', () => {
        beforeEach(() => {
            commentsService.setup(x => x.getComments(commentsUrl, new Version('-1')))
                .returns(() => of(oldComments)).verifiable();

            commentsState.load().subscribe();
        });

        it('should add comment to snapshot if created', () => {
            const newComment = new CommentDto('3', modified, 'text3', undefined, creator);

            const request = { text: 'text3', url: 'url3' };

            commentsService.setup(x => x.postComment(commentsUrl, request))
                .returns(() => of(newComment)).verifiable();

            commentsState.create('text3', 'url3').subscribe();

            expect(commentsState.snapshot.comments).toEqual([
                new CommentDto('1', modified, 'text1', undefined, creator),
                new CommentDto('2', modified, 'text2', undefined, creator),
                new CommentDto('3', modified, 'text3', undefined, creator),
            ]);
        });

        it('should update properties if updated', () => {
            const request = { text: 'text2_2' };

            commentsService.setup(x => x.putComment(commentsUrl, '2', request))
                .returns(() => of({})).verifiable();

            commentsState.update(oldComments.createdComments[1], 'text2_2', modified).subscribe();

            expect(commentsState.snapshot.comments).toEqual([
                new CommentDto('1', modified, 'text1', undefined, creator),
                new CommentDto('2', modified, 'text2_2', undefined, creator),
            ]);
        });

        it('should remove comment from snapshot if deleted', () => {
            commentsService.setup(x => x.deleteComment(commentsUrl, '2'))
                .returns(() => of({})).verifiable();

            commentsState.delete(oldComments.createdComments[1]).subscribe();

            expect(commentsState.snapshot.comments).toEqual([
                new CommentDto('1', modified, 'text1', undefined, creator),
            ]);
        });
    });
});
