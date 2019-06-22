/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Observable } from 'rxjs';
import { map, tap } from 'rxjs/operators';

import {
    DateTime,
    DialogService,
    ImmutableArray,
    shareSubscribed,
    State,
    Version
} from '@app/framework';

import { CommentDto, CommentsService } from './../services/comments.service';
import { AppsState } from './apps.state';

interface Snapshot {
    // The current comments.
    comments: CommentsList;

    // The version of the comments state.
    version: Version;

    // Indicates if the comments are loaded.
    isLoaded?: boolean;
}

type CommentsList = ImmutableArray<CommentDto>;

export class CommentsState extends State<Snapshot> {
    public comments =
        this.project(x => x.comments);

    public isLoaded =
        this.project(x => !!x.isLoaded);

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
            tap(payload => {
                this.next(s => {
                    let comments = s.comments;

                    for (let created of payload.createdComments) {
                        if (!comments.find(x => x.id === created.id)) {
                            comments = comments.push(created);
                        }
                    }

                    for (let updated of payload.updatedComments) {
                        comments = comments.replaceBy('id', updated);
                    }

                    for (let deleted of payload.deletedComments) {
                        comments = comments.filter(x => x.id !== deleted);
                    }

                    return { ...s, comments, isLoaded: true, version: payload.version };
                });
            }),
            shareSubscribed(this.dialogs));
    }

    public create(text: string): Observable<CommentDto> {
        return this.commentsService.postComment(this.appName, this.commentsId, { text }).pipe(
            tap(comment => {
                this.next(s => {
                    const comments = s.comments.push(comment);

                    return { ...s, comments };
                });
            }),
            shareSubscribed(this.dialogs));
    }

    public delete(comment: CommentDto): Observable<any> {
        return this.commentsService.deleteComment(this.appName, this.commentsId, comment.id).pipe(
            tap(() => {
                this.next(s => {
                    const comments = s.comments.removeBy('id', comment);

                    return { ...s, comments };
                });
            }),
            shareSubscribed(this.dialogs));
    }

    public update(comment: CommentDto, text: string, now?: DateTime): Observable<CommentDto> {
        return this.commentsService.putComment(this.appName, this.commentsId, comment.id, { text }).pipe(
            map(() => update(comment, text, now || DateTime.now())),
            tap(updated => {
                this.next(s => {
                    const comments = s.comments.replaceBy('id', updated);

                    return { ...s, comments };
                });
            }),
            shareSubscribed(this.dialogs));
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