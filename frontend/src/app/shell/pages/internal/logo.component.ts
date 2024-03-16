/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { ChangeDetectionStrategy, Component, Input } from '@angular/core';

@Component({
    selector: 'sqx-logo',
    styleUrls: ['./logo.component.scss'],
    templateUrl: './logo.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush,
})
export class LogoComponent {
    @Input()
    public isLoading?: boolean | null;
}
