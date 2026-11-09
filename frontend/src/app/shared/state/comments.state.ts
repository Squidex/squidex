/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { computed, inject, Service } from '@angular/core';
import { DateTime, MathHelper } from '@app/framework';
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

    // Indicated whether the comment has been resolved.
    isResolved?: boolean;

    // Indicates whether this comment has been read.
    isRead?: boolean;

    // The editor ID.
    editorId?: string;

    // The selection range.
    from?: number;

    // The selection number.
    to?: number;
}

@Service({ autoProvided: false })
export class CommentsState {
    private readonly collaboration = inject(CollaborationService);
    private readonly comments = this.collaboration.getArray<Comment>('stream');

    public readonly items = computed(() => {
        return this.comments().items();
    });

    public readonly unreadCount = computed(() => {
        return this.items().filter(c => !c.isRead).length;
    });

    public readonly groupedComments = computed(() => {
        return groupComments(this.items(), false);
    });

    public readonly groupedUnresolvedComments = computed(() => {
        return groupComments(this.items(), true);
    });

    public create(user: string, text: string, url: string, optional?: Pick<Comment, 'editorId' | 'from' | 'to' | 'replyTo'>) {
        this.updateInternal(comments => {
            const comment = {
                id: MathHelper.guid(),
                user,
                text,
                url,
                time: DateTime.now().toISOString(),
                ...optional || {}
            };

            comments.add(comment);
        });
    }

    public update(index: number, update: Partial<Comment>) {
        this.updateInternal(comments => {
            const comment = comments.items()[index];

            if (comment) {
                comments.set(index, { ...comment, ...update });
            }
        });
    }

    public prune(maxCount: number) {
        this.updateInternal(comments => {
            const toDelete = comments.items().length - maxCount;

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
            comments.items().forEach((comment, index) => {
                if (!comment.isRead) {
                    comments.set(index, { ...comment, isRead: true });
                }
            });
        });
    }

    public updateAnnotations(editorId: string, updates: ReadonlyArray<Annotation>) {
        this.updateInternal(comments => {
            comments.items().map((comment, index) => {
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
        const comments = this.comments();

        if (!comments.doc) {
            return;
        }

        comments.doc.transact(() => {
            update(comments);
        });

    }

    public getAnnotations(editorId: string | undefined | null): ReadonlyArray<Annotation> {
        if (!editorId) {
            return [];
        }

        const annotations: Annotation[] = [];

        for (const comment of this.items()) {
            if (isAnnotation(comment, editorId)) {
                annotations.push(comment as never);
            }
        }

        return annotations;
    }
}

function isAnnotation(comment: Comment, editorId: string) {
    if (comment.editorId !== editorId) {
        return false;
    }

    if (!comment.from || !comment.to || !comment.id) {
        return false;
    }

    return comment.isResolved !== true;
}

function groupComments(comments: ReadonlyArray<Comment>, unresolvedOnly = true) {
    const result: CommentItem[] = [];

    comments.forEach((comment, index) => {
        const item = { comment, index, id: comment.id || index.toString(), replies: [] };

        if (comment.replyTo) {
            const replied = result.find(x => x.comment.id === comment.replyTo);

            if (replied) {
                replied.replies.push(item);
            }
        } else {
            result.push(item);
        }
    });

    if (unresolvedOnly) {
        return result.filter(x => !x.comment.isResolved);
    }

    return result;
}

export type CommentItem = { comment: Comment; index: number; replies: CommentItem[] };