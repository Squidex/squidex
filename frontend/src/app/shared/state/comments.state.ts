/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Observable } from 'rxjs';
import { map, tap } from 'rxjs/operators';
import { DateTime, DialogService, LoadingState, shareSubscribed, State, Version } from '@app/framework';
import { CommentDto, CommentsService } from './../services/comments.service';

interface Snapshot extends LoadingState {
    // The current comments.
    comments:  ReadonlyArray<CommentDto>;

    // The version of the comments state.
    version: Version;
}

export class CommentsState extends State<Snapshot> {
    public comments =
        this.project(x => x.comments);

    public isLoading =
        this.project(x => x.isLoading === true);

    public isLoaded =
        this.project(x => x.isLoaded === true);

    public versionNumber =
        this.project(x => parseInt(x.version.value, 10));

    constructor(
        private readonly commentsUrl: string,
        private readonly commentsService: CommentsService,
        private readonly dialogs: DialogService,
        private readonly orderDescending = false,
        initialVersion = -1,
    ) {
        super({ comments: [], version: new Version(initialVersion.toString()) }, 'Comments');
    }

    public load(silent = false): Observable<any> {
        return this.commentsService.getComments(this.commentsUrl, this.version).pipe(
            tap(payload => {
                this.next(s => {
                    let comments = s.comments;

                    for (const created of payload.createdComments) {
                        if (!comments.find(x => x.id === created.id)) {
                            if (this.orderDescending) {
                                comments = [created, ...comments];
                            } else {
                                comments = [...comments, created];
                            }
                        }
                    }

                    for (const updated of payload.updatedComments) {
                        comments = comments.replacedBy('id', updated);
                    }

                    for (const deleted of payload.deletedComments) {
                        comments = comments.filter(x => x.id !== deleted);
                    }

                    return { ...s, comments, isLoaded: true, version: payload.version };
                }, 'Loading Done');
            }),
            shareSubscribed(this.dialogs, { silent }));
    }

    public create(text: string, url?: string): Observable<CommentDto> {
        return this.commentsService.postComment(this.commentsUrl, { text, url }).pipe(
            tap(created => {
                this.next(s => {
                    const comments = [...s.comments, created];

                    return { ...s, comments };
                }, 'Created');
            }),
            shareSubscribed(this.dialogs));
    }

    public delete(comment: CommentDto): Observable<any> {
        return this.commentsService.deleteComment(this.commentsUrl, comment.id).pipe(
            tap(() => {
                this.next(s => {
                    const comments = s.comments.removedBy('id', comment);

                    return { ...s, comments };
                }, 'Deleted');
            }),
            shareSubscribed(this.dialogs));
    }

    public update(comment: CommentDto, text: string, now?: DateTime): Observable<CommentDto> {
        return this.commentsService.putComment(this.commentsUrl, comment.id, { text }).pipe(
            map(() => update(comment, text, now)),
            tap(updated => {
                this.next(s => {
                    const comments = s.comments.replacedBy('id', updated);

                    return { ...s, comments };
                }, 'Updated');
            }),
            shareSubscribed(this.dialogs));
    }

    private get version() {
        return this.snapshot.version;
    }
}

const update = (comment: CommentDto, text: string, time?: DateTime) =>
    new CommentDto(
        comment.id,
        time || DateTime.now(),
        text,
        comment.url,
        comment.user);
