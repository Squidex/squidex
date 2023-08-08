/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { booleanAttribute, ChangeDetectionStrategy, Component, Input } from '@angular/core';

@Component({
    selector: 'sqx-logo',
    styleUrls: ['./logo.component.scss'],
    templateUrl: './logo.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush,
})
export class LogoComponent {
    @Input({ transform: booleanAttribute })
    public isLoading?: boolean | null;
}
