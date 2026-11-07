/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Component, computed, inject, Signal } from '@angular/core';
import { toSignal } from '@angular/core/rxjs-interop';
import { FormsModule, ReactiveFormsModule, UntypedFormControl } from '@angular/forms';
import { RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { AppDto, AppsState, getCategoryTree, LayoutComponent, SchemaCategoryComponent, SchemaDto, SchemasState, Settings, TitleComponent, TranslatePipe, UIOptions, value$ } from '@app/shared';

@Component({
    selector: 'sqx-schemas-page',
    styleUrls: ['./schemas-page.component.scss'],
    templateUrl: './schemas-page.component.html',
    imports: [
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
    private readonly schemasState = inject(SchemasState);
    private readonly appsState = inject(AppsState);
    private readonly schemasFilterValue: Signal<string | undefined>;

    private readonly schemas = computed(() => {
        const app = this.appsState.snapshot.selectedApp!;

        return this.schemasState.schemas().filter(schema => isVisible(schema, app));
    });

    protected readonly schemasFilter = new UntypedFormControl();

    protected readonly isEmbedded = inject(UIOptions).value.embedded;

    protected readonly categories = computed(() => {
        return getCategoryTree(this.schemas(), this.schemasState.addedCategories(), this.schemasFilterValue());
    });

    constructor() {
        this.schemasFilterValue = toSignal(value$(this.schemasFilter));
    }
}

function isVisible(schema: SchemaDto, app: AppDto) {
    if (!schema.canReadContents || !schema.isPublished) {
        return false;
    }

    if (schema.type === 'Component') {
        return false;
    }

    return !app.roleProperties[Settings.AppProperties.HIDE_CONTENTS(schema.name)];
}
