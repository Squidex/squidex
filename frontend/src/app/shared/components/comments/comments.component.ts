/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { AsyncPipe } from '@angular/common';
import { Component, Input, OnInit, ViewChild } from '@angular/core';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { BehaviorSubject } from 'rxjs';
import { AutocompleteComponent, Keys, MessageBus, Subscriptions, TranslatePipe } from '@app/framework';
import { AnnotationCreateAfterNavigate, AnnotationsSelectAfterNavigate, AuthService, CommentsState, UpsertCommentForm } from '@app/shared/internal';
import { UserPicturePipe } from '../pipes';
import { CommentComponent } from './comment.component';
import { ContributorsDataSource } from './data-source';

@Component({
    selector: 'sqx-comments',
    styleUrls: ['./comments.component.scss'],
    templateUrl: './comments.component.html',
    providers: [
        ContributorsDataSource,
    ],
    imports: [
        AsyncPipe,
        AutocompleteComponent,
        CommentComponent,
        FormsModule,
        ReactiveFormsModule,
        TranslatePipe,
        UserPicturePipe,
    ],
})
export class CommentsComponent implements OnInit {
    private readonly subscriptions = new Subscriptions();
    private readonly selection = new BehaviorSubject<ReadonlyArray<string>>([]);
    private reference?: AnnotationCreateAfterNavigate;

    @ViewChild('input', { static: false })
    public input!: AutocompleteComponent;

    @Input()
    public commentsId = '';

    public commentForm = new UpsertCommentForm();
    public commentsItems = this.commentsState.getGroupedComments(this.selection);
    public commentUser: string;

    constructor(authService: AuthService,
        public readonly commentsState: CommentsState,
        public readonly router: Router,
        public readonly contributorsDataSource: ContributorsDataSource,
        private readonly messageBus: MessageBus,
    ) {
        this.commentUser = authService.user!.token;
    }

    public ngOnInit() {
        this.contributorsDataSource.loadIfNotLoaded();

        this.subscriptions.add(
            this.messageBus.of(AnnotationsSelectAfterNavigate)
                .subscribe(message => {
                    this.selection.next(message.annotations);
                }));

        this.subscriptions.add(
            this.messageBus.of(AnnotationCreateAfterNavigate)
                .subscribe(message => {
                    this.reference = message;

                    this.input.focus();
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

    public onKeyPress(event: KeyboardEvent) {
        if (Keys.isEnter(event) && !event.altKey && !event.shiftKey) {
            this.comment();
            event.preventDefault();
        }
    }

    public onBlur() {
        setTimeout(() => {
            this.reference = undefined;
        }, 100);
    }
}