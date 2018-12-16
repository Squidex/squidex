/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

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

import { AppsState } from './apps.state';

interface Snapshot {
    comments: ImmutableArray<CommentDto>;

    version: Version;

    isLoaded?: boolean;
}

export class CommentsState extends State<Snapshot> {
    public comments =
        this.changes.pipe(map(x => x.comments),
            distinctUntilChanged());

    public isLoaded =
        this.changes.pipe(map(x => !!x.isLoaded),
            distinctUntilChanged());

    constructor(
        private readonly appsState: AppsState,
        private readonly commentsId: string,
        private readonly commentsService: CommentsService,
        private readonly dialogs: DialogService
    ) {
        super({ comments: ImmutableArray.empty(), version: new Version('-1') });
    }

    public load(): Observable<any> {
        return this.commentsService.getComments(this.appName, this.commentsId, this.version).pipe(
            tap(dtos => {
                this.next(s => {
                    let comments = s.comments;

                    for (let created of dtos.createdComments) {
                        if (!comments.find(x => x.id === created.id)) {
                            comments = comments.push(created);
                        }
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

    public create(text: string): Observable<any> {
        return this.commentsService.postComment(this.appName, this.commentsId, new UpsertCommentDto(text)).pipe(
            tap(dto => {
                this.next(s => {
                    const comments = s.comments.push(dto);

                    return { ...s, comments };
                });
            }),
            notify(this.dialogs));
    }

    public update(commentId: string, text: string, now?: DateTime): Observable<any> {
        return this.commentsService.putComment(this.appName, this.commentsId, commentId, new UpsertCommentDto(text)).pipe(
            tap(() => {
                this.next(s => {
                    const comments = s.comments.map(c => c.id === commentId ? update(c, text, now || DateTime.now()) : c);

                    return { ...s, comments };
                });
            }),
            notify(this.dialogs));
    }

    public delete(commentId: string): Observable<any> {
        return this.commentsService.deleteComment(this.appName, this.commentsId, commentId).pipe(
            tap(() => {
                this.next(s => {
                    const comments = s.comments.filter(c => c.id !== commentId);

                    return { ...s, comments };
                });
            }),
            notify(this.dialogs));
    }

    private get version() {
        return this.snapshot.version;
    }

    private get appName() {
        return this.appsState.appName;
    }
}

const update = (comment: CommentDto, text: string, time: DateTime) =>
    comment.with({ text, time });