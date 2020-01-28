/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { ChangeDetectorRef, Component, Input, OnInit } from '@angular/core';
import { FormBuilder } from '@angular/forms';
import { Router } from '@angular/router';
import { timer } from 'rxjs';
import { filter, map, onErrorResumeNext, switchMap } from 'rxjs/operators';

import {
    AppsState,
    AuthService,
    CommentDto,
    CommentsService,
    CommentsState,
    ContributorsState,
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
    @Input()
    public commentsId: string;

    public commentsUrl: string;
    public commentsState: CommentsState;
    public commentForm = new UpsertCommentForm(this.formBuilder);

    public mentionUsers = this.contributorsState.contributors.pipe(map(x => x.map(c => c.contributorEmail), filter(x => !!x)));
    public mentionConfig = { dropUp: true };

    public userToken: string;

    constructor(authService: AuthService,
        private readonly appsState: AppsState,
        private readonly commentsService: CommentsService,
        private readonly contributorsState: ContributorsState,
        private readonly changeDetector: ChangeDetectorRef,
        private readonly dialogs: DialogService,
        private readonly formBuilder: FormBuilder,
        private readonly router: Router
    ) {
        super();

        this.userToken = authService.user!.token;
    }

    public ngOnInit() {
        this.contributorsState.load();

        this.commentsUrl = `apps/${this.appsState.appName}/comments/${this.commentsId}`;
        this.commentsState = new CommentsState(this.commentsUrl, this.commentsService, this.dialogs);

        this.own(timer(0, 4000).pipe(switchMap(() => this.commentsState.load(true).pipe(onErrorResumeNext()))));
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
            this.commentsState.create(value.text, this.router.url);

            this.changeDetector.detectChanges();
        }

        this.commentForm.submitCompleted();
    }

    public trackByComment(index: number, comment: CommentDto) {
        return comment.id;
    }
}