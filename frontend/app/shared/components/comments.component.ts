/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { ChangeDetectorRef, Component, Input, OnInit } from '@angular/core';
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

    @Input()
    public commentsId: string;

    public commentsState: CommentsState;
    public commentForm = new UpsertCommentForm(this.formBuilder);

    public userId: string;
    public users: ReadonlyArray<string> = ['foobar@abc'];

    constructor(authService: AuthService,
        private readonly appsState: AppsState,
        private readonly commentsService: CommentsService,
        private readonly changeDetector: ChangeDetectorRef,
        private readonly dialogs: DialogService,
        private readonly formBuilder: FormBuilder
    ) {
        super();

        this.userId = authService.user!.token;
    }

    public ngOnInit() {
        this.commentsState = new CommentsState(this.appsState, this.commentsId, this.commentsService, this.dialogs);

        this.own(timer(0, 4000).pipe(switchMap(() => this.commentsState.load())));
    }

    public delete(comment: CommentDto) {
        this.commentsState.delete(comment);
    }

    public update(comment: CommentDto, text: string) {
        this.commentsState.update(comment, text);
    }

    public comment() {
        const value = this.commentForm.submit();

        if (value && value.text && value.text.length > 0) {
            this.commentsState.create(value.text);
            this.commentForm.submitCompleted();

            this.changeDetector.detectChanges();
        }
    }

    public trackByComment(index: number, comment: CommentDto) {
        return comment.id;
    }
}