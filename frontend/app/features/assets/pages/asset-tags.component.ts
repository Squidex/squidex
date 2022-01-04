/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

/* eslint-disable @typescript-eslint/no-unnecessary-boolean-literal-compare */

import { ChangeDetectionStrategy, Component, EventEmitter, Input, Output } from '@angular/core';
import { DialogModel, TagItem, TagsSelected } from '@app/shared';

@Component({
    selector: 'sqx-asset-tags[tags][tagsSelected]',
    styleUrls: ['./asset-tags.component.scss'],
    templateUrl: './asset-tags.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush,
})
export class AssetTagsComponent {
    @Output()
    public reset = new EventEmitter();

    @Output()
    public toggle = new EventEmitter<string>();

    @Input()
    public tags!: ReadonlyArray<TagItem>;

    @Input()
    public tagsSelected!: TagsSelected;

    @Input()
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

    public trackByTag(_index: number, tag: TagItem) {
        return tag.name;
    }
}
