/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { ChangeDetectionStrategy, Component, EventEmitter, Input, Output } from '@angular/core';

import { AssetPathItem } from '@app/shared/internal';

@Component({
    selector: 'sqx-asset-path',
    template: `
        <ng-container *ngIf="path.length === 0; else normalPath">
            <span class="btn">Search Results</span>
        </ng-container>
        <ng-template #normalPath>
            <ng-container *ngFor="let item of path; let i = index">
                <i class="icon-angle-right" *ngIf="i > 0"></i>

                <a class="btn" (click)="emitNavigate(item)" [class.force]="i < path.length - 1">
                    {{item.folderName}}
                </a>
            </ng-container>
        </ng-template>
    `,
    styles: [`
        i {
            vertical-align: middle;
        }`
    ],
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class AssetPathComponent {
    @Output()
    public navigate = new EventEmitter<AssetPathItem>();

    @Input()
    public path: ReadonlyArray<AssetPathItem>;

    public emitNavigate(item: AssetPathItem) {
        this.navigate.emit(item);
    }
}