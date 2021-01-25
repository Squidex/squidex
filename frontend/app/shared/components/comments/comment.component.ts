/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { ChangeDetectionStrategy, ChangeDetectorRef, Component, Input, OnChanges } from '@angular/core';
import { CommentDto, CommentsState, ContributorDto, DialogService, Keys, StatefulComponent } from '@app/shared/internal';
import { MentionConfig } from 'angular-mentions';

interface State {
    isEditing: boolean;
}

@Component({
    selector: 'sqx-comment',
    styleUrls: ['./comment.component.scss'],
    templateUrl: './comment.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class CommentComponent extends StatefulComponent<State> implements OnChanges {
    @Input()
    public canFollow = false;

    @Input()
    public canDelete = false;

    @Input()
    public canEdit = false;

    @Input()
    public commentsState: CommentsState;

    @Input()
    public confirmDelete = true;

    @Input()
    public comment: CommentDto;

    @Input()
    public userToken: string;

    @Input()
    public mentionUsers: ReadonlyArray<ContributorDto>;

    public mentionConfig: MentionConfig = { dropUp: true, labelKey: 'contributorEmail' };

    public isDeletable = false;
    public isEditable = false;

    public editingText: string;

    constructor(changeDetector: ChangeDetectorRef,
        private readonly dialogs: DialogService
    ) {
        super(changeDetector, {
            isEditing: false
        });
    }

    public ngOnChanges() {
        const isMyComment = this.comment.user === this.userToken;

        this.isDeletable = isMyComment;
        this.isEditable = isMyComment;
    }

    public startEdit() {
        this.editingText = this.comment.text;

        this.next({ isEditing: true });
    }

    public cancelEdit() {
        this.next({ isEditing: false });
    }

    public delete() {
        if (!this.isDeletable && !this.canDelete) {
            return;
        }

        this.commentsState.delete(this.comment);
    }

    public updateWhenEnter(event: KeyboardEvent) {
        if (Keys.isEnter(event) && !event.altKey && !event.shiftKey && !event.defaultPrevented) {
            event.preventDefault();
            event.stopImmediatePropagation();
            event.stopPropagation();

            this.update();
        }
    }

    public update() {
        if (!this.isEditable) {
            return;
        }

        const text = this.editingText;

        if (!text || text.length === 0) {
            this.dialogs.confirm('i18n:comments.deleteConfirmTitle', 'i18n:comments.deleteConfirmText')
                .subscribe(confirmed => {
                    if (confirmed) {
                        this.delete();
                    }
                });
        } else {
            this.commentsState.update(this.comment, text);

            this.cancelEdit();
        }
    }
}