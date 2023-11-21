/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Injectable, OnDestroy } from '@angular/core';
import { BehaviorSubject, combineLatest, distinctUntilChanged, map, Observable, of, Subscription, switchMap } from 'rxjs';
import { DateTime, MathHelper, Types } from '@app/framework';
import { CollaborationService, SharedArray } from '../services/collaboration.service';

export interface Comment {
    // The timestamp when the comment was created.
    time: string;

    // The actual text.
    text: string;

    // The user token.
    user: string;

    // The url.
    url?: string;

    // The reply.
    replyTo?: string;

    // The ID of the comment.
    id?: string;

    // Indicates whether this comment has been read.
    isRead?: boolean;

    // The editor ID.
    editorId?: string;

    // The selection range.
    from?: number;

    // The selection number.
    to?: number;
}

@Injectable()
export class CommentsState implements OnDestroy {
    private readonly subscription: Subscription;
    private readonly comments = new BehaviorSubject<SharedArray<Comment>>(null!);

    public get itemsChanges() {
        return this.comments.pipe(switchMap(x => x.itemsChanges));
    }

    public get unreadCountChanges() {
        return this.itemsChanges.pipe(map(x => x.filter(c => !c.isRead).length));
    }

    constructor(
        collaboration: CollaborationService,
    )  {
        this.subscription =
            collaboration.getArray<Comment>('stream')
                .subscribe(comments => {
                    this.comments.next(comments);
                });
    }

    public ngOnDestroy() {
        this.subscription.unsubscribe();
    }

    public create(user: string, text: string, url: string, optional?: Pick<Comment, 'editorId' | 'from' | 'to' | 'replyTo'>) {
        this.updateInternal(comments => {
            const comment = { user, text, url, id: MathHelper.guid(), time: DateTime.now().toISOString(), ...optional || {} };

            comments.add(comment);
        });
    }

    public update(index: number, update: Partial<Comment>) {
        this.updateInternal(comments => {
            const comment = comments.items[index];

            if (comment) {
                comments.set(index, { ...comment, ...update });
            }
        });
    }

    public prune(maxCount: number) {
        this.updateInternal(comments => {
            const toDelete = comments.items.length - maxCount;

            if (toDelete > 0) {
                comments.remove(0, toDelete);
            }
        });
    }

    public remove(index: number) {
        this.updateInternal(comments => {
            comments.remove(index);
        });
    }

    public markRead() {
        this.updateInternal(comments => {
            comments.items.filter(x => !x.isRead).map((comment, index) => {
                comments.set(index, { ...comment, isRead: true });
            });
        });
    }

    public updateAnnotations(editorId: string, updates: ReadonlyArray<Annotation>) {
        this.updateInternal(comments => {
            comments.items.map((comment, index) => {
                if (!comment.id || comment.editorId !== editorId) {
                    return;
                }

                const update = updates.find(x => x.id === comment.id);
                if (update) {
                    const newComment = { ...comment, ...update };

                    comments.set(index, newComment);
                } else {
                    const newComment = { ...comment };

                    delete newComment.to;
                    delete newComment.from;
                    delete newComment.editorId;

                    comments.set(index, newComment);
                }
            });
        });
    }

    private updateInternal(update: (comments: SharedArray<Comment>) => void) {
        const comments = this.comments.value;

        if (!comments.doc) {
            return;
        }

        comments.doc.transact(() => {
            update(comments);
        });

    }

    public getAnnotations(editorId: string | undefined | null) {
        if (!editorId) {
            return of([]);
        }

        return this.itemsChanges.pipe(
            map(c => {
                const annotations: Annotation[] = [];

                for (const comment of c) {
                    if (comment.editorId &&
                        comment.editorId === editorId &&
                        comment.from &&
                        comment.to &&
                        comment.id) {
                        annotations.push(comment as never);
                    }
                }

                return annotations;
            }),
            distinctUntilChanged(Types.equals));
    }

    public getGroupedComments(selection: Observable<ReadonlyArray<string>>) {
        return combineLatest([this.itemsChanges, selection], (comments, selection) => {
            const result: CommentItem[] = [];

            comments.forEach((comment, index) => {
                const isSelected = !!comment.id && selection.indexOf(comment.id) >= 0;

                const item = { comment, index, isSelected, replies: [] };

                if (comment.replyTo) {
                    const replied = result.find(x => x.comment.id === comment.replyTo);

                    if (replied) {
                        replied.replies.push(item);
                    }
                } else {
                    result.push(item);
                }
            });

            return result;
        });
    }
}

export type CommentItem = { comment: Comment; index: number; isSelected: boolean; replies: CommentItem[] };