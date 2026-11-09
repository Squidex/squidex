/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

/* eslint-disable @angular-eslint/no-input-rename */

import { computed, Directive, effect, ElementRef, inject, input, numberAttribute, Pipe, PipeTransform, Renderer2 } from '@angular/core';
import { ContentDto, FieldSizes, META_FIELDS, TableField, TableSettings } from '@app/shared/internal';

export function getCellWidth(field: TableField, sizes: FieldSizes | undefined | null) {
    const size = sizes?.[field.name] || 0;

    if (size > 0) {
        return size;
    }

    switch (field) {
        case META_FIELDS.id:
            return 280;
        case META_FIELDS.created:
            return 150;
        case META_FIELDS.createdByAvatar:
            return 55;
        case META_FIELDS.createdByName:
            return 150;
        case META_FIELDS.lastModified:
            return 150;
        case META_FIELDS.lastModifiedByAvatar:
            return 55;
        case META_FIELDS.lastModifiedByName:
            return 150;
        case META_FIELDS.status:
            return 200;
        case META_FIELDS.statusNext:
            return 240;
        case META_FIELDS.statusColor:
            return 80;
        case META_FIELDS.version:
            return 80;
        default:
            return 200;
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

@Directive({
    selector: '[sqxContentListWidth]',
})
export class ContentListWidthDirective {
    private readonly element = inject(ElementRef);
    private readonly renderer = inject(Renderer2);

    /** The fields to calculate the minimum width of the table from. */
    public readonly fields = input<ReadonlyArray<TableField> | null | undefined>(undefined, { alias: 'sqxContentListWidth' });

    /** The table settings that provide the custom column sizes. */
    public readonly tableSettings = input<TableSettings | null | undefined>(undefined, { alias: 'fields' });

    protected readonly size = computed(() => {
        const fields = this.fields();
        if (!fields) {
            return undefined;
        }

        const sizes = this.tableSettings()?.fieldSizes();

        let size = 100;

        for (const field of fields) {
            size += getCellWidth(field, sizes);
        }

        return size;
    });

    constructor() {
        effect(() => {
            const size = this.size();
            if (size === undefined) {
                return;
            }

            this.renderer.setStyle(this.element.nativeElement, 'min-width', `${size}px`);
        });
    }
}

@Directive({
    selector: '[sqxContentListCell]',
})
export class ContentListCellDirective {
    private readonly element = inject(ElementRef);
    private readonly renderer = inject(Renderer2);

    /** The field of the cell to calculate the width for. */
    public readonly field = input.required<TableField>();

    /** The table settings that provide the custom column sizes. */
    public readonly tableSettings = input<TableSettings | null | undefined>(undefined, { alias: 'fields' });

    protected readonly size = computed(() => {
        const field = this.field();
        if (!field.name) {
            return undefined;
        }

        return getCellWidth(field, this.tableSettings()?.fieldSizes());
    });

    constructor() {
        effect(() => {
            const size = this.size();
            if (size === undefined) {
                return;
            }

            const width = `${size}px`;

            this.renderer.setStyle(this.element.nativeElement, 'min-width', width);
            this.renderer.setStyle(this.element.nativeElement, 'max-width', width);
            this.renderer.setStyle(this.element.nativeElement, 'width', width);
        });
    }
}

@Directive({
    selector: '[sqxContentListCellResize][field][fields]',
})
export class ContentListCellResizeDirective {
    private readonly element = inject<ElementRef<HTMLTableCellElement>>(ElementRef);
    private readonly renderer = inject(Renderer2);
    private mouseMove?: Function;
    private mouseUp?: Function;
    private resizer: any;
    private startOffset = 0;
    private startWidth = 0;
    private windowBlur?: Function;

    /** The field of the cell to resize. */
    public readonly field = input<TableField>();

    /** The table settings to store the new size in. */
    public readonly tableFields = input<TableSettings | null | undefined>(undefined, { alias: 'fields' });

    /** The minimum width of the cell in pixels. */
    public readonly minimumWidth = input(50, { transform: numberAttribute });

    protected readonly isResizable = computed(() => {
        return !!this.field() && !!this.tableFields();
    });

    constructor() {
        effect(onCleanup => {
            if (!this.isResizable()) {
                return;
            }

            const resizer = this.renderer.createElement('span');

            this.renderer.addClass(resizer, 'resize-holder');
            this.renderer.appendChild(this.element.nativeElement, resizer);

            const mouseDown = this.renderer.listen(resizer, 'mousedown', (event: MouseEvent) => this.onMouseDown(event));
            const mouseBlur = this.renderer.listen(resizer, 'blur', (event: MouseEvent) => this.onMouseUp(event));

            this.resizer = resizer;

            onCleanup(() => {
                mouseDown();
                mouseBlur();

                this.resetMovement();
                this.renderer.removeChild(this.element.nativeElement, resizer);
            });
        });
    }

    private onMouseDown(event: MouseEvent) {
        if (!this.isResizable()) {
            return;
        }

        this.resizer.focus();

        this.mouseMove = this.renderer.listen('document', 'mousemove', (event: MouseEvent) => this.onMouseMove(event));
        this.mouseUp = this.renderer.listen('document', 'mouseup', (event: MouseEvent) => this.onMouseUp(event));
        this.windowBlur = this.renderer.listen('window', 'blur', () => this.onBlur());

        this.startOffset = event.pageX;
        this.startWidth = this.element.nativeElement.offsetWidth;
    }

    private onMouseMove(event: MouseEvent) {
        if (!this.mouseMove || !this.isResizable()) {
            return;
        }

        try {
            this.updateWidth(event, false);
        } catch {
            this.resetMovement();
        }
    }

    private onMouseUp(event: MouseEvent) {
        if (!this.mouseMove || !this.isResizable()) {
            return;
        }

        try {
            this.updateWidth(event, true);
        } finally {
            this.resetMovement();
        }
    }

    private onBlur() {
        this.resetMovement();
    }

    private updateWidth(event: MouseEvent, save: boolean) {
        let width = this.startWidth + (event.pageX - this.startOffset);

        if (width < this.minimumWidth()) {
            width = this.minimumWidth();
        }

        this.tableFields()!.updateSize(this.field()!.name!, width, save);
    }

    private resetMovement() {
        this.mouseMove?.();
        this.mouseMove = undefined;
        this.mouseUp?.();
        this.mouseUp = undefined;
        this.windowBlur?.();
        this.windowBlur = undefined;
    }
}
