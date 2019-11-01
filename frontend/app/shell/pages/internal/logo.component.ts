/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { ChangeDetectionStrategy, Component, Input } from '@angular/core';

@Component({
    selector: 'sqx-logo',
    template: `
        <img [class.hidden]="!isLoading" class="loader" src="./images/loader-white.gif" />

        <i [class.hidden]="isLoading" class="icon-logo"></i>
    `,
    styles: [`
        .loader {
            position: absolute; top: 12px; left: 40px;
        }`
    ],
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class LogoComponent {
    @Input()
    public isLoading = false;
}