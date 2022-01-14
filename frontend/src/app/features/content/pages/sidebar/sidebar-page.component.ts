/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { ChangeDetectionStrategy, Component } from '@angular/core';
import { combineLatest } from 'rxjs';
import { map } from 'rxjs/operators';
import { defined } from '@app/framework/internal';
import { ContentsState, SchemasState } from '@app/shared';

@Component({
    selector: 'sqx-sidebar-page',
    styleUrls: ['./sidebar-page.component.scss'],
    templateUrl: './sidebar-page.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush,
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
