/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { booleanAttribute, Component, computed, inject, input, signal, viewChild } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { FormField } from '@angular/forms/signals';
import { Router } from '@angular/router';
import { AutocompleteComponent, Keys, MessageBus, TranslatePipe } from '@app/framework';
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
        AutocompleteComponent,
        CommentComponent,
        FormField,
        FormsModule,
        TranslatePipe,
        UserPicturePipe,
    ],
})
export class CommentsComponent {
    private readonly messageBus = inject(MessageBus);
    private readonly input = viewChild<AutocompleteComponent>('input');
    private readonly selection = signal<ReadonlyArray<string>>([]);

    protected readonly commentsState = inject(CommentsState);
    protected readonly router = inject(Router);
    protected readonly contributorsDataSource = inject(ContributorsDataSource);

    /** Shows all comments instead of only the unresolved comments. */
    public readonly resolved = input(false, { transform: booleanAttribute });

    /** The content the comments belong to. */
    public readonly content = input<ContentDto | null>();

    /** The ID of the comments thread. */
    public readonly commentsId = input<string | null>('');

    protected readonly commentForm = new UpsertCommentForm();
    protected readonly commentUser = inject(AuthService).user!.token;
    protected readonly reference = signal<AnnotationCreateAfterNavigate | undefined>(undefined);

    protected readonly commentItems = computed(() => {
        return this.resolved() ?
            this.commentsState.groupedComments() :
            this.commentsState.groupedUnresolvedComments();
    });

    constructor() {
        this.contributorsDataSource.loadIfNotLoaded();

        this.messageBus.on(FieldSelected, message => {
            for (const content of this.commentsState.items()) {
                if (content.editorId === message.editorId) {
                    this.selection.set([content.id!]);
                }
            }
        });

        this.messageBus.on(AnnotationsSelectedAfterNavigate, message => {
            this.selection.set(message.commentIds);
        });

        this.messageBus.on(AnnotationCreateAfterNavigate, message => {
            this.reference.set(message);
            this.input()?.focus();
        });
    }

    protected comment() {
        this.commentForm.submit(async ({ text }) => {
            if (text && text.length > 0) {
                const reference = this.reference();

                const { from, to } = reference?.annotation || {};

                this.commentsState.create(this.commentUser, text, this.router.url, { editorId: reference?.editorId, from, to });
            }
        }, { resetAfterSubmit: true });
    }

    protected onKeyPress(event: KeyboardEvent) {
        if (Keys.isEnter(event) && !event.altKey && !event.shiftKey) {
            this.comment();
            event.preventDefault();
        }
    }

    protected onBlur() {
        setTimeout(() => {
            this.reference.set(undefined);
        }, 100);
    }

    protected isSelected(id: string | undefined) {
        return !!id && this.selection().includes(id);
    }
}
