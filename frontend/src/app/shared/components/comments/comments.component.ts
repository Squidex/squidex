/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { AsyncPipe } from '@angular/common';
import { Component, ElementRef, Input, OnInit, ViewChild } from '@angular/core';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { MentionConfig, MentionModule } from 'angular-mentions';
import { BehaviorSubject } from 'rxjs';
import { MessageBus, ResizedDirective, Subscriptions, TranslatePipe } from '@app/framework';
import { AnnotationCreateAfterNavigate, AnnotationsSelectAfterNavigate, AuthService, CommentsState, ContributorsState, UpsertCommentForm } from '@app/shared/internal';
import { CommentComponent } from './comment.component';

@Component({
    standalone: true,
    selector: 'sqx-comments',
    styleUrls: ['./comments.component.scss'],
    templateUrl: './comments.component.html',
    imports: [
        AsyncPipe,
        CommentComponent,
        FormsModule,
        MentionModule,
        ReactiveFormsModule,
        ResizedDirective,
        TranslatePipe,
    ],
})
export class CommentsComponent implements OnInit {
    private readonly subscriptions = new Subscriptions();
    private readonly selection = new BehaviorSubject<ReadonlyArray<string>>([]);
    private reference?: AnnotationCreateAfterNavigate;

    @ViewChild('input', { static: false })
    public input!: ElementRef<HTMLInputElement>;

    @Input()
    public commentsId = '';

    public mentionUsers = this.contributorsState.contributors;
    public mentionConfig: MentionConfig = { dropUp: true, labelKey: 'contributorEmail' };

    public commentForm = new UpsertCommentForm();
    public commentsItems = this.commentsState.getGroupedComments(this.selection);
    public commentUser: string;

    constructor(authService: AuthService,
        public readonly commentsState: CommentsState,
        public readonly router: Router,
        private readonly contributorsState: ContributorsState,
        private readonly messageBus: MessageBus,
    ) {
        this.commentUser = authService.user!.token;
    }

    public ngOnInit() {
        this.subscriptions.add(
            this.messageBus.of(AnnotationsSelectAfterNavigate)
                .subscribe(message => {
                    this.selection.next(message.annotations);
                }));

        this.subscriptions.add(
            this.messageBus.of(AnnotationCreateAfterNavigate)
                .subscribe(message => {
                    this.reference = message;

                    this.input.nativeElement.focus();
                }));
    }

    public comment() {
        const { text } = this.commentForm.submit() || {};

        if (text && text.length > 0) {
            const { from, to } = this.reference?.annotation || {};

            this.commentsState.create(this.commentUser, text, this.router.url, { editorId: this.reference?.editorId, from, to });
        }

        this.commentForm.submitCompleted();
    }

    public blurComment() {
        setTimeout(() => {
            this.reference = undefined;
        }, 100);
    }
}
