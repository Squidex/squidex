/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { ChangeDetectionStrategy, Component, computed } from '@angular/core';
import { ContentsState, LayoutComponent, SchemasState } from '@app/shared';
import { ContentExtensionComponent } from '../../shared/content-extension.component';

@Component({
    selector: 'sqx-sidebar-page',
    styleUrls: ['./sidebar-page.component.scss'],
    templateUrl: './sidebar-page.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush,
    imports: [
        ContentExtensionComponent,
        LayoutComponent,
    ],
})
export class SidebarPageComponent {
    protected readonly url = computed(() => {
        const schema = this.schemasState.selectedSchema();
        if (!schema) {
            return undefined;
        }

        const url =
            this.contentsState.selectedContent() ?
            schema.properties.contentSidebarUrl :
            schema.properties.contentsSidebarUrl;

        return url;
    });

    constructor(
        protected readonly contentsState: ContentsState,
        protected readonly schemasState: SchemasState,
    ) {
    }
}
