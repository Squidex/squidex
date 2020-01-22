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
            <span class="html-value" [innerHTML]="value.html"></span>
        </ng-template>
    `,
    styles: [`
        .html-value {
            position: relative;
        }
        ::ng-deep .html-value img {
            position: absolute;
            min-height: 50px;
            max-height: 50px;
            margin-top: -25px;
        }`
    ],
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class ContentValueComponent {
    @Input()
    public value: any;

    public get isPlain() {
        return !Types.is(this.value, HtmlValue);
    }
}