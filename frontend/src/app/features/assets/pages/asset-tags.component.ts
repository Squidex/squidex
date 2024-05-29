/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

/* eslint-disable @typescript-eslint/no-unnecessary-boolean-literal-compare */


import { booleanAttribute, ChangeDetectionStrategy, Component, EventEmitter, Input, Output } from '@angular/core';
import { DialogModel, ModalDirective, StopClickDirective, TagItem, TagsSelected, TranslatePipe } from '@app/shared';
import { AssetTagDialogComponent } from './asset-tag-dialog.component';

@Component({
    standalone: true,
    selector: 'sqx-asset-tags',
    styleUrls: ['./asset-tags.component.scss'],
    templateUrl: './asset-tags.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush,
    imports: [
        AssetTagDialogComponent,
        ModalDirective,
        StopClickDirective,
        TranslatePipe,
    ],
})
export class AssetTagsComponent {
    @Output()
    public tagsReset = new EventEmitter();

    @Output()
    public toggle = new EventEmitter<string>();

    @Input({ required: true })
    public tags!: ReadonlyArray<TagItem>;

    @Input({ required: true })
    public tagsSelected!: TagsSelected;

    @Input({ transform: booleanAttribute })
    public canRename = false;

    public tagRenaming?: TagItem;
    public tagRenameDialog = new DialogModel();

    public isEmpty() {
        return Object.keys(this.tagsSelected).length === 0;
    }

    public isSelected(tag: TagItem) {
        return this.tagsSelected[tag.name] === true;
    }

    public renameTag(tag: TagItem) {
        this.tagRenaming = tag;
        this.tagRenameDialog.show();
    }
}
