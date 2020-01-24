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
    template: `
        <a class="sidebar-item" (click)="reset.emit()" [class.active]="isEmpty()">
            <div class="row">
                <div class="col">
                    All tags
                </div>
            </div>
        </a>

        <a class="sidebar-item" *ngFor="let tag of tags; trackBy: trackByTag" (click)="toggle.emit(tag.name)" [class.active]="isSelected(tag)">
            <div class="row">
                <div class="col">
                    {{tag.name}}
                </div>
                <div class="col-auto">
                    {{tag.count}}
                </div>
            </div>
        </a>
    `,
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
        return !!this.tagsSelected[tag.name];
    }

    public trackByTag(index: number, tag: Tag) {
        return tag.name;
    }
}