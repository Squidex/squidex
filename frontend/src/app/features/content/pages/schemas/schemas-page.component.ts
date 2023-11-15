/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { AsyncPipe, NgFor, NgIf } from '@angular/common';
import { Component, inject } from '@angular/core';
import { FormsModule, ReactiveFormsModule, UntypedFormControl } from '@angular/forms';
import { RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { combineLatest } from 'rxjs';
import { map } from 'rxjs/operators';
import { AppsState, getCategoryTree, LayoutComponent, SchemaCategory, SchemaCategoryComponent, SchemasState, Settings, TitleComponent, TranslatePipe, UIOptions, value$ } from '@app/shared';

@Component({
    selector: 'sqx-schemas-page',
    styleUrls: ['./schemas-page.component.scss'],
    templateUrl: './schemas-page.component.html',
    standalone: true,
    imports: [
        TitleComponent,
        NgIf,
        LayoutComponent,
        FormsModule,
        ReactiveFormsModule,
        RouterLink,
        RouterLinkActive,
        NgFor,
        SchemaCategoryComponent,
        RouterOutlet,
        AsyncPipe,
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
            value$(this.schemasFilter),
            this.schemas,
            this.schemasState.addedCategories,
        ], (filter, schemas, categories) => {
            return getCategoryTree(schemas, categories, filter);
        });

    constructor(
        public readonly schemasState: SchemasState,
        private readonly appsState: AppsState,
    ) {
    }

    public trackByCategory(_index: number, category: SchemaCategory) {
        return category.name;
    }
}
