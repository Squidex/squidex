/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { ChangeDetectionStrategy, Component, EventEmitter, Input, Output } from '@angular/core';
import { Tag, TagsSelected } from '@app/shared';

@Component({
    selector: 'sqx-asset-tags',
    styleUrls: ['./asset-tags.component.scss'],
    templateUrl: './asset-tags.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class AssetTagsComponent {
    @Output()
    public reset = new EventEmitter();

    @Output()
    public toggle = new EventEmitter<string>();

    @Input()
    public tags: ReadonlyArray<Tag>;

    @Input()
    public tagsSelected: TagsSelected;

    public isEmpty() {
        return Object.keys(this.tagsSelected).length === 0;
    }

    public isSelected(tag: Tag) {
        return this.tagsSelected[tag.name] === true;
    }

    public trackByTag(_index: number, tag: Tag) {
        return tag.name;
    }
}