/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { AsyncPipe } from '@angular/common';
import { ChangeDetectionStrategy, Component } from '@angular/core';
import { LayoutComponent, SchemasState } from '@app/shared';
import { ContentExtensionComponent } from '../../shared/content-extension.component';

@Component({
    standalone: true,
    selector: 'sqx-contents-plugin',
    styleUrls: ['./contents-plugin.component.scss'],
    templateUrl: './contents-plugin.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush,
    imports: [
        AsyncPipe,
        ContentExtensionComponent,
        LayoutComponent,
    ],
})
export class ContentsPluginComponent {
    public schema = this.schemasState.selectedSchema;

    constructor(
        private readonly schemasState: SchemasState,
    ) {
    }
}