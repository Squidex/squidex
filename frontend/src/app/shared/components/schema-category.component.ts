/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { CdkDrag, CdkDragDrop, CdkDragHandle, CdkDragStart, CdkDropList } from '@angular/cdk/drag-drop';

import { ChangeDetectionStrategy, Component, computed, inject, input, linkedSignal, output } from '@angular/core';
import { RouterLink, RouterLinkActive } from '@angular/router';
import { EditableTitleComponent, StopDragDirective, TooltipDirective, TourStepDirective, TranslatePipe } from '@app/framework';
import { LocalStoreService, SchemaCategory, SchemaDto, SchemasState } from '@app/shared/internal';

const ITEM_HEIGHT = 2.5;

@Component({
    selector: 'sqx-schema-category',
    styleUrls: ['./schema-category.component.scss'],
    templateUrl: './schema-category.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush,
    imports: [
        CdkDrag,
        CdkDragHandle,
        CdkDropList,
        EditableTitleComponent,
        RouterLink,
        RouterLinkActive,
        StopDragDirective,
        TooltipDirective,
        TourStepDirective,
        TranslatePipe,
    ],
})
export class SchemaCategoryComponent {
    private readonly localStore = inject(LocalStoreService);
    private readonly schemasState = inject(SchemasState);

    /** Emits the name of the category that should be removed. */
    public readonly remove = output<string>();

    /** The category with the schemas and the child categories. */
    public readonly schemaCategory = input.required<SchemaCategory>();

    /** Defines whether the links point to the schema editor or to the contents. */
    public readonly schemaTarget = input<'Schema' | 'Contents'>();

    protected readonly isCollapsed = linkedSignal(() => {
        const schemaCategory = this.schemaCategory();
        if (schemaCategory.countSchemasInSubtreeFiltered < schemaCategory.countSchemasInSubtree) {
            return false;
        }

        return this.isCollapsedStore()();
    });

    protected readonly isCollapsedStore = computed(() => {
        return this.localStore.boolean(getCollapsedKey(this.schemaCategory()));
    });

    protected readonly forContent = computed(() => {
        return this.schemaTarget() === 'Contents';
    });

    protected readonly schemas = computed(() => {
        return this.schemaCategory().schemasFiltered;
    });

    protected readonly containerHeight = computed(() => {
        return `${ITEM_HEIGHT * this.schemas().length}rem`;
    });

    protected toggle() {
        const isCollapsed = !this.isCollapsed();

        this.isCollapsed.set(isCollapsed);
        this.isCollapsedStore().set(isCollapsed);
    }

    protected schemaRoute(schema: SchemaDto) {
        if (schema.type === 'Singleton' && this.forContent()) {
            return [schema.name, schema.id, 'history'];
        } else {
            return [schema.name];
        }
    }

    protected changeName(name: string) {
        const displayName = this.schemaCategory().displayName;
        if (name === displayName) {
            return;
        }

        this.schemasState.renameCategory(displayName, name);
    }

    protected changeCategory(drag: CdkDragDrop<any>) {
        if (drag.previousContainer === drag.container) {
            return;
        }

        this.schemasState.changeCategory(drag.item.data, this.schemaCategory().name);
    }

    protected dragStarted(event: CdkDragStart) {
        setTimeout(() => {
            const dropContainer = event.source._dragRef['_dropContainer'];

            if (dropContainer) {
                dropContainer['_cacheOwnPosition']();
                dropContainer['_cacheItemPositions']();
            }
        });
    }
}

function getCollapsedKey(schemaCategory: SchemaCategory) {
    return `squidex.schema.category.${schemaCategory.name}.collapsed`;
}
