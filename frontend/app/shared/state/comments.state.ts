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
    shareSubscribed,
    State,
    Version
} from '@app/framework';

import { CommentDto, CommentsService } from './../services/comments.service';

interface Snapshot {
    // The current comments.
    comments: CommentsList;

    // The version of the comments state.
    version: Version;

    // Indicates if the comments are loaded.
    isLoaded?: boolean;
}

type CommentsList = ReadonlyArray<CommentDto>;

export class CommentsState extends State<Snapshot> {
    public comments =
        this.project(x => x.comments);

    public isLoaded =
        this.project(x => x.isLoaded === true);

    public versionNumber =
        this.project(x => parseInt(x.version.value, 10));

    constructor(
        private readonly commentsUrl: string,
        private readonly commentsService: CommentsService,
        private readonly dialogs: DialogService,
        initialVersion = -1
    ) {
        super({ comments: [], version: new Version(initialVersion.toString()) });
    }

    public load(silent = false): Observable<any> {
        return this.commentsService.getComments(this.commentsUrl, this.version).pipe(
            tap(payload => {
                this.next(s => {
                    let comments = s.comments;

                    for (const created of payload.createdComments) {
                        if (!comments.find(x => x.id === created.id)) {
                            comments = [...comments, created];
                        }
                    }

                    for (const updated of payload.updatedComments) {
                        comments = comments.replaceBy('id', updated);
                    }

                    for (const deleted of payload.deletedComments) {
                        comments = comments.filter(x => x.id !== deleted);
                    }

                    return { ...s, comments, isLoaded: true, version: payload.version };
                });
            }),
            shareSubscribed(this.dialogs, { silent }));
    }

    public create(text: string, url?: string): Observable<CommentDto> {
        return this.commentsService.postComment(this.commentsUrl, { text, url }).pipe(
            tap(created => {
                this.next(s => {
                    const comments = [...s.comments, created];

                    return { ...s, comments };
                });
            }),
            shareSubscribed(this.dialogs));
    }

    public delete(comment: CommentDto): Observable<any> {
        return this.commentsService.deleteComment(this.commentsUrl, comment.id).pipe(
            tap(() => {
                this.next(s => {
                    const comments = s.comments.removeBy('id', comment);

                    return { ...s, comments };
                });
            }),
            shareSubscribed(this.dialogs));
    }

    public update(comment: CommentDto, text: string, now?: DateTime): Observable<CommentDto> {
        return this.commentsService.putComment(this.commentsUrl, comment.id, { text }).pipe(
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
}

const update = (comment: CommentDto, text: string, time: DateTime) =>
    comment.with({ text, time });