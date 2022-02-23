/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Directive, ElementRef, Input, OnChanges, OnDestroy, OnInit, Pipe, PipeTransform, Renderer2 } from '@angular/core';
import { Subscription } from 'rxjs';
import { RootFieldDto, TableFields } from '@app/shared';
import { ContentDto, MetaFields, TableField, TableSizes, Types } from '@app/shared/internal';

export function getCellWidth(field: TableField, sizes: TableSizes | undefined | null) {
    if (Types.is(field, RootFieldDto)) {
        field = field.name;
    }

    const size = sizes?.[field] || 0;

    if (size > 0) {
        return size;
    }

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
export class ContentListWidthDirective implements OnChanges, OnDestroy {
    private subscription?: Subscription;
    private sizes?: TableSizes;
    private size = -1;

    @Input('sqxContentListWidth')
    public fields!: ReadonlyArray<TableField>;

    @Input('fields')
    public set tableFields(value: TableFields | undefined | null) {
        this.subscription?.unsubscribe();
        this.subscription = value?.listSizes.subscribe(sizes => {
            this.sizes = sizes;

            this.updateSize();
        });
    }

    constructor(
        private readonly element: ElementRef,
        private readonly renderer: Renderer2,
    ) {
    }

    public ngOnDestroy() {
        this.subscription?.unsubscribe();
        this.subscription = undefined;
    }

    public ngOnChanges() {
        this.updateSize();
    }

    private updateSize() {
        if (!this.fields) {
            return;
        }
    
        let size = 100;

        for (const field of this.fields) {
            size += getCellWidth(field, this.sizes);
        }

        if (size === this.size) {
            return;
        }
    
        const width = `${size}px`;

        this.renderer.setStyle(this.element.nativeElement, 'min-width', width);

        this.size === size;
    }
}

@Directive({
    selector: '[sqxContentListCell]',
})
export class ContentListCellDirective implements OnChanges, OnDestroy {
    private subscription?: Subscription;
    private sizes?: TableSizes;
    private size = -1;
    private fieldName?: string;

    @Input('field')
    public set field(value: TableField) {
        this.fieldName = Types.is(value, RootFieldDto) ? value.name : value;
    }

    @Input('fields')
    public set tableFields(value: TableFields | undefined | null) {
        this.subscription?.unsubscribe();
        this.subscription = value?.listSizes.subscribe(sizes => {
            this.sizes = sizes;

            this.updateSize();
        });
    }

    constructor(
        private readonly element: ElementRef,
        private readonly renderer: Renderer2,
    ) {
    }

    public ngOnDestroy() {
        this.subscription?.unsubscribe();
        this.subscription = undefined;
    }

    public ngOnChanges() {
        this.updateSize();
    }

    private updateSize() {
        if (!this.fieldName) {
            return;
        }
    
        const size = getCellWidth(this.fieldName, this.sizes);

        if (size === this.size) {
            return;
        }
    
        const width = `${size}px`;

        this.renderer.setStyle(this.element.nativeElement, 'min-width', width);
        this.renderer.setStyle(this.element.nativeElement, 'max-width', width);
        this.renderer.setStyle(this.element.nativeElement, 'width', width);

        this.size === size;
    }
}

@Directive({
    selector: '[sqxContentListCellResize]',
})
export class ContentListCellResizeDirective implements OnInit, OnDestroy {
    private mouseMove?: Function;
    private mouseUp?: Function;
    private mouseDown?: Function;
    private mouseBlur?: Function;
    private documentBlur?: Function;
    private startOffset = 0;
    private startWidth = 0;
    private fieldName?: string;
    private resizer: any;

    @Input('field')
    public set field(value: TableField) {
        this.fieldName = Types.is(value, RootFieldDto) ? value.name : undefined;
    }

    @Input('fields')
    public tableFields?: TableFields;

    constructor(
        private readonly element: ElementRef<HTMLTableCellElement>,
        private readonly renderer: Renderer2,
    ) {
    }

    public ngOnDestroy() {
        this.mouseDown?.();
        this.mouseDown = undefined;
        this.mouseBlur?.();
        this.mouseBlur = undefined;
        
        this.resetMovement();
    }
  
    public ngOnInit() {  
        if (!this.tableFields || !this.fieldName) {
            return;
        }

        this.resizer = this.renderer.createElement('span');

        this.renderer.addClass(this.resizer, 'resize-holder');
        this.renderer.appendChild(this.element.nativeElement, this.resizer);

        this.mouseDown = this.renderer.listen(this.resizer, 'mousedown', this.onMouseDown);
        this.mouseBlur = this.renderer.listen(this.resizer, 'blur', this.onMouseUp);
    }
  
    private onMouseDown = (event: MouseEvent) => {
        if (!this.tableFields || !this.fieldName) {
            return;
        }

        this.resizer.focus();

        this.mouseMove = this.renderer.listen('document', 'mousemove', this.onMouseMove);
        this.mouseUp = this.renderer.listen('document', 'mouseup', this.onMouseUp);
        this.documentBlur = this.renderer.listen('document', 'blur', this.onBlur);

        this.startOffset = event.pageX;
        this.startWidth = this.element.nativeElement.offsetWidth;
    };
  
    private onMouseMove = (event: MouseEvent) => {
        if (!this.mouseMove || !this.tableFields || !this.fieldName) {
            return;
        }
  
        const width = this.startWidth + (event.pageX - this.startOffset);
  
        this.tableFields.updateSize(this.fieldName, width, false);
    };
  
    private onMouseUp = (event: MouseEvent) => {
        if (!this.mouseMove || !this.tableFields || !this.fieldName) {
            return;
        }

        this.resetMovement();

        const width = this.startWidth + (event.pageX - this.startOffset);
  
        this.tableFields.updateSize(this.fieldName, width, true);
    };

    private onBlur = () => {
        this.resetMovement();
    };

    private resetMovement() {
        this.mouseMove?.();
        this.mouseMove = undefined;
        this.mouseUp?.();
        this.mouseUp = undefined;
        this.documentBlur?.();
        this.documentBlur = undefined;
    }
}