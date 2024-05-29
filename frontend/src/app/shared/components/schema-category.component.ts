/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { CdkDrag, CdkDragDrop, CdkDragHandle, CdkDragStart, CdkDropList } from '@angular/cdk/drag-drop';

import { ChangeDetectionStrategy, Component, EventEmitter, Input, Output } from '@angular/core';
import { RouterLink, RouterLinkActive } from '@angular/router';
import { EditableTitleComponent, StopDragDirective, TooltipDirective, TourStepDirective, TranslatePipe } from '@app/framework';
import { LocalStoreService, SchemaCategory, SchemaDto, SchemasState } from '@app/shared/internal';

const ITEM_HEIGHT = 2.5;

@Component({
    standalone: true,
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
    @Output()
    public remove = new EventEmitter<string>();

    @Input({ required: true })
    public schemaCategory!: SchemaCategory;

    @Input()
    public schemaTarget?: 'Schema' | 'Contents';

    public isCollapsed = false;

    public get forContent() {
        return this.schemaTarget === 'Contents';
    }

    public get schemas() {
        return this.schemaCategory.schemasFiltered;
    }

    constructor(
        private readonly localStore: LocalStoreService,
        private readonly schemasState: SchemasState,
    ) {
    }

    public toggle() {
        this.isCollapsed = !this.isCollapsed;

        this.localStore.setBoolean(this.isCollapsedKey(), this.isCollapsed);
    }

    public ngOnChanges() {
        if (this.schemaCategory.countSchemasInSubtreeFiltered < this.schemaCategory.countSchemasInSubtree) {
            this.isCollapsed = false;
        } else {
            this.isCollapsed = this.localStore.getBoolean(this.isCollapsedKey());
        }
    }

    public schemaRoute(schema: SchemaDto) {
        if (schema.type === 'Singleton' && this.forContent) {
            return [schema.name, schema.id, 'history'];
        } else {
            return [schema.name];
        }
    }

    public changeName(name: string) {
        if (name !== this.schemaCategory.displayName) {
            this.schemasState.renameCategory(this.schemaCategory.displayName, name);
        }
    }

    public changeCategory(drag: CdkDragDrop<any>) {
        if (drag.previousContainer !== drag.container) {
            this.schemasState.changeCategory(drag.item.data, this.schemaCategory.name);
        }
    }

    public dragStarted(event: CdkDragStart) {
        setTimeout(() => {
            const dropContainer = event.source._dragRef['_dropContainer'];

            if (dropContainer) {
                dropContainer['_cacheOwnPosition']();
                dropContainer['_cacheItemPositions']();
            }
        });
    }

    public getItemHeight() {
        return `${ITEM_HEIGHT}rem`;
    }

    public getContainerHeight() {
        return `${ITEM_HEIGHT * this.schemas.length}rem`;
    }

    private isCollapsedKey(): string {
        return `squidex.schema.category.${this.schemaCategory.name}.collapsed`;
    }
}
