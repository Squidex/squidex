/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { of } from 'rxjs';
import { IMock, It, Mock } from 'typemoq';
import * as Y from 'yjs';
import { CollaborationService, SharedArray } from '../internal';
import { Comment, CommentItem, CommentsState } from './comments.state';

describe('CommentsState', () => {
    let collaborationSevice: IMock<CollaborationService>;
    let commentsState: CommentsState;
    let sharedArray: SharedArray<Comment>;

    beforeEach(() => {
        const yDoc = new Y.Doc();
        const yArray = yDoc.getArray<Comment>();

        sharedArray = new SharedArray<Comment>(yDoc, yArray);

        collaborationSevice = Mock.ofType<CollaborationService>();
        collaborationSevice.setup(x => x.getArray<Comment>(It.isAnyString()))
            .returns(() => of(sharedArray));

        commentsState = new CommentsState(collaborationSevice.object);
    });

    it('should get total items', () => {
        sharedArray.add({} as any);
        sharedArray.add({} as any);

        let items: ReadonlyArray<Comment> = [];
        commentsState.itemsChanges.subscribe(result => {
            items = result;
        });

        expect(items.length).toEqual(2);
    });

    it('should get unread items count', () => {
        sharedArray.add({} as any);
        sharedArray.add({} as any);
        sharedArray.add({ isRead: true } as any);

        let unreadCount = 0;
        commentsState.unreadCountChanges.subscribe(result => {
            unreadCount = result;
        });

        expect(unreadCount).toEqual(2);
    });

    it('should add comment', () => {
        const comment = { user: 'me', text: 'My Text', url: '/url/to/comment' };

        let items: ReadonlyArray<Comment> = [];
        commentsState.itemsChanges.subscribe(result => {
            items = result;
        });

        commentsState.create(comment.user, comment.text, comment.url);

        expect(items.length).toEqual(1);
        expect(items[0].text).toEqual(comment.text);
        expect(items[0].url).toEqual(comment.url);
        expect(items[0].user).toEqual(comment.user);
        expect(items[0].id).toBeDefined();
        expect(items[0].time).toBeDefined();
    });

    it('should update comment', () => {
        const comment = { user: 'me', text: 'My Text', url: '/url/to/comment' };

        sharedArray.add({} as any);

        let items: ReadonlyArray<Comment> = [];
        commentsState.itemsChanges.subscribe(result => {
            items = result;
        });

        commentsState.update(0, comment);

        expect(items.length).toEqual(1);
        expect(items[0].text).toEqual(comment.text);
        expect(items[0].url).toEqual(comment.url);
        expect(items[0].user).toEqual(comment.user);
    });

    it('should prune comments', () => {
        sharedArray.add({ id: '1' } as any);
        sharedArray.add({ id: '2' } as any);
        sharedArray.add({ id: '3' } as any);
        sharedArray.add({ id: '4' } as any);

        let items: ReadonlyArray<Comment> = [];
        commentsState.itemsChanges.subscribe(result => {
            items = result;
        });

        commentsState.prune(2);

        expect(items.map(x => x.id)).toEqual(['3', '4']);
    });

    it('should not prune comments if max not reached', () => {
        sharedArray.add({ id: '1' } as any);
        sharedArray.add({ id: '2' } as any);
        sharedArray.add({ id: '3' } as any);
        sharedArray.add({ id: '4' } as any);

        let items: ReadonlyArray<Comment> = [];
        commentsState.itemsChanges.subscribe(result => {
            items = result;
        });

        commentsState.prune(4);

        expect(items.map(x => x.id)).toEqual(['1', '2', '3', '4']);
    });

    it('should mark comments as read', () => {
        sharedArray.add({ id: '1' } as any);
        sharedArray.add({ id: '2' } as any);
        sharedArray.add({ id: '3' } as any);
        sharedArray.add({ id: '4' } as any);

        let items: ReadonlyArray<Comment> = [];
        commentsState.itemsChanges.subscribe(result => {
            items = result;
        });

        commentsState.markRead();

        expect(items.map(x => x.isRead)).toEqual([true, true, true, true]);
    });

    it('should update annotations', () => {
        sharedArray.add({ id: '1', editorId: '1', from: 11, to: 12 } as any);
        sharedArray.add({ id: '2', editorId: '2' } as any);

        let items: ReadonlyArray<Comment> = [];
        commentsState.itemsChanges.subscribe(result => {
            items = result;
        });

        commentsState.updateAnnotations('2', [{  id: '2', from: 13, to: 52 }]);

        expect(items).toEqual([
            { id: '1', editorId: '1', from: 11, to: 12 } as any,
            { id: '2', editorId: '2', from: 13, to: 52 } as any,
        ]);
    });

    it('should unset annotations', () => {
        sharedArray.add({ editorId: '1', id: '1', from: 13, to: 52 } as any);
        sharedArray.add({ editorId: '1', id: '2' } as any);

        let items: ReadonlyArray<Comment> = [];
        commentsState.itemsChanges.subscribe(result => {
            items = result;
        });

        commentsState.updateAnnotations('1', [{ id: '2', from: 13, to: 52 }]);

        expect(items).toEqual([
            { id: '1' } as any,
            { id: '2', editorId: '1', from: 13, to: 52 } as any,
        ]);
    });

    it('should get annotations', () => {
        sharedArray.add({ editorId: '1', id: '1', from: 11, to: 12 } as any);
        sharedArray.add({ editorId: '2', id: '2', from: 21, to: 22 } as any);

        let items: ReadonlyArray<Annotation> = [];
        commentsState.getAnnotations('2').subscribe(result => {
            items = result;
        });

        expect(items).toEqual([
            { editorId: '2', id: '2', from: 21, to: 22 } as any,
        ]);
    });

    it('should get empty annotations', () => {
        sharedArray.add({ editorId: '1', id: '1', from: 11, to: 12 } as any);
        sharedArray.add({ editorId: '2', id: '2', from: 21, to: 22 } as any);

        let items: ReadonlyArray<Annotation> = [];
        commentsState.getAnnotations(undefined).subscribe(result => {
            items = result;
        });

        expect(items).toEqual([]);
    });

    it('should get grouped comments', () => {
        const selection = of<ReadonlyArray<string>>(['1']);

        sharedArray.add({ id: '1' } as any);
        sharedArray.add({ id: '2' } as any);
        sharedArray.add({ id: '3', replyTo: '5' } as any);
        sharedArray.add({ id: '4', replyTo: '2' } as any);

        let items: ReadonlyArray<CommentItem> = [];
        commentsState.getGroupedComments(selection).subscribe(result => {
            items = result;
        });

        expect(items).toEqual([
            {
                index: 0,
                comment: {
                    id: '1',
                } as any as Comment,
                isSelected: true,
                replies: [],
            }, {
                index: 1,
                comment: {
                    id: '2',
                } as any as Comment,
                isSelected: false,
                replies: [
                    {
                        index: 3,
                        comment: {
                            id: '4',
                            replyTo: '2',
                        } as any as Comment,
                        replies: [],
                        isSelected: false,
                    },
                ],
            },
        ]);
    });
});