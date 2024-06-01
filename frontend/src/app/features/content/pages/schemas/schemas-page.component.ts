/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { AsyncPipe } from '@angular/common';
import { Component, inject } from '@angular/core';
import { FormsModule, ReactiveFormsModule, UntypedFormControl } from '@angular/forms';
import { RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { combineLatest } from 'rxjs';
import { map } from 'rxjs/operators';
import { AppsState, getCategoryTree, LayoutComponent, SchemaCategoryComponent, SchemasState, Settings, TitleComponent, TranslatePipe, UIOptions, value$ } from '@app/shared';

@Component({
    standalone: true,
    selector: 'sqx-schemas-page',
    styleUrls: ['./schemas-page.component.scss'],
    templateUrl: './schemas-page.component.html',
    imports: [
        AsyncPipe,
        FormsModule,
        LayoutComponent,
        ReactiveFormsModule,
        RouterLink,
        RouterLinkActive,
        RouterOutlet,
        SchemaCategoryComponent,
        TitleComponent,
        TranslatePipe,
    ],
        })
export class SchemasPageComponent {
    public schemasFilter = new UntypedFormControl();

    public readonly isEmbedded = inject(UIOptions).value.embedded;

    public schemas =
        this.schemasState.schemas.pipe(
            map(schemas => {
                const app = this.appsState.snapshot.selectedApp!;

                return schemas.filter(schema =>
                    schema.canReadContents &&
                    schema.isPublished &&
                    schema.type !== 'Component' &&
                    !app.roleProperties[Settings.AppProperties.HIDE_CONTENTS(schema.name)],
                );
            }));

    public categories =
        combineLatest([
            this.schemas,
            this.schemasState.addedCategories,
            value$(this.schemasFilter),
        ], getCategoryTree);

    constructor(
        public readonly schemasState: SchemasState,
        private readonly appsState: AppsState,
    ) {
    }
}
