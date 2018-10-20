/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { distinctUntilChanged, map, tap } from 'rxjs/operators';

import {
    DateTime,
    DialogService,
    ImmutableArray,
    notify,
    State,
    Version
} from '@app/framework';

import {
    CommentDto,
    CommentsService,
    UpsertCommentDto
} from './../services/comments.service';

interface Snapshot {
    comments: ImmutableArray<CommentDto>;

    version: Version;

    isLoaded?: boolean;
}

@Injectable()
export class CommentsState extends State<Snapshot> {
    public comments =
        this.changes.pipe(map(x => x.comments),
            distinctUntilChanged());

    public isLoaded =
        this.changes.pipe(map(x => !!x.isLoaded),
            distinctUntilChanged());

    constructor(
        private readonly commentsId: string,
        private readonly commentsService: CommentsService,
        private readonly dialogs: DialogService
    ) {
        super({ comments: ImmutableArray.empty(), version: new Version('') });
    }

    public load(): Observable<any> {
        return this.commentsService.getComments(this.commentsId, this.version).pipe(
            tap(dtos => {
                this.next(s => {
                    let comments = s.comments;

                    for (let created of dtos.createdComments) {
                        comments = comments.push(created);
                    }

                    for (let updated of dtos.updatedComments) {
                        comments = comments.replaceBy('id', updated);
                    }

                    for (let deleted of dtos.deletedComments) {
                        comments = comments.filter(x => x.id !== deleted);
                    }

                    return { ...s, comments, isLoaded: true, version: dtos.version };
                });
            }),
            notify(this.dialogs));
    }

    public create(request: UpsertCommentDto): Observable<any> {
        return this.commentsService.postComment(this.commentsId, request).pipe(
            tap(dto => {
                this.next(s => {
                    const comments = s.comments.push(dto);

                    return { ...s, comments };
                });
            }),
            notify(this.dialogs));
    }

    public update(commentId: string, request: UpsertCommentDto, now?: DateTime): Observable<any> {
        return this.commentsService.putComment(this.commentsId, commentId, request).pipe(
            tap(() => {
                this.next(s => {
                    const comments = s.comments.map(c => c.id === commentId ? update(c, request, now || DateTime.now()) : c);

                    return { ...s, comments };
                });
            }),
            notify(this.dialogs));
    }

    public delete(commentId: string): Observable<any> {
        return this.commentsService.deleteComment(this.commentsId, commentId).pipe(
            tap(dto => {
                this.next(s => {
                    const comments = s.comments.filter(c => c.id !== commentId);

                    return { ...s, comments, version: dto.version };
                });
            }),
            notify(this.dialogs));
    }

    private get version() {
        return this.snapshot.version;
    }
}

const update = (comment: CommentDto, request: UpsertCommentDto, now: DateTime) =>
    comment.with({ text: request.text, time: now });