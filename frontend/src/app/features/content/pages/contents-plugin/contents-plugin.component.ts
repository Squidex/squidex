/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { ChangeDetectionStrategy, Component, inject } from '@angular/core';
import { LayoutComponent, SchemasState } from '@app/shared';
import { ContentExtensionComponent } from '../../shared/content-extension.component';

@Component({
    selector: 'sqx-contents-plugin',
    styleUrls: ['./contents-plugin.component.scss'],
    templateUrl: './contents-plugin.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush,
    imports: [
        ContentExtensionComponent,
        LayoutComponent,
    ],
})
export class ContentsPluginComponent {
    private readonly schemasState = inject(SchemasState);

    protected readonly schema = this.schemasState.selectedSchema;
}