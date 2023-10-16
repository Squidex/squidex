/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { booleanAttribute, ChangeDetectionStrategy, Component, EventEmitter, Input, Output } from '@angular/core';
import { MentionConfig } from 'angular-mentions';
import { Comment, ContributorDto, DialogService, Keys, StatefulComponent } from '@app/shared/internal';

interface State {
    isEditing: boolean;
}

@Component({
    selector: 'sqx-comment',
    styleUrls: ['./comment.component.scss'],
    templateUrl: './comment.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush,
})
export class CommentComponent extends StatefulComponent<State> {
    @Output()
    public delete = new EventEmitter();

    @Output()
    public edit = new EventEmitter<string>();

    @Input({ transform: booleanAttribute })
    public canFollow?: boolean | null;

    @Input({ transform: booleanAttribute })
    public canDelete?: boolean | null;

    @Input({ transform: booleanAttribute })
    public canEdit?: boolean | null;

    @Input({ transform: booleanAttribute })
    public confirmDelete?: boolean | null = true;

    @Input({ required: true })
    public comment!: Comment;

    @Input({ required: true })
    public commentId!: any;

    @Input()
    public userToken = '';

    @Input()
    public mentionUsers?: ReadonlyArray<ContributorDto>;

    public mentionConfig: MentionConfig = { dropUp: true, labelKey: 'contributorEmail' };

    public isDeletable = false;
    public isEditable = false;

    public editingText = '';

    constructor(
        private readonly dialogs: DialogService,
    ) {
        super({ isEditing: false });
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

    public doDelete() {
        if (!this.isDeletable && !this.canDelete) {
            return;
        }

        this.delete.emit();
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
                        this.doDelete();
                    }
                });
        } else {
            this.edit.emit(text);

            this.cancelEdit();
        }
    }
}
