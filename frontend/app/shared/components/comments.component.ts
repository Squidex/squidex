/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Component, Input, OnInit } from '@angular/core';
import { FormBuilder } from '@angular/forms';
import { timer } from 'rxjs';
import { switchMap } from 'rxjs/operators';

import {
    AppsState,
    AuthService,
    CommentDto,
    CommentsService,
    CommentsState,
    DialogService,
    ResourceOwner,
    UpsertCommentForm
} from '@app/shared/internal';

@Component({
    selector: 'sqx-comments',
    styleUrls: ['./comments.component.scss'],
    templateUrl: './comments.component.html'
})
export class CommentsComponent extends ResourceOwner implements OnInit {
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
        super();

        this.userId = authService.user!.token;
    }

    public ngOnInit() {
        this.state = new CommentsState(this.appsState, this.commentsId, this.commentsService, this.dialogs);

        this.own(timer(0, 4000).pipe(switchMap(() => this.state.load())));
    }

    public delete(comment: CommentDto) {
        this.state.delete(comment);
    }

    public update(comment: CommentDto, text: string) {
        this.state.update(comment, text);
    }

    public comment() {
        const value = this.commentForm.submit();

        if (value) {
            this.state.create(value.text);

            this.commentForm.submitCompleted();
        }
    }

    public trackByComment(index: number, comment: CommentDto) {
        return comment.id;
    }
}