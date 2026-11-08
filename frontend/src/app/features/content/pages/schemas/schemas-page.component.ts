/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Component, computed, inject, signal, ChangeDetectionStrategy } from '@angular/core';
import { form, FormField } from '@angular/forms/signals';
import { RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { AppDto, AppsState, getCategoryTree, LayoutComponent, SchemaCategoryComponent, SchemaDto, SchemasState, Settings, TitleComponent, TranslatePipe, UIOptions } from '@app/shared';

@Component({
    selector: 'sqx-schemas-page',
    styleUrls: ['./schemas-page.component.scss'],
    templateUrl: './schemas-page.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush,
    imports: [
        FormField,
        LayoutComponent,
        RouterLink,
        RouterLinkActive,
        RouterOutlet,
        SchemaCategoryComponent,
        TitleComponent,
        TranslatePipe,
    ],
})
export class SchemasPageComponent {
    private readonly appsState = inject(AppsState);
    private readonly schemasState = inject(SchemasState);
    private readonly uiOptions = inject(UIOptions);

    protected readonly schemas = computed(() => {
        const app = this.appsState.snapshot.selectedApp!;

        return this.schemasState.schemas().filter(schema => isVisible(schema, app));
    });

    protected readonly categories = computed(() => {
        return getCategoryTree(this.schemas(), this.schemasState.addedCategories(), this.schemasFilter().value());
    });

    protected readonly schemasFilter = form(signal(''));

    protected readonly isEmbedded = this.uiOptions.value.embedded;
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
