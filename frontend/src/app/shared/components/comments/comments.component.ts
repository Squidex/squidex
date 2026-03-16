/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { AsyncPipe } from '@angular/common';
import { booleanAttribute, ChangeDetectorRef, Component, Input, OnInit, ViewChild } from '@angular/core';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { AutocompleteComponent, Keys, MessageBus, Subscriptions, TranslatePipe } from '@app/framework';
import { AnnotationCreateAfterNavigate, AnnotationsSelectedAfterNavigate, AuthService, CommentsState, ContentDto, FieldSelected, UpsertCommentForm } from '@app/shared/internal';
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
    private selection: ReadonlyArray<string> = [];

    @ViewChild('input', { static: false })
    public input!: AutocompleteComponent;

    @Input({ transform: booleanAttribute })
    public resolved = false;

    @Input()
    public content?: ContentDto | null;

    @Input()
    public commentsId = '';

    public commentForm = new UpsertCommentForm();
    public commentUser: string;

    public reference?: AnnotationCreateAfterNavigate;

    public get commentItems() {
        return this.resolved ?
            this.commentsState.groupedComments :
            this.commentsState.groupedUnresolvedComments;
    }

    constructor(authService: AuthService,
        public readonly commentsState: CommentsState,
        public readonly router: Router,
        public readonly contributorsDataSource: ContributorsDataSource,
        private readonly changeDetector: ChangeDetectorRef,
        private readonly messageBus: MessageBus,
    ) {
        this.commentUser = authService.user!.token;
    }

    public ngOnInit() {
        this.contributorsDataSource.loadIfNotLoaded();

        this.subscriptions.add(
            this.messageBus.of(FieldSelected)
                .subscribe(message => {
                    for (let content of this.commentsState.items) {
                        if (content.editorId === message.editorId) {
                            this.selection = [content.id!];
                        }
                    }

                    this.changeDetector.detectChanges();
                }));

        this.subscriptions.add(
            this.messageBus.of(AnnotationsSelectedAfterNavigate)
                .subscribe(message => {
                    this.selection = message.commentIds;
                    this.changeDetector.detectChanges();
                }));

        this.subscriptions.add(
            this.messageBus.of(AnnotationCreateAfterNavigate)
                .subscribe(message => {
                    this.reference = message;
                    this.input.focus();
                    this.changeDetector.detectChanges();
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

    public isSelected(id: string | undefined) {
        return id && this.selection.includes(id);
    }
}