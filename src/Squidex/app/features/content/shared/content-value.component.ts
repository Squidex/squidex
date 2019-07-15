/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { ChangeDetectionStrategy, Component, Input } from '@angular/core';

import { HtmlValue, Types } from '@app/shared';

@Component({
    selector: 'sqx-content-value',
    template: `
        <ng-container *ngIf="isPlain; else html">
            <span class="truncate">{{value}}</span>
        </ng-container>
        <ng-template #html>
            <span class="truncate" [innerHTML]="value.html"></span>
        </ng-template>
    `,
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class ContentValueComponent {
    public get isPlain() {
        return !Types.is(this.value, HtmlValue);
    }

    @Input()
    public value: any;
}