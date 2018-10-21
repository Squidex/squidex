/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Component, Input, OnDestroy, OnInit } from '@angular/core';
import { FormBuilder } from '@angular/forms';
import { Subscription, timer } from 'rxjs';
import { onErrorResumeNext, switchMap } from 'rxjs/operators';

import {
    AppsState,
    AuthService,
    CommentDto,
    CommentsService,
    CommentsState,
    DialogService,
    UpsertCommentForm
} from '@app/shared/internal';

@Component({
    selector: 'sqx-comments',
    styleUrls: ['./comments.component.scss'],
    templateUrl: './comments.component.html'
})
export class CommentsComponent implements OnDestroy, OnInit {
    private timer: Subscription;

    public state: CommentsState;

    public userId: string;

    public commentForm = new UpsertCommentForm(this.formBuilder);

    @Input()
    public commentsId: string;

    constructor(authService: AuthService,
        private readonly appsState: AppsState,
        private readonly commentsService: CommentsService,
        private readonly dialogs: DialogService,
        private readonly formBuilder: FormBuilder
    ) {
        this.userId = authService.user!.token;
    }

    public ngOnDestroy() {
        this.timer.unsubscribe();
    }

    public ngOnInit() {
        this.state = new CommentsState(this.appsState, this.commentsId, this.commentsService, this.dialogs);

        this.timer = timer(0, 4000).pipe(switchMap(() => this.state.load()), onErrorResumeNext()).subscribe();
    }

    public delete(comment: CommentDto) {
        this.state.delete(comment.id).pipe(onErrorResumeNext()).subscribe();
    }

    public update(comment: CommentDto, text: string) {
        this.state.update(comment.id, text).pipe(onErrorResumeNext()).subscribe();
    }

    public comment() {
        const value = this.commentForm.submit();

        if (value) {
            this.state.create(value.text).pipe(onErrorResumeNext()).subscribe();

            this.commentForm.submitCompleted({});
        }
    }

    public trackByComment(index: number, comment: CommentDto) {
        return comment.id;
    }
}