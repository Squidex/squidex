/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

/* eslint-disable @angular-eslint/no-output-native */
/* eslint-disable @typescript-eslint/no-unnecessary-boolean-literal-compare */

import { booleanAttribute, Component, computed, input, output, signal } from '@angular/core';
import { DialogModel, ModalDirective, StopClickDirective, TagItem, TagsSelected, TranslatePipe } from '@app/shared';
import { AssetTagDialogComponent } from './asset-tag-dialog.component';

@Component({
    selector: 'sqx-asset-tags',
    styleUrls: ['./asset-tags.component.scss'],
    templateUrl: './asset-tags.component.html',
    imports: [
        AssetTagDialogComponent,
        ModalDirective,
        StopClickDirective,
        TranslatePipe,
    ],
})
export class AssetTagsComponent {
    /** Emits when the user clicks the "all tags" item to clear the tag selection. */
    public readonly tagsReset = output();

    /** Emits the name of a tag when the user clicks it to select or unselect it. */
    public readonly toggle = output<string>();

    /** The tags to list, with their usage count. */
    public readonly tags = input.required<ReadonlyArray<TagItem>>();

    /** The currently selected tags, highlighted in the list. */
    public readonly tagsSelected = input.required<TagsSelected>();

    /** Shows a button to rename each tag. */
    public readonly canRename = input(false, { transform: booleanAttribute });

    protected readonly tagRenaming = signal<TagItem | undefined>(undefined);

    protected readonly isEmpty = computed(() => {
        return Object.keys(this.tagsSelected()).length === 0;
    });

    protected readonly tagRenameDialog = new DialogModel();

    protected isSelected(tag: TagItem) {
        return this.tagsSelected()[tag.name] === true;
    }

    protected renameTag(tag: TagItem) {
        this.tagRenaming.set(tag);
        this.tagRenameDialog.show();
    }
}
