/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Observable } from 'rxjs';
import { distinctUntilChanged, map, share } from 'rxjs/operators';

import {
    DateTime,
    DialogService,
    ImmutableArray,
    notify,
    State,
    Version
} from '@app/framework';

import { CommentDto,  CommentsDto, CommentsService } from './../services/comments.service';
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

    public load(): Observable<CommentsDto> {
        const stream =
            this.commentsService.getComments(this.appName, this.commentsId, this.version).pipe(
                share());

        stream.subscribe(response => {
            this.next(s => {
                let comments = s.comments;

                for (let created of response.createdComments) {
                    if (!comments.find(x => x.id === created.id)) {
                        comments = comments.push(created);
                    }
                }

                for (let updated of response.updatedComments) {
                    comments = comments.replaceBy('id', updated);
                }

                for (let deleted of response.deletedComments) {
                    comments = comments.filter(x => x.id !== deleted);
                }

                return { ...s, comments, isLoaded: true, version: response.version };
            });
        }, error => {
            this.dialogs.notifyError(error);
        });

        return stream;
    }

    public create(text: string): Observable<CommentDto> {
        const stream =
            this.commentsService.postComment(this.appName, this.commentsId, { text }).pipe(
                share());

        stream.subscribe(comment => {
            this.next(s => {
                const comments = s.comments.push(comment);

                return { ...s, comments };
            });
        }, error => {
            this.dialogs.notifyError(error);
        });

        return stream;
    }

    public update(comment: CommentDto, text: string, now?: DateTime): Observable<CommentDto> {
        const stream =
            this.commentsService.putComment(this.appName, this.commentsId, comment.id, { text }).pipe(
                map(() => update(comment, text, now || DateTime.now())), share());

        stream.subscribe(updated => {
            this.next(s => {
                const comments = s.comments.replaceBy('id', updated);

                return { ...s, comments };
            });
        }, error => {
            this.dialogs.notifyError(error);
        });

        return stream;
    }

    public delete(comment: CommentDto): Observable<any> {
        const stream =
            this.commentsService.deleteComment(this.appName, this.commentsId, comment.id).pipe(
                share());

        stream.subscribe(() => {
            this.next(s => {
                const comments = s.comments.removeBy('id', comment);

                return { ...s, comments };
            });
        }, error => {
            this.dialogs.notifyError(error);
        });

        return stream;
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