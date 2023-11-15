/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { AsyncPipe } from '@angular/common';
import { ChangeDetectionStrategy, Component } from '@angular/core';
import { combineLatest } from 'rxjs';
import { map } from 'rxjs/operators';
import { LayoutComponent } from '@app/shared';
import { defined } from '@app/shared';
import { ContentsState, SchemasState } from '@app/shared';
import { ContentExtensionComponent } from '../../shared/content-extension.component';

@Component({
    selector: 'sqx-sidebar-page',
    styleUrls: ['./sidebar-page.component.scss'],
    templateUrl: './sidebar-page.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush,
    standalone: true,
    imports: [
        LayoutComponent,
        ContentExtensionComponent,
        AsyncPipe,
    ],
})
export class SidebarPageComponent {
    public url = combineLatest([
        this.schemasState.selectedSchema.pipe(defined()),
        this.contentsState.selectedContent,
    ]).pipe(map(([schema, content]) => {
        const url =
            content ?
            schema.properties.contentSidebarUrl :
            schema.properties.contentsSidebarUrl;

        return url;
    }));

    constructor(
        public readonly contentsState: ContentsState,
        public readonly schemasState: SchemasState,
    ) {
    }
}
