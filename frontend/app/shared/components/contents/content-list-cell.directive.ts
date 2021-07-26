/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Directive, ElementRef, Input, OnChanges, Pipe, PipeTransform, Renderer2 } from '@angular/core';
import { ContentDto, MetaFields, RootFieldDto, TableField, Types } from '@app/shared/internal';

export function getTableWidth(fields: ReadonlyArray<TableField>) {
    let result = 0;

    for (const field of fields) {
        result += getCellWidth(field);
    }

    return result;
}

export function getCellWidth(field: TableField) {
    if (Types.is(field, RootFieldDto)) {
        return 220;
    } else {
        switch (field) {
            case MetaFields.id:
                return 280;
            case MetaFields.created:
                return 150;
            case MetaFields.createdByAvatar:
                return 55;
            case MetaFields.createdByName:
                return 150;
            case MetaFields.lastModified:
                return 150;
            case MetaFields.lastModifiedByAvatar:
                return 55;
            case MetaFields.lastModifiedByName:
                return 150;
            case MetaFields.status:
                return 200;
            case MetaFields.statusNext:
                return 240;
            case MetaFields.statusColor:
                return 50;
            case MetaFields.version:
                return 80;
        }

        return 0;
    }
}

@Pipe({
    name: 'sqxContentsColumns',
    pure: true,
})
export class ContentsColumnsPipe implements PipeTransform {
    public transform(value: ReadonlyArray<ContentDto>) {
        let columns = 1;

        for (const content of value) {
            columns = Math.max(columns, content.referenceFields.length);
        }

        return columns;
    }
}

@Pipe({
    name: 'sqxContentListWidth',
    pure: true,
})
export class ContentListWidthPipe implements PipeTransform {
    public transform(value: ReadonlyArray<TableField>) {
        if (!value) {
            return 0;
        }

        return `${getTableWidth(value) + 100}px`;
    }
}

@Directive({
    selector: '[sqxContentListCell]',
})
export class ContentListCellDirective implements OnChanges {
    @Input('sqxContentListCell')
    public field: TableField;

    constructor(
        private readonly element: ElementRef,
        private readonly renderer: Renderer2,
    ) {
    }

    public ngOnChanges() {
        if (Types.isString(this.field) && this.field) {
            const width = `${getCellWidth(this.field)}px`;

            this.renderer.setStyle(this.element.nativeElement, 'min-width', width);
            this.renderer.setStyle(this.element.nativeElement, 'max-width', width);
            this.renderer.setStyle(this.element.nativeElement, 'width', width);
        }
    }
}
